using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PDAAplication.Core;
using PDAAplication.MVVM.Model;
using PDAAplication.MVVM.View;
using Quadtree.StructureClasses;

namespace PDAAplication.MVVM.ViewModel
{
    class MainViewModel : ObservableObjects
    {
        private QuadTree<ObjectModel> _quadTreeNehnutelnost;
        private QuadTree<ObjectModel> _quadTreeParcela;
        private QuadTree<ObjectModel> _quadTreeJednotne;

        private List<ObjectModel> _allNehnutelnosti;
        private List<ObjectModel> _allParcelas;

        public RelayCommand GenerateDataCommand { get; set; }
        public RelayCommand FindBuildingsCommand { get; set; }
        public RelayCommand FindObjectCommand { get; set; }
        public RelayCommand AddBuildingCommand { get; set; }
        public RelayCommand ShowAllComand { get; set; }

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
        }

        private void InicializeButtons()
        {
            GenerateDataCommand = new RelayCommand(o => { GenerateData(); });
            FindBuildingsCommand = new RelayCommand(o => { FindBuildings(); });
            FindObjectCommand = new RelayCommand(o => { FindObject(); });
            AddBuildingCommand = new RelayCommand(o => { AddBuilding(); });
            ShowAllComand = new RelayCommand(o => { ShowAll(); });
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

            Random rnd = new Random(0);
            var tmpNehnutelnosti = new List<ObjectModel>(pocetNehnutelnosti);
            var tmpParcely = new List<ObjectModel>(pocetParciel);
            GPS serveroVychodneGPS = new(juhoZapadneGPS.X + sirka, juhoZapadneGPS.Y + dlzka);
            for (int i = 0; i < pocetNehnutelnosti; i++)
            {
                GPS tmpGps1 = new(Math.Round(NextDouble(juhoZapadneGPS.X + 1, serveroVychodneGPS.X - 3, rnd), 6),
                    Math.Round(NextDouble(juhoZapadneGPS.Y + 1, serveroVychodneGPS.Y - 3, rnd),6));
                GPS tmpGps2 = new(Math.Round(NextDouble(tmpGps1.X + 1, serveroVychodneGPS.X - 1, rnd),6),
                    Math.Round(NextDouble(tmpGps1.Y + 1, serveroVychodneGPS.Y - 1, rnd), 6));

                tmpNehnutelnosti.Add(new(i, "Nehnutelnost: " + i, tmpGps1, tmpGps2, ObjectType.Nehnutelnost));
            }

            for (int i = 0; i < pocetParciel; i++)
            {
                GPS tmpGps1 = new(Math.Round(NextDouble(juhoZapadneGPS.X + 1, serveroVychodneGPS.X - 3, rnd), 6),
                    Math.Round(NextDouble(juhoZapadneGPS.Y + 1, serveroVychodneGPS.Y - 3, rnd), 6));
                GPS tmpGps2 = new(Math.Round(NextDouble(tmpGps1.X + 1, serveroVychodneGPS.X - 1, rnd), 6),
                    Math.Round(NextDouble(tmpGps1.Y + 1, serveroVychodneGPS.Y - 1, rnd), 6));

                tmpParcely.Add(new(i, "Parcela: " + i, tmpGps1, tmpGps2, Core.ObjectType.Parcela));
            }

            _quadTreeNehnutelnost = new QuadTree<ObjectModel>(juhoZapadneGPS.X, juhoZapadneGPS.Y, sirka, dlzka, 22);
            _quadTreeParcela = new QuadTree<ObjectModel>(juhoZapadneGPS.X, juhoZapadneGPS.Y, sirka, dlzka, 22);
            _quadTreeJednotne = new QuadTree<ObjectModel>(juhoZapadneGPS.X, juhoZapadneGPS.Y, sirka, dlzka, 22);
            
            //Parcely vložíme do quad tree
            foreach (ObjectModel parcela in tmpParcely)
            {
                _quadTreeParcela.Insert(parcela.JuhoZapadnyBod.X, parcela.JuhoZapadnyBod.Y, parcela.SeveroVychodnyBod.X, parcela.SeveroVychodnyBod.Y, parcela);
                _quadTreeJednotne.Insert(parcela.JuhoZapadnyBod.X, parcela.JuhoZapadnyBod.Y, parcela.SeveroVychodnyBod.X, parcela.SeveroVychodnyBod.Y, parcela);
            }
            
            // Budeme prechádzať všetky nehnutelnosti
            // pre každú nehnuteľnosť zýskame všetky parcely, ktoré ju obsahujú
            // potom pridáme túto parcelu do zoznamu pre nehnutelnosť
            // a zase parcelu pridáme do nehnuteľnosti
            // na záver vložíme nehnuteľnosť do quad tree
            foreach (ObjectModel nehnutelnost in tmpNehnutelnosti)
            {
                var tmpListParciel = _quadTreeParcela.FindOverlapingData(nehnutelnost.JuhoZapadnyBod.X, nehnutelnost.JuhoZapadnyBod.Y, nehnutelnost.SeveroVychodnyBod.X, nehnutelnost.SeveroVychodnyBod.Y);
                foreach (ObjectModel parcela in tmpListParciel)
                {
                    nehnutelnost.ZoznamObjektov.Add(parcela);
                    parcela.ZoznamObjektov.Add(nehnutelnost);
                }
                _quadTreeNehnutelnost.Insert(nehnutelnost.JuhoZapadnyBod.X, nehnutelnost.JuhoZapadnyBod.Y, nehnutelnost.SeveroVychodnyBod.X, nehnutelnost.SeveroVychodnyBod.Y, nehnutelnost);
                _quadTreeJednotne.Insert(nehnutelnost.JuhoZapadnyBod.X, nehnutelnost.JuhoZapadnyBod.Y, nehnutelnost.SeveroVychodnyBod.X, nehnutelnost.SeveroVychodnyBod.Y, nehnutelnost);
            }
            
            // Aby sme si zobrazili nehnutelnosti a parcely tak ich pridáme do observable kolekcie
            ListNehnutelnost = new ObservableCollection<ObjectModel>(tmpNehnutelnosti);
            ListParcela = new ObservableCollection<ObjectModel>(tmpParcely);
            
            _allNehnutelnosti = tmpNehnutelnosti;
            _allParcelas = tmpParcely;


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
            ListNehnutelnost = new ObservableCollection<ObjectModel>(_quadTreeNehnutelnost.FindOverlapingData(juhoZapadneGPS.X, juhoZapadneGPS.Y, severoVýchodneGPS.X, severoVýchodneGPS.Y));
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
            ListNehnutelnost = new ObservableCollection<ObjectModel>(_quadTreeNehnutelnost.Find(juhoZapadneGPS.X, juhoZapadneGPS.Y, severoVýchodneGPS.X, severoVýchodneGPS.Y));
            ListParcela = new ObservableCollection<ObjectModel>(_quadTreeParcela.Find(juhoZapadneGPS.X, juhoZapadneGPS.Y, severoVýchodneGPS.X, severoVýchodneGPS.Y));
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
            ListNehnutelnost = new ObservableCollection<ObjectModel>(_allNehnutelnosti);
            ListParcela = new ObservableCollection<ObjectModel>(_allParcelas);
        }

        private static double NextDouble(double min, double max, Random rnd)
        {
            return rnd.NextDouble() * (max - min) + min;
        }
    }
}
