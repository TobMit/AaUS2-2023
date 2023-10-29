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
        private QuadTree<Nehnutelnost> _quadTreeNehnutelnost;
        private QuadTree<Parcela> _quadTreeParcela;

        public RelayCommand GenerateDataCommand { get; set; }
        public RelayCommand FindBuildingsCommand { get; set; }
        public RelayCommand FindObjectCommand { get; set; }
        public RelayCommand AddBuildingCommand { get; set; }

        private ObservableCollection<Nehnutelnost> _listNehnutelnost;

        public ObservableCollection<Nehnutelnost> ListNehnutelnost
        {
            get { return _listNehnutelnost; }
            set
            {
                _listNehnutelnost = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Parcela> _listParcela;

        public ObservableCollection<Parcela> ListParcela
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
        }

        private void InicializeButtons()
        {
            GenerateDataCommand = new RelayCommand(o => { GenerateData(); });
            FindBuildingsCommand = new RelayCommand(o => { FindBuildings(); });
            FindObjectCommand = new RelayCommand(o => { FindObject(); });
            AddBuildingCommand = new RelayCommand(o => { AddBuilding(); });
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
            var tmpNehnutelnosti = new List<Nehnutelnost>(pocetNehnutelnosti);
            var tmpParcely = new List<Parcela>(pocetParciel);
            GPS serveroVychodneGPS = new(juhoZapadneGPS.X + sirka, juhoZapadneGPS.Y + dlzka);
            for (int i = 0; i < pocetNehnutelnosti; i++)
            {
                GPS tmpGps1 = new(Math.Round(NextDouble(juhoZapadneGPS.X + 1, serveroVychodneGPS.X - 3, rnd), 6),
                    Math.Round(NextDouble(juhoZapadneGPS.Y + 1, serveroVychodneGPS.Y - 3, rnd),6));
                GPS tmpGps2 = new(Math.Round(NextDouble(tmpGps1.X + 1, serveroVychodneGPS.X - 1, rnd),6),
                    Math.Round(NextDouble(tmpGps1.Y + 1, serveroVychodneGPS.Y - 1, rnd), 6));

                tmpNehnutelnosti.Add(new(i, "Nehnutelnost: " + i, tmpGps1, tmpGps2));
            }

            for (int i = 0; i < pocetParciel; i++)
            {
                GPS tmpGps1 = new(Math.Round(NextDouble(juhoZapadneGPS.X + 1, serveroVychodneGPS.X - 3, rnd), 6),
                    Math.Round(NextDouble(juhoZapadneGPS.Y + 1, serveroVychodneGPS.Y - 3, rnd), 6));
                GPS tmpGps2 = new(Math.Round(NextDouble(tmpGps1.X + 1, serveroVychodneGPS.X - 1, rnd), 6),
                    Math.Round(NextDouble(tmpGps1.Y + 1, serveroVychodneGPS.Y - 1, rnd), 6));

                tmpParcely.Add(new(i, "Parcela: " + i, tmpGps1, tmpGps2));
            }

            _quadTreeNehnutelnost = new QuadTree<Nehnutelnost>(juhoZapadneGPS.X, juhoZapadneGPS.Y, sirka, dlzka, 22);
            _quadTreeParcela = new QuadTree<Parcela>(juhoZapadneGPS.X, juhoZapadneGPS.Y, sirka, dlzka, 22);
            
            //Parcely vložíme do quad tree
            foreach (Parcela parcela in tmpParcely)
            {
                _quadTreeParcela.Insert(parcela.JuhoZapadnyBod.X, parcela.JuhoZapadnyBod.Y, parcela.SeveroVychodnyBod.X, parcela.SeveroVychodnyBod.Y, parcela);
            }
            
            // Budeme prechádzať všetky nehnutelnosti
            // pre každú nehnuteľnosť zýskame všetky parcely, ktoré ju obsahujú
            // potom pridáme túto parcelu do zoznamu pre nehnutelnosť
            // a zase parcelu pridáme do nehnuteľnosti
            // na záver vložíme nehnuteľnosť do quad tree
            foreach (Nehnutelnost nehnutelnost in tmpNehnutelnosti)
            {
                var tmpListParciel = _quadTreeParcela.FindOverlapingData(nehnutelnost.JuhoZapadnyBod.X, nehnutelnost.JuhoZapadnyBod.Y, nehnutelnost.SeveroVychodnyBod.X, nehnutelnost.SeveroVychodnyBod.Y);
                foreach (Parcela parcela in tmpListParciel)
                {
                    nehnutelnost.ZoznamParciel.Add(parcela);
                    parcela.ZoznamNehnutelnosti.Add(nehnutelnost);
                }
                _quadTreeNehnutelnost.Insert(nehnutelnost.JuhoZapadnyBod.X, nehnutelnost.JuhoZapadnyBod.Y, nehnutelnost.SeveroVychodnyBod.X, nehnutelnost.SeveroVychodnyBod.Y, nehnutelnost);
            }
            
            // Aby sme si zobrazili nehnutelnosti a parcely tak ich pridáme do observable kolekcie
            ListNehnutelnost = new ObservableCollection<Nehnutelnost>(tmpNehnutelnosti);
            ListParcela = new ObservableCollection<Parcela>(tmpParcely);


        }

        private void FindBuildings()
        {
            if (_quadTreeNehnutelnost is null)
            {
                return;
            }
            //todo add functionality
        }

        private void FindObject()
        {
            if (_quadTreeNehnutelnost is null || _quadTreeParcela is null)
            {
                return;
            }
            //todo add functionality
        }

        private void AddBuilding()
        {
            if (_quadTreeNehnutelnost is null)
            {
                return;
            }
            //todo add functionality
        }

        private static double NextDouble(double min, double max, Random rnd)
        {
            return rnd.NextDouble() * (max - min) + min;
        }
    }
}
