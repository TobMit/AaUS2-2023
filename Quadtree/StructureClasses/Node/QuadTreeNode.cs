using System.Drawing;

namespace Quadtree.StructureClasses;

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
}