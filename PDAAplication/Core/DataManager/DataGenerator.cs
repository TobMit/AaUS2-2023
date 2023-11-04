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
        public static void GenerateData(QuadTree<int, ObjectModel> nehnutelnostiQuadTree,
            QuadTree<int, ObjectModel> parcelyQuadTree,
            QuadTree<int, ObjectModel> jednotneQuadTree,
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

            var sirka = Math.Abs(severovychodneGps.X - juhozapadneGps.X);
            var vyska = Math.Abs(severovychodneGps.Y - juhozapadneGps.Y);
                
            for (int i = 0; i < pocetParciel; i++)
            {
                GPS tmpGps1 = new(NextDouble(juhozapadneGps.X + 1, severovychodneGps.X - 3, rnd), 'W',
                    NextDouble(juhozapadneGps.Y + 1, severovychodneGps.Y - 3, rnd), 'S');

                // Druhú poziciu gps vypočítame tak že k prvej pozicií pripočítame náhodnú širku a výšku
                // šírku a výšku vypočítame ako nahodne číslo medzi 1 pozíciou a minimom medzi (celková šírka /2, a šíkra k druhému bodu)
                // toto nam zaruší generovanie objektov menších ako 50% celkovej plochy a tým pádom aj lepšie rozloženie v strome
                var tmpSirka = NextDouble(0, Math.Min(sirka * Constants.MAX_SIZE_OF_OBJCET_PERCENTAGE, severovychodneGps.X - 1 - tmpGps1.X), rnd);
                var tmpViska = NextDouble(0, Math.Min(vyska * Constants.MAX_SIZE_OF_OBJCET_PERCENTAGE, severovychodneGps.Y - 1 - tmpGps1.Y), rnd);

                GPS tmpGps2 = new(tmpGps1.X + tmpSirka,'E', tmpGps1.Y + tmpViska, 'N');

                ObjectModel tmpParcela = new(i, "Parcela: " + i, tmpGps1, tmpGps2, Core.ObjectType.Parcela);

                parcelyQuadTree.Insert(tmpGps1.X, tmpGps1.Y, tmpGps2.X, tmpGps2.Y, tmpParcela);
                jednotneQuadTree.Insert(tmpGps1.X, tmpGps1.Y, tmpGps2.X, tmpGps2.Y, tmpParcela);

                parcelyList.Add(tmpParcela);
                if (i <= Constants.MAX_SIZE_TO_SHOW)
                {
                    observableCollectionParcely.Add(tmpParcela);
                }
            }
            // Budeme postupne generovať nehnuteľnosti
            // pre každú nehnuteľnosť zýskame všetky parcely, ktoré ju obsahujú
            // potom pridáme túto parcelu do zoznamu pre nehnutelnosť
            // a zase parcelu pridáme do nehnuteľnosti
            // na záver vložíme nehnuteľnosť do quad tree

            for (int i = 0; i < pocetNehnutelnosti; i++)
            {
                GPS tmpGps1 = new(NextDouble(juhozapadneGps.X + 1, severovychodneGps.X - 3, rnd), 'W',
                    NextDouble(juhozapadneGps.Y + 1, severovychodneGps.Y - 3, rnd), 'S');

                // Druhú poziciu gps vypočítame tak že k prvej pozicií pripočítame náhodnú širku a výšku
                // šírku a výšku vypočítame ako nahodne číslo medzi 1 pozíciou a minimom medzi (celková šírka /2, a šíkra k druhému bodu)
                // toto nam zaruší generovanie objektov menších ako 50% celkovej plochy a tým pádom aj lepšie rozloženie v strome
                var tmpSirka = NextDouble(0, Math.Min(sirka * Constants.MAX_SIZE_OF_OBJCET_PERCENTAGE, severovychodneGps.X - 1 - tmpGps1.X), rnd);
                var tmpViska = NextDouble(0, Math.Min(vyska * Constants.MAX_SIZE_OF_OBJCET_PERCENTAGE, severovychodneGps.Y - 1 - tmpGps1.Y), rnd);

                GPS tmpGps2 = new(tmpGps1.X + tmpSirka, 'E', tmpGps1.Y + tmpViska, 'N');

                ObjectModel tmpNehnutelnost = new(i, "Nehnuteľnosť: " + i, tmpGps1, tmpGps2, Core.ObjectType.Nehnutelnost);
                var tmpListParciel = parcelyQuadTree.FindOverlapingData(tmpNehnutelnost.GpsBod1.X, tmpNehnutelnost.GpsBod1.Y,
                    tmpNehnutelnost.GpsBod2.X, tmpNehnutelnost.GpsBod2.Y);
                foreach (ObjectModel parcela in tmpListParciel)
                {
                    tmpNehnutelnost.ZoznamObjektov.Add(parcela);
                    parcela.ZoznamObjektov.Add(tmpNehnutelnost);
                }

                nehnutelnostiQuadTree.Insert(tmpGps1.X, tmpGps1.Y, tmpGps2.X, tmpGps2.Y, tmpNehnutelnost);
                jednotneQuadTree.Insert(tmpGps1.X, tmpGps1.Y, tmpGps2.X, tmpGps2.Y, tmpNehnutelnost);

                nehnutelnostiList.Add(tmpNehnutelnost);
                if (i <= Constants.MAX_SIZE_TO_SHOW)
                {
                    observableCollectionNehnutelnosti.Add(tmpNehnutelnost);
                }
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
