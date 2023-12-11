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
        public QuadTree<int, ObjectModelQuad>? LoadData(string file)
        {
            var lines = System.IO.File.ReadAllLines(file);

            if (lines.Length <= 0)
            {
                return null;
            }

            bool firstLine = true;

            QuadTree<int, ObjectModelQuad> quadTree = null;

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
                    Constants.IdObjektu = int.Parse(values[4]);

                    quadTree = new QuadTree<int, ObjectModelQuad>(gps1.X, gps1.Y, sirka, dlzka);
                }
                else
                {
                    if (quadTree is not null)
                    {
                        var values = line.Split(';');
                        var id = int.Parse(values[0]);
                        var gps1 = new GPS(double.Parse(values[1]), char.Parse(values[2]), double.Parse(values[3]), char.Parse(values[4]));
                        var gps2 = new GPS(double.Parse(values[5]), char.Parse(values[6]), double.Parse(values[7]), char.Parse(values[8]));

                        GPS checkedGps1 = new();
                        GPS checkedGps2 = new();
                        Utils.CheckAndRecalculateGps(gps1, gps2, checkedGps1, checkedGps2);

                        var objectModel = new ObjectModelQuad(id, gps1, gps2);

                        quadTree.Insert(checkedGps1.X, checkedGps1.Y, checkedGps2.X, checkedGps2.Y, objectModel);
                    }
                }
            }

            return quadTree;
        }
    }
}
