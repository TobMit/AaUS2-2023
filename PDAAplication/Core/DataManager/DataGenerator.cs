using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PDAAplication.MVVM.Model;
using Quadtree.StructureClasses;

namespace PDAAplication.Core.DataManager
{
    public static class DataGenerator
    {
        public static void GenerateData(QuadTree<ObjectModel> nehnutelnostiQuadTree,
            QuadTree<ObjectModel> parcelyQuadTree,
            QuadTree<ObjectModel> jednotneQuadTree,
            ObservableCollection<ObjectModel> observableCollectionNehnutelnosti,
            ObservableCollection<ObjectModel> observableCollectionParcely,
            List<ObjectModel> nehnutelnostiList,
            List<ObjectModel> parcelyList,
            GPS juhozapadneGps,
            GPS severovychodneGps,
            int pocetNehnutelnosti,
            int pocetParciel)
        {
            Random rnd = new Random(0);

            //nehnutelnostiList = new List<ObjectModel>(pocetNehnutelnosti);
            //parcelyList = new List<ObjectModel>(pocetParciel);

            for (int i = 0; i < pocetParciel; i++)
            {
                GPS tmpGps1 = new(NextDouble(juhozapadneGps.X + 1, severovychodneGps.X - 3, rnd),
                    NextDouble(juhozapadneGps.Y + 1, severovychodneGps.Y - 3, rnd));
                GPS tmpGps2 = new(NextDouble(tmpGps1.X + 1, severovychodneGps.X - 1, rnd),
                    NextDouble(tmpGps1.Y + 1, severovychodneGps.Y - 1, rnd));

                ObjectModel tmpParcela = new(i, "Parcela: " + i, tmpGps1, tmpGps2, Core.ObjectType.Parcela);

                parcelyQuadTree.Insert(tmpGps1.X, tmpGps1.Y, tmpGps2.X, tmpGps2.Y, tmpParcela);
                jednotneQuadTree.Insert(tmpGps1.X, tmpGps1.Y, tmpGps2.X, tmpGps2.Y, tmpParcela);

                parcelyList.Add(tmpParcela);
                observableCollectionParcely.Add(tmpParcela);
            }
            // Budeme postupne generovať nehnuteľnosti
            // pre každú nehnuteľnosť zýskame všetky parcely, ktoré ju obsahujú
            // potom pridáme túto parcelu do zoznamu pre nehnutelnosť
            // a zase parcelu pridáme do nehnuteľnosti
            // na záver vložíme nehnuteľnosť do quad tree

            for (int i = 0; i < pocetNehnutelnosti; i++)
            {
                GPS tmpGps1 = new(NextDouble(juhozapadneGps.X + 1, severovychodneGps.X - 3, rnd),
                    NextDouble(juhozapadneGps.Y + 1, severovychodneGps.Y - 3, rnd));
                GPS tmpGps2 = new(NextDouble(tmpGps1.X + 1, severovychodneGps.X - 1, rnd),
                    NextDouble(tmpGps1.Y + 1, severovychodneGps.Y - 1, rnd));

                ObjectModel tmpNehnutelnost = new(i, "Nehnuteľnosť: " + i, tmpGps1, tmpGps2, Core.ObjectType.Nehnutelnost);
                var tmpListParciel = parcelyQuadTree.FindOverlapingData(tmpNehnutelnost.JuhoZapadnyBod.X, tmpNehnutelnost.JuhoZapadnyBod.Y,
                    tmpNehnutelnost.SeveroVychodnyBod.X, tmpNehnutelnost.SeveroVychodnyBod.Y);
                foreach (ObjectModel parcela in tmpListParciel)
                {
                    tmpNehnutelnost.ZoznamObjektov.Add(parcela);
                    parcela.ZoznamObjektov.Add(tmpNehnutelnost);
                }

                nehnutelnostiQuadTree.Insert(tmpGps1.X, tmpGps1.Y, tmpGps2.X, tmpGps2.Y, tmpNehnutelnost);
                jednotneQuadTree.Insert(tmpGps1.X, tmpGps1.Y, tmpGps2.X, tmpGps2.Y, tmpNehnutelnost);

                nehnutelnostiList.Add(tmpNehnutelnost);
                observableCollectionNehnutelnosti.Add(tmpNehnutelnost);
            }

            //observableCollectionNehnutelnosti = new ObservableCollection<ObjectModel>(nehnutelnostiList);
            //observableCollectionParcely = new ObservableCollection<ObjectModel>(parcelyList);
        }

        private static double NextDouble(double min, double max, Random rnd, int pocDesMiest = 6)
        {
            // zaokruhlenie je pre to aby používateľ nemusel zadávať xy des miest pri vyhľadávani objektu
            return Math.Round(rnd.NextDouble() * (max - min) + min, pocDesMiest);
        }
    }
}
