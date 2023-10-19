using System.Collections;
using System.Drawing;
using Quadtree.StructureClasses.Node;

namespace Quadtree.StructureClasses;

/// <summary>
/// Trieda slúži na vytvorenie uzlov
/// </summary>
/// <typeparam name="T"></typeparam>
public class QuadTreeNodeLeaf<T> : QuadTreeNode<T>
{
    private List<QuadTreeNodeData<T>> data;

    /// <summary>
    /// <p> --------x </p>
    /// <p> | 2 | 3 | </p>
    /// <p> |-------| </p>
    /// <p> | 1 | 4 | </p>
    /// <p> x-------- </p>
    /// </summary>
    public QuadTreeNodeLeaf<T>[] Leafs;


    public QuadTreeNodeLeaf(Point pPointDownLeft, Point pPointUpRight)
    {
        _pointDownLeft = pPointDownLeft;
        _pointUpRight = pPointUpRight;
        Leafs = new QuadTreeNodeLeaf<T>[4];
        data = new();
    }
    
    public QuadTreeNodeLeaf(Point pPointDownLeft, Point pPointUpRight, QuadTreeNodeData<T> pData)
    {
        _pointDownLeft = pPointDownLeft;
        _pointUpRight = pPointUpRight;
        Leafs = new QuadTreeNodeLeaf<T>[4];
        data = new();
        data.Add(pData);
    }
    
    
    public void AddData(QuadTreeNodeData<T> pdata)
    {
        data.Add(pdata);
    }
    
    public void AddData(List<QuadTreeNodeData<T>> pdata)
    {
        data.AddRange(pdata);
    }
    
    public void RemoveData(QuadTreeNodeData<T> pdata)
    {
        data.Remove(pdata);
    }
    
    public int DataCount()
    {
        return data.Count;
    }

    public List<QuadTreeNodeData<T>> GetArrayListData()
    {
        return data;
    }
    
    public QuadTreeNodeData<T> GetData(int index)
    {
        return data[index]!;
    }
    
    public bool DataIsEmpty()
    {
        return data.Count == 0;
    }
}