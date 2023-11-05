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

        private string _healthNehnutelnosti;

        public string HealthNehnutelnosti
        {
            get
            {
                return "Zdravie stromu nehnutelnsti: " + _healthNehnutelnosti + "%";
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
            ListNehnutelnost = new ObservableCollection<ObjectModel>(_quadTreeNehnutelnost.FindOverlapingData(gps1.X, gps1.Y, gps2.X, gps2.Y));
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
            ListParcela = new ObservableCollection<ObjectModel>(_quadTreeParcela.FindOverlapingData(gps1.X, gps1.Y, gps2.X, gps2.Y));
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

            GPS checketGps1 = new GPS();
            GPS checketGps2 = new GPS();

            if (!Core.Utils.CheckAndRecalculateGps(dlgGps1, dlgGps2, checketGps1, checketGps2))
            {
                MessageBox.Show("Zle zadaný vstup!", "Chyba vstupu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ListParcela = new ObservableCollection<ObjectModel>(_quadTreeJednotne.FindOverlapingData(checketGps1.X, checketGps1.Y, checketGps2.X, checketGps2.Y));
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

            GPS checketGps1 = new GPS();
            GPS checketGps2 = new GPS();

            if (!Core.Utils.CheckAndRecalculateGps(dlgGps1, dlgGps2, checketGps1, checketGps2))
            {
                MessageBox.Show("Nie je možné pridať nehnuteľnosť do stromu, zlý vstup", "Chyba vstupu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // kontrolujem či vôbec môžem pridať do stromu
            if (!_quadTreeNehnutelnost.QuadTreeCanContain(checketGps1.X, checketGps1.Y, checketGps2.X, checketGps2.Y))
            {
                MessageBox.Show("Nie je možné pridať nehnuteľnosť do stromu", "Chyba vstupu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            // musím nájsť všetky parcely, ktoré obsahujú túto nehnuteľnosť
            // pridať tam tú nehnuteľnosť
            // pridať nehnuteľnosť do zoznamu všetkých nehnutelností a všetky parcely do nehnuteľnosti
            // pridať nehnuteľnosť do quad tree
            ObjectModel tmpNehnutelnost = new(idCislo, popis, dlgGps1, dlgGps2, Core.ObjectType.Nehnutelnost);
            var tmpListParciel = _quadTreeParcela.FindOverlapingData(checketGps1.X, checketGps1.Y, checketGps2.X, checketGps2.Y);
            foreach (ObjectModel parcela in tmpListParciel)
            {
                tmpNehnutelnost.ZoznamObjektov.Add(parcela);
                parcela.ZoznamObjektov.Add(tmpNehnutelnost);
            }
            _quadTreeNehnutelnost.Insert(checketGps1.X, checketGps1.Y, checketGps2.X, checketGps2.Y, tmpNehnutelnost);
            _quadTreeJednotne.Insert(checketGps1.X, checketGps1.Y, checketGps2.X, checketGps2.Y, tmpNehnutelnost);
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

            GPS checketGps1 = new GPS();
            GPS checketGps2 = new GPS();

            if (!Core.Utils.CheckAndRecalculateGps(dlgGps1, dlgGps2, checketGps1, checketGps2))
            {
                MessageBox.Show("Nie je možné pridať parcelu do stromu, zlý vstup", "Chyba vstupu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // kontrolujem či vôbec môžem pridať do stromu
            if (!_quadTreeParcela.QuadTreeCanContain(checketGps1.X, checketGps1.Y, checketGps2.X, checketGps2.Y))
            {
                MessageBox.Show("Nie je možné pridať nehnuteľnosť do stromu", "Chyba vstupu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            ObjectModel tmpNehnutelnost = new(idCislo, popis, dlgGps1, dlgGps2, Core.ObjectType.Parcela);
            var tmpListParciel = _quadTreeNehnutelnost.FindOverlapingData(checketGps1.X, checketGps1.Y, checketGps2.X, checketGps2.Y);
            foreach (ObjectModel parcela in tmpListParciel)
            {
                tmpNehnutelnost.ZoznamObjektov.Add(parcela);
                parcela.ZoznamObjektov.Add(tmpNehnutelnost);
            }
            _quadTreeParcela.Insert(checketGps1.X, checketGps1.Y, checketGps2.X, checketGps2.Y, tmpNehnutelnost);
            _quadTreeJednotne.Insert(checketGps1.X, checketGps1.Y, checketGps2.X, checketGps2.Y, tmpNehnutelnost);
            _allParcelas.Add(tmpNehnutelnost);

            HealthParcela = Math.Round(_quadTreeParcela.Health * 100).ToString();
            HealthJednotne = Math.Round(_quadTreeJednotne.Health * 100).ToString();
        }

        private void EditObject(ObjectModel objectModel)
        {
            var dlg = new EditObject(objectModel);
            dlg.ShowDialog();
            GPS dlgGpsOfiginal1 = new GPS(objectModel.GpsBod1);
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
            if (dlgGpsOfiginal1 == dlgGps1 && dlgGpsOriginal2 == dlgGps2)
            {
                return;
            }
            // zmenili sa skontrolujeme či sú nové súradnice ok

            GPS checketGps1 = new GPS();
            GPS checketGps2 = new GPS();

            if (!Core.Utils.CheckAndRecalculateGps(dlgGps1, dlgGps2, checketGps1, checketGps2))
            {
                MessageBox.Show("Nie je možné zmeniť súradnice, chybný vstup.", "Chyba vstupu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // kontrolujem či sa môžu zmeniť súradnice

            if (!_quadTreeJednotne.QuadTreeCanContain(checketGps1.X, checketGps1.Y, checketGps2.X, checketGps2.Y))
            {
                MessageBox.Show("Nie je možné zmeniť súradnice, zlý rozsah.", "Chyba vstupu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            objectModel.GpsBod1 = checketGps1;
            objectModel.GpsBod2 = checketGps2;

            // musím zmazať zo všetkých parciel záznam na tento objekt
            foreach (ObjectModel parcela in objectModel.ZoznamObjektov)
            {
                parcela.ZoznamObjektov.Remove(objectModel);
            }

            // vymažem sa z oboch stromov
            _quadTreeJednotne.Delete(dlgGpsOfiginal1.X, dlgGpsOfiginal1.Y, dlgGpsOriginal2.X, dlgGpsOriginal2.Y,
                originalID);
            if (objectModel.ObjectType == ObjectType.Nehnutelnost)
            {
                _quadTreeNehnutelnost.Delete(dlgGpsOfiginal1.X, dlgGpsOfiginal1.Y, dlgGpsOriginal2.X, dlgGpsOriginal2.Y,
                    originalID);
            }
            else
            {
                _quadTreeParcela.Delete(dlgGpsOfiginal1.X, dlgGpsOfiginal1.Y, dlgGpsOriginal2.X, dlgGpsOriginal2.Y,
                    originalID);
            }
            objectModel.ZoznamObjektov.Clear();

            // teraz postupujem ako pri vytváraní nového objektu
            // nájdem overlaping objekty, seba tam pridám a na záver pridám aj seba do oboch stromov

            List<ObjectModel> tmpListOverlapingData;

            if (objectModel.ObjectType == ObjectType.Nehnutelnost)
            {
                tmpListOverlapingData = _quadTreeParcela.FindOverlapingData(checketGps1.X, checketGps1.Y, checketGps2.X, checketGps2.Y);
            }
            else
            {
                tmpListOverlapingData = _quadTreeNehnutelnost.FindOverlapingData(checketGps1.X, checketGps1.Y, checketGps2.X, checketGps2.Y);
            }

            foreach (ObjectModel model in tmpListOverlapingData)
            {
                objectModel.ZoznamObjektov.Add(model);
                model.ZoznamObjektov.Add(objectModel);
            }

            if (objectModel.ObjectType == ObjectType.Nehnutelnost)
            {
                _quadTreeNehnutelnost.Insert(checketGps1.X, checketGps1.Y, checketGps2.X, checketGps2.Y, objectModel);
                _allNehnutelnosti = new(_quadTreeNehnutelnost.ToList());
                ListNehnutelnost =
                    new(_allNehnutelnosti.GetRange(0, _allNehnutelnosti.Count > 500 ? 500 : _allNehnutelnosti.Count)); // zobrazíme len prvých 500 (aby UI išlo normálne a nesekalo sa)
            }
            else
            {
                _quadTreeParcela.Insert(checketGps1.X, checketGps1.Y, checketGps2.X, checketGps2.Y, objectModel);
                _allParcelas = new(_quadTreeParcela.ToList());
                ListParcela =
                    new(_allParcelas.GetRange(0, _allParcelas.Count > 500 ? 500 : _allParcelas.Count)); // zobrazíme len prvých 500 (aby UI išlo normálne a nesekalo sa)
            }
            _quadTreeJednotne.Insert(checketGps1.X, checketGps1.Y, checketGps2.X, checketGps2.Y, objectModel);

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

            GPS checketGps1 = new GPS();
            GPS checketGps2 = new GPS();

            Core.Utils.CheckAndRecalculateGps(objectModel.GpsBod1, objectModel.GpsBod2, checketGps1, checketGps2);

            // vymažem sa z oboch stromov
            if (objectModel.ObjectType == ObjectType.Nehnutelnost)
            {
                _quadTreeNehnutelnost.Delete(checketGps1.X, checketGps1.Y, checketGps2.X, checketGps2.Y,
                    objectModel.IdObjektu);
            }
            else
            {
                _quadTreeParcela.Delete(checketGps1.X, checketGps1.Y, checketGps2.X, checketGps2.Y, objectModel.IdObjektu);
            }

            _quadTreeJednotne.Delete(checketGps1.X, checketGps1.Y, checketGps2.X, checketGps2.Y, objectModel.IdObjektu);
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
        /// Zmení zobraznie split na single a naopak
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
