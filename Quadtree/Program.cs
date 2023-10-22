using System;
using System.Collections.Generic;
using Quadtree.StructureClasses;

/*
public class QuadTreeNode<T>
{
    public double X { get; }
    public double Y { get; }
    public T Data { get; set; }
    public QuadTreeNode<T> NW { get; set; }
    public QuadTreeNode<T> NE { get; set; }
    public QuadTreeNode<T> SW { get; set; }
    public QuadTreeNode<T> SE { get; set; }

    public QuadTreeNode(double x, double y, T data)
    {
        X = x;
        Y = y;
        Data = data;
    }
}

public class QuadTree<T>
{
    private QuadTreeNode<T> root;
    private int capacity;

    public QuadTree(double x, double y, double width, double height, int capacity)
    {
        root = new QuadTreeNode<T>(x + width / 2, y + height / 2, default(T));
        this.capacity = capacity;
    }

    public void Insert(double x, double y, T data)
{
    QuadTreeNode<T> current = root;
    QuadTreeNode<T> parent = null;
    bool isLeft = false;
    bool isAbove = false;

    while (current != null)
    {
        if (Math.Abs(current.X - x) < double.Epsilon && Math.Abs(current.Y - y) < double.Epsilon)
        {
            current.Data = data; // Update the data
            return;
        }

        parent = current;
        isLeft = x < current.X;
        isAbove = y < current.Y;

        if (isLeft)
        {
            if (isAbove)
            {
                if (current.NW == null)
                {
                    current.NW = new QuadTreeNode<T>(current.X - Math.Abs(current.X / 2), current.Y - Math.Abs(current.Y / 2), data);
                    return;
                }
                current = current.NW;
            }
            else
            {
                if (current.SW == null)
                {
                    current.SW = new QuadTreeNode<T>(current.X - Math.Abs(current.X / 2), current.Y + Math.Abs(current.Y / 2), data);
                    return;
                }
                current = current.SW;
            }
        }
        else
        {
            if (isAbove)
            {
                if (current.NE == null)
                {
                    current.NE = new QuadTreeNode<T>(current.X + Math.Abs(current.X / 2), current.Y - Math.Abs(current.Y / 2), data);
                    return;
                }
                current = current.NE;
            }
            else
            {
                if (current.SE == null)
                {
                    current.SE = new QuadTreeNode<T>(current.X + Math.Abs(current.X / 2), current.Y + Math.Abs(current.Y / 2), data);
                    return;
                }
                current = current.SE;
            }
        }
    }

    // If we reach this point, a new node is created with the provided data
    parent = new QuadTreeNode<T>(x, y, data);
    if (x < root.X)
    {
        if (y < root.Y)
            root.NW = parent;
        else
            root.SW = parent;
    }
    else
    {
        if (y < root.Y)
            root.NE = parent;
        else
            root.SE = parent;
    }
}


    public void InsertPolygon(List<(double, double)> polygon, T data)
    {
        // Insert each vertex of the polygon into the quadtree
        foreach (var vertex in polygon)
        {
            Insert(vertex.Item1, vertex.Item2, data);
        }
    }

    public bool Search(double x, double y)
    {
        QuadTreeNode<T> current = root;
        while (current != null)
        {
            if (Math.Abs(current.X - x) < double.Epsilon && Math.Abs(current.Y - y) < double.Epsilon)
                return true;

            if (x < current.X)
            {
                if (y < current.Y)
                    current = current.NW;
                else
                    current = current.SW;
            }
            else
            {
                if (y < current.Y)
                    current = current.NE;
                else
                    current = current.SE;
            }
        }

        return false; // Not found
    }


    public void Delete(double x, double y)
    {
        var stack = new Stack<QuadTreeNode<T>>();
        stack.Push(root);
        QuadTreeNode<T> parent = null;
        QuadTreeNode<T> current = root;
        bool isLeft = false;
        bool isAbove = false;

        while (current != null)
        {
            if (Math.Abs(current.X - x) < double.Epsilon && Math.Abs(current.Y - y) < double.Epsilon)
            {
                // Found the node to delete
                if (parent != null)
                {
                    if (isLeft)
                    {
                        if (isAbove)
                            parent.NW = null;
                        else
                            parent.SW = null;
                    }
                    else
                    {
                        if (isAbove)
                            parent.NE = null;
                        else
                            parent.SE = null;
                    }
                }
                return;
            }

            parent = current;
            isLeft = x < current.X;
            isAbove = y < current.Y;

            if (isLeft)
            {
                if (isAbove)
                    current = parent.NW;
                else
                    current = parent.SW;
            }
            else
            {
                if (isAbove)
                    current = parent.NE;
                else
                    current = parent.SE;
            }

            if (current != null)
            {
                stack.Push(current);
            }
            else
            {
                // Not found, exit the loop
                break;
            }
        }
    }

    public List<(double, double, T)> QueryRange(double x, double y, double radius)
    {
        var result = new List<(double, double, T)>();
        QueryRange(root, x, y, radius, result);
        return result;
    }

    private void QueryRange(QuadTreeNode<T> node, double x, double y, double radius, List<(double, double, T)> result)
    {
        var stack = new Stack<QuadTreeNode<T>>();
        stack.Push(node);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (current == null)
                continue;

            double halfWidth = Math.Abs(current.X - current.NE.X) / 2;
            double halfHeight = Math.Abs(current.Y - current.NE.Y) / 2;

            bool inRangeX = Math.Abs(current.X - x) < halfWidth + radius;
            bool inRangeY = Math.Abs(current.Y - y) < halfHeight + radius;

            if (inRangeX && inRangeY)
            {
                if (Math.Sqrt(Math.Pow(current.X - x, 2) + Math.Pow(current.Y - y, 2)) <= radius)
                {
                    if (current.Data != null) // Check for null Data property
                        result.Add((current.X, current.Y, current.Data));
                }

                stack.Push(current.NW);
                stack.Push(current.NE);
                stack.Push(current.SW);
                stack.Push(current.SE);
            }
        }
    }



}
*/
public class Program
{
    public static void Main()
    {
        // QuadTree<string> quadtree = new QuadTree<string>(0, 0, 100, 100, 4);

        // Insert a polygon (a square in this example)
        // var polygon = new List<(double, double)>
        // {
            // (20, 20),
            // (30, 20),
            // (30, 30),
            // (20, 30)
        // };
        // quadtree.InsertPolygon(polygon, "Square");

        // Console.WriteLine("Search for (25, 25): " + quadtree.Search(25, 25));

        // You can also query for polygons within a range
        // List<(double, double, string)> polygonsInRange = quadtree.QueryRange(25, 25, 5);

        // Console.WriteLine("Polygons within range:");
        // foreach (var polygonInfo in polygonsInRange)
        // {
            // Console.WriteLine($"Center: ({polygonInfo.Item1}, {polygonInfo.Item2}), Data: {polygonInfo.Item3}");
        // }

        QuadTree<int> quadtree = new QuadTree<int>(-50, -50, 100, 100, 4);
        quadtree.Insert(-5.0, -5.0, -1, -1, 5);
        quadtree.Insert(-5.0, -5.0, -1, -1, 6);
        quadtree.Insert(-45.0, -45.0, 30, 30, 1);
        quadtree.Insert(-45.0, -45.0, -10, -10, 2);
        quadtree.Insert(-20.0, -20.0, -10, -10, 3);
        quadtree.Insert(-10.0, -10.0, -1, -1, 4);
        Console.WriteLine("Count pred delete: " + quadtree.Count);
        //var tmp = quadtree.Delete(-20.0, -20.0, -10, -10);
        var tmp = quadtree.Delete(-20.0, -20.0, -1, -1);
        Console.WriteLine("Count po delete: " + quadtree.Count);
        foreach (var i in tmp)
        {
            Console.WriteLine(i);
        }

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
