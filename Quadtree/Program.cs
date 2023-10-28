﻿using System;
using System.Collections.Generic;
using System.Drawing;
using Quadtree.StructureClasses;
using Quadtree.StructureClasses.Node;

public class Program
{
    public static void Main()
    {
        int MAX_UNITS = 1000000;
        int MAX_TEST = 100000;
        double PROBABILITY_FD = 0.6;
        double PROBABILITY_FO_FR = 0.1;
        double PROBABILITY_DEPTH = 0.01;
        
        
        int HODNOTA = 10000000; // !! Nemeniť inak nebudú spravné súradnice
        int lastestLovest = int.MaxValue;
        List<int> tieKtoreSaSemDostali = new();
        int seed = 0;
        
        
        while (lastestLovest >= 30)
        {
            Random rnd = new Random(seed);
            
            
            double MIN_X = NextDouble(-180, 175, rnd);
            double MAX_X = NextDouble(MIN_X+1,180, rnd );
            double MIN_Y = NextDouble(-90, 85, rnd);
            double MAX_Y = NextDouble(MIN_Y+1,90, rnd );
            int startDepth = rnd.Next(10, 22);
            
            Console.WriteLine("SEED: " + seed);
            Console.WriteLine(" Min x: " + MIN_X);
            Console.WriteLine(" Max x: " + MAX_X);
            Console.WriteLine(" Min y: " + MIN_Y);
            Console.WriteLine(" Max y: " + MAX_Y);
            Console.WriteLine(" Start depth: " + startDepth);
            
            
            List<int> toInsert = new(MAX_UNITS);
            List<QuadTreeNodeData<int>> toDelete = new(MAX_UNITS);
            
            for (int i = 0; i < MAX_UNITS; i++)
            {
                toInsert.Add(i);
            }

            QuadTree<int> quadtree = new QuadTree<int>(MIN_X, MIN_Y, 
                Math.Abs(MIN_X - MAX_X), Math.Abs(MIN_Y - MAX_Y), startDepth);
            for (int i = 0; i < MAX_TEST; i++)
            {
                double x = NextDouble(MIN_X, MAX_X-2, rnd);
                double y = NextDouble(MIN_Y, MAX_Y-2, rnd);
                double x2 = NextDouble(x+1, MAX_X, rnd);
                double y2 = NextDouble(y+1, MAX_Y, rnd);

                var rndValue = rnd.NextDouble();
                if (rndValue < PROBABILITY_DEPTH)
                {
                    var newDepth = rnd.Next(10, 22);
                    quadtree.setQuadTreeDepth(newDepth);
                    if (quadtree.Count != quadtree.Recount())
                    {
                        lastestLovest = i;
                        Console.WriteLine("Lastest lowest in set depth: " + lastestLovest + " in seed: " + seed);
                        tieKtoreSaSemDostali.Add(seed);
                        break;
                    }
                }
                else if (rndValue < PROBABILITY_FO_FR)
                {
                    
                    if (rnd.NextDouble() < 0.5)
                    {
                        var tmpData = quadtree.FindInterval(MIN_X, MIN_Y, MAX_X, MAX_Y);
                        if (tmpData.Count != toDelete.Count)
                        {
                            lastestLovest = i;
                            Console.WriteLine("Lastest lowest in find interval: " + lastestLovest + " in seed: " + seed);
                            Console.WriteLine("     Find: " + tmpData.Count + " toDelete: " + toDelete.Count + " realCount " + quadtree.Recount());
                            tieKtoreSaSemDostali.Add(seed);
                            break;
                        }
                    }
                    else
                    {
                        var tmpData = quadtree.FindOverlapingData(MIN_X, MIN_Y, MAX_X, MAX_Y);
                        if (tmpData.Count != toDelete.Count)
                        {
                            lastestLovest = i;
                            Console.WriteLine("Lastest lowest in find overlaping: " + lastestLovest + " in seed: " + seed);
                            Console.WriteLine("     Find: " + tmpData.Count + " toDelete: " + toDelete.Count + " realCount " + quadtree.Recount());
                            tieKtoreSaSemDostali.Add(seed);
                            break;
                        }
                    }
                }
                else if (rndValue < PROBABILITY_FD)
                {
                    if (toInsert.Count > 0)
                    {
                        int index = rnd.Next(0, toInsert.Count);
                        int value = toInsert[index];
                        toInsert.RemoveAt(index);
                        toDelete.Add(new(new(QuadTree<int>.QuadTreeRound(x), QuadTree<int>.QuadTreeRound(y)), 
                            new(QuadTree<int>.QuadTreeRound(x2), QuadTree<int>.QuadTreeRound(y2)), value));
                        
                        quadtree.Insert(x, y, 
                            x2, y2, value);
                        
                        var insertedValue = quadtree.Find(x , y, 
                            x2, y2);
                        if (!insertedValue.Contains(value))
                        {
                            lastestLovest = i;
                            Console.WriteLine("Lastest lowest in insert: " + lastestLovest + " in seed: " + seed);
                            tieKtoreSaSemDostali.Add(seed);
                            break;
                        }

                        var realCount = quadtree.Recount();
                        if (realCount != toDelete.Count)
                        {
                            lastestLovest = i;
                            Console.WriteLine("Lastest lowest in insert: " + lastestLovest + " in seed: " + seed);
                            Console.WriteLine("real count: " + realCount + " toDelete: " + toDelete.Count);
                            tieKtoreSaSemDostali.Add(seed);
                            break;
                        }
                    }
                    else
                    {
                        // aby sa mi zachoval počet operácií
                        i--;
                    }
                }
                else
                {
                    if (toDelete.Count > 0)
                    {
                        
                        int index = rnd.Next(0, toDelete.Count);
                        var value = toDelete[index];
                        toDelete.RemoveAt(index);
                        toInsert.Add(value.Data);
                        var deltedValue = quadtree.Delete((double)value.PointDownLeft.X / HODNOTA, (double)value.PointDownLeft.Y / HODNOTA,
                            (double)value.PointUpRight.X / HODNOTA, (double)value.PointUpRight.Y / HODNOTA);
                        if (deltedValue.Count == 0)
                        {
                            lastestLovest = i;
                            Console.WriteLine("Lastest lowest in delete returned empty: " + lastestLovest + " in seed: " + seed);
                            Console.WriteLine("SEED: " + seed);
                            tieKtoreSaSemDostali.Add(seed);
                            break;
                        }
                        if (!deltedValue.Contains(value.Data))
                        {
                            lastestLovest = i;
                            Console.WriteLine("Lastest lowest in delete: " + lastestLovest + " in seed: " + seed);
                            Console.WriteLine("SEED: " + seed);
                            tieKtoreSaSemDostali.Add(seed);
                            break;
                        }

                        foreach (var returnedValue in deltedValue)
                        {
                            if (returnedValue != value.Data)
                            {
                                for (int j = 0; j < toDelete.Count; j++)
                                {
                                    if (toDelete[j].Data == returnedValue)
                                    {
                                        toDelete.RemoveAt(j);
                                        toInsert.Add(returnedValue);
                                        break;
                                    }
                                }
                            }
                        }
                        
                        var realCount = quadtree.Recount();
                        if (realCount != toDelete.Count)
                        {
                            lastestLovest = i;
                            Console.WriteLine("Lastest lowest in delete: " + lastestLovest + " in seed: " + seed);
                            Console.WriteLine("real count: " + realCount + " toDelete: " + toDelete.Count + " in seed: " + seed);
                            tieKtoreSaSemDostali.Add(seed);
                            break;
                        }
                        
                    }
                    else
                    {
                        // aby sa mi zachoval počet operácií
                        i--;
                    }
                }
            }
            seed++;
        }
        
        Console.WriteLine("Najdeny SEED: " + --seed);

        /*
        QuadTree<int> quadtree = new QuadTree<int>(-50, -50, 100, 100, 4);
        quadtree.Insert(-5.0, -5.0, -1, -1, 5);
        quadtree.Insert(-5.0, -5.0, -1, -1, 6);
        quadtree.Insert(-45.0, -45.0, 30, 30, 1);
        quadtree.Insert(-45.0, -45.0, -10, -10, 2);
        quadtree.Insert(-20.0, -20.0, -10, -10, 3);
        quadtree.Insert(-10.0, -10.0, -1, -1, 4);
        Console.WriteLine("Count pred delete: " + quadtree.Count);
        //var tmp = quadtree.Delete(-20.0, -20.0, -10, -10);
        var tmp = quadtree.DeleteInterval(-20.0, -20.0, -1, -1);
        Console.WriteLine("Count po delete: " + quadtree.Count);
        foreach (var i in tmp)
        {
            Console.WriteLine(i);
        }
        tmp = quadtree.DeleteInterval(-20.0, -20.0, -1, -1);
        Console.WriteLine("Count po delete: " + quadtree.Count);
        foreach (var i in tmp)
        {
            Console.WriteLine(i);
        }

        tmp = quadtree.DeleteInterval(-50, -50, 50, 50);
        Console.WriteLine("Count po delete: " + quadtree.Count);
        foreach (var i in tmp)
        {
            Console.WriteLine(i);
        }

        quadtree.Insert(-5.0, -5.0, -1, -1, 5);
        Console.WriteLine("Count po inserte: " + quadtree.Count);
        tmp = quadtree.DeleteInterval(-50, -50, 50, 50);
        Console.WriteLine("Count po delete: " + quadtree.Count);
        foreach (var i in tmp)
        {
            Console.WriteLine(i);
        }*/

        // long hodnota =  1 * 10000000;
        // Console.WriteLine(hodnota);
        // Console.WriteLine(int.MaxValue);
        // Console.WriteLine(double.MaxValue);
        // Console.WriteLine(double.MinValue);
        // int pocetDeleni = 0;

        // while (hodnota > 1)
        // {
        // hodnota /= 2;
        // pocetDeleni++;
        // }
        // Console.WriteLine(pocetDeleni);
    }

    private static double NextDouble(double min, double max, Random rnd)
    {
        return rnd.NextDouble() * (max - min) + min;
    }
}
