using System.Drawing;
using Quadtree.StructureClasses.HelperClass;

namespace Quadtree.StructureClasses.Node;

/// <summary>
/// Trieda slúži na ukladanie dát do uzlov
/// </summary>
/// <typeparam name="T"></typeparam>
public class QuadTreeNodeData<TKey, TValue> : QuadTreeNode<TKey, TValue> where TKey : IComparable<TKey> where TValue : IComparable<TKey>
{
    public TValue Data { get; set; }
    
public QuadTreeNodeData(PointD pPointDownLeft, PointD pPointUpRight, TValue pData)
    {
        _pointDownLeft = pPointDownLeft;
        _pointUpRight = pPointUpRight;
        Data = pData;
    }
}