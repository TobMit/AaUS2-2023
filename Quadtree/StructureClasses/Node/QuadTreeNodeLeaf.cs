using System.Collections;
using System.Drawing;
using Quadtree.StructureClasses.HelperClass;

namespace Quadtree.StructureClasses.Node;

/// <summary>
/// Trieda slúži na vytvorenie uzlov
/// </summary>
public class QuadTreeNodeLeaf<TKey, TValue> : QuadTreeNode<TKey, TValue> where TKey : IComparable<TKey> where TValue : IComparable<TKey>
{
    private List<QuadTreeNodeData<TKey, TValue>> data;

    /// <summary>
    /// <p> --------x </p>
    /// <p> | 2 | 3 | </p>
    /// <p> |-------| </p>
    /// <p> | 1 | 4 | </p>
    /// <p> x-------- </p>
    /// </summary>
    private QuadTreeNodeLeaf<TKey, TValue>[] Leafs;

    private bool _leafsInitialised;
    public bool LeafsInitialised
    {
        get => _leafsInitialised;
    }

    public QuadTreeNodeLeaf<TKey, TValue> Parent { get; }


    public QuadTreeNodeLeaf(PointD pPointDownLeft, PointD pPointUpRight)
    {
        _pointDownLeft = pPointDownLeft;
        _pointUpRight = pPointUpRight;
        Leafs = new QuadTreeNodeLeaf<TKey, TValue>[4];
        data = new();
        _leafsInitialised = false;
    }
    
    public QuadTreeNodeLeaf(PointD pPointDownLeft, PointD pPointUpRight, QuadTreeNodeLeaf<TKey, TValue> parent)
    {
        _pointDownLeft = pPointDownLeft;
        _pointUpRight = pPointUpRight;
        Leafs = new QuadTreeNodeLeaf<TKey, TValue>[4];
        data = new();
        Parent = parent;
        _leafsInitialised = false;
    }
    
    public QuadTreeNodeLeaf(PointD pPointDownLeft, PointD pPointUpRight, QuadTreeNodeData<TKey, TValue> pData)
    {
        _pointDownLeft = pPointDownLeft;
        _pointUpRight = pPointUpRight;
        Leafs = new QuadTreeNodeLeaf<TKey, TValue>[4];
        data = new();
        data.Add(pData);
        _leafsInitialised = false;
    }
    
    public bool AnyInitSubNodeContainDataNode(QuadTreeNode<TKey, TValue> pData)
    {
        return AnySubNodeContainDataNode(pData, false);
    }
    
