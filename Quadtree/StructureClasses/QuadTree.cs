using System.Drawing;
using System.Runtime.InteropServices.JavaScript;
using Quadtree.StructureClasses.Node;

namespace Quadtree.StructureClasses;

public class QuadTree<T>
{
    private QuadTreeNodeLeaf<T> root;
    private const int HODNOTA = 10000000;
    private const int MAX_DEPTH = 29; // je to kôli tomu že keď zoberiem max rozmer tak ho viem deliť iba 29 krát kým by som nedostal samé 1
    private int max_depth;
    public int Count { get; set; }

    /// <summary>
    /// Quad tree structure
    /// </summary>
    /// <param name="pX">down left point</param>
    /// <param name="pY">donw left point</param>
    /// <param name="width">expand to the right</param>
    /// <param name="height">expand to the up</param>
    /// <param name="pMaxDepth">max deepth of the tree</param>
    public QuadTree(double pX, double pY, double width, double height, int pMaxDepth)
    {
        if (!CheckCoordinates(QuadTreeRound(pX), QuadTreeRound(pY)) 
            || !CheckCoordinates(QuadTreeRound(pX + width), QuadTreeRound(pY + height)))
        {
            throw new Exception("Wrong world coordination");
        }

        if (pMaxDepth > MAX_DEPTH)
        {
            throw new Exception("Max depth is 29");
        }
        
        root = new(new(QuadTreeRound(pX), QuadTreeRound(pY)),
            new(QuadTreeRound(pX + width), QuadTreeRound(pY + height)));
        
        this.max_depth = pMaxDepth;
        Count = 0;
    }

    /// <summary>
    /// Insert do QaudTree struktur x ma obmedzenie &lt;-180.00000, 180.00000&gt; y ma obmedzenie &lt;-90.00000,90.00000&gt;
    /// </summary>
    /// <param name="pX"></param>
    /// <param name="pY"></param>
    /// <param name="pData"></param>
    /// <exception cref="Exception">Ak su zle suradnice</exception>
    public void Insert(double xDownLeft, double yDownLeft, double xUpRight, double yUpRight, T pData)
    {
        if (!root.ContainsPoints(new(QuadTreeRound(xDownLeft), QuadTreeRound(yDownLeft)), 
                new(QuadTreeRound(xUpRight), QuadTreeRound(yUpRight))))
        {
            throw new Exception("Coordinates exceed parameter size");
        }
        QuadTreeNodeLeaf<T>? current = root;
        QuadTreeNodeData<T> currentDataNode = new(new(QuadTreeRound(xDownLeft), QuadTreeRound(yDownLeft)), 
          new (QuadTreeRound(xUpRight), QuadTreeRound(yUpRight)), pData);
        
        int depth = 0;
        while (current is not null)
        {
            // pozrieme sa či sa nejaký polygón nachádza v danom uzle a nie sú listy tak to môžeme vložiť
            if (current.DataIsEmpty() && !current.LeafsInicialised)
            {
                current.AddData(currentDataNode);
                Count++;
                current = null;
            }
            // ak nie je prázdny
            //Skontrolujeme či nie je naplnená hlbka
            else if (depth == max_depth)
            {
                current.AddData(currentDataNode);
                Count++;
                current = null;
            }
            // môžu nastať 2 prípady
            // 1 skontrolujeme či sa objekt nezmestí do nejakého poduzla
            else if (current.AnySubNodeContainDataNode(currentDataNode))
            {
                // Ak sa zmestí tak current = poduzol
                var tmp = current.GetLeafThatCanContainDataNode(currentDataNode);
                if (tmp is null) throw new Exception("Error in QuadTree, this shouldnt happend");
                current = tmp;
                depth++;
            }
            // Ak sa nezmestí do žiadného poduzla tak skontrolujeme počet objektov v danom uzle
            else if (current.DataIsEmpty() || current.DataCount() > 1)
            {
                // ak ich tam je viack ako 1 alebo žiaden to znamená že objekty ktoré sú tam sa už nikde nedajú presunúť
                current.AddData(currentDataNode);
                Count++;
                current = null;
            }
            // Ak je tam jeden objekt tak 
            else
            {
                // skontrolujeme či tento objekt sa nezmesti do nejakého poduzla
                var tmpDataNode = current.GetData(0);
                if (current.AnySubNodeContainDataNode(tmpDataNode))
                {
                    // potom z daného objektu vymažeme nový objekt
                    current.RemoveData(tmpDataNode);
                    Count--;
                    // ak sa zmestí do niektorého poduzla tak aktuálny objekt vložíme
                    current.AddData(currentDataNode);
                    Count++;
                    //potom zmeníme premennu currentData na tento nový objekt
                    currentDataNode = tmpDataNode;
                    var tmp = current.GetLeafThatCanContainDataNode(currentDataNode);
                    if (tmp is null) throw new Exception("Error in QuadTree, this shouldnt happend");
                    // a current = poduzol do ktorého sa zmesti a pokračujeme v cykle
                    current = tmp;
                    depth++;
                }
                else
                {
                    // ak sa nezmesti nechámeho tam a pridáme aktuálny objekt k nemu
                    current.AddData(currentDataNode);
                    Count++;
                    current = null;
                }
            }
        }
        
    }

    public List<T> DeleteInterval(double xDownLeft, double yDownLeft, double xUpRight, double yUpRight)
    {
        return Interval(xDownLeft, yDownLeft, xUpRight, yUpRight, true);
    }
    
