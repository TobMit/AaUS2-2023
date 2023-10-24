using System;
using System.Collections.Generic;
using System.Drawing;
using Quadtree.StructureClasses;
using Quadtree.StructureClasses.Node;

public class Program
{
    public static void Main()
    {
        int MAX_UNITS = 10000000;
        int MAX_TEST = 100 ;
        double PROBABILITY = 0.6;
        int MAX_X = 180;
        int MIN_X = -180;
        int MAX_Y = 90;
        int MIN_Y = -90;

        int lastestLovest = int.MaxValue;
        int seed = 2;

        while (lastestLovest >= 30)
        {
            Random rnd = new Random(seed);
            List<int> toInsert = new(MAX_UNITS);
            List<QuadTreeNodeData<int>> toDelete = new(MAX_UNITS);

            for (int i = 0; i < MAX_UNITS; i++)
            {
                toInsert.Add(i);
            }

            QuadTree<int> quadtree = new QuadTree<int>(MIN_X, MIN_Y, 360, 180, 28);
            for (int i = 0; i < MAX_TEST; i++)
            {
                int x = rnd.Next(MIN_X, MAX_X);
                int y = rnd.Next(MIN_Y, MAX_Y);
                int x2 = rnd.Next(x, MAX_X);
                int y2 = rnd.Next(y, MAX_Y);

                // Console.WriteLine(quadtree.Count);


                if (rnd.NextDouble() < PROBABILITY)
                {
                    if (toInsert.Count > 0)
                    {
                        int index = rnd.Next(0, toInsert.Count);
                        int value = toInsert[index];
                        toInsert.RemoveAt(index);
                        toDelete.Add(new(new(x, y), new(x2, y2), value));
                        quadtree.Insert(x, y, x2, y2, value);
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
                            value.PointUpRight.X, value.PointUpRight.Y);
                        if (!deltedValue.Contains(value.Data))
                        {
                            //throw new Exception("Deleted value is not the same as inserted value");}}
                            lastestLovest = i;
                            Console.WriteLine();
                            Console.WriteLine("Lastest lowest: " + lastestLovest);
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
            Console.WriteLine("SEED: " + seed);
            seed++;
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

        // long hodnota =  90 * 10000000;
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
}
