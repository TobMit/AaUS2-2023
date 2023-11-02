using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using Quadtree.StructureClasses;
using Quadtree.StructureClasses.Node;

public class Program
{
    private static bool parallel = false;
    
    private static int MAX_UNITS = 1000000;
    private static int MAX_TEST = 100000;
    private static double PROBABILITY_FD = 0.6;
    private static double PROBABILITY_FO_FR = 0.1;
    private static double PROBABILITY_DEPTH = 0.01;
        
        
    private static int lastestLovest = int.MaxValue;
    private static int seed = 0;
    private static int maxSeed = 4;
    // private static int maxSeed = int.MaxValue;
    public static void Main()
    {
        if (parallel)
        {
            
            Parallel.For(seed, maxSeed, (iSeed) =>
            {
                try
                {
                    int result = TestInstance(iSeed);
                    if (result < 30)
                    {
                        Console.WriteLine("Najdeny SEED: " + iSeed);
                        return;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("---------------------------Error v seede: " + iSeed + "\n" + e.Message);
                    return;
                }
            });
        }
        else
        {
            while (lastestLovest >= 30)
            {
                lastestLovest = TestInstance(seed);
                seed++;
            }
            Console.WriteLine("Najdeny SEED: " + --seed);
        }

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
    }

    private static int TestInstance(int seed)
    {
        Random rnd = new Random(seed);

        bool seedOk = true;    
        
        double MIN_X = NextDouble(-180, 165, rnd);
        double MAX_X = NextDouble(MIN_X+5,180, rnd );
        double MIN_Y = NextDouble(-90, 75, rnd);
        double MAX_Y = NextDouble(MIN_Y+5,90, rnd );
        //int startDepth = rnd.Next(10, 22);
        
        //Console.WriteLine("SEED: " + seed + "\n Min x: " + MIN_X + "\n Max x: " + MAX_X + "\n Min y: " + MIN_Y + "\n Max y: " + MAX_Y + "\n Start depth: " + startDepth);
        Console.WriteLine("SEED: " + seed + "\n Min x: " + MIN_X + "\n Max x: " + MAX_X + "\n Min y: " + MIN_Y + "\n Max y: " + MAX_Y);
        
        
        List<int> toInsert = new(MAX_UNITS);
        List<QuadTreeNodeData<int, int>> toDelete = new(MAX_UNITS);
        
        for (int i = 0; i < MAX_UNITS; i++)
        {
            toInsert.Add(i);
        }

        QuadTree<int, int> quadtree = new QuadTree<int, int>(MIN_X, MIN_Y, 
            Math.Abs(MIN_X - MAX_X), Math.Abs(MIN_Y - MAX_Y));
        for (int i = 0; i < MAX_TEST; i++)
        {
            double x = NextDouble(MIN_X, MAX_X-2, rnd);
            double y = NextDouble(MIN_Y, MAX_Y-2, rnd);
            double x2 = NextDouble(x+1, MAX_X, rnd);
            double y2 = NextDouble(y+1, MAX_Y, rnd);

            var rndValue = rnd.NextDouble();
            if (rndValue < PROBABILITY_DEPTH)
            {
                var newDepth = rnd.Next(30, 200);
                quadtree.SetQuadTreeDepth(newDepth);
                if (quadtree.Count != quadtree.Recount())
                {
                    seedOk = false;
                    lastestLovest = i;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Lastest lowest in set depth: " + lastestLovest + " in seed: " + seed);
                    Console.ForegroundColor = ConsoleColor.White;
                    return lastestLovest;
                }
            }
            else if (rndValue < PROBABILITY_FO_FR)
            {
                
                if (rnd.NextDouble() < 0.5)
                {
                    var tmpData = quadtree.FindInterval(MIN_X, MIN_Y, MIN_X + Math.Abs(MIN_X - MAX_X), MIN_Y + Math.Abs(MIN_Y - MAX_Y));
                    if (tmpData.Count != toDelete.Count)
                    {
                        seedOk = false;
                        lastestLovest = i;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Lastest lowest in find interval: " + lastestLovest + " in seed: " + seed);
                        Console.WriteLine("     Find: " + tmpData.Count + " toDelete: " + toDelete.Count + " realCount " + quadtree.Recount());
                        Console.ForegroundColor = ConsoleColor.White;
                        return lastestLovest;
                    }
                }
                else
                {
                    var tmpData = quadtree.FindOverlapingData(MIN_X, MIN_Y, MAX_X, MAX_Y);
                    if (tmpData.Count != toDelete.Count)
                    {
                        seedOk = false;
                        lastestLovest = i;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Lastest lowest in find overlaping: " + lastestLovest + " in seed: " + seed);
                        Console.WriteLine("     Find: " + tmpData.Count + " toDelete: " + toDelete.Count + " realCount " + quadtree.Recount());
                        Console.ForegroundColor = ConsoleColor.White;
                        return lastestLovest;
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
                    toDelete.Add(new(new(x, y), 
                        new(x2, y2), value));
                    
                    quadtree.Insert(x, y, 
                        x2, y2, value);
                    
                    var insertedValue = quadtree.Find(x , y, 
                        x2, y2);
                    if (!insertedValue.Contains(value))
                    {
                        seedOk = false;
                        lastestLovest = i;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Lastest lowest in insert: " + lastestLovest + " in seed: " + seed);
                        Console.ForegroundColor = ConsoleColor.White;
                        return lastestLovest;
                    }

                    var realCount = quadtree.Recount();
                    if (realCount != toDelete.Count)
                    {
                        seedOk = false;
                        lastestLovest = i;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Lastest lowest in insert: " + lastestLovest + " in seed: " + seed);
                        Console.WriteLine("real count: " + realCount + " toDelete: " + toDelete.Count);
                        Console.ForegroundColor = ConsoleColor.White;
                        return lastestLovest;
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
                    var deltedValue = quadtree.Delete((double)value.PointDownLeft.X, (double)value.PointDownLeft.Y,
                        (double)value.PointUpRight.X, (double)value.PointUpRight.Y, value.Data);
                    if (deltedValue.Count != 1)
                    {
                        seedOk = false;
                        lastestLovest = i;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Lastest lowest in delete returned empty: " + lastestLovest + " in seed: " + seed);
                        Console.WriteLine("SEED: " + seed);
                        Console.ForegroundColor = ConsoleColor.White;
                        return lastestLovest;
                    }
                    if (!deltedValue.Contains(value.Data))
                    {
                        seedOk = false;
                        lastestLovest = i;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Lastest lowest in delete: " + lastestLovest + " in seed: " + seed);
                        Console.WriteLine("SEED: " + seed);
                        Console.ForegroundColor = ConsoleColor.White;
                        return lastestLovest;
                    }

                    // foreach (var returnedValue in deltedValue)
                    // {
                    //     if (returnedValue != value.Data)
                    //     {
                    //         for (int j = 0; j < toDelete.Count; j++)
                    //         {
                    //             if (toDelete[j].Data == returnedValue)
                    //             {
                    //                 toDelete.RemoveAt(j);
                    //                 toInsert.Add(returnedValue);
                    //                 break;
                    //             }
                    //         }
                    //     }
                    // }
                    
                    var realCount = quadtree.Recount();
                    if (realCount != toDelete.Count)
                    {
                        seedOk = false;
                        lastestLovest = i;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Lastest lowest in delete: " + lastestLovest + " in seed: " + seed);
                        Console.WriteLine("real count: " + realCount + " toDelete: " + toDelete.Count + " in seed: " + seed);
                        Console.ForegroundColor = ConsoleColor.White;
                        return lastestLovest;
                    }
                    
                }
                else
                {
                    // aby sa mi zachoval počet operácií
                    i--;
                }
            }
        }

        if (seedOk)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("SEED OK: " + seed);
            Console.ForegroundColor = ConsoleColor.White;
        }
        return int.MaxValue;
    }

    public static double NextDouble(double min, double max, Random rnd)
    {
        return Math.Round(rnd.NextDouble() * (max - min) + min, 8);
    }
}
