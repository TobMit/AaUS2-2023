using System.Collections;
using System.Drawing;
using Quadtree.StructureClasses.HelperClass;

namespace Quadtree.StructureClasses.Node;

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
    private QuadTreeNodeLeaf<T>[] Leafs;

    private bool _leafsInicialised;
    public bool LeafsInicialised
    {
        get => _leafsInicialised;
    }

    public QuadTreeNodeLeaf<T> Parent { get; set; }


    public QuadTreeNodeLeaf(PointD pPointDownLeft, PointD pPointUpRight)
    {
        _pointDownLeft = pPointDownLeft;
        _pointUpRight = pPointUpRight;
        Leafs = new QuadTreeNodeLeaf<T>[4];
        data = new();
        _leafsInicialised = false;
    }
    
    public QuadTreeNodeLeaf(PointD pPointDownLeft, PointD pPointUpRight, QuadTreeNodeLeaf<T> parent)
    {
        _pointDownLeft = pPointDownLeft;
        _pointUpRight = pPointUpRight;
        Leafs = new QuadTreeNodeLeaf<T>[4];
        data = new();
        Parent = parent;
        _leafsInicialised = false;
    }
    
    public QuadTreeNodeLeaf(PointD pPointDownLeft, PointD pPointUpRight, QuadTreeNodeData<T> pData)
    {
        _pointDownLeft = pPointDownLeft;
        _pointUpRight = pPointUpRight;
        Leafs = new QuadTreeNodeLeaf<T>[4];
        data = new();
        data.Add(pData);
        _leafsInicialised = false;
    }
    
    public bool AnyInitSubNodeContainDataNode(QuadTreeNode<T> pData)
    {
        return AnySubNodeContainDataNode(pData, false);
    }
    
    public bool AnySubNodeContainDataNode(QuadTreeNode<T> pData)
    {
        return AnySubNodeContainDataNode(pData, true);
    }
    private bool AnySubNodeContainDataNode(QuadTreeNode<T> pData, bool inicialiseLeafs )
    {
        //todo add tests
        // môžu nastať 2 situácie
            // poduzol existuje a porovnáme do ktorého sa zmestí
        if (_leafsInicialised)
        {
            // skontroluj či sa zmetie s niektorým z listov
            foreach (QuadTreeNodeLeaf<T> leaf in Leafs)
            {
                if (leaf.ContainNode(pData))
                {
                    return true;
                }
            }
        }
            // poduzol neexistuje
        else
        {
            if (!inicialiseLeafs)
            {
                // ak nechceme inicializovať tak vrátime false
                return false;
            }
            
            // inicializuj tmp listy
            InitLeafs();
            // skontroluj či sa zmetie s niektorým z listov
            foreach (QuadTreeNodeLeaf<T> leaf in Leafs)
            {
                if (leaf.ContainNode(pData))
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Vráti list v ktorom sa zmestí daný hladany uzol, čo znamená že vráti vždy iba jeden
    /// </summary>
    /// <param name="pData"></param>
    /// <returns></returns>
    public QuadTreeNodeLeaf<T>? GetLeafThatCanContainDataNode(QuadTreeNode<T> pData)
    {
        foreach (QuadTreeNodeLeaf<T> leaf in Leafs)
        {
            if (leaf.ContainNode(pData))
            {
                return leaf;
            }
        }

        return null;
    }
    
    private void InitLeafs()
    {
        double x1 = _pointDownLeft.X;
        double y1 = _pointDownLeft.Y;
        double x2 = _pointUpRight.X;
        double y2 = _pointUpRight.Y;
        double xS = (x1 + x2) / 2;
        double yS = (y1 + y2) / 2;
        Leafs[0] = new(new(x1, y1), new(xS, yS), this);
        Leafs[1] = new(new(x1, yS), new(xS, y2), this);
        Leafs[2] = new(new(xS, yS), new(x2, y2), this);
        Leafs[3] = new(new(xS, y1), new(x2, yS), this);
        _leafsInicialised = true;
    }
    
    public List<QuadTreeNodeLeaf<T>> GetOverlapingLefs(QuadTreeNode<T> pData)
    {
        List<QuadTreeNodeLeaf<T>> returnList = new();
        if (_leafsInicialised)
        {
            foreach (QuadTreeNodeLeaf<T> leaf in Leafs)
            {
                if (leaf.OverlapNode(pData) || pData.OverlapNode(leaf) || leaf.ContainNode(pData) || pData.ContainNode(leaf))
                {
                    returnList.Add(leaf);
                }
            }
        }

        return returnList;
    }

    public List<T> GetOverlapingData(QuadTreeNode<T> pNode)
    {
        List<T> returnList = new();
        foreach (QuadTreeNodeData<T> dataNode in data)
        {
            if (dataNode.OverlapNode(pNode))
            {
                returnList.Add(dataNode.Data);
            }
        }

        return returnList;
    }

    public List<T> RemoveDataInRange(QuadTreeNode<T> node)
    {
        return DataInRange(node, true);
    }
    
    public List<T> GetDataInRange(QuadTreeNode<T> node)
    {
        return DataInRange(node, false);
    }
    
    private List<T> DataInRange(QuadTreeNode<T> node, bool removeData)
    {
        List<T> returnData = new();
        for (int i = 0; i < data.Count; i++)
        {
            if (node.ContainNode(data[i]))
            {
                returnData.Add(data[i].Data);
                if (removeData)
                {
                    data.RemoveAt(i);
                    i--;   
                }
            }
        }

        return returnData;
    }

    
    public List<T> RemoveDataWithSamePoints(QuadTreeNode<T> node)
    {
        return DataWithSamePoints(node, true);
    }
    
    public List<T> GetDataWithSamePoints(QuadTreeNode<T> node)
    {
        return DataWithSamePoints(node, false);
    }
    
    private List<T> DataWithSamePoints(QuadTreeNode<T> node, bool removeData)
    {
        List<T> returnData = new();
        for (int i = 0; i < data.Count; i++)
        {
            if (node.HaveSamePoints(data[i]))
            {
                returnData.Add(data[i].Data);
                if (removeData)
                {
                    data.RemoveAt(i);
                    i--;   
                }
            }
        }

        return returnData;
    }
    
    public bool CanBeRemoved()
    {
        
        if (!_leafsInicialised)
        {
            return true;
        }
        
        bool canBeRemoved = true;
        foreach (var leaf in Leafs)
        {
            canBeRemoved = canBeRemoved && !leaf.LeafsInicialised && leaf.DataIsEmpty();
        }

        if (canBeRemoved)
        {
            for (int i = 0; i < Leafs.Length; i++)
            {
                Leafs[i] = null;
            }

            _leafsInicialised = false;
        }

        return canBeRemoved;
    }
    
    public void AddData(QuadTreeNodeData<T> pdata)
    {
        if (!ContainNode(pdata))
        {
            throw new Exception("Data is not in this node. This shouldn't happened");
        }
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
    
    public void ClearData()
    {
        data.Clear();
    }

    /// <summary>
    /// Metóda slúži iba na testovanie, nevolajte ju v kódé, testuje správnu inicializáciu listov
    /// </summary>
    /// <returns>Listy či sú spravne inicializované</returns>
    public QuadTreeNodeLeaf<T>[] TestInitLeafs()
    {
        InitLeafs();
        return Leafs;
    }

    public QuadTreeNodeLeaf<T>[] TestGetLeafs()
    {
        return Leafs;
    }
    
}