    public List<T> FindInterval(double xDownLeft, double yDownLeft, double xUpRight, double yUpRight)
    {
        return Interval(xDownLeft, yDownLeft, xUpRight, yUpRight, false);
    }
    
    private class RangeNodes
    {
        public QuadTreeNodeLeaf<T> Node { get; }
        /// <summary>
        /// <p> 0 - mod vyhľadavanie </p>
        /// <p> 1 - mod mazania </p>
        /// </summary>
        public int Mode { get; }
        public RangeNodes(QuadTreeNodeLeaf<T> pNode, int pMode)
        {
            Node = pNode;
            Mode = pMode;
        }
    }

    private List<T> Interval(double xDownLeft, double yDownLeft, double xUpRight, double yUpRight, bool delete)
    {
        QuadTreeNodeLeaf<T> areaToDelete = new(new(QuadTreeRound(xDownLeft), QuadTreeRound(yDownLeft)),
            new(QuadTreeRound(xUpRight), QuadTreeRound(yUpRight)));
        
        List<T> returnData = new();
        Stack<RangeNodes> stack = new();
        stack.Push(new(root, 0));
        while (stack.Count != 0)
        {
            // Prebieha v 2 režimoch
            var current = stack.Pop();
            if (current.Mode == 0)
            { 
                // 1. hľadanie poduzla do ktorého sa zmesti hladaná area
                if (current.Node.ContainNode(areaToDelete))
                {
                    // hľadanie prebieha tak že sa pozeráme ktorý uzol vie obsiahnuť vymazávanú areu a či aj potomkovia dokážu obsiahnuť hľadanú areu
                    if (current.Node.AnyInitSubNodeContainDataNode(areaToDelete))
                    {
                        // ak áno tak ten poduzol ktorý obsahuje náš objekt pridáme do stacku
                        var tmp = current.Node.GetLeafThatCanContainDataNode(areaToDelete);
                        if (tmp is null) throw new Exception("Error in QuadTree, this shouldnt happend");
                        stack.Push(new(tmp, 0));
                    
                    }
                    else
                    {
                        // ak nie tak pridáme do stacku všetky listy ktoré sa prekrývajú v rátane seba samáho na kontrolu dát
                        var tmp = current.Node.GetOverlapingLefs(areaToDelete);
                        foreach (var leaf in tmp)
                        {
                            stack.Push(new(leaf, 1));
                        }
                        stack.Push(new (current.Node, 1));
                    }
                }
            }
            else
            {
                // režim 2
                // 1. Skontrolujeme či sa nachádzajú nejaké dáta v danom uzle, ak áno tak ich spracujeme a pridáme do returnData
                    // nájdeme ich tak že skontrolujeme či sú v hľadanej arey
                List<T> tmpList = new();
                if (delete)
                {
                    tmpList = current.Node.RemoveDataInRange(areaToDelete);
                    Count -= tmpList.Count;
                }
                else
                {
                    tmpList = current.Node.GetDataInRange(areaToDelete);
                }
                returnData.AddRange(tmpList);
                // 2. skontrolujeme s ktorými potrvkami sa prekrýva vymazávaná area, alebo čiastočne prekrýva
                    // potomok ktorý sa prekrýva pridáme do staku
                var tmp = current.Node.GetOverlapingLefs(areaToDelete);
                foreach (var leaf in tmp)
                {
                    stack.Push(new(leaf, 1));
                }
            }
        }

        return returnData;
    }
    
    //Find Interval / Delete interval
    // flaga sa môže nachádzať v 2 režimoch
    // ak je nastavená na vyhľadávanie bodu
        // pozriem či sa hladaný objekt nachádza v danom uzle
        // ak áno tak skontrolujem či sa nenacháda hľadaný objekt/objekty v uložených dátach
        // ak áno tak ich pridám k vráteným objektom
        // skontrolujem či sa hladaný objekt nevie zmestiť do niektorého poduzla
        // ak áno ta current = poduzol
        // ak sa stane že nemáme už žiadné dáta a ani potomka tak vrátime do current = predka a flag označíme na vymazávanie nodu
    // ak je flag nastavený na vymazávanie nodu tak sa pozrieme či niektorý s potomkov má dáta
        // ak nemá dáta tak ich zmažeme a current = predok flaga ostáva označená na mazanie node
        // ak má dáta tak current = null
    
    

    /// <summary>
    /// Round data and decimal numbers for this structure
    /// </summary>
    /// <param name="value">double that will be rounded</param>
    /// <returns>rounded integer</returns>
    private static int QuadTreeRound(double value)
    {
        return (int)double.Round(value * HODNOTA, 5);
    }

    /// <summary>
    /// Check if the world coordinations are valid for this structure
    /// </summary>
    /// <returns>true if are valid, false if not</returns>
    private static bool CheckCoordinates(int x, int y)
    {
        if (x < -180 * HODNOTA || x > 180 * HODNOTA || y < -90 * HODNOTA || y > 90 * HODNOTA)
        {
            return false;
        }

        return true;
    }


    /// <summary>
    /// This is only for testing, after that will be removed
    /// </summary>
    public static int TestQuadTreeRound(double value)
    {
        return QuadTreeRound(value);
    }
    
    /// <summary>
    /// This is only for testing, after that will be removed
    /// </summary>
    public static bool TestCheckCoordinates(int x, int y)
    {
        return CheckCoordinates(x, y);
    }
}