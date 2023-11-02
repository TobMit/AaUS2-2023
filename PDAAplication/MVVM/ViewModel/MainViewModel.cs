using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using PDAAplication.Core;
using PDAAplication.Core.DataManager.FileManager;
using PDAAplication.MVVM.Model;
using PDAAplication.MVVM.View;
using Quadtree.StructureClasses;

namespace PDAAplication.MVVM.ViewModel
{
    class MainViewModel : ObservableObjects
    {
        private QuadTree<int,ObjectModel> _quadTreeNehnutelnost;
        private QuadTree<int,ObjectModel> _quadTreeParcela;
        private QuadTree<int,ObjectModel> _quadTreeJednotne;

        private List<ObjectModel> _allNehnutelnosti;
        private List<ObjectModel> _allParcelas;

        public RelayCommand GenerateDataCommand { get; set; }
        public RelayCommand FindBuildingsCommand { get; set; }
        public RelayCommand FindObjectCommand { get; set; }
        public RelayCommand AddBuildingCommand { get; set; }
        public RelayCommand ShowAllComand { get; set; }
        public RelayCommand SaveDataCommand { get; set; }
        public RelayCommand LoadDataCommand { get; set; }

        private Visibility splitViewShow;

        public Visibility SplitViewShow
        {
            get
            {
                return splitViewShow;
            }
            set
            {
                splitViewShow = value;
                OnPropertyChanged();
            }
        }

        private Visibility singleViewShow;

