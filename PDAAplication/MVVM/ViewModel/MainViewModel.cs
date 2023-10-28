using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PDAAplication.Core;
using PDAAplication.MVVM.Model;
using PDAAplication.MVVM.View;

namespace PDAAplication.MVVM.ViewModel
{
    class MainViewModel : ObservableObjects
    {


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
            GPS juhoZapadneGPS;
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

            Random rnd = new Random();
            var tmpNehnutelnosti = new List<Nehnutelnost>(pocetNehnutelnosti);
            var tmpParcely = new List<Parcela>(pocetParciel);

        }

        private void FindBuildings()
        {
            //todo add functionality
        }

        private void FindObject()
        {
            //todo add functionality
        }

        private void AddBuilding()
        {
            //todo add functionality
        }

        private static double NextDouble(double min, double max, Random rnd)
        {
            return rnd.NextDouble() * (max - min) + min;
        }
    }
}
