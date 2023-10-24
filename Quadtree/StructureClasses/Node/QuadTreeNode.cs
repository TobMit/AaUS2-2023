using System.Drawing;

namespace Quadtree.StructureClasses.Node;

/// <summary>
/// Abstraktna trieda pre uzly
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class QuadTreeNode<T>
{
    protected Point _pointDownLeft;
    public Point PointDownLeft
    {
        get => _pointDownLeft;
    }
    protected Point _pointUpRight;
    public Point PointUpRight
    {
        get => _pointUpRight;
    }
    
    public bool ContainsPoints(Point pPointDownLeft, Point pPointUpRight) {
        return PointDownLeft.X <= pPointDownLeft.X && PointDownLeft.Y <= pPointDownLeft.Y &&
               PointUpRight.X >= pPointUpRight.X && PointUpRight.Y >= pPointUpRight.Y;
    }
    
    public bool ContainNode(QuadTreeNode<T> nodeLeaf)
    {
        return ContainsPoints(nodeLeaf.PointDownLeft, nodeLeaf.PointUpRight);
    }
    
    public bool OverpalapPoints(Point pPointDownLeft, Point pPointUpRight) {
        return PointDownLeft.X < pPointUpRight.X && PointDownLeft.Y < pPointUpRight.Y &&
               PointUpRight.X > pPointDownLeft.X && PointUpRight.Y > pPointDownLeft.Y;
    }
    
    public bool OverlapNode(QuadTreeNode<T> nodeLeaf)
    {
        return OverpalapPoints(nodeLeaf.PointDownLeft, nodeLeaf.PointUpRight);
    }

    public bool HaveSamePoints(QuadTreeNode<T> node)
    {
        return node.PointDownLeft == PointDownLeft && node.PointUpRight == PointUpRight;
    }
}