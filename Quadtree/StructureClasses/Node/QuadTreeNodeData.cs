using System.Drawing;

namespace Quadtree.StructureClasses.Node;

/// <summary>
/// Trieda slúži na ukladanie dát do uzlov
/// </summary>
/// <typeparam name="T"></typeparam>
public class QuadTreeNodeData<T> : QuadTreeNode<T>
{
    public T Data { get; set; }
    
public QuadTreeNodeData(Point pPointDownLeft, Point pPointUpRight, T pData)
    {
        _pointDownLeft = pPointDownLeft;
        _pointUpRight = pPointUpRight;
        Data = pData;
    }
}