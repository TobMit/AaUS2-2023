using Microsoft.Win32;
using PDAAplication.MVVM.Model;
using Quadtree.StructureClasses;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDAAplication.Core.DataManager.FileManager
{
    class DataLoader
    {
        public DataLoader()
        {
            
        }

        public void LoadFirstLine()
        {

        }

        public async void LoadData(QuadTree<ObjectModel> nehnutelnostiQuadTree,
            QuadTree<ObjectModel> parcelyQuadTree,
            QuadTree<ObjectModel> jednotneQuadTree,
            ObservableCollection<ObjectModel> observableCollectionNehnutelnosti,
            ObservableCollection<ObjectModel> observableCollectionParcely,
            List<ObjectModel> nehnutelnostiList,
            List<ObjectModel> parcelyList)
        {
            var opneFileDialog = new OpenFileDialog();
            opneFileDialog.Filter = "Data (*.csv)|*.csv";
            opneFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            opneFileDialog.Title = "Vyberte súbor s dátami";

            if (opneFileDialog.ShowDialog() == true)
            {
                var file = opneFileDialog.FileName;
                var lines = await System.IO.File.ReadAllLinesAsync(file);

                bool firstLine = true;

                foreach (var line in lines)
                {
                    // ak je prvý riadok tak načítam zakladné rzoloženie quadtree
                    if (firstLine)
                    {
                        firstLine = false;
                        var values = line.Split(';');
                        var gps1 = new GPS(double.Parse(values[0]), double.Parse(values[1]));
                        var gps2 = new GPS(double.Parse(values[2]), double.Parse(values[3]));
                        var sirka = Math.Abs(gps2.X - gps1.X);
                        var dlzka = Math.Abs(gps2.Y - gps1.Y);

                        nehnutelnostiQuadTree = new QuadTree<ObjectModel>(gps1.X, gps1.Y, sirka, dlzka);
                        parcelyQuadTree = new QuadTree<ObjectModel>(gps1.X, gps1.Y, sirka, dlzka);
                        jednotneQuadTree = new QuadTree<ObjectModel>(gps1.X, gps1.Y, sirka, dlzka);
                    } 
                    else
                    {
                        var values = line.Split(';');
                        var id = int.Parse(values[0]);
                        var name = values[1];
                        var gps1 = new GPS(double.Parse(values[2]), double.Parse(values[3]));
                        var gps2 = new GPS(double.Parse(values[4]), double.Parse(values[5]));
                        var type = (ObjectType)Enum.Parse(typeof(ObjectType), values[6]);

                        var objectModel = new ObjectModel(id, name, gps1, gps2, type);

                        if (type == ObjectType.Nehnutelnost)
                        {
                            nehnutelnostiQuadTree.Insert(gps1.X, gps1.Y, gps2.X, gps2.Y, objectModel);
                            jednotneQuadTree.Insert(gps1.X, gps1.Y, gps2.X, gps2.Y, objectModel);
                            nehnutelnostiList.Add(objectModel);
                            observableCollectionNehnutelnosti.Add(objectModel);
                        }
                        else
                        {
                            parcelyQuadTree.Insert(gps1.X, gps1.Y, gps2.X, gps2.Y, objectModel);
                            jednotneQuadTree.Insert(gps1.X, gps1.Y, gps2.X, gps2.Y, objectModel);
                            parcelyList.Add(objectModel);
                            observableCollectionParcely.Add(objectModel);
                        }
                    }
                }
                LinkData(nehnutelnostiList, parcelyQuadTree);
            }
        }

        private void LinkData(List<ObjectModel> nehnutelnostiList, QuadTree<ObjectModel> parcelyQuadTree)
        {
            foreach (var objectModel in nehnutelnostiList)
            {
                var tmpListParciel = parcelyQuadTree.FindOverlapingData(objectModel.JuhoZapadnyBod.X, objectModel.JuhoZapadnyBod.Y,
                    objectModel.SeveroVychodnyBod.X, objectModel.SeveroVychodnyBod.Y);
                foreach (ObjectModel parcela in tmpListParciel)
                {
                    objectModel.ZoznamObjektov.Add(parcela);
                    parcela.ZoznamObjektov.Add(objectModel);
                }
            }
        }
    }
}
