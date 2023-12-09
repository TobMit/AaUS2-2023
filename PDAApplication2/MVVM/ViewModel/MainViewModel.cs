using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using DynamicHashFileStructure.StructureClasses;
using PDAApplication2.Core;
using PDAApplication2.Core.DataManager.FileManager;
using PDAApplication2.MVVM.Model;
using PDAApplication2.MVVM.View;
using Quadtree.StructureClasses;

namespace PDAApplication2.MVVM.ViewModel
{
    class MainViewModel : ObservableObjects
    {
        private QuadTree<int,ObjectModelQuad> _quadTreeNehnutelnost;
        private QuadTree<int, ObjectModelQuad> _quadTreeParcela;

        private string _primaryFileNameParcela = "primariFileParcela.bin";
        private string _preplnovakFileNameParcela = "preplnovakFileNameParcela.bin";
        private DynamicHashFile<int, ObjectModelParcela> _dynamicHashFileParcela;
        private string _primaryFileNameNehnutelnost = "primariFileNehnutelnost.bin";
        private string _preplnovakFileNameNehnutelnost = "preplnovakFileNameNehnutelnost.bin";
        private DynamicHashFile<int, ObjectModelNehnutelnost> _dynamicHashFileNehnutelnost;

        //private List<ObjectModel> _allNehnutelnosti;
        //private List<ObjectModel> _allParcelas;

        public RelayCommand GenerateDataCommand { get; set; }
        public RelayCommand FindBuildingsCommand { get; set; }
        public RelayCommand FindParcelaCommand { get; set; }
        public RelayCommand AddBuildingCommand { get; set; }
        public RelayCommand AddParcelaCommand { get; set; }
        public RelayCommand EditNehnutelnostiCommand { get; set; }
        public RelayCommand EditParcelaCommand { get; set; }
        public RelayCommand DeleteNehnutelnostCommand { get; set; }
        public RelayCommand DeleteParcelaCommand { get; set; }
        public RelayCommand ShowFileNehnutelnostiCommand { get; set; }
        public RelayCommand ShowFileParcelyCommand { get; set; }
        public RelayCommand SaveDataCommand { get; set; }
        public RelayCommand LoadDataCommand { get; set; }
        public RelayCommand ForceOptimisationCommand { get; set; }

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


        private SolidColorBrush _color;

        public SolidColorBrush MenuColor
        {
            get => _color;
            set
            {
                _color = value;
                OnPropertyChanged();
            }
        }


        public MainViewModel()
        {
            InitializeButtons();
            //_allNehnutelnosti = new();
            //_allParcelas = new();
            SplitViewShow = Visibility.Visible;
            SingleViewShow = Visibility.Hidden;
        }

        private void InitializeButtons()
        {
            GenerateDataCommand = new RelayCommand(o => { GenerateData(); });
            FindBuildingsCommand = new RelayCommand(o => { FindBuildings(); });
            FindParcelaCommand = new RelayCommand(o => { FindParcela(); });
            AddBuildingCommand = new RelayCommand(o => { AddBuilding(); });
            AddParcelaCommand = new RelayCommand(o => { AddParcela(); });
            EditNehnutelnostiCommand = new RelayCommand(o =>
            {
                var tmp = (ObjectModel)o;
                EditNehnutelnostObject(tmp);
            });
            EditParcelaCommand = new RelayCommand(o =>
            {
                var tmp = (ObjectModel)o;
                EditParcelyObject(tmp);
            });
            DeleteNehnutelnostCommand = new RelayCommand(o =>
            {
                var tmp = (ObjectModel)o;
                DeleteNehnutelnost(tmp);
            });
            DeleteParcelaCommand = new RelayCommand(o =>
            {
                var tmp = (ObjectModel)o;
                DeleteParcela(tmp);
            });
            ShowFileNehnutelnostiCommand = new RelayCommand(o => { ShowFileNehnutelnosti(); });
            ShowFileParcelyCommand = new RelayCommand(o => { ShowFileParcely(); });
            SaveDataCommand = new RelayCommand(o => { SaveData(); });
            LoadDataCommand = new RelayCommand(o => { LoadData(); });
            ForceOptimisationCommand = new RelayCommand(o => { ForceOptimisation(); });
        }

