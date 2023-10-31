using System.Drawing;
using Quadtree.StructureClasses.HelperClass;

namespace Quadtree.StructureClasses.Node;

/// <summary>
/// Trieda slúži na ukladanie dát do uzlov
/// </summary>
/// <typeparam name="T"></typeparam>
public class QuadTreeNodeData<T> : QuadTreeNode<T>
{
    public T Data { get; set; }
    
public QuadTreeNodeData(PointD pPointDownLeft, PointD pPointUpRight, T pData)
    {
        _pointDownLeft = pPointDownLeft;
        _pointUpRight = pPointUpRight;
        Data = pData;
    }
}