﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicHashFileStructure.StructureClasses;
using PDAApplication2.MVVM.Model;
using Quadtree.StructureClasses;

namespace PDAApplication2.Core.DataManager
{
    public static class DataGenerator
    {
        public static void GenerateData(
            QuadTree<int, ObjectModelQuad> nehnutelnostiQuadTree,
            QuadTree<int, ObjectModelQuad> parcelyQuadTree,
            DynamicHashFile<int, ObjectModelNehnutelnost> dhfNehutelnosti,
            DynamicHashFile<int, ObjectModelParcela> dhfParcely,
            GPS gps1,
            GPS gps2,
            int pocetNehnutelnosti,
            int pocetParciel)
        {
            Random rnd = new Random(0);

            var sirka = Math.Abs(gps2.X - gps1.X);
            var vyska = Math.Abs(gps2.Y - gps1.Y);

            int count = 0;
                
            for (int i = 0; i < pocetParciel; i++)
            {
                GPS tmpGps1 = new(NextDouble(gps1.X + 1, gps2.X - 3, rnd), 'W',
                    NextDouble(gps1.Y + 1, gps2.Y - 3, rnd), 'S');

                // Druhú pozíciu gps vypočítame tak že k prvej pozícií pripočítame náhodnú širku a výšku
                // šírku a výšku vypočítame ako náhodné číslo medzi 1 pozíciou a minimom medzi (celková šírka /2, a šírka k druhému bodu)
                // toto nam zabráni generovanie objektov menších ako 50% celkovej plochy a tým pádom aj lepšie rozloženie v strome
                var tmpSirka = NextDouble(0, Math.Min(sirka * Constants.MAX_SIZE_OF_OBJCET_PERCENTAGE, gps2.X - 1 - tmpGps1.X), rnd);
                var tmpViska = NextDouble(0, Math.Min(vyska * Constants.MAX_SIZE_OF_OBJCET_PERCENTAGE, gps2.Y - 1 - tmpGps1.Y), rnd);

                GPS tmpGps2 = new(tmpGps1.X + tmpSirka,'E', tmpGps1.Y + tmpViska, 'N');

                ObjectModelQuad tmpParcela = new(count, tmpGps1, tmpGps2); //todo spraviť tak aby to išlo s constants
                ObjectModelParcela tmpDHFParcela = new(count, "Parcela: ", tmpGps1, tmpGps2);

                parcelyQuadTree.Insert(tmpGps1.X, tmpGps1.Y, tmpGps2.X, tmpGps2.Y, tmpParcela);
                dhfParcely.Insert(tmpDHFParcela.GetKey(), tmpDHFParcela);

                count++;
            }
            // Budeme postupne generovať nehnuteľnosti
            // pre každú nehnuteľnosť získame všetky parcely, ktoré ju obsahujú
            // potom pridáme túto parcelu do zoznamu pre nehnuteľnosť
            // a zase parcelu pridáme do nehnuteľnosti
            // na záver vložíme nehnuteľnosť do quad tree

            //todo toto ešte nemám dokončené
            for (int i = 0; i < pocetNehnutelnosti; i++)
            {
                GPS tmpGps1 = new(NextDouble(gps1.X + 1, gps2.X - 3, rnd), 'W',
                    NextDouble(gps1.Y + 1, gps2.Y - 3, rnd), 'S');

                // Druhú pozíciu gps vypočítame tak že k prvej pozícií pripočítame náhodnú širku a výšku
                // šírku a výšku vypočítame ako náhodné číslo medzi 1 pozíciou a minimom medzi (celková šírka /2, a šírka k druhému bodu)
                // toto nam zaručí generovanie objektov menších ako 50% celkovej plochy a tým pádom aj lepšie rozloženie v strome
                var tmpSirka = NextDouble(0, Math.Min(sirka * Constants.MAX_SIZE_OF_OBJCET_PERCENTAGE, gps2.X - 1 - tmpGps1.X), rnd);
                var tmpViska = NextDouble(0, Math.Min(vyska * Constants.MAX_SIZE_OF_OBJCET_PERCENTAGE, gps2.Y - 1 - tmpGps1.Y), rnd);

                GPS tmpGps2 = new(tmpGps1.X + tmpSirka, 'E', tmpGps1.Y + tmpViska, 'N');

                ObjectModel tmpNehnutelnost = new(count, "Nehnuteľnosť: " + count, tmpGps1, tmpGps2, Core.ObjectType.Nehnutelnost);
                var tmpListParciel = parcelyQuadTree.FindIntervalOverlapping(tmpNehnutelnost.GpsBod1.X, tmpNehnutelnost.GpsBod1.Y,
                    tmpNehnutelnost.GpsBod2.X, tmpNehnutelnost.GpsBod2.Y);
                
                bool isOk = tmpListParciel.Count <= Constants.MAX_COUNT_PARCELS_IN_NEHNUTELNOST;
                if (isOk)
                {
                    foreach (ObjectModel parcela in tmpListParciel)
                    {
                        if (parcela.ZoznamObjektov.Count >= Constants.MAX_COUNT_NEHNUTELNOST_IN_PARCEL)
                        {
                            isOk = false;
                            break;
                        }
                    }
                }

                if (isOk)
                {
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
                    
                    count++;
                }
                else
                {
                    i--;
                }
            }

        }

        private static double NextDouble(double min, double max, Random rnd, int pocDesMiest = 6)
        {
            // zaokrúhlenie je pre to aby používateľ nemusel zadávať xy des miest pri vyhľadávaní objektu
            return Math.Round(rnd.NextDouble() * (max - min) + min, pocDesMiest);
        }
    }
}
