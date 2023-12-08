using Microsoft.Win32;
using PDAApplication2.MVVM.Model;
using Quadtree.StructureClasses;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDAApplication2.Core.DataManager.FileManager
{
    class DataLoader
    {
        public DataLoader()
        {
            
        }


        /// <summary>
        /// Načíta data zo súboru
        /// </summary>
        public QuadTree<int, ObjectModelQuad>[] LoadData(QuadTree<int, ObjectModelQuad> nehnutelnostiQuadTree,
            QuadTree<int, ObjectModelQuad> parcelyQuadTree,
            QuadTree<int, ObjectModelQuad> jednotneQuadTree,
            ObservableCollection<ObjectModelQuad> observableCollectionNehnutelnosti,
            ObservableCollection<ObjectModelQuad> observableCollectionParcely,
            List<ObjectModelQuad> nehnutelnostiList,
            List<ObjectModelQuad> parcelyList)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Data (*.csv)|*.csv";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            openFileDialog.Title = "Vyberte súbor s dátami";

            if (openFileDialog.ShowDialog() == true)
            {
                var file = openFileDialog.FileName;
                var lines =  System.IO.File.ReadAllLines(file);

                bool firstLine = true;

                foreach (var line in lines)
                {
                    // ak je prvý riadok tak načítam zakladané rozloženie quadtree
                    if (firstLine)
                    {
                        firstLine = false;
                        var values = line.Split(';');
                        var gps1 = new GPS(double.Parse(values[0]), double.Parse(values[1]));
                        var gps2 = new GPS(double.Parse(values[2]), double.Parse(values[3]));
                        var sirka = Math.Abs(gps2.X - gps1.X);
                        var dlzka = Math.Abs(gps2.Y - gps1.Y);

                        nehnutelnostiQuadTree = new QuadTree<int, ObjectModelQuad>(gps1.X, gps1.Y, sirka, dlzka);
                        parcelyQuadTree = new QuadTree<int, ObjectModelQuad>(gps1.X, gps1.Y, sirka, dlzka);
                        jednotneQuadTree = new QuadTree<int, ObjectModelQuad>(gps1.X, gps1.Y, sirka, dlzka);
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

                        var objectModel = new ObjectModelQuad(id, gps1, gps2);

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
            }
            QuadTree<int, ObjectModelQuad>[] tree = new QuadTree<int, ObjectModelQuad>[3];
            tree[0] = nehnutelnostiQuadTree;
            tree[1] = parcelyQuadTree;
            tree[2] = jednotneQuadTree;
            return tree;
        }
    }
}
