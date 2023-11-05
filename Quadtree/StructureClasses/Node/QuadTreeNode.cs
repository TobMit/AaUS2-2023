using System.Drawing;
using Quadtree.StructureClasses.HelperClass;

namespace Quadtree.StructureClasses.Node;

/// <summary>
/// Abstraktná trieda pre uzly
/// </summary>
public abstract class QuadTreeNode<TKey, TValue> where TKey : IComparable<TKey> where TValue : IComparable<TKey>
{
    protected PointD _pointDownLeft;
    public PointD PointDownLeft
    {
        get => _pointDownLeft;
    }
    protected PointD _pointUpRight;
    public PointD PointUpRight
    {
        get => _pointUpRight;
    }
    
    /// <summary>
    /// Vráti true ak obsahuje v sebe zadané body
    /// </summary>
    public bool ContainsPoints(PointD pPointDownLeft, PointD pPointUpRight) {
        return PointDownLeft.X <= pPointDownLeft.X && PointDownLeft.Y <= pPointDownLeft.Y &&
                      PointUpRight.X >= pPointUpRight.X && PointUpRight.Y >= pPointUpRight.Y;
    }
    
    
    /// <summary>
    /// Vráti true ak v sebe obsahuje zadaný node
    /// </summary>
    public bool ContainNode(QuadTreeNode<TKey, TValue> nodeLeaf)
    {
        return ContainsPoints(nodeLeaf.PointDownLeft, nodeLeaf.PointUpRight);
    }
    
    /// <summary>
    /// Vráti true ak sa prekrýva s danou plochou
    /// </summary>
    public bool OverlappingPoints(PointD pPointDownLeft, PointD pPointUpRight) {
        return PointDownLeft.X < pPointUpRight.X && PointDownLeft.Y < pPointUpRight.Y &&
               PointUpRight.X > pPointDownLeft.X && PointUpRight.Y > pPointDownLeft.Y;
    }
    
    /// <summary>
    /// Vráti true ak sa prekrýva s danou plochou
    /// </summary>
    public bool OverlapNode(QuadTreeNode<TKey, TValue> nodeLeaf)
    {
        return OverlappingPoints(nodeLeaf.PointDownLeft, nodeLeaf.PointUpRight);
    }

    /// <summary>
    /// Vráti true ak sú body identické
    /// </summary>
    public bool HaveSamePoints(QuadTreeNode<TKey, TValue> node)
    {
        return node.PointDownLeft == PointDownLeft && node.PointUpRight == PointUpRight;
    }
}