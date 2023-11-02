using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices.JavaScript;
using System.Security.Cryptography.X509Certificates;
using Quadtree.StructureClasses.HelperClass;
using Quadtree.StructureClasses.Node;

namespace Quadtree.StructureClasses;

public class QuadTree<TKey, TValue> where TKey : IComparable<TKey> where TValue : IComparable<TKey>
{
    private QuadTreeNodeLeaf<TKey, TValue> _root;
    //private const int HODNOTA = 10000000;
    //private const int MAX_DEPTH = 22; // je to kôli tomu že keď zoberiem max rozmer tak ho viem deliť iba 23 krát kým by som nedostal samé 1 a z bezpečnostných dôvodou iba 23
    private const int OPERATION_TO_OPTIMALIZE = 100;
    private const int MIN_DATA_TO_OPTIMALIZE = 1000;
    private const double OPTIMALIZE_RATIO = 0.6;
    private const double CHANGE_SIZE_RATIO = 0.2;
    // private const int MAX_X = 180;
    // private const int MAX_Y = 90;
    private int _maxDepth;
    private int _operationCount;
    
    private bool _optimalize = true;
    
    public PointD OriginalPointDownLeft { get; set; }
    public PointD OriginalPointUpRight { get; set; }
    public int Count { get; set; }
    
    /// <summary>
    /// Helath funkcie je v rozsahu (0,1) kde 0 je najhoršie a 1 je najlepšie
    /// </summary>
    public double Health { get; set; }

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
        // if (!CheckCoordinates(QuadTreeRound(pX), QuadTreeRound(pY)) 
        //     || !CheckCoordinates(QuadTreeRound(pX + width), QuadTreeRound(pY + height)))
        // {
        //     throw new Exception("Wrong world coordination");
        // }

        // if (pMaxDepth > MAX_DEPTH)
        // {
        //     throw new Exception("Max depth is 22");
        // }
        
        if (pMaxDepth <= 0)
        {
            throw new Exception("Min depth is 1");
        }
        
        OriginalPointDownLeft = new(pX, pY);
        OriginalPointUpRight = new(pX + width, pY + height);
        
        _root = new(new(pX, pY),
            new(pX + width, pY + height));
        
