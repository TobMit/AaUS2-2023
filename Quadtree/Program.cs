using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using Quadtree.StructureClasses;
using Quadtree.StructureClasses.Node;

public class Program
{
    private static bool parallel = true; // pri paralelnom testovaní treba dať max seed na nejaké rozumné čislo (napr. 30)aby program skončil v nejakom rozumnom čase, keby bolo nejaké veľké čislo, tak sa NET snaží najskôr naplánovať všetky tasky a potom ich spúšťa, a to pri veľom množstve už trvá dlho.
    private static bool testForProfiler = false;
    
    private static int MAX_UNITS = 1000000;
    private static int MAX_TEST = 100000;
    private static double PROBABILITY_FD = 0.6;
    private static double PROBABILITY_FO_FR = 0.1;
    private static double PROBABILITY_DEPTH = 0.0001;
    private static double FILL_PROBABILITY = 0.3;
    private static double MAX_SIZE_OF_OBJCET_PERCENTAGE = 0.25;
    private static int STARTUP_FILL_COUNT = 11000;
    
    private static int OPERATION_COUNT = 10000;
    private static bool OPTIMALIZATION_ON = true;
        
    private static int lastestLovest = int.MaxValue;
    private static int seed = 0;
    private static int maxSeed = 20;
    // private static int maxSeed = int.MaxValue;
    public static void Main()
    {
        if (!testForProfiler)
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
        }
        else
        {
            ProfillerTest();
        }
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
            Math.Abs(MAX_X - MIN_X), Math.Abs(MAX_Y - MIN_Y));
        
        if (rnd.NextDouble() <= FILL_PROBABILITY)
        {
            for (int i = 0; i < STARTUP_FILL_COUNT; i++)
            {
                double x = NextDouble(MIN_X, MAX_X-2, rnd);
                double y = NextDouble(MIN_Y, MAX_Y-2, rnd);
                var tmpSirka = NextDouble(0, Math.Min(Math.Abs(MAX_X - MIN_X) * MAX_SIZE_OF_OBJCET_PERCENTAGE, MAX_X - 1 - x), rnd);
                var tmpViska = NextDouble(0, Math.Min(Math.Abs(MAX_Y - MIN_Y) * MAX_SIZE_OF_OBJCET_PERCENTAGE, MAX_Y - 1 - y), rnd);
                double x2 = x + tmpSirka;
                double y2 = y + tmpViska;
            
                int index = rnd.Next(0, toInsert.Count);
                int value = toInsert[index];
                toInsert.RemoveAt(index);
                toDelete.Add(new(new(x, y), 
                    new(x2, y2), value));
                    
                quadtree.Insert(x, y, 
                    x2, y2, value);
            }
        }
        
        for (int i = 0; i < MAX_TEST; i++)
        {
            double x = NextDouble(MIN_X, MAX_X-2, rnd);
            double y = NextDouble(MIN_Y, MAX_Y-2, rnd);
            var tmpSirka = NextDouble(0, Math.Min(Math.Abs(MAX_X - MIN_X) * MAX_SIZE_OF_OBJCET_PERCENTAGE, MAX_X - 1 - x), rnd);
            var tmpViska = NextDouble(0, Math.Min(Math.Abs(MAX_Y - MIN_Y) * MAX_SIZE_OF_OBJCET_PERCENTAGE, MAX_Y - 1 - y), rnd);
            double x2 = x + tmpSirka;
            double y2 = y + tmpViska;

            var rndValue = rnd.NextDouble();
            if (rndValue < PROBABILITY_DEPTH)
            {
                var newDepth = rnd.Next(2, 20);
                //Console.WriteLine("Set depth: " + newDepth + " in seed: " + seed + " operation count: " + i);
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
                    var deltedValue = quadtree.Delete(value.PointDownLeft.X, value.PointDownLeft.Y,
                        value.PointUpRight.X, value.PointUpRight.Y, value.Data);
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

    private static void ProfillerTest()
    {
        double MIN_X = 0;
        double MAX_X = 100;
        double MIN_Y = 0;
        double MAX_Y = 100;
        
        Random rnd = new Random(0);
        QuadTree<int, int> quadTree = new(0.0, 0.0, 100.0, 100.0);
        
        List<int> toInsert = new(MAX_UNITS);
        List<QuadTreeNodeData<int, int>> toDelete = new(MAX_UNITS);
        
        for (int i = 0; i < MAX_UNITS; i++)
        {
            toInsert.Add(i);
        }
        
        quadTree.OptimalizationOn = false;
        
        for (int i = 0; i < STARTUP_FILL_COUNT; i++)
        {
            double x = NextDouble(MIN_X, MAX_X-2, rnd);
            double y = NextDouble(MIN_Y, MAX_Y-2, rnd);
            var tmpSirka = NextDouble(0, Math.Min(Math.Abs(MAX_X - MIN_X) * MAX_SIZE_OF_OBJCET_PERCENTAGE, MAX_X - 1 - x), rnd);
            var tmpViska = NextDouble(0, Math.Min(Math.Abs(MAX_Y - MIN_Y) * MAX_SIZE_OF_OBJCET_PERCENTAGE, MAX_Y - 1 - y), rnd);
            double x2 = x + tmpSirka;
            double y2 = y + tmpViska;
            
            int index = rnd.Next(0, toInsert.Count);
            int value = toInsert[index];
            toInsert.RemoveAt(index);
            toDelete.Add(new(new(x, y), 
                new(x2, y2), value));
                    
            quadTree.Insert(x, y, x2, y2, value);
        }

        if (OPTIMALIZATION_ON)
        {
            quadTree.OptimalizationOn = true;
            quadTree.Optimalise(true);
        }
        
        
        MakeTest(MIN_X, MAX_X, rnd, MIN_Y, MAX_Y, toInsert, toDelete, quadTree);
        
        
    }

    private static void MakeTest(double MIN_X, double MAX_X, Random rnd, double MIN_Y, double MAX_Y, List<int> toInsert,
        List<QuadTreeNodeData<int, int>> toDelete, QuadTree<int, int> quadTree)
    {
        for (int i = 0; i < OPERATION_COUNT; i++)
        {
            double x = NextDouble(MIN_X, MAX_X - 2, rnd);
            double y = NextDouble(MIN_Y, MAX_Y - 2, rnd);
            var tmpSirka = NextDouble(0, Math.Min(Math.Abs(MAX_X - MIN_X) * MAX_SIZE_OF_OBJCET_PERCENTAGE, MAX_X - 1 - x),
                rnd);
            var tmpViska = NextDouble(0, Math.Min(Math.Abs(MAX_Y - MIN_Y) * MAX_SIZE_OF_OBJCET_PERCENTAGE, MAX_Y - 1 - y),
                rnd);
            double x2 = x + tmpSirka;
            double y2 = y + tmpViska;

            if (rnd.NextDouble() <= 0.5)
            {
                // ideme insertovať
                int index = rnd.Next(0, toInsert.Count);
                int value = toInsert[index];
                toInsert.RemoveAt(index);
                toDelete.Add(new(new(x, y),
                    new(x2, y2), value));

                quadTree.Insert(x, y,
                    x2, y2, value);
            }
            else
            {
                int index = rnd.Next(0, toDelete.Count);
                var value = toDelete[index];
                toDelete.RemoveAt(index);
                toInsert.Add(value.Data);
                var deltedValue = quadTree.Delete(value.PointDownLeft.X, value.PointDownLeft.Y,
                    value.PointUpRight.X, value.PointUpRight.Y, value.Data);
            }
        }
    }

    public static double NextDouble(double min, double max, Random rnd)
    {
        return Math.Round(rnd.NextDouble() * (max - min) + min, 8);
    }
}