        private void GenerateData()
        {
            MenuColor = new(Color.FromRgb(129, 199, 131));
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
                MenuColor = new(Color.FromRgb(240, 240, 240));
                return;
            }

            _quadTreeNehnutelnost = new QuadTree<int, ObjectModelQuad>(gps1.X, gps1.Y, sirka, dlzka);
            _quadTreeParcela = new QuadTree<int, ObjectModelQuad>(gps1.X, gps1.Y, sirka, dlzka);

            if (_dynamicHashFileParcela is not null)
            {
                _dynamicHashFileParcela.CloseFile();
            }

            if (_dynamicHashFileNehnutelnost is not null)
            {
                _dynamicHashFileNehnutelnost.CloseFile();
            }


            // musíme zmazať predchadzajúce súbori aby sa zabránilo vpisovaniu do existujúcich súborov
            File.Delete(_primaryFileNameParcela);
            File.Delete(_preplnovakFileNameParcela);
            _dynamicHashFileParcela =
                new DynamicHashFile<int, ObjectModelParcela>(_primaryFileNameParcela, _preplnovakFileNameParcela);
            
            File.Delete(_primaryFileNameNehnutelnost);
            File.Delete(_preplnovakFileNameNehnutelnost);
            _dynamicHashFileNehnutelnost =
                new DynamicHashFile<int, ObjectModelNehnutelnost>(_primaryFileNameNehnutelnost,
                    _preplnovakFileNameNehnutelnost);

            ListParcela = new ();
            ListNehnutelnost = new ();
            //_allNehnutelnosti = new(pocetNehnutelnosti);
            //_allParcelas = new(pocetParciel);

            Constants.IdObjektu = 0;

            Core.DataManager.DataGenerator.GenerateData(_quadTreeNehnutelnost,
                _quadTreeParcela,
                _dynamicHashFileNehnutelnost,
                _dynamicHashFileParcela,
                gps1,
                new(gps1.X + sirka, gps1.Y + dlzka),
                pocetNehnutelnosti,
                pocetParciel);
            
            MenuColor = new(Color.FromRgb(240, 240, 240));
            
            //_dynamicHashFileNehnutelnost.PrintFile();
            //_dynamicHashFileParcela.PrintFile();
            
            HealthNehnutelnosti = Math.Round(_quadTreeNehnutelnost.Health*100).ToString();
            HealthParcela = Math.Round(_quadTreeParcela.Health * 100).ToString();

        }