    public bool AnySubNodeContainDataNode(QuadTreeNode<TKey, TValue> pData)
    {
        return AnySubNodeContainDataNode(pData, true);
    }
    private bool AnySubNodeContainDataNode(QuadTreeNode<TKey, TValue> pData, bool initialisedLeafs )
    {
        // môžu nastať 2 situácie
            // poduzol existuje a porovnáme do ktorého sa zmestí
        if (_leafsInitialised)
        {
            // skontroluj či sa zmetie s niektorým z listov
            foreach (QuadTreeNodeLeaf<TKey, TValue> leaf in Leafs)
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
            if (!initialisedLeafs)
            {
                // ak nechceme inicializovať tak vrátime false
                return false;
            }
            
            // inicializuj tmp listy
            InitLeafs();
            // skontroluj či sa zmetie s niektorým z listov
            foreach (QuadTreeNodeLeaf<TKey, TValue> leaf in Leafs)
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
    /// Vráti list v ktorom sa zmestí daný hľadaný uzol, čo znamená že vráti vždy iba jeden
    /// </summary>
    public QuadTreeNodeLeaf<TKey, TValue>? GetLeafThatCanContainDataNode(QuadTreeNode<TKey, TValue> pData)
    {
        foreach (QuadTreeNodeLeaf<TKey, TValue> leaf in Leafs)
        {
            if (leaf.ContainNode(pData))
            {
                return leaf;
            }
        }

        return null;
    }
    
    /// <summary>
    /// Vytvorí a nastaví listy
    /// </summary>
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
        _leafsInitialised = true;
    }
    
    public List<QuadTreeNodeLeaf<TKey, TValue>> GetOverlappingLeafs(QuadTreeNode<TKey, TValue> pData)
    {
        List<QuadTreeNodeLeaf<TKey, TValue>> returnList = new();
        if (_leafsInitialised)
        {
            foreach (QuadTreeNodeLeaf<TKey, TValue> leaf in Leafs)
            {
                if (leaf.OverlapNode(pData) || pData.OverlapNode(leaf) || leaf.ContainNode(pData) || pData.ContainNode(leaf))
                {
                    returnList.Add(leaf);
                }
            }
        }

        return returnList;
    }

    public List<TValue> GetOverlappingData(QuadTreeNode<TKey, TValue> pNode)
    {
        List<TValue> returnList = new();
        foreach (QuadTreeNodeData<TKey, TValue> dataNode in data)
        {
            if (dataNode.OverlapNode(pNode))
            {
                returnList.Add(dataNode.Data);
            }
        }

        return returnList;
    }

    public List<TValue> RemoveDataInRange(QuadTreeNode<TKey, TValue> node)
    {
        return DataInRange(node, true);
    }
    
    public List<TValue> GetDataInRange(QuadTreeNode<TKey, TValue> node)
    {
        return DataInRange(node, false);
    }
    
    private List<TValue> DataInRange(QuadTreeNode<TKey, TValue> node, bool removeData)
    {
        List<TValue> returnData = new();
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

    /// <summary>
    /// Zmaže konkrétny záznam ktorý má rovnaké súradnice ako daný uzol a kľúč, ak sa nachádza viacero záznamov tak vymaže iba prvý
    /// </summary>
    /// <param name="node">Node s ktorým porovnávam súradnice</param>
    /// <param name="key">Kúč ktorý rozhodne či bude dáta vymazané, ak má rovnaký kľúč a aj súradnice tak sa vymaže</param>
    /// <returns>Vymazané data</returns>
    public List<TValue> RemoveDataWithSamePointsAndKey(QuadTreeNode<TKey, TValue> node, TKey key)
    {
        List<TValue> returnData = new();
        for (int i = 0; i < data.Count; i++)
        {
            if (node.HaveSamePoints(data[i]) && data[i].Data.CompareTo(key) == 0)
            {
                returnData.Add(data[i].Data);
                data.RemoveAt(i);
                return returnData;
            }
        }

        return returnData;
    }
    
    public List<TValue> RemoveDataWithSamePoints(QuadTreeNode<TKey, TValue> node)
    {
        return DataWithSamePoints(node, true);
    }
    
    public List<TValue> GetDataWithSamePoints(QuadTreeNode<TKey, TValue> node)
    {
        return DataWithSamePoints(node, false);
    }
    
    private List<TValue> DataWithSamePoints(QuadTreeNode<TKey, TValue> node, bool removeData)
    {
        List<TValue> returnData = new();
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
    
    /// <summary>
    /// Ak môžu byť listy vymazané tak ich vymaže
    /// </summary>
    /// <returns>Vráti true ak listy môžu byť vymazané</returns>
    public bool CanLeafsBeRemoved()
    {
        // ak nie sú listy inicializované tak ich môžeme vymazať
        if (!_leafsInitialised)
        {
            return true;
        }
        
        // skontrolujeme či sa v listoch nachádzajú nejaké dáta a my nemáme žiadne dáta a ak sa nachádza práve 1 tak ich môžeme prehodiť k sebe
        if (DataIsEmpty() && _leafsInitialised)
        {
            int indexLeaf = 0;
            int dataCount = 0;
            
            bool hasLeafs = false;
            for (int i = 0; i < Leafs.Length; i++)
            {
                if (!Leafs[i].DataIsEmpty())
                {
                    indexLeaf = i;
                    dataCount += Leafs[i].DataCount();
                }
                
                // ak má inicializované listy tak nemôžeme mazať lebo by sme stratili dáta
                if (Leafs[i].LeafsInitialised)
                {
                    hasLeafs = true;
                }
            }

            if (dataCount == 1 && !hasLeafs)
            {
                data.Add(Leafs[indexLeaf].GetData(0));
                Leafs[indexLeaf].ClearData();
            }
        }
        
        // na záver skontrolujem či môžu byť listy vymazané, kontrola 2
        bool canBeRemoved = true;
        foreach (var leaf in Leafs)
        {
            canBeRemoved = canBeRemoved && !leaf.LeafsInitialised && leaf.DataIsEmpty();
        }

        if (canBeRemoved)
        {
            for (int i = 0; i < Leafs.Length; i++)
            {
                Leafs[i] = null;
            }

            _leafsInitialised = false;
        }

        return canBeRemoved;
    }
    
    public void AddData(QuadTreeNodeData<TKey, TValue> pdata)
    {
        if (!ContainNode(pdata))
        {
            throw new Exception("Data is not in this node. This shouldn't happened");
        }
        data.Add(pdata);
    }
    
    public void AddData(List<QuadTreeNodeData<TKey, TValue>> pdata)
    {
        data.AddRange(pdata);
    }
    
    public void RemoveData(QuadTreeNodeData<TKey, TValue> pdata)
    {
        data.Remove(pdata);
    }
    
    public int DataCount()
    {
        return data.Count;
    }

    public List<QuadTreeNodeData<TKey, TValue>> GetArrayListData()
    {
        return new(data);
    }
    
    public QuadTreeNodeData<TKey, TValue> GetData(int index)
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
    /// <returns>Listy či sú správne inicializované</returns>
    public QuadTreeNodeLeaf<TKey, TValue>[] TestInitLeafs()
    {
        InitLeafs();
        return Leafs;
    }

    /// <summary>
    /// Získame listy ktoré ktoré sú potrebné pri prechádzaní v stromu
    /// </summary>
    /// <returns>Pole listov, musí byť inicializované</returns>
    public QuadTreeNodeLeaf<TKey, TValue>[] GetLeafs()
    {
        return Leafs;
    }
    
}