        _maxDepth = pMaxDepth;
        Count = 0;
        _operationCount = 0;
    }
    
    /// <summary>
    /// Quad tree structure
    /// </summary>
    /// <param name="pX">down left point</param>
    /// <param name="pY">donw left point</param>
    /// <param name="width">expand to the right</param>
    /// <param name="height">expand to the up</param>
    /// <param name="pMaxDepth">max deepth of the tree</param>
    public QuadTree(double pX, double pY, double width, double height)
    {
        // if (!CheckCoordinates(QuadTreeRound(pX), QuadTreeRound(pY)) 
        //     || !CheckCoordinates(QuadTreeRound(pX + width), QuadTreeRound(pY + height)))
        // {
        //     throw new Exception("Wrong world coordination");
        // }
        
        
        OriginalPointDownLeft = new(pX, pY);
        OriginalPointUpRight = new(pX + width, pY + height);
        
        _root = new(new(pX, pY),
            new(pX + width, pY + height));
        
        _maxDepth = int.MaxValue;
        Count = 0;
        _operationCount = 0;
    }

    /// <summary>
    /// Insert do QaudTree struktur x ma obmedzenie &lt;-180.00000, 180.00000&gt; y ma obmedzenie &lt;-90.00000,90.00000&gt;
    /// </summary>
    /// <param name="pX"></param>
    /// <param name="pY"></param>
    /// <param name="pData"></param>
    /// <exception cref="Exception">Ak su zle suradnice</exception>
    public void Insert(double xDownLeft, double yDownLeft, double xUpRight, double yUpRight, TValue pData)
    {
        if (!_root.ContainsPoints(new(xDownLeft, yDownLeft), 
                new(xUpRight, yUpRight)))
        {
            throw new Exception("Coordinates exceed parameter size");
        }
        QuadTreeNodeLeaf<TKey, TValue>? current = _root;
        QuadTreeNodeData<TKey, TValue> currentDataNode = new(new(xDownLeft, yDownLeft), 
          new (xUpRight, yUpRight), pData);
        
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
            else if (depth == _maxDepth)
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

        if (_optimalize)
        {
            Optimalise();
        }
        
    }

    public List<TValue> DeleteInterval(double xDownLeft, double yDownLeft, double xUpRight, double yUpRight)
    {
        return Interval(xDownLeft, yDownLeft, xUpRight, yUpRight, true);
    }
    
    public List<TValue> FindInterval(double xDownLeft, double yDownLeft, double xUpRight, double yUpRight)
    {
        return Interval(xDownLeft, yDownLeft, xUpRight, yUpRight, false);
    }
    
    private class RangeNodes
    {
        public QuadTreeNodeLeaf<TKey, TValue> Node { get; }
        /// <summary>
        /// <p> 0 - mod vyhľadavanie </p>
        /// <p> 1 - mod mazania </p>
        /// </summary>
        public int Mode { get; }
        
        public bool LeafsAlreadyInStack { get; }
        public RangeNodes(QuadTreeNodeLeaf<TKey, TValue> pNode, int pMode)
        {
            Node = pNode;
            Mode = pMode;
            LeafsAlreadyInStack = false;
        }
        
        public RangeNodes(QuadTreeNodeLeaf<TKey, TValue> pNode, int pMode, bool pLeafsAlreadyInStack)
        {
            Node = pNode;
            Mode = pMode;
            LeafsAlreadyInStack = pLeafsAlreadyInStack;
        }
    }

    private List<TValue> Interval(double xDownLeft, double yDownLeft, double xUpRight, double yUpRight, bool delete)
    {
        QuadTreeNodeLeaf<TKey, TValue> areaToFind = new(new(xDownLeft, yDownLeft),
            new(xUpRight, yUpRight));
        
        List<TValue> returnData = new();
        Stack<RangeNodes> stack = new();
        stack.Push(new(_root, 0));
        while (stack.Count != 0)
        {
            // Prebieha v 2 režimoch
            var current = stack.Pop();
            if (current.Mode == 0)
            { 
                // 1. hľadanie poduzla do ktorého sa zmesti hladaná area
                if (current.Node.ContainNode(areaToFind))
                {
                    // hľadanie prebieha tak že sa pozeráme ktorý uzol vie obsiahnuť vymazávanú areu a či aj potomkovia dokážu obsiahnuť hľadanú areu
                    if (current.Node.AnyInitSubNodeContainDataNode(areaToFind))
                    {
                        // ak áno tak ten poduzol ktorý obsahuje náš objekt pridáme do stacku
                        var tmp = current.Node.GetLeafThatCanContainDataNode(areaToFind);
                        if (tmp is null) throw new Exception("Error in QuadTree, this shouldnt happend");
                        stack.Push(new(tmp, 0));
                    
                    }
                    else
                    {
                        // ak nie tak pridáme do stacku všetky listy ktoré sa prekrývajú v rátane seba samáho na kontrolu dát
                        var tmp = current.Node.GetOverlapingLefs(areaToFind);
                        foreach (var leaf in tmp)
                        {
                            stack.Push(new(leaf, 1));
                        }
                        stack.Push(new (current.Node, 1, true));
                    }
                }
            }
            else
            {
                // režim 2
                // 1. Skontrolujeme či sa nachádzajú nejaké dáta v danom uzle, ak áno tak ich spracujeme a pridáme do returnData
                    // nájdeme ich tak že skontrolujeme či sú v hľadanej arey
                List<TValue> tmpList = new();
                if (delete)
                {
                    tmpList = current.Node.RemoveDataInRange(areaToFind);
                    Count -= tmpList.Count;
                }
                else
                {
                    tmpList = current.Node.GetDataInRange(areaToFind);
                }
                returnData.AddRange(tmpList);
                // ak sa už nachádzajú vo staku tak už ich nemusíme pridávať
                if (!current.LeafsAlreadyInStack)
                {
                    // 2. skontrolujeme s ktorými potrvkami sa prekrýva vymazávaná area, alebo čiastočne prekrýva
                        // potomok ktorý sa prekrýva pridáme do staku
                    var tmp = current.Node.GetOverlapingLefs(areaToFind);
                    foreach (var leaf in tmp)
                    {
                        stack.Push(new(leaf, 1));
                    }
                }
            }
        }

        if (_optimalize && delete)
        {
            Optimalise();
        }

        return returnData;
    }
    
    public List<TValue> Delete(double xDownLeft, double yDownLeft, double xUpRight, double yUpRight, TKey key)
    {
        return FindAndDelete(xDownLeft, yDownLeft, xUpRight, yUpRight, true, key);
    }
    
    public List<TValue> Find(double xDownLeft, double yDownLeft, double xUpRight, double yUpRight)
    {
        return FindAndDelete(xDownLeft, yDownLeft, xUpRight, yUpRight, false);
    }

    private List<TValue> FindAndDelete(double xDownLeft, double yDownLeft, double xUpRight, double yUpRight, bool delete, TKey? key = default )
    {
        // Flag = 0 tak vyhľadávame
        // Flag = 1 tak mazeme
        int flag = 0;
        QuadTreeNodeLeaf<TKey, TValue> objectToFind = new(new(xDownLeft, yDownLeft),
            new(xUpRight, yUpRight));
        
        QuadTreeNodeLeaf<TKey, TValue>? current;

        List<TValue> returnData = new();

        current = _root;
        while (current is not null)
        {
            // flaga sa môže nachádzať v 2 režimoch
            if (flag == 0)
            {
                // ak je nastavená na vyhľadávanie bodu
                // pozriem či sa hladaný objekt nachádza v danom uzle
                if (current.ContainNode(objectToFind))
                {
                    // ak áno tak skontrolujem či sa nenacháda hľadaný objekt/objekty v uložených dátach
                    // ak áno tak ich pridám k vráteným objektom
                    if (delete)
                    {
                        if (key is not null)
                        {
                            var tmpData = current.RemoveDataWithSamePointsAndKey(objectToFind, key);
                            Count -= tmpData.Count;
                            returnData.AddRange(tmpData);
                            // ak sa nič nenašlo tak pokračujeme v prehľadávani daľej, ak sa našlo tak current = null
                            if (tmpData.Count != 0)
                            {
                                current = null;   
                            }
                        }
                        // ak by bol kluč null tak sa maže iba podľa súradnic, toto je optional funkcionalita
                        else
                        {
                            var tmpData = current.RemoveDataWithSamePoints(objectToFind);
                            Count -= tmpData.Count;
                            returnData.AddRange(tmpData);
                            current = null;
                        }
                    }
                    else
                    {
                        returnData.AddRange(current.GetDataWithSamePoints(objectToFind));
                    }

                    // skontrolujem či current už medzi časom nie je null
                    if (current is not null)
                    {
                        // skontrolujem či sa hladaný objekt nevie zmestiť do niektorého poduzla
                        if (current.AnyInitSubNodeContainDataNode(objectToFind))
                        {
                            // ak áno ta current = poduzol
                            current = current.GetLeafThatCanContainDataNode(objectToFind);
                        }
                        // ak sa stane že nemáme už žiadné dáta a ani potomka tak vrátime do current = predka a flag označíme na vymazávanie nodu
                        else if (current.DataIsEmpty())
                        {
                            //current = current.Parent;
                            flag = 1;
                        }
                        else
                        {
                            current = null;
                        }
                    }
                }
                else
                {
                    //je to tu skôr pre istotu ak by sa stala niekde chyba a priradil by sa zlý leafs
                    current = null;
                }
            }
            else
            {
                // ak je flag nastavený na vymazávanie nodu tak sa pozrieme či niektorý s potomkov má dáta
                    // ak nemá dáta tak ich zmažeme a current = predok flaga ostáva označená na mazanie node
                    // ak má dáta tak current = null
                    // Poznámka: canBeRemoved vráti true aj ked current obsahuje data, keďže sa tento objekt zmaže až
                    // z predka tak nemusíme riešiť že current má data. Metóda CanBeRemoved nedovolí nás zmazať lebo máme dáta
                if (current.CanLeafsBeRemoved())
                {
                    current = current.Parent;
                }
                else
                {
                    current = null;
                }
            }
        }

        if (_optimalize && delete)
        {
            Optimalise();
        }
        
        return returnData;
    }

    public List<TValue> FindOverlapingData(double xDownLeft, double yDownLeft, double xUpRight, double yUpRight)
    {
        QuadTreeNodeLeaf<TKey, TValue> objectToFind = new(new(xDownLeft, yDownLeft),
            new(xUpRight, yUpRight));
        
        List<TValue> returnData = new();
        
        Stack<QuadTreeNodeLeaf<TKey, TValue>> stack = new();
        // prdidáme do stakú root
        stack.Push(_root);
        
        while (stack.Count!= 0)
        {
            var current = stack.Pop();
            // 1. skontrolujeme či na naše dva objekty prekrývajú
            if (current.OverlapNode(objectToFind) || objectToFind.OverlapNode(current))
            {
                // pridáme dáta ktoré sa prekrývajú
                // do staku pridáme listy ktoré sa prekrývajú
                returnData.AddRange(current.GetOverlapingData(objectToFind));
                
                // do staku pridáme listy ktoré sa prekrývajú
                var tmpLeafs = current.GetOverlapingLefs(objectToFind);
                foreach (var leaf in tmpLeafs)
                {
                    stack.Push(leaf);
                }
            }
        }

        return returnData;
    }
    
    public void Optimalise()
    {
        _operationCount++;
        // Optimalizáciu spúšťam až keď tam je apoň 1000 dát
        if (Count < MIN_DATA_TO_OPTIMALIZE)
        {
            return;
        }
        
        // spočítame dáta v 4 quadrantoch rootu
        double x1 = (double)_root.PointDownLeft.X;
        double y1 = (double)_root.PointDownLeft.Y;
        double x2 = (double)_root.PointUpRight.X;
        double y2 = (double)_root.PointUpRight.Y;
        double xS = (x1 + x2) / 2;
        double yS = (y1 + y2) / 2;
        //todo da sa to zýsakť aj pri inserte a delete
        int first = FindInterval(x1, y1, xS, yS).Count;
        int second = FindInterval(x1, yS, xS, y2).Count;
        int third = FindInterval(xS, yS, x2, y2).Count;
        int forth = FindInterval(xS, y1, x2, yS).Count;
        
        // spočítame koľko dát sa nachádza v Severnej časti a južnej časti, tak isto koľko sa nachádza vo východnej a západnej časti

        double sirka = Math.Abs(x2 - x1);
        double vyska = Math.Abs(y2 - y1);
        
        double sever = second + third;
        double juh = first + forth;
        double vychod = third + forth;
        double zapad = first + second;
        
        // vypočítame percentuálne obsadenie juhu a severu, východu a západu
        double percentoSever = sever/ (sever + juh);
        double percentoJuh = 1 - percentoSever;
        double percentoVychod = vychod / (vychod + zapad);
        double percentoZapad = 1 - percentoVychod;

        double rozdielSJ = percentoSever - percentoJuh;
        double rozdielVZ = percentoVychod - percentoZapad;
        
        // vypočítame zdravie
        Health = 1 - (Math.Abs(rozdielSJ) + Math.Abs(rozdielVZ)) / 2;

        double newX1 = x1;
        double newY1 = y1;
        double newX2 = x2;
        double newY2 = y2;
        
        // volanie počítanie zdravia
        if (_operationCount < OPERATION_TO_OPTIMALIZE)
        {
            return;
        }
        
        
        var newRoot = false;
        
        if (rozdielSJ is > OPTIMALIZE_RATIO or < -OPTIMALIZE_RATIO)
        {
            newRoot = true;
            
            // ak je sever menší ako juh posunieme Pravé horné y dole, môže ale nastať situácia že y by bolo menšie ako pôvodné
            // ale musíme dbať na to aby sa neprekročily max hranice štruktúry
            // tým pádom nastavýme pôvodné Y a posunieme pravé doľné y smerom dolu
            
            // takže ak je percentuálne obsadenie severu a juhu väčšie ako 40% tak pravé horné y posunieme o 10 % hore
            if (rozdielSJ > 0)
            {
                // idem stred posúvať hore
                var tmpY1 = y2 - (vyska * (1 - CHANGE_SIZE_RATIO));
                var tmpY2 = y1 + (vyska * (1 + CHANGE_SIZE_RATIO));
                // skontrolujem či som náhodov niekedy pred tým neposúval spodnú hranicu dole
                if (Math.Abs(y1 - OriginalPointDownLeft.Y) > Double.Epsilon)
                {
                    // ak áno posuniem spodnú hranicu hore a ak je validná tak ju priradím ak nie tak pousniem hornú hranicu
                    if (tmpY1 <= OriginalPointDownLeft.Y)
                    {
                        newY1 = tmpY1;
                    }
                    else
                    {
                        newY2 = tmpY2;
                    } 
                }
                // ak nie tak pousniem hranicu hore
                else
                {
                    newY2 = tmpY2;
                }
            }
            else
            {
                // idem stred posúvať dole
                var tmpY2 = y1 + (vyska * (1 - CHANGE_SIZE_RATIO));
                var tmpY1 = y2 - (vyska * (1 + CHANGE_SIZE_RATIO));
                // skontrolujem či som náhodov niekedy pred tým neposúval hornú hranicu hranicu hore
                if (Math.Abs(y2 - OriginalPointUpRight.Y) > Double.Epsilon)
                {
                    // ak áno posuniem spodnú hornú hore a ak je validná tak ju priradím ak nie tak pousniem dolnú hranicu
                    if (tmpY2 >= OriginalPointUpRight.Y)
                    {
                        newY2 = tmpY2;
                    }
                    else
                    {
                        newY1 = tmpY1;
                    } 
                }
                // ak nie tak pousniem hranicu hore
                else
                {
                    newY1 = tmpY1;
                }
            }
        } 

        if (rozdielVZ is > OPTIMALIZE_RATIO or < -OPTIMALIZE_RATIO)
        {
            newRoot = true;
            if (rozdielVZ > 0)
            {
                // idem stred posúvať do prava
                var tmpX1 = x2 - (sirka * (1 - CHANGE_SIZE_RATIO));
                var tmpX2 = x1 + (sirka * (1 + CHANGE_SIZE_RATIO));
                // skontrolujem či som náhodov niekedy pred tým neposúval spodnú hranicu dole
                if (Math.Abs(x1 - OriginalPointDownLeft.X) > Double.Epsilon)
                {
                    // ak áno posuniem spodnú hranicu v úravo a ak je validná tak ju priradím ak nie tak pousniem hornú hranicu
                    if (tmpX1 <= OriginalPointDownLeft.X)
                    {
                        newX1 = tmpX1;
                    }
                    else
                    {
                        newX2 = tmpX2;
                    } 
                }
                // ak nie tak pousniem hornú hranicu v pravo
                else
                {
                    newX2 = tmpX2;
                }
            }
            else
            {
                // idem stred posúvať do lava
                var tmpX2 = x1 + (sirka * (1 - CHANGE_SIZE_RATIO));
                var tmpX1 = x2 - (sirka * (1 + CHANGE_SIZE_RATIO));
                // skontrolujem či som náhodov niekedy pred tým neposúval hornú hranicu hranicu v ľavo
                if (Math.Abs(x2 - OriginalPointUpRight.X) > Double.Epsilon)
                {
                    // ak áno posuniem spodnú hornú v ľavo a ak je validná tak ju priradím ak nie tak pousniem dolnú hranicu
                    if (tmpX2 >= OriginalPointUpRight.X)
                    {
                        newX2 = tmpX2;
                    }
                    else
                    {
                        newX1 = tmpX1;
                    } 
                }
                // ak nie tak pousniem hranicu v ľavo
                else
                {
                    newX1 = tmpX1;
                }
            }
        }
        
        if (_operationCount < OPERATION_TO_OPTIMALIZE)
        {
            return;
        }
        _operationCount = 0;

        if (!newRoot)
        {
            return;
        }
        
        // skontrolujem či súradnice nepresialhly limity ak ano nebudm pokračovať
        // if (!CheckCoordinates(QuadTreeRound(newX1), QuadTreeRound(newY1)) 
        //     || !CheckCoordinates(QuadTreeRound(newX2), QuadTreeRound(newY2)))
        // {
        //     return;
        // }

        List<QuadTreeNodeData<TKey, TValue>> tmpData = new();
        Stack<QuadTreeNodeLeaf<TKey, TValue>> stack = new();
        stack.Push(_root);
        while (stack.Count != 0)
        {
            var current = stack.Pop();
            tmpData.AddRange(current.GetArrayListData());
            if (current.LeafsInicialised)
            {
                var tmpLeafs = current.TestGetLeafs();
                foreach (var leaf in tmpLeafs)
                {
                    stack.Push(leaf);
                }
            }
        }
        Count = 0;
        _root = new(new(newX1, newY1),
            new(newX2, newY2));
        _optimalize = false;
        foreach (var data in tmpData)
        {
            Insert(data.PointDownLeft.X, data.PointDownLeft.Y, data.PointUpRight.X,
                data.PointUpRight.Y, data.Data);
        }

        _optimalize = true;
    }

    /// <summary>
    /// Zmení aktuálnu hĺbku stromu na novú zadanú
    /// </summary>
    /// <param name="newDepth"> novej hlbky stromu</param>
    /// <exception cref="Exception"> ak je hĺbka stromu zadaná nesprávna</exception>
    public void SetQuadTreeDepth(int newDepth)
    {
        // if (newDepth > MAX_DEPTH)
        // {
        //     throw new Exception("Max depth is 28");
        // }
        if (newDepth <= 0)
        {
            throw new Exception("Min depth is 1");
        }

        // ak sa hlbka nezmenila tak nebudem pokračovať
        if (newDepth == _maxDepth)
        {
            return;
        }

        List<QuadTreeNodeData<TKey, TValue>> tmpData = new();
        Stack<QuadTreeNodeLeaf<TKey, TValue>> stack = new();
        // prdidáme do stakú root
        stack.Push(_root);
        while (stack.Count != 0)
        {
            var current = stack.Pop();
            tmpData.AddRange(current.GetArrayListData());
            if (current.LeafsInicialised)
            {
                var tmpLeafs = current.TestGetLeafs();
                foreach (var leaf in tmpLeafs)
                {
                    stack.Push(leaf);
                }
            }
        }
        _maxDepth = newDepth;
        Count = 0;
        _root = new(_root.PointDownLeft, _root.PointUpRight);
        _optimalize = false;
        foreach (var data in tmpData)
        {
            Insert(data.PointDownLeft.X, data.PointDownLeft.Y, data.PointUpRight.X,
                data.PointUpRight.Y, data.Data);
        }

        _optimalize = true;

    }
    
    /// <summary>
    /// Nanovo prepočíta všetky dáta ktoré sa tam nachádzajú
    /// </summary>
    /// <returns> Počet reálnych dát</returns>
    public int Recount()
    {
        int newCount = 0;
        Stack<QuadTreeNodeLeaf<TKey, TValue>> stack = new();
        // prdidáme do stakú root
        stack.Push(_root);
        while (stack.Count != 0)
        {
            var current = stack.Pop();
            newCount += current.DataCount();
            if (current.LeafsInicialised)
            {
                var tmpLeafs = current.TestGetLeafs();
                foreach (var leaf in tmpLeafs)
                {
                    stack.Push(leaf);
                }
            }
        }

        return newCount;
    }

    /// <summary>
    /// Transformuje QuadTree na List dát
    /// </summary>
    /// <returns>List dát ktoré sa nachádzajú v strome</returns>
    public List<TValue> ToList()
    {
        List<TValue> newList = new(Count);
        Stack<QuadTreeNodeLeaf<TKey, TValue>> stack = new();
        // prdidáme do stakú root
        stack.Push(_root);
        while (stack.Count != 0)
        {
            var current = stack.Pop();
            newList.AddRange(current.GetArrayListData().Select(data => data.Data));
            if (current.LeafsInicialised)
            {
                var tmpLeafs = current.TestGetLeafs();
                foreach (var leaf in tmpLeafs)
                {
                    stack.Push(leaf);
                }
            }
        }

        return newList;
    }
}