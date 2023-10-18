using System.Drawing;

namespace Quadtree.StructureClasses;

public class QuadTreeNode<T>
{
    public Point pointDownLeft { get; }
    public Point pointUpRight { get; }

    public T? Data { get; set; }

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
        Data = default;
        isLeaf = false;
    }
    
    public QuadTreeNode(Point pPointDownLeft, Point pPointUpRight, T pData)
    {
        pointDownLeft = pPointDownLeft;
        pointUpRight = pPointUpRight;
        isLeaf = true;
        Data = pData;
    }
    
    public bool containsPoints(Point pPointDownLeft, Point pPointUpRight) {
        return pointDownLeft.X <= pPointDownLeft.X && pointDownLeft.Y <= pPointDownLeft.Y &&
               pointUpRight.X >= pPointUpRight.X && pointUpRight.Y >= pPointUpRight.Y;
    }

    public bool containNode(QuadTreeNode<T> node)
    {
        return containsPoints(node.pointDownLeft, node.pointUpRight);
    }
}