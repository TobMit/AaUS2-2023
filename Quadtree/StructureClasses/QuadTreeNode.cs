using System.Collections;
using System.Drawing;

namespace Quadtree.StructureClasses;

public class QuadTreeNode<T>
{
    public Point pointDownLeft { get; }
    public Point pointUpRight { get; }

    private ArrayList data;

    /**
     * Severo zapadny
     */
    public QuadTreeNode<T> NW { get; set; }

    /**
    * Severo vychodny
    */
    public QuadTreeNode<T> NE { get; set; }

    /**
    * Jouho zapadny
    */
    public QuadTreeNode<T> SW { get; set; }

    /**
    * Juho vychodny
    */
    public QuadTreeNode<T> SE { get; set; }

    private bool isLeaf;
    public bool IsLeaf { get => isLeaf; }

    public QuadTreeNode(Point pPointDownLeft, Point pPointUpRight)
    {
        pointDownLeft = pPointDownLeft;
        pointUpRight = pPointUpRight;
        isLeaf = true;
        data = new();
    }
    
    public QuadTreeNode(Point pPointDownLeft, Point pPointUpRight, T pData)
    {
        pointDownLeft = pPointDownLeft;
        pointUpRight = pPointUpRight;
        isLeaf = false;
        data = new();
        data.Add(pData);
    }
    
    public bool ContainsPoints(Point pPointDownLeft, Point pPointUpRight) {
        return pointDownLeft.X <= pPointDownLeft.X && pointDownLeft.Y <= pPointDownLeft.Y &&
               pointUpRight.X >= pPointUpRight.X && pointUpRight.Y >= pPointUpRight.Y;
    }

    public bool ContainNode(QuadTreeNode<T> node)
    {
        return ContainsPoints(node.pointDownLeft, node.pointUpRight);
    }

    public void AddData(T pdata)
    {
        data.Add(pdata);
    }
    
    public void AddData(ArrayList pdata)
    {
        data.AddRange(pdata);
    }
    
    public void RemoveData(T pdata)
    {
        data.Remove(pdata);
    }
    
    public int DataCount()
    {
        return data.Count;
    }

    public ArrayList GetArrayListData()
    {
        return data;
    }
    
    public T? GetData(int index)
    {
        return (T) data[index]!;
    }
    
    public bool dataIsEmpty()
    {
        return data.Count == 0;
    }
}