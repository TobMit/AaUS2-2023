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
        public RelayCommand FindParcelaCommand { get; set; }
        public RelayCommand FindObjectCommand { get; set; }
        public RelayCommand AddBuildingCommand { get; set; }
        public RelayCommand AddParcelaCommand { get; set; }
        public RelayCommand EditCommand { get; set; }
        public RelayCommand DeleteCommand { get; set; }
        public RelayCommand ShowAllCommand { get; set; }
        public RelayCommand SaveDataCommand { get; set; }
        public RelayCommand LoadDataCommand { get; set; }

        private Visibility _splitViewShow;

        public Visibility SplitViewShow
        {
            get => _splitViewShow;
            set
            {
                _splitViewShow = value;
                OnPropertyChanged();
            }
        }

        private Visibility _singleViewShow;

        public Visibility SingleViewShow
        {
            get => _singleViewShow;
            set
            {
                _singleViewShow = value;
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

        private string _healthNehnutelnosti;

        public string HealthNehnutelnosti
        {
            get
            {
                return "Zdravie stromu nehnuteľností: " + _healthNehnutelnosti + "%";
            }
            set
            {
                _healthNehnutelnosti = value;
                OnPropertyChanged();
            }
        }

        private string _healthParcela;

        public string HealthParcela
        {
            get
            {
                return "Zdravie stromu parciel: " + _healthParcela + "%";
            }
            set
            {
                _healthParcela = value;
                OnPropertyChanged();
            }
        }

        private string _healthJednotne;

        public string HealthJednotne
        {
            get
            {
                return "Zdravie stromu zjednotených dát: " + _healthJednotne + "%";
            }
            set
            {
                _healthJednotne = value;
                OnPropertyChanged();
            }
        }



        public MainViewModel()
        {
            InitializeButtons();
            _allNehnutelnosti = new();
            _allParcelas = new();
            SplitViewShow = Visibility.Visible;
            SingleViewShow = Visibility.Hidden;
        }

        private void InitializeButtons()
        {
            GenerateDataCommand = new RelayCommand(o => { GenerateData(); });
            FindBuildingsCommand = new RelayCommand(o => { FindBuildings(); });
            FindParcelaCommand = new RelayCommand(o => { FindParcela(); });
            FindObjectCommand = new RelayCommand(o => { FindObject(); });
            AddBuildingCommand = new RelayCommand(o => { AddBuilding(); });
            AddParcelaCommand = new RelayCommand(o => { AddParcela(); });
            EditCommand = new RelayCommand(o =>
            {
                var tmp = (ObjectModel)o;
                EditObject(tmp);
            });
            DeleteCommand = new RelayCommand(o =>
            {
                var tmp = (ObjectModel)o;
                DeleteObject(tmp);
            });
            ShowAllCommand = new RelayCommand(o => { ShowAll(); });
            SaveDataCommand = new RelayCommand(o => { SaveData(); });
            LoadDataCommand = new RelayCommand(o => { LoadData(); });
        }

        private void GenerateData()
        {
            var dlg = new DataGeneratorDialog();
            dlg.ShowDialog();
            int pocetNehnutelnosti = 0;
            int pocetParciel = 0;
            GPS gps1 = new GPS();
            int dlzka = 0;
            int sirka = 0;
            if (dlg.DialogResult == true)
            {
                pocetNehnutelnosti = dlg.PocetNehnutelnosti;
                pocetParciel = dlg.PocetParciel;
                gps1 = new(dlg.x, dlg.y);
                dlzka = dlg.dlzka;
                sirka = dlg.sirka;
            }

            if (pocetParciel <= 0 || pocetNehnutelnosti <= 0 || dlzka <= 0 || sirka <= 0)
            {
                return;
            }

            _quadTreeNehnutelnost = new QuadTree<int, ObjectModel>(gps1.X, gps1.Y, sirka, dlzka);
            _quadTreeParcela = new QuadTree<int, ObjectModel>(gps1.X, gps1.Y, sirka, dlzka);
            _quadTreeJednotne = new QuadTree<int, ObjectModel>(gps1.X, gps1.Y, sirka, dlzka);

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
                gps1,
                new(gps1.X + sirka, gps1.Y + dlzka),
                pocetNehnutelnosti,
                pocetParciel);

            HealthJednotne = Math.Round(_quadTreeJednotne.Health * 100).ToString();
            HealthNehnutelnosti = Math.Round(_quadTreeNehnutelnost.Health*100).ToString();
            HealthParcela = Math.Round(_quadTreeParcela.Health * 100).ToString();

        }

        private void FindBuildings()
        {
            if (_quadTreeNehnutelnost is null)
            {
                return;
            }
            var dlg = new FindObject("Vyhľadanie budovy");
            dlg.ShowDialog();
            GPS gps1 = new GPS();
            GPS gps2 = new GPS();
            bool cancel = true;
            if (dlg.DialogResult == true)
            {
                gps1 = new(dlg.x, dlg.y);
                gps2 = new(dlg.x, dlg.y);
                cancel = false;
            }

            if (cancel)
            {
                return;
            }
            ListNehnutelnost = new ObservableCollection<ObjectModel>(_quadTreeNehnutelnost.FindIntervalOverlapping(gps1.X, gps1.Y, gps2.X, gps2.Y));
            ListParcela = new();
        }

        private void FindParcela()
        {
            if (_quadTreeNehnutelnost is null)
            {
                return;
            }
            var dlg = new FindObject("Vyhľadanie parcely");
            dlg.ShowDialog();
            GPS gps1 = new GPS();
            GPS gps2 = new GPS();
            bool cancel = true;
            if (dlg.DialogResult == true)
            {
                gps1 = new(dlg.x, dlg.y);
                gps2 = new(dlg.x, dlg.y);
                cancel = false;
            }

            if (cancel)
            {
                return;
            }
            ListParcela = new ObservableCollection<ObjectModel>(_quadTreeParcela.FindIntervalOverlapping(gps1.X, gps1.Y, gps2.X, gps2.Y));
            ListNehnutelnost = new();
        }

        private void FindObject()
        {
            if (_quadTreeNehnutelnost is null || _quadTreeParcela is null)
            {
                return;
            }
            var dlg = new FindObjects("Vyhľadanie objektov");
            dlg.ShowDialog();
            GPS dlgGps1 = new GPS();
            GPS dlgGps2 = new GPS();
            bool cancel = true;
            if (dlg.DialogResult == true)
            {
                dlgGps1 = new(dlg.x, dlg.xOz, dlg.y, dlg.yOz);
                dlgGps2 = new(dlg.x2, dlg.x2Oz, dlg.y2, dlg.y2Oz);
                cancel = false;
            }

            if (cancel)
            {
                return;
            }

            ChangeView(false);

            GPS checkedGps1 = new GPS();
            GPS checkedGps2 = new GPS();

            if (!Core.Utils.CheckAndRecalculateGps(dlgGps1, dlgGps2, checkedGps1, checkedGps2))
            {
                MessageBox.Show("Zle zadaný vstup!", "Chyba vstupu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ListParcela = new ObservableCollection<ObjectModel>(_quadTreeJednotne.FindIntervalOverlapping(checkedGps1.X, checkedGps1.Y, checkedGps2.X, checkedGps2.Y));
        }

        private void AddBuilding()
        {
            if (_quadTreeNehnutelnost is null)
            {
                return;
            }
            var dlg = new AddObject(Core.ObjectType.Nehnutelnost);
            dlg.ShowDialog();
            GPS dlgGps1 = new GPS();
            GPS dlgGps2 = new GPS();
            string popis = "";
            int idCislo = 0;
            bool cancel = true;
            if (dlg.DialogResult == true)
            {
                dlgGps1 = new(dlg.x, dlg.xOz, dlg.y, dlg.yOz);
                dlgGps2 = new(dlg.x2, dlg.x2Oz, dlg.y2, dlg.y2Oz);
                popis = dlg.popis;
                idCislo = dlg.IdCislo;
                cancel = false;
            }

            if (cancel)
            {
                return;
            }

            GPS checkedGps1 = new GPS();
            GPS checkedGps2 = new GPS();

            if (!Core.Utils.CheckAndRecalculateGps(dlgGps1, dlgGps2, checkedGps1, checkedGps2))
            {
                MessageBox.Show("Nie je možné pridať nehnuteľnosť do stromu, zlý vstup", "Chyba vstupu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // kontrolujem či vôbec môžem pridať do stromu
            if (!_quadTreeNehnutelnost.QuadTreeCanContain(checkedGps1.X, checkedGps1.Y, checkedGps2.X, checkedGps2.Y))
            {
                MessageBox.Show("Nie je možné pridať nehnuteľnosť do stromu", "Chyba vstupu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            // musím nájsť všetky parcely, ktoré obsahujú túto nehnuteľnosť
            // pridať tam tú nehnuteľnosť
            // pridať nehnuteľnosť do zoznamu všetkých nehnuteľností a všetky parcely do nehnuteľnosti
            // pridať nehnuteľnosť do quad tree
            ObjectModel tmpNehnutelnost = new(idCislo, popis, dlgGps1, dlgGps2, Core.ObjectType.Nehnutelnost);
            var tmpListParciel = _quadTreeParcela.FindIntervalOverlapping(checkedGps1.X, checkedGps1.Y, checkedGps2.X, checkedGps2.Y);
            foreach (ObjectModel parcela in tmpListParciel)
            {
                tmpNehnutelnost.ZoznamObjektov.Add(parcela);
                parcela.ZoznamObjektov.Add(tmpNehnutelnost);
            }
            _quadTreeNehnutelnost.Insert(checkedGps1.X, checkedGps1.Y, checkedGps2.X, checkedGps2.Y, tmpNehnutelnost);
            _quadTreeJednotne.Insert(checkedGps1.X, checkedGps1.Y, checkedGps2.X, checkedGps2.Y, tmpNehnutelnost);
            _allNehnutelnosti.Add(tmpNehnutelnost);

            HealthNehnutelnosti = Math.Round(_quadTreeNehnutelnost.Health * 100).ToString();
            HealthJednotne = Math.Round(_quadTreeJednotne.Health * 100).ToString();
        }

        private void AddParcela()
        {
            if (_quadTreeParcela is null)
            {
                return;
            }
            var dlg = new AddObject(Core.ObjectType.Parcela);
            dlg.ShowDialog();
            GPS dlgGps1 = new GPS();
            GPS dlgGps2 = new GPS();
            string popis = "";
            int idCislo = 0;
            bool cancel = true;
            if (dlg.DialogResult == true)
            {
                dlgGps1 = new(dlg.x, dlg.xOz, dlg.y, dlg.yOz);
                dlgGps2 = new(dlg.x2, dlg.x2Oz, dlg.y2, dlg.y2Oz);
                popis = dlg.popis;
                idCislo = dlg.IdCislo;
                cancel = false;
            }

            if (cancel)
            {
                return;
            }

            GPS checkedGps1 = new GPS();
            GPS checkedGps2 = new GPS();

            if (!Core.Utils.CheckAndRecalculateGps(dlgGps1, dlgGps2, checkedGps1, checkedGps2))
            {
                MessageBox.Show("Nie je možné pridať parcelu do stromu, zlý vstup", "Chyba vstupu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // kontrolujem či vôbec môžem pridať do stromu
            if (!_quadTreeParcela.QuadTreeCanContain(checkedGps1.X, checkedGps1.Y, checkedGps2.X, checkedGps2.Y))
            {
                MessageBox.Show("Nie je možné pridať nehnuteľnosť do stromu", "Chyba vstupu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            ObjectModel tmpNehnutelnost = new(idCislo, popis, dlgGps1, dlgGps2, Core.ObjectType.Parcela);
            var tmpListParciel = _quadTreeNehnutelnost.FindIntervalOverlapping(checkedGps1.X, checkedGps1.Y, checkedGps2.X, checkedGps2.Y);
            foreach (ObjectModel parcela in tmpListParciel)
            {
                tmpNehnutelnost.ZoznamObjektov.Add(parcela);
                parcela.ZoznamObjektov.Add(tmpNehnutelnost);
            }
            _quadTreeParcela.Insert(checkedGps1.X, checkedGps1.Y, checkedGps2.X, checkedGps2.Y, tmpNehnutelnost);
            _quadTreeJednotne.Insert(checkedGps1.X, checkedGps1.Y, checkedGps2.X, checkedGps2.Y, tmpNehnutelnost);
            _allParcelas.Add(tmpNehnutelnost);

            HealthParcela = Math.Round(_quadTreeParcela.Health * 100).ToString();
            HealthJednotne = Math.Round(_quadTreeJednotne.Health * 100).ToString();
        }

        private void EditObject(ObjectModel objectModel)
        {
            var dlg = new EditObject(objectModel);
            dlg.ShowDialog();
            GPS dlgGpsOriginal1 = new GPS(objectModel.GpsBod1);
            GPS dlgGpsOriginal2 = new GPS(objectModel.GpsBod2);
            int originalID = objectModel.IdObjektu;
            GPS dlgGps1 = new GPS();
            GPS dlgGps2 = new GPS();
            string popis = "";
            int idCislo = 0;
            bool cancel = true;
            if (dlg.DialogResult == true)
            {
                dlgGps1 = new(dlg.x, dlg.xOz, dlg.y, dlg.yOz);
                dlgGps2 = new(dlg.x2, dlg.x2Oz, dlg.y2, dlg.y2Oz);
                popis = dlg.popis;
                idCislo = dlg.IdCislo;
                cancel = false;
            }

            if (cancel)
            {
                return;
            }
            
            //naplníme dáta do modelu
            objectModel.IdObjektu = idCislo;
            objectModel.Popis = popis;
            
            // skontrolujeme či sa zmenili gps súradnice
            if (dlgGpsOriginal1 == dlgGps1 && dlgGpsOriginal2 == dlgGps2)
            {
                return;
            }
            // zmenili sa skontrolujeme či sú nové súradnice ok

            GPS checkedGps1 = new GPS();
            GPS checkedGps2 = new GPS();

            if (!Core.Utils.CheckAndRecalculateGps(dlgGps1, dlgGps2, checkedGps1, checkedGps2))
            {
                MessageBox.Show("Nie je možné zmeniť súradnice, chybný vstup.", "Chyba vstupu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            // prepočítam si dáta pôvodné ktore sa používaju v kúči v stromu
            GPS origanalCheckedGps1 = new GPS();
            GPS origanalCheckedGps2 = new GPS();
            Core.Utils.CheckAndRecalculateGps(objectModel.GpsBod1, objectModel.GpsBod2, origanalCheckedGps1, origanalCheckedGps2);

            // kontrolujem či sa môžu zmeniť súradnice

            if (!_quadTreeJednotne.QuadTreeCanContain(checkedGps1.X, checkedGps1.Y, checkedGps2.X, checkedGps2.Y))
            {
                MessageBox.Show("Nie je možné zmeniť súradnice, zlý rozsah.", "Chyba vstupu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            // musím zmazať zo všetkých parciel záznam na tento objekt
            foreach (ObjectModel parcela in objectModel.ZoznamObjektov)
            {
                parcela.ZoznamObjektov.Remove(objectModel);
            }

            // vymažem sa z oboch stromov
            // _quadTreeJednotne.Delete(dlgGpsOriginal1.X, dlgGpsOriginal1.Y, dlgGpsOriginal2.X, dlgGpsOriginal2.Y,
            //     originalID);
            // if (objectModel.ObjectType == ObjectType.Nehnutelnost)
            // {
            //     _quadTreeNehnutelnost.Delete(dlgGpsOriginal1.X, dlgGpsOriginal1.Y, dlgGpsOriginal2.X, dlgGpsOriginal2.Y,
            //         originalID);
            // }
            // else
            // {
            //     _quadTreeParcela.Delete(dlgGpsOriginal1.X, dlgGpsOriginal1.Y, dlgGpsOriginal2.X, dlgGpsOriginal2.Y,
            //         originalID);
            // }
            objectModel.ZoznamObjektov.Clear();

            // teraz postupujem ako pri vytváraní nového objektu
            // nájdem prekrývajúce sa objekty, seba tam pridám a na záver pridám aj seba do oboch stromov

            List<ObjectModel> tmpListOverlappingData;

            if (objectModel.ObjectType == ObjectType.Nehnutelnost)
            {
                tmpListOverlappingData = _quadTreeParcela.FindIntervalOverlapping(checkedGps1.X, checkedGps1.Y, checkedGps2.X, checkedGps2.Y);
            }
            else
            {
                tmpListOverlappingData = _quadTreeNehnutelnost.FindIntervalOverlapping(checkedGps1.X, checkedGps1.Y, checkedGps2.X, checkedGps2.Y);
            }

            foreach (ObjectModel model in tmpListOverlappingData)
            {
                objectModel.ZoznamObjektov.Add(model);
                model.ZoznamObjektov.Add(objectModel);
            }

            if (objectModel.ObjectType == ObjectType.Nehnutelnost)
            {
                _quadTreeNehnutelnost.Edit(origanalCheckedGps1.X, origanalCheckedGps1.Y, origanalCheckedGps2.X, origanalCheckedGps2.Y,
                    checkedGps1.X, checkedGps1.Y, checkedGps2.X, checkedGps2.Y, objectModel.IdObjektu);
                _allNehnutelnosti = new(_quadTreeNehnutelnost.ToList());
                ListNehnutelnost =
                    new(_allNehnutelnosti.GetRange(0, _allNehnutelnosti.Count > 500 ? 500 : _allNehnutelnosti.Count)); // zobrazíme len prvých 500 (aby UI išlo normálne a nesekalo sa)
            }
            else
            {
                _quadTreeParcela.Edit(origanalCheckedGps1.X, origanalCheckedGps1.Y, origanalCheckedGps2.X, origanalCheckedGps2.Y,
                    checkedGps1.X, checkedGps1.Y, checkedGps2.X, checkedGps2.Y, objectModel.IdObjektu);
                _allParcelas = new(_quadTreeParcela.ToList());
                ListParcela =
                    new(_allParcelas.GetRange(0, _allParcelas.Count > 500 ? 500 : _allParcelas.Count)); // zobrazíme len prvých 500 (aby UI išlo normálne a nesekalo sa)
            }
            _quadTreeJednotne.Edit(origanalCheckedGps1.X, origanalCheckedGps1.Y, origanalCheckedGps2.X, origanalCheckedGps2.Y,
                checkedGps1.X, checkedGps1.Y, checkedGps2.X, checkedGps2.Y, objectModel.IdObjektu);

            // nahrám nové súradnice
            objectModel.GpsBod1 = dlgGps1;
            objectModel.GpsBod2 = dlgGps2;
            
            HealthParcela = Math.Round(_quadTreeParcela.Health * 100).ToString();
            HealthNehnutelnosti = Math.Round(_quadTreeNehnutelnost.Health * 100).ToString();
            HealthJednotne = Math.Round(_quadTreeJednotne.Health * 100).ToString();
            ChangeView(true);
        }

        private void DeleteObject(ObjectModel objectModel)
        {
            // musím zmazať zo všetkých parciel záznam na tento objekt
            foreach (ObjectModel parcela in objectModel.ZoznamObjektov)
            {
                parcela.ZoznamObjektov.Remove(objectModel);
            }

            GPS checkedGps1 = new GPS();
            GPS checkedGps2 = new GPS();

            Core.Utils.CheckAndRecalculateGps(objectModel.GpsBod1, objectModel.GpsBod2, checkedGps1, checkedGps2);

            // vymažem sa z oboch stromov
            if (objectModel.ObjectType == ObjectType.Nehnutelnost)
            {
                _quadTreeNehnutelnost.Delete(checkedGps1.X, checkedGps1.Y, checkedGps2.X, checkedGps2.Y,
                    objectModel.IdObjektu);
            }
            else
            {
                _quadTreeParcela.Delete(checkedGps1.X, checkedGps1.Y, checkedGps2.X, checkedGps2.Y, objectModel.IdObjektu);
            }

            _quadTreeJednotne.Delete(checkedGps1.X, checkedGps1.Y, checkedGps2.X, checkedGps2.Y, objectModel.IdObjektu);
            objectModel.ZoznamObjektov.Clear();

            if (objectModel.ObjectType == ObjectType.Nehnutelnost)
            {
                _allNehnutelnosti = new(_quadTreeNehnutelnost.ToList());
                ListNehnutelnost =
                    new(_allNehnutelnosti.GetRange(0, _allNehnutelnosti.Count > 500 ? 500 : _allNehnutelnosti.Count)); // zobrazíme len prvých 500 (aby UI išlo normálne a nesekalo sa)
            }
            else
            {
                _allParcelas = new(_quadTreeParcela.ToList());
                ListParcela =
                    new(_allParcelas.GetRange(0, _allParcelas.Count > 500 ? 500 : _allParcelas.Count)); // zobrazíme len prvých 500 (aby UI išlo normálne a nesekalo sa)
            }

            ChangeView(true);
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


            var tmp = loader.LoadData(_quadTreeNehnutelnost,
                _quadTreeParcela,
                _quadTreeJednotne,
                ListNehnutelnost,
                ListParcela,
                _allNehnutelnosti,
                _allParcelas);
            _quadTreeNehnutelnost = tmp[0];
            _quadTreeParcela = tmp[1];
            _quadTreeJednotne = tmp[2];

            HealthJednotne = Math.Round(_quadTreeJednotne.Health * 100).ToString();
            HealthNehnutelnosti = Math.Round(_quadTreeNehnutelnost.Health * 100).ToString();
            HealthParcela = Math.Round(_quadTreeParcela.Health * 100).ToString();
        }

        private static double NextDouble(double min, double max, Random rnd)
        {
            return rnd.NextDouble() * (max - min) + min;
        }


        /// <summary>
        /// Zmení zobrazenie split na single a naopak
        /// </summary>
        /// <param name="splitView">Ak je true tak sa zobrazí split, aj je false tak sa zobrazí single</param>
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