        public Visibility SingleViewShow
        {
            get
            {
                return singleViewShow;
            }
            set
            {
                singleViewShow = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<ObjectModel> _listNehnutelnost;

        public ObservableCollection<ObjectModel> ListNehnutelnost
        {
            get { return _listNehnutelnost; }
            set
            {
                _listNehnutelnost = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<ObjectModel> _listParcela;

        public ObservableCollection<ObjectModel> ListParcela
        {
            get { return _listParcela; }
            set
            {
                _listParcela = value;
                OnPropertyChanged();
            }
        }



        public MainViewModel()
        {
            InicializeButtons();
            _allNehnutelnosti = new();
            _allParcelas = new();
            SplitViewShow = Visibility.Visible;
            SingleViewShow = Visibility.Hidden;
        }

        private void InicializeButtons()
        {
            GenerateDataCommand = new RelayCommand(o => { GenerateData(); });
            FindBuildingsCommand = new RelayCommand(o => { FindBuildings(); });
            FindObjectCommand = new RelayCommand(o => { FindObject(); });
            AddBuildingCommand = new RelayCommand(o => { AddBuilding(); });
            ShowAllComand = new RelayCommand(o => { ShowAll(); });
            SaveDataCommand = new RelayCommand(o => { SaveData(); });
            LoadDataCommand = new RelayCommand(o => { LoadData(); });
        }

        private void GenerateData()
        {
            var dlg = new DataGeneratorDialog();
            dlg.ShowDialog();
            int pocetNehnutelnosti = 0;
            int pocetParciel = 0;
            GPS juhoZapadneGPS = new GPS();
            int dlzka = 0;
            int sirka = 0;
            if (dlg.DialogResult == true)
            {
                pocetNehnutelnosti = dlg.PocetNehnutelnosti;
                pocetParciel = dlg.PocetParciel;
                juhoZapadneGPS = new(dlg.x, dlg.y);
                dlzka = dlg.dlzka;
                sirka = dlg.sirka;
            }

            if (pocetParciel <= 0 || pocetNehnutelnosti <= 0 || dlzka <= 0 || sirka <= 0)
            {
                return;
            }

            _quadTreeNehnutelnost = new QuadTree<int, ObjectModel>(juhoZapadneGPS.X, juhoZapadneGPS.Y, sirka, dlzka);
            _quadTreeParcela = new QuadTree<int, ObjectModel>(juhoZapadneGPS.X, juhoZapadneGPS.Y, sirka, dlzka);
            _quadTreeJednotne = new QuadTree<int, ObjectModel>(juhoZapadneGPS.X, juhoZapadneGPS.Y, sirka, dlzka);

            ListParcela = new ();
            ListNehnutelnost = new ();
            _allNehnutelnosti = new(pocetNehnutelnosti);
            _allParcelas = new(pocetParciel);

            Core.DataManager.DataGenerator.GenerateData(_quadTreeNehnutelnost,
                _quadTreeParcela,
                _quadTreeJednotne,
                ListNehnutelnost,
                ListParcela,
                _allNehnutelnosti,
                _allParcelas,
                juhoZapadneGPS,
                new(juhoZapadneGPS.X + sirka, juhoZapadneGPS.Y + dlzka),
                pocetNehnutelnosti,
                pocetParciel);
            //Console.WriteLine("Dogenerovane");
            //ListNehnutelnost = _listNehnutelnost;
            //ListParcela = _listParcela;



        }

        private void FindBuildings()
        {
            if (_quadTreeNehnutelnost is null)
            {
                return;
            }
            var dlg = new FindBuilding();
            dlg.ShowDialog();
            GPS juhoZapadneGPS = new GPS();
            GPS severoVýchodneGPS = new GPS();
            bool cancel = true;
            if (dlg.DialogResult == true)
            {
                juhoZapadneGPS = new(dlg.x, dlg.y);
                severoVýchodneGPS = new(dlg.x, dlg.y);
                cancel = false;
            }

            if (cancel)
            {
                return;
            }
            ListNehnutelnost = new ObservableCollection<ObjectModel>(_quadTreeNehnutelnost.Find(juhoZapadneGPS.X, juhoZapadneGPS.Y, severoVýchodneGPS.X, severoVýchodneGPS.Y));
            ListParcela = new();
        }

        private void FindObject()
        {
            if (_quadTreeNehnutelnost is null || _quadTreeParcela is null)
            {
                return;
            }
            var dlg = new FindObjects("Vyhľadanie objektov");
            dlg.ShowDialog();
            GPS juhoZapadneGPS = new GPS();
            GPS severoVýchodneGPS = new GPS();
            bool cancel = true;
            if (dlg.DialogResult == true)
            {
                juhoZapadneGPS = new(dlg.x, dlg.y);
                severoVýchodneGPS = new(dlg.x2, dlg.y2);
                cancel = false;
            }

            if (cancel)
            {
                return;
            }

            ChangeView(false);

            ListParcela = new ObservableCollection<ObjectModel>(_quadTreeJednotne.FindOverlapingData(juhoZapadneGPS.X, juhoZapadneGPS.Y, severoVýchodneGPS.X, severoVýchodneGPS.Y));
        }

        private void AddBuilding()
        {
            if (_quadTreeNehnutelnost is null)
            {
                return;
            }
            var dlg = new AddNehnutelnost();
            dlg.ShowDialog();
            GPS juhoZapadneGPS = new GPS();
            GPS severoVýchodneGPS = new GPS();
            string popis = "";
            int supisneCislo = 0;
            bool cancel = true;
            if (dlg.DialogResult == true)
            {
                juhoZapadneGPS = new(Math.Round(dlg.x), Math.Round(dlg.y));
                severoVýchodneGPS = new(Math.Round(dlg.x2), Math.Round(dlg.y2));
                popis = dlg.popis;
                supisneCislo = dlg.supisneCislo;
                cancel = false;
            }

            if (cancel)
            {
                return;
            }
            // musím nájsť všetky parcely, ktoré obsahujú túto nehnuteľnosť
            // pridať tam tú nehnuteľnosť
            // pridať nehnuteľnosť do zoznamu všetkých nehnutelností a všetky parcely do nehnuteľnosti
            // pridať nehnuteľnosť do quad tree
            ObjectModel tmpNehnutelnost = new(supisneCislo, popis, juhoZapadneGPS, severoVýchodneGPS, Core.ObjectType.Nehnutelnost);
            var tmpListParciel = _quadTreeParcela.FindOverlapingData(juhoZapadneGPS.X, juhoZapadneGPS.Y, severoVýchodneGPS.X, severoVýchodneGPS.Y);
            foreach (ObjectModel parcela in tmpListParciel)
            {
                tmpNehnutelnost.ZoznamObjektov.Add(parcela);
                parcela.ZoznamObjektov.Add(tmpNehnutelnost);
            }
            _quadTreeNehnutelnost.Insert(juhoZapadneGPS.X, juhoZapadneGPS.Y, severoVýchodneGPS.X, severoVýchodneGPS.Y, tmpNehnutelnost);
            _allNehnutelnosti.Add(tmpNehnutelnost);
        }

        private void ShowAll()
        {
            ChangeView(true);
            ListNehnutelnost = new ObservableCollection<ObjectModel>(_allNehnutelnosti);
            ListParcela = new ObservableCollection<ObjectModel>(_allParcelas);
        }

        private void SaveData()
        {
            DataSaver<ObjectModel> saver = new();
            saver.AddLine(_quadTreeJednotne.OriginalPointDownLeft.X + ";" + _quadTreeJednotne.OriginalPointDownLeft.Y + ";" + _quadTreeJednotne.OriginalPointUpRight.X + ";" + _quadTreeJednotne.OriginalPointUpRight.Y + "\n");
            saver.PrepareForSave(_quadTreeJednotne.ToList());
            saver.SaveData();
        }

        private void LoadData()
        {
            DataLoader loader = new();

            ListParcela = new();
            ListNehnutelnost = new();
            _allNehnutelnosti = new();
            _allParcelas = new();

            
            loader.LoadData(_quadTreeNehnutelnost,
                _quadTreeParcela,
                _quadTreeJednotne,
                ListNehnutelnost,
                ListParcela,
                _allNehnutelnosti,
                _allParcelas);

        }

        private static double NextDouble(double min, double max, Random rnd)
        {
            return rnd.NextDouble() * (max - min) + min;
        }

        private void ChangeView(bool splitView)
        {
            if (splitView)
            {
                SplitViewShow = Visibility.Visible;
                SingleViewShow = Visibility.Hidden;
            }
            else
            {
                SplitViewShow = Visibility.Hidden;
                SingleViewShow = Visibility.Visible;
            }
        }
    }
}
