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


        public QuadTree<int, ObjectModel>[] LoadData(QuadTree<int, ObjectModel> nehnutelnostiQuadTree,
            QuadTree<int, ObjectModel> parcelyQuadTree,
            QuadTree<int, ObjectModel> jednotneQuadTree,
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
                var lines =  System.IO.File.ReadAllLines(file);

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

                        nehnutelnostiQuadTree = new QuadTree<int, ObjectModel>(gps1.X, gps1.Y, sirka, dlzka);
                        parcelyQuadTree = new QuadTree<int, ObjectModel>(gps1.X, gps1.Y, sirka, dlzka);
                        jednotneQuadTree = new QuadTree<int, ObjectModel>(gps1.X, gps1.Y, sirka, dlzka);
                    } 
                    else
                    {
                        var values = line.Split(';');
                        var id = int.Parse(values[0]);
                        var name = values[1];
                        var gps1 = new GPS(double.Parse(values[2]), char.Parse(values[3]), double.Parse(values[4]), char.Parse(values[5]));
                        var gps2 = new GPS(double.Parse(values[6]), char.Parse(values[7]), double.Parse(values[8]), char.Parse(values[9]));
                        var type = (ObjectType)Enum.Parse(typeof(ObjectType), values[10]);

                        GPS checkedGps1 = new();
                        GPS checkedGps2 = new();
                        Utils.CheckAndRecalculateGps(gps1, gps2, checkedGps1, checkedGps2);

                        var objectModel = new ObjectModel(id, name, gps1, gps2, type);

                        if (type == ObjectType.Nehnutelnost)
                        {
                            nehnutelnostiQuadTree.Insert(checkedGps1.X, checkedGps1.Y, checkedGps2.X, checkedGps2.Y, objectModel);
                            jednotneQuadTree.Insert(checkedGps1.X, checkedGps1.Y, checkedGps2.X, checkedGps2.Y, objectModel);
                            nehnutelnostiList.Add(objectModel);
                            if (observableCollectionNehnutelnosti.Count <= Constants.MAX_SIZE_TO_SHOW)
                            {
                                observableCollectionNehnutelnosti.Add(objectModel);
                            }
                        }
                        else
                        {
                            parcelyQuadTree.Insert(checkedGps1.X, checkedGps1.Y, checkedGps2.X, checkedGps2.Y, objectModel);
                            jednotneQuadTree.Insert(checkedGps1.X, checkedGps1.Y, checkedGps2.X, checkedGps2.Y, objectModel);
                            parcelyList.Add(objectModel);
                            if (observableCollectionParcely.Count <= Constants.MAX_SIZE_TO_SHOW)
                            {   
                                observableCollectionParcely.Add(objectModel);
                            }
                        }
                    }
                }
                // data linkujem preto, lebo môže sa stať že nehnuteľnosti sú pomiešané s parcelami, takto by som nebol schopný získať všetky parcely pre nehnuteľnosť
                // preto sa musia najskôr načítať a potom linkovať
                LinkData(nehnutelnostiList, parcelyQuadTree);
            }
            QuadTree<int, ObjectModel>[] tree = new QuadTree<int, ObjectModel>[3];
            tree[0] = nehnutelnostiQuadTree;
            tree[1] = parcelyQuadTree;
            tree[2] = jednotneQuadTree;
            return tree;
        }

        private void LinkData(List<ObjectModel> nehnutelnostiList, QuadTree<int, ObjectModel> parcelyQuadTree)
        {
            foreach (var objectModel in nehnutelnostiList)
            {
                var tmpListParciel = parcelyQuadTree.FindOverlapingData(objectModel.GpsBod1.X, objectModel.GpsBod1.Y,
                    objectModel.GpsBod2.X, objectModel.GpsBod2.Y);
                foreach (ObjectModel parcela in tmpListParciel)
                {
                    objectModel.ZoznamObjektov.Add(parcela);
                    parcela.ZoznamObjektov.Add(objectModel);
                }
            }
        }
    }
}