        private void FindBuildings()
        {
            
            if (_quadTreeNehnutelnost is null || _dynamicHashFileNehnutelnost is null)
            {
                return;
            }
            var dlg = new FindObject("Vyhľadanie budovy");
            dlg.ShowDialog();
            int id = 0;
            bool cancel = true;
            if (dlg.DialogResult == true)
            {
                id = dlg.ID;
                cancel = false;
            }

            if (cancel)
            {
                return;
            }

            ObjectModelNehnutelnost? mainNehnutelnost = null;
            ChangeView(true);
            try
            {
                mainNehnutelnost = _dynamicHashFileNehnutelnost.Find(id);
            }
            catch (Exception e)
            {
                MessageBox.Show("Nesprávne ID nehnuteľnosti", "Nesprávne ID", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (mainNehnutelnost is null)
            {
                MessageBox.Show("Nesprávne ID nehnuteľnosti", "Nesprávne ID", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            foreach (int tmpID in mainNehnutelnost.ZoznamObjektov)
            {
                var tmp = _dynamicHashFileParcela.Find(tmpID);
                if (tmp is not null)
                {
                    mainNehnutelnost.ZoznamObjektovGUI.Add(tmp);
                }
            }

            ListNehnutelnost = new ObservableCollection<ObjectModel>();
            ListNehnutelnost.Add(mainNehnutelnost);
        }

        private void FindParcela()
        {
            if (_quadTreeParcela is null || _dynamicHashFileParcela is null)
            {
                return;
            }
            var dlg = new FindObject("Vyhľadanie parcely");
            dlg.ShowDialog();
            int id = 0;
            bool cancel = true;
            if (dlg.DialogResult == true)
            {
                id = dlg.ID;
                cancel = false;
            }
            if (cancel)
            {
                return;
            }

            ObjectModelParcela? mainParcela = null;
            ChangeView(true);
            try
            {
                mainParcela = _dynamicHashFileParcela.Find(id);
            }
            catch (Exception e)
            {
                MessageBox.Show("Nesprávne ID parcely", "Nesprávne ID", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (mainParcela is null)
            {
                MessageBox.Show("Nesprávne ID parcely", "Nesprávne ID", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            foreach (int tmpID in mainParcela.ZoznamObjektov)
            {
                var tmp = _dynamicHashFileNehnutelnost.Find(tmpID);
                if (tmp is not null)
                {
                    mainParcela.ZoznamObjektovGUI.Add(tmp);
                }
            }

            ListParcela = new ObservableCollection<ObjectModel>();
            ListParcela.Add(mainParcela);
        }


        private void AddBuilding()
        {
            if (_quadTreeNehnutelnost is null || _dynamicHashFileNehnutelnost is null )
            {
                return;
            }
            var dlg = new AddObject(Core.ObjectType.Nehnutelnost);
            dlg.ShowDialog();
            GPS dlgGps1 = new GPS();
            GPS dlgGps2 = new GPS();
            string popis = "";
            bool cancel = true;
            if (dlg.DialogResult == true)
            {
                dlgGps1 = new(dlg.x, dlg.xOz, dlg.y, dlg.yOz);
                dlgGps2 = new(dlg.x2, dlg.x2Oz, dlg.y2, dlg.y2Oz);
                popis = dlg.popis;
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
            // skontrolovač či môžeme vkladať
            // pridať tam tú nehnuteľnosť
            // pridať nehnuteľnosť do zoznamu všetkých nehnuteľností a všetky parcely do nehnuteľnosti
            // pridať nehnuteľnosť do quad tree a do dynamic hash file
            ObjectModelNehnutelnost tmpNehnutelnost = new(Constants.IdObjektu, popis, dlgGps1, dlgGps2);
            ObjectModelQuad tmpQuad = new(Constants.IdObjektu, dlgGps1, dlgGps2);
            Constants.IdObjektu++;
            var tmpListParciel = _quadTreeParcela.FindIntervalOverlapping(checkedGps1.X, checkedGps1.Y, checkedGps2.X, checkedGps2.Y);

            bool isOk = tmpListParciel.Count <= Constants.MAX_COUNT_PARCELS_IN_NEHNUTELNOST;
            if (isOk)
            {
                // musím si pred tým skontrolovať či mám dostatok miesta v parcelách
                foreach (ObjectModelQuad parcela in tmpListParciel)
                {
                    var tmpParcela = (ObjectModelParcela)_dynamicHashFileParcela.Find(parcela.IdObjektu);
                    if (tmpParcela.ZoznamObjektov.Count >= Constants.MAX_COUNT_NEHNUTELNOST_IN_PARCEL)
                    {
                        isOk = false;
                        break;
                    }
                }
            }

            if (isOk)
            {
                foreach (ObjectModelQuad parcela in tmpListParciel)
                {
                    var tmpParcela = (ObjectModelParcela)_dynamicHashFileParcela.Remove(parcela.IdObjektu);
                    tmpNehnutelnost.ZoznamObjektov.Add(parcela.IdObjektu);
                    tmpParcela.ZoznamObjektov.Add(tmpNehnutelnost.IdObjektu);
                    _dynamicHashFileParcela.Insert(tmpParcela.GetKey(), tmpParcela);
                }
                _quadTreeNehnutelnost.Insert(checkedGps1.X, checkedGps1.Y, checkedGps2.X, checkedGps2.Y, tmpQuad);
                _dynamicHashFileNehnutelnost.Insert(tmpNehnutelnost.GetKey(), tmpNehnutelnost);
            }
            else
            {
                MessageBox.Show("Presiahnutý počet zaznamov", "Chyba vstupu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            //_allNehnutelnosti.Add(tmpNehnutelnost);

            HealthNehnutelnosti = Math.Round(_quadTreeNehnutelnost.Health * 100).ToString();
        }

        private void AddParcela()
        {
            if (_quadTreeParcela is null || _dynamicHashFileParcela is null)
            {
                return;
            }
            var dlg = new AddObject(Core.ObjectType.Parcela);
            dlg.ShowDialog();
            GPS dlgGps1 = new GPS();
            GPS dlgGps2 = new GPS();
            string popis = "";
            bool cancel = true;
            if (dlg.DialogResult == true)
            {
                dlgGps1 = new(dlg.x, dlg.xOz, dlg.y, dlg.yOz);
                dlgGps2 = new(dlg.x2, dlg.x2Oz, dlg.y2, dlg.y2Oz);
                popis = dlg.popis;
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
            ObjectModelParcela tmpParcela = new(Constants.IdObjektu, popis, dlgGps1, dlgGps2);
            ObjectModelQuad tmpQuad = new(Constants.IdObjektu, dlgGps1, dlgGps2);
            Constants.IdObjektu++;
            var tmpListNehnutelnosti = _quadTreeNehnutelnost.FindIntervalOverlapping(checkedGps1.X, checkedGps1.Y, checkedGps2.X, checkedGps2.Y);

            bool isOk = tmpListNehnutelnosti.Count <= Constants.MAX_COUNT_NEHNUTELNOST_IN_PARCEL;
            if (isOk)
            {
                // musím si pred tým skontrolovať či mám dostatok miesta v nehnutelnostiach
                foreach (ObjectModelQuad nehnutelnost in tmpListNehnutelnosti)
                {
                    var tmpNehnutelnost = (ObjectModelNehnutelnost)_dynamicHashFileNehnutelnost.Find(nehnutelnost.IdObjektu);
                    if (tmpNehnutelnost.ZoznamObjektov.Count >= Constants.MAX_COUNT_PARCELS_IN_NEHNUTELNOST)
                    {
                        isOk = false;
                        break;
                    }
                }
            }

            if (isOk)
            {
                foreach (ObjectModelQuad nehnutelnost in tmpListNehnutelnosti)
                {
                    var tmpNehnutelnost = (ObjectModelNehnutelnost)_dynamicHashFileNehnutelnost.Remove(nehnutelnost.IdObjektu);
                    tmpNehnutelnost.ZoznamObjektov.Add(tmpParcela.IdObjektu);
                    tmpParcela.ZoznamObjektov.Add(tmpNehnutelnost.IdObjektu);
                    _dynamicHashFileNehnutelnost.Insert(tmpNehnutelnost.GetKey(), tmpNehnutelnost);
                }
                _quadTreeParcela.Insert(checkedGps1.X, checkedGps1.Y, checkedGps2.X, checkedGps2.Y, tmpQuad);
                _dynamicHashFileParcela.Insert(tmpParcela.GetKey(), tmpParcela);
            }
            else
            {
                MessageBox.Show("Presiahnutý počet zaznamov", "Chyba vstupu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            HealthParcela = Math.Round(_quadTreeParcela.Health * 100).ToString();
            
        }

        private void EditNehnutelnostObject(ObjectModel objectModel)
        {
            var dlg = new EditObject(objectModel);
            dlg.ShowDialog();
            GPS dlgGpsOriginal1 = new GPS(objectModel.GpsBod1);
            GPS dlgGpsOriginal2 = new GPS(objectModel.GpsBod2);
            int originalID = objectModel.IdObjektu;
            GPS dlgGps1 = new GPS();
            GPS dlgGps2 = new GPS();
            string popis = "";
            bool cancel = true;
            if (dlg.DialogResult == true)
            {
                dlgGps1 = new(dlg.x, dlg.xOz, dlg.y, dlg.yOz);
                dlgGps2 = new(dlg.x2, dlg.x2Oz, dlg.y2, dlg.y2Oz);
                popis = dlg.popis;
                cancel = false;
            }

            if (cancel)
            {
                return;
            }
            
            //naplníme dáta do modelu
            objectModel.Popis = popis;
            
            // skontrolujeme či sa zmenili gps súradnice
            if (dlgGpsOriginal1 == dlgGps1 && dlgGpsOriginal2 == dlgGps2)
            {
                var toUpdateNehnutelnost = _dynamicHashFileNehnutelnost.Remove(objectModel.IdObjektu);
                toUpdateNehnutelnost.Popis = popis;
                _dynamicHashFileNehnutelnost.Insert(toUpdateNehnutelnost.GetKey(), toUpdateNehnutelnost);
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

            if (!_quadTreeNehnutelnost.QuadTreeCanContain(checkedGps1.X, checkedGps1.Y, checkedGps2.X, checkedGps2.Y))
            {
                MessageBox.Show("Nie je možné zmeniť súradnice, zlý rozsah.", "Chyba vstupu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // postupujem podobne ako pri vkladaní
            var tmpListParciel = _quadTreeParcela.FindIntervalOverlapping(checkedGps1.X, checkedGps1.Y, checkedGps2.X, checkedGps2.Y);

            bool isOk = tmpListParciel.Count <= Constants.MAX_COUNT_PARCELS_IN_NEHNUTELNOST;
            if (isOk)
            {
                // musím si pred tým skontrolovať či mám dostatok miesta v parcelách
                foreach (ObjectModelQuad parcela in tmpListParciel)
                {
                    var tmpParcela = (ObjectModelParcela)_dynamicHashFileParcela.Find(parcela.IdObjektu);
                    if (tmpParcela.ZoznamObjektov.Count >= Constants.MAX_COUNT_NEHNUTELNOST_IN_PARCEL)
                    {
                        isOk = false;
                        break;
                    }
                }
            }

            if (isOk)
            {
                // musím zmazať zo všetkých parciel záznam na tento objekt

                _dynamicHashFileNehnutelnost.Remove(objectModel.IdObjektu);

                foreach (int tmpID in objectModel.ZoznamObjektov)
                {
                    var tmpParcela = _dynamicHashFileParcela.Remove(tmpID);
                    tmpParcela.ZoznamObjektov.Remove(objectModel.IdObjektu);
                    _dynamicHashFileParcela.Insert(tmpParcela.IdObjektu, tmpParcela);
                }

                // potrebujem si to aj v stome upraviť
                var quadTmpNehnutelnost = _quadTreeNehnutelnost.Find(origanalCheckedGps1.X, origanalCheckedGps1.Y, origanalCheckedGps2.X, origanalCheckedGps2.Y);
                quadTmpNehnutelnost[0].GpsBod1 = dlgGps1;
                quadTmpNehnutelnost[0].GpsBod2 = dlgGps2;

                _quadTreeNehnutelnost.Edit(origanalCheckedGps1.X, origanalCheckedGps1.Y, origanalCheckedGps2.X, origanalCheckedGps2.Y,
                    checkedGps1.X, checkedGps1.Y, checkedGps2.X, checkedGps2.Y, objectModel.IdObjektu);

                ObjectModelNehnutelnost updatedNehnutelnost = new(originalID, popis, dlgGps1, dlgGps2);
                tmpListParciel = _quadTreeParcela.FindIntervalOverlapping(checkedGps1.X, checkedGps1.Y, checkedGps2.X, checkedGps2.Y);
                foreach (ObjectModelQuad parcela in tmpListParciel)
                {
                    var tmpParcela = (ObjectModelParcela)_dynamicHashFileParcela.Remove(parcela.IdObjektu);
                    updatedNehnutelnost.ZoznamObjektov.Add(parcela.IdObjektu);
                    tmpParcela.ZoznamObjektov.Add(updatedNehnutelnost.IdObjektu);
                    _dynamicHashFileParcela.Insert(tmpParcela.GetKey(), tmpParcela);
                }
                _dynamicHashFileNehnutelnost.Insert(updatedNehnutelnost.GetKey(), updatedNehnutelnost);
            }
            else
            {
                MessageBox.Show("Presiahnutý počet zaznamov", "Chyba vstupu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            HealthNehnutelnosti = Math.Round(_quadTreeNehnutelnost.Health * 100).ToString();
            ChangeView(true);
            
        }

        private void EditParcelyObject(ObjectModel objectModel)
        {
            var dlg = new EditObject(objectModel);
            dlg.ShowDialog();
            GPS dlgGpsOriginal1 = new GPS(objectModel.GpsBod1);
            GPS dlgGpsOriginal2 = new GPS(objectModel.GpsBod2);
            int originalID = objectModel.IdObjektu;
            GPS dlgGps1 = new GPS();
            GPS dlgGps2 = new GPS();
            string popis = "";
            bool cancel = true;
            if (dlg.DialogResult == true)
            {
                dlgGps1 = new(dlg.x, dlg.xOz, dlg.y, dlg.yOz);
                dlgGps2 = new(dlg.x2, dlg.x2Oz, dlg.y2, dlg.y2Oz);
                popis = dlg.popis;
                cancel = false;
            }

            if (cancel)
            {
                return;
            }

            //naplníme dáta do modelu
            objectModel.Popis = popis;

            // skontrolujeme či sa zmenili gps súradnice
            if (dlgGpsOriginal1 == dlgGps1 && dlgGpsOriginal2 == dlgGps2)
            {
                var toUpdateParcela = _dynamicHashFileParcela.Remove(objectModel.IdObjektu);
                toUpdateParcela.Popis = popis;
                _dynamicHashFileParcela.Insert(toUpdateParcela.GetKey(), toUpdateParcela);
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

            if (!_quadTreeParcela.QuadTreeCanContain(checkedGps1.X, checkedGps1.Y, checkedGps2.X, checkedGps2.Y))
            {
                MessageBox.Show("Nie je možné zmeniť súradnice, zlý rozsah.", "Chyba vstupu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // postupujem podobne ako pri vkladaní
            var tmpListNehnutelnosti = _quadTreeNehnutelnost.FindIntervalOverlapping(checkedGps1.X, checkedGps1.Y, checkedGps2.X, checkedGps2.Y);

            bool isOk = tmpListNehnutelnosti.Count <= Constants.MAX_COUNT_NEHNUTELNOST_IN_PARCEL;
            if (isOk)
            {
                // musím si pred tým skontrolovať či mám dostatok miesta v parcelách
                foreach (ObjectModelQuad nehnutelnost in tmpListNehnutelnosti)
                {
                    var tmpNehnutelnost = (ObjectModelNehnutelnost)_dynamicHashFileNehnutelnost.Find(nehnutelnost.IdObjektu);
                    if (tmpNehnutelnost.ZoznamObjektov.Count >= Constants.MAX_COUNT_PARCELS_IN_NEHNUTELNOST)
                    {
                        isOk = false;
                        break;
                    }
                }
            }

            if (isOk)
            {
                // musím zmazať zo všetkých parciel záznam na tento objekt

                _dynamicHashFileParcela.Remove(objectModel.IdObjektu);

                foreach (int tmpID in objectModel.ZoznamObjektov)
                {
                    var tmpNehnutelnost = _dynamicHashFileNehnutelnost.Remove(tmpID);
                    tmpNehnutelnost.ZoznamObjektov.Remove(objectModel.IdObjektu);
                    _dynamicHashFileNehnutelnost.Insert(tmpNehnutelnost.IdObjektu, tmpNehnutelnost);
                }

                // potrebujem si to aj v stome upraviť
                var quadTmpParcla = _quadTreeParcela.Find(origanalCheckedGps1.X, origanalCheckedGps1.Y, origanalCheckedGps2.X, origanalCheckedGps2.Y);
                quadTmpParcla[0].GpsBod1 = dlgGps1;
                quadTmpParcla[0].GpsBod2 = dlgGps2;

                _quadTreeParcela.Edit(origanalCheckedGps1.X, origanalCheckedGps1.Y, origanalCheckedGps2.X, origanalCheckedGps2.Y,
                    checkedGps1.X, checkedGps1.Y, checkedGps2.X, checkedGps2.Y, objectModel.IdObjektu);

                ObjectModelParcela updatedParcela = new(originalID, popis, dlgGps1, dlgGps2);
                tmpListNehnutelnosti = _quadTreeNehnutelnost.FindIntervalOverlapping(checkedGps1.X, checkedGps1.Y, checkedGps2.X, checkedGps2.Y);
                foreach (ObjectModelQuad nehnutelnost in tmpListNehnutelnosti)
                {
                    var tmpNehnutelnost = (ObjectModelNehnutelnost)_dynamicHashFileNehnutelnost.Remove(nehnutelnost.IdObjektu);
                    updatedParcela.ZoznamObjektov.Add(nehnutelnost.IdObjektu);
                    tmpNehnutelnost.ZoznamObjektov.Add(updatedParcela.IdObjektu);
                    _dynamicHashFileNehnutelnost.Insert(tmpNehnutelnost.GetKey(), tmpNehnutelnost);
                }
                _dynamicHashFileParcela.Insert(updatedParcela.GetKey(), updatedParcela);
            }
            else
            {
                MessageBox.Show("Presiahnutý počet zaznamov", "Chyba vstupu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            HealthParcela = Math.Round(_quadTreeParcela.Health * 100).ToString();
            ChangeView(true);

        }

        private void DeleteNehnutelnost(ObjectModel objectModel)
        {
            
            // musím zmazať zo všetkých parciel záznam na tento objekt

            var tmpObject = _dynamicHashFileNehnutelnost.Remove(objectModel.IdObjektu);

            foreach (int tmpID in objectModel.ZoznamObjektov)
            {
                var tmpParcela = _dynamicHashFileParcela.Remove(tmpID);
                tmpParcela.ZoznamObjektov.Remove(objectModel.IdObjektu);
                _dynamicHashFileParcela.Insert(tmpParcela.IdObjektu, tmpParcela);
            }

            GPS checkedGps1 = new GPS();
            GPS checkedGps2 = new GPS();

            Core.Utils.CheckAndRecalculateGps(objectModel.GpsBod1, objectModel.GpsBod2, checkedGps1, checkedGps2);
            _quadTreeNehnutelnost.Delete(checkedGps1.X, checkedGps1.Y, checkedGps2.X, checkedGps2.Y,
                objectModel.IdObjektu);
            ListNehnutelnost = new();
            ChangeView(true);

            HealthNehnutelnosti = Math.Round(_quadTreeNehnutelnost.Health * 100).ToString();
        }

        private void DeleteParcela(ObjectModel objectModel)
        {

            // musím zmazať zo všetkých parciel záznam na tento objekt

            var tmpObject = _dynamicHashFileParcela.Remove(objectModel.IdObjektu);

            foreach (int tmpID in objectModel.ZoznamObjektov)
            {
                var tmpParcela = _dynamicHashFileNehnutelnost.Remove(tmpID);
                tmpParcela.ZoznamObjektov.Remove(objectModel.IdObjektu);
                _dynamicHashFileNehnutelnost.Insert(tmpParcela.IdObjektu, tmpParcela);
            }

            GPS checkedGps1 = new GPS();
            GPS checkedGps2 = new GPS();

            Core.Utils.CheckAndRecalculateGps(objectModel.GpsBod1, objectModel.GpsBod2, checkedGps1, checkedGps2);
            _quadTreeParcela.Delete(checkedGps1.X, checkedGps1.Y, checkedGps2.X, checkedGps2.Y,
                objectModel.IdObjektu);
            ListParcela = new();
            ChangeView(true);
            HealthParcela = Math.Round(_quadTreeParcela.Health * 100).ToString();
        }


        private void SaveData()
        {
            if (_quadTreeNehnutelnost is null || _quadTreeParcela is null || _dynamicHashFileNehnutelnost is null || _dynamicHashFileParcela is null)
            {
                return;
            }

            _dynamicHashFileNehnutelnost.Save();
            _dynamicHashFileParcela.Save();


            DataSaver<ObjectModelQuad> saverNehnutelnosti = new();
            saverNehnutelnosti.AddLine(_quadTreeNehnutelnost.OriginalPointDownLeft.X + ";" + _quadTreeNehnutelnost.OriginalPointDownLeft.Y + ";" + _quadTreeNehnutelnost.OriginalPointUpRight.X + ";" + _quadTreeNehnutelnost.OriginalPointUpRight.Y + "\n");
            saverNehnutelnosti.PrepareForSave(_quadTreeNehnutelnost.ToList());
            saverNehnutelnosti.SaveData("quadDataNehnutelnosti.csv");

            DataSaver<ObjectModelQuad> saverParcely = new();
            saverParcely.AddLine(_quadTreeParcela.OriginalPointDownLeft.X + ";" + _quadTreeParcela.OriginalPointDownLeft.Y + ";" + _quadTreeParcela.OriginalPointUpRight.X + ";" + _quadTreeParcela.OriginalPointUpRight.Y + "\n");
            saverParcely.PrepareForSave(_quadTreeParcela.ToList());
            saverParcely.SaveData("quadDataParcely.csv");

        }

        private void LoadData()
        {
            if (_dynamicHashFileNehnutelnost is not null && _dynamicHashFileParcela is not null)
            {
                _dynamicHashFileNehnutelnost.CloseFile();
                _dynamicHashFileParcela.CloseFile();
            }

            _dynamicHashFileNehnutelnost = new(_primaryFileNameNehnutelnost, _preplnovakFileNameNehnutelnost);
            _dynamicHashFileParcela = new(_primaryFileNameParcela, _preplnovakFileNameParcela);

            DataLoader loader = new();
            var nehnutelnosti = loader.LoadData("quadDataNehnutelnosti.csv");
            var parcely = loader.LoadData("quadDataParcely.csv");
            if (nehnutelnosti is not null && parcely is not null)
            {
                _quadTreeNehnutelnost = nehnutelnosti;
                _quadTreeParcela = parcely;
                _dynamicHashFileNehnutelnost.Load();
                _dynamicHashFileParcela.Load();
            }

            ListParcela = new();
            ListNehnutelnost = new();

            HealthNehnutelnosti = Math.Round(_quadTreeNehnutelnost.Health * 100).ToString();
            HealthParcela = Math.Round(_quadTreeParcela.Health * 100).ToString();
        }

        private void ForceOptimisation()
        {
            if (_quadTreeNehnutelnost is null || _quadTreeParcela is null)
            {
                return;
            }
            _quadTreeNehnutelnost.Optimalise(true);
            _quadTreeParcela.Optimalise(true);
        }

        private void ShowFileNehnutelnosti()
        {
            if (_dynamicHashFileNehnutelnost is not null)
            {
                var dlg = new SequeFilePrint("Nehnutelnosti", _dynamicHashFileNehnutelnost.GetPrimaryFileSequece(),
                    _dynamicHashFileNehnutelnost.GetPreplnovaciFileSequece());
                dlg.Show();
            }
        }

        private void ShowFileParcely()
        {
            if (_dynamicHashFileParcela is not null)
            {
                var dlg = new SequeFilePrint("Nehnutelnosti", _dynamicHashFileParcela.GetPrimaryFileSequece(),
                    _dynamicHashFileParcela.GetPreplnovaciFileSequece());
                dlg.Show();
            }
        }


        /// <summary>
        /// Zmení zobrazenie split na single a naopak, ak true tak split
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
