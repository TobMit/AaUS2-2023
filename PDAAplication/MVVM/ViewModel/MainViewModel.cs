using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PDAAplication.Core;

namespace PDAAplication.MVVM.ViewModel
{
    class MainViewModel
    {
        public RelayCommand GenerateDataCommand { get; set; }
        public RelayCommand FindBuildingsCommand { get; set; }
        public RelayCommand FindObjectCommand { get; set; }
        public RelayCommand AddBuildingCommand { get; set; }



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
            //todo add functionality
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
    }
}
