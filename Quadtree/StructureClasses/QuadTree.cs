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
    private const int OPERATION_TO_OPTIMALIZE = 100;
    private const int MIN_DATA_TO_OPTIMALIZE = 100;
    private const double OPTIMALIZE_RATIO = 0.4;
    private const double CHANGE_SIZE_RATIO = 0.3;
    private int _maxDepth;
    private int _operationCount;
    
    private bool _optimalize = true;
    
    public PointD OriginalPointDownLeft { get; set; }
    public PointD OriginalPointUpRight { get; set; }

    private QuadTreeNodeLeaf<int, int> _originalRoot;
    public int Count { get; set; }
    
    /// <summary>
    /// Health funkcie je v rozsahu (0,1) kde 0 je najhoršie a 1 je najlepšie
    /// </summary>
    public double Health { get; set; }

    public bool OptimalizationOn { get; set; }


    private int[] _quadrantCount = new int[4];

    /// <summary>
    /// Quad tree structure
    /// </summary>
    /// <param name="pX">down left point</param>
    /// <param name="pY">down left point</param>
    /// <param name="width">expand to the right</param>
    /// <param name="height">expand to the up</param>
    /// <param name="pMaxDepth">max depth of the tree</param>
    public QuadTree(double pX, double pY, double width, double height, int pMaxDepth)
    {
        if (pMaxDepth <= 0)
        {
            throw new Exception("Min depth is 1");
        }
        
        OriginalPointDownLeft = new(pX, pY);
        OriginalPointUpRight = new(pX + width, pY + height);
        
        _root = new(new(pX, pY),
            new(pX + width, pY + height));
        _originalRoot = new(new(pX, pY),
            new(pX + width, pY + height));

        _maxDepth = pMaxDepth;
        Count = 0;
        _operationCount = 0;
        OptimalizationOn = true;
    }
    
    /// <summary>
    /// Quad tree structure
    /// </summary>
    /// <param name="pX">down left point</param>
    /// <param name="pY">down left point</param>
    /// <param name="width">expand to the right</param>
    /// <param name="height">expand to the up</param>
    /// <param name="pMaxDepth">max depth of the tree</param>
    public QuadTree(double pX, double pY, double width, double height)
    {
        OriginalPointDownLeft = new(pX, pY);
        OriginalPointUpRight = new(pX + width, pY + height);
        
        _root = new(new(pX, pY),
            new(pX + width, pY + height));
        _originalRoot = new(new(pX, pY),
            new(pX + width, pY + height));

        _maxDepth = int.MaxValue;
        Count = 0;
        _operationCount = 0;
        OptimalizationOn = true;
    }

    /// <summary>
    /// Insert do QuadTree
    /// </summary>
    /// <exception cref="Exception">Ak su zle súradnice</exception>
    public void Insert(double xDownLeft, double yDownLeft, double xUpRight, double yUpRight, TValue pData)
    {
        if (!QuadTreeCanContain(xDownLeft, yDownLeft, xUpRight, yUpRight))
        {
            throw new Exception("Coordinates exceed parameter size");
        }
        QuadTreeNodeLeaf<TKey, TValue>? current = _root;
        QuadTreeNodeData<TKey, TValue> currentDataNode = new(new(xDownLeft, yDownLeft), 
            new (xUpRight, yUpRight), pData);
        
        // zozbieranie info pre optimalizáciu
        CalculateOptimalizationQuadrant(currentDataNode, false);
        
        Insert(current, currentDataNode, 0);
    }
    private void Insert(QuadTreeNodeLeaf<TKey, TValue> root, QuadTreeNodeData<TKey, TValue> data, int pDepth)
    {
        QuadTreeNodeLeaf<TKey, TValue>? current = root;
        QuadTreeNodeData<TKey, TValue> currentDataNode = data;
        
        int depth = pDepth;
        while (current is not null)
        {
            // pozrieme sa či sa nejaký polygón nachádza v danom uzle a nie sú listy tak to môžeme vložiť
            if (current.DataIsEmpty() && !current.LeafsInitialised)
            {
                current.AddData(currentDataNode);
                Count++;
                current = null;
            }
            // ak nie je prázdny
            //Skontrolujeme či nie je naplnená hĺbka
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
                if (tmp is null) throw new Exception("Error in QuadTree, this shouldn't happened");
                current = tmp;
                depth++;
            }
            // Ak sa nezmestí do žiadneho poduzla tak skontrolujeme počet objektov v danom uzle
            else if (current.DataIsEmpty() || current.DataCount() > 1)
            {
                // ak ich tam je viac ako 1 alebo žiaden to znamená že objekty ktoré sú tam sa už nikde nedajú presunúť
                current.AddData(currentDataNode);
                Count++;
                current = null;
            }
            // Ak je tam jeden objekt tak 
            else
            {
                // skontrolujeme či tento objekt sa nezmestí do nejakého poduzla
                var tmpDataNode = current.GetData(0);
                if (current.AnySubNodeContainDataNode(tmpDataNode))
                {
                    // potom z daného objektu vymažeme nový objekt
                    current.RemoveData(tmpDataNode);
                    Count--;
                    // ak sa zmestí do niektorého poduzla tak aktuálny objekt vložíme
                    current.AddData(currentDataNode);
                    Count++;
                    //potom zmeníme premenu currentData na tento nový objekt
                    currentDataNode = tmpDataNode;
                    var tmp = current.GetLeafThatCanContainDataNode(currentDataNode);
                    if (tmp is null) throw new Exception("Error in QuadTree, this shouldn't happened");
                    // a current = poduzol do ktorého sa zmesti a pokračujeme v cykle
                    current = tmp;
                    depth++;
                }
                else
                {
                    // ak sa nezmestí, necháme ho tam a pridáme aktuálny objekt k nemu
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
    
    private class WrapClass
    {
        public QuadTreeNodeLeaf<TKey, TValue>? Node { get; }
        /// <summary>
        /// <p> 0 - mod vyhľadávanie </p>
        /// <p> 1 - mod mazania </p>
        /// <p> Taktiež sa používa pri zmene výšky stromu na ručenie hĺbky</p>
        /// </summary>
        public int ModeOrDepth { get; }
        
        public bool LeafsAlreadyInStack { get; }
        public WrapClass(QuadTreeNodeLeaf<TKey, TValue> pNode, int modeOrDepth)
        {
            Node = pNode;
            ModeOrDepth = modeOrDepth;
            LeafsAlreadyInStack = false;
        }
        
        public WrapClass(QuadTreeNodeLeaf<TKey, TValue> pNode, int modeOrDepth, bool pLeafsAlreadyInStack)
        {
            Node = pNode;
            ModeOrDepth = modeOrDepth;
            LeafsAlreadyInStack = pLeafsAlreadyInStack;
        }
    }

    private List<TValue> Interval(double xDownLeft, double yDownLeft, double xUpRight, double yUpRight, bool delete)
    {
        QuadTreeNodeLeaf<TKey, TValue> areaToFind = new(new(xDownLeft, yDownLeft),
            new(xUpRight, yUpRight));
        
        List<TValue> returnData = new();
        Stack<WrapClass> stack = new();
        stack.Push(new(_root, 0));
        while (stack.Count != 0)
        {
            // Prebieha v 2 režimoch
            var current = stack.Pop();
            if (current.ModeOrDepth == 0)
            { 
                // 1. hľadanie poduzla do ktorého sa zmesti hľadaná area
                if (current.Node.OverlapNode(areaToFind))
                {
                    // hľadanie prebieha tak že sa pozeráme ktorý uzol vie obsiahnuť vymazávanú areu a či aj potomkovia dokážu obsiahnuť hľadanú areu
                    if (current.Node.AnyInitSubNodeContainDataNode(areaToFind))
                    {
                        // ak áno tak ten poduzol ktorý obsahuje náš objekt pridáme do staku
                        var tmp = current.Node.GetLeafThatCanContainDataNode(areaToFind);
                        if (tmp is null) throw new Exception("Error in QuadTree, this shouldn't happened");
                        stack.Push(new(tmp, 0));
                    
                    }
                    else
                    {
                        // ak nie tak pridáme do staku všetky listy ktoré sa prekrývajú vrátane seba samého na kontrolu dát
                        var tmp = current.Node.GetOverlappingLeafs(areaToFind);
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
                    // 2. skontrolujeme s ktorými prvkami sa prekrýva vymazávaná area, alebo čiastočne prekrýva
                        // potomok ktorý sa prekrýva pridáme do staku
                    var tmp = current.Node.GetOverlappingLeafs(areaToFind);
                    foreach (var leaf in tmp)
                    {
                        stack.Push(new(leaf, 1));
                    }
                }
            }
        }
        
        // zbieranie info pre optimalizáciu
        if (delete && returnData.Count != 0)
        {
            // prechádza sa to v cykle keďže môže nastať situácia že sa vymaže viac objektov
            // to je iba v takom prípade ak je kľúč null inak je pole o dĺžky 1
            foreach (var data in returnData)
            {
                CalculateOptimalizationQuadrant(areaToFind, true);
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
        // Flag = 1 tak mažeme
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
                // pozriem či sa hľadaný objekt nachádza v danom uzle
                if (current.ContainNode(objectToFind))
                {
                    // ak áno tak skontrolujem či sa nenachádza hľadaný objekt/objekty v uložených dátach
                    // ak áno tak ich pridám k vráteným objektom
                    if (delete)
                    {
                        if (key is not null)
                        {
                            var tmpData = current.RemoveDataWithSamePointsAndKey(objectToFind, key);
                            Count -= tmpData.Count;
                            returnData.AddRange(tmpData);
                            // ak sa nič nenašlo tak pokračujeme v prehľadávaní dalej, ak sa našlo tak current = null
                            if (tmpData.Count != 0)
                            {
                                current = null;   
                            }
                        }
                        // ak by bol kľúč null tak sa maže iba podľa súradníc, toto je optional funkcionalita
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
                        // skontrolujem či sa hladný objekt nevie zmestiť do niektorého poduzla
                        if (current.AnyInitSubNodeContainDataNode(objectToFind))
                        {
                            // ak áno ta current = poduzol
                            current = current.GetLeafThatCanContainDataNode(objectToFind);
                        }
                        // ak sa stane že nemáme už žiadané dáta a ani potomka tak vrátime do current = predka a flag označíme na vymazávanie nodu
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
        
        // zbieranie info pre optimalizáciu
        if (delete && returnData.Count != 0)
        {
            // prechádza sa to v cykle keďže môže nastať situácia že sa vymaže viac objektov
            // to je iba v takom prípade ak je kľúč null inak je pole o dĺžky 1
            foreach (var data in returnData)
            {
                CalculateOptimalizationQuadrant(objectToFind, true);
            }
        }

        if (_optimalize && delete)
        {
            Optimalise();
        }
        
        return returnData;
    }

    /// <summary>
    /// Vyhľadanie dát ktoré sa zo zadanou oblasťou aspoň čiastočne prekrývajú
    /// </summary>
    /// <returns>Všetky dáta ktoré do danej oblasti aspoň čiastočne zasahujú</returns>
    public List<TValue> FindIntervalOverlapping(double xDownLeft, double yDownLeft, double xUpRight, double yUpRight)
    {
        QuadTreeNodeLeaf<TKey, TValue> objectToFind = new(new(xDownLeft, yDownLeft),
            new(xUpRight, yUpRight));
        
        List<TValue> returnData = new();
        
        Stack<QuadTreeNodeLeaf<TKey, TValue>> stack = new();
        // pridáme do staku root
        stack.Push(_root);
        
        while (stack.Count!= 0)
        {
            var current = stack.Pop();
            // 1. skontrolujeme či na naše dva objekty prekrývajú
            if (current.OverlapNode(objectToFind) || objectToFind.OverlapNode(current))
            {
                // pridáme dáta ktoré sa prekrývajú
                // do staku pridáme listy ktoré sa prekrývajú
                returnData.AddRange(current.GetOverlappingData(objectToFind));
                
                // do staku pridáme listy ktoré sa prekrývajú
                var tmpLeafs = current.GetOverlappingLeafs(objectToFind);
                foreach (var leaf in tmpLeafs)
                {
                    stack.Push(leaf);
                }
            }
        }

        return returnData;
    }
    
    /// <summary>
    /// Optimalizácia stromu ktorá zväčšuje alebo zmenšuje strom na tú stranu na ktorej sa nachádza viac dát. Tým sa zaručí že najväčší zhluk dát bude v strede alebo blízko jeho okolia
    /// </summary>
    /// <param name="force">Ak je nastavené na true tak sa obídu všetky zábrany spúšťania optimalizácia po každej vykonanej operácií, defaultne je nastavené na false</param>
    public void Optimalise(bool force = false)
    {
        _operationCount++;
        // Optimalizáciu spúšťam až keď tam je apoň 1000 dát
        if (!force)
        {
            if (Count < MIN_DATA_TO_OPTIMALIZE)
            {
                return;
            }
        }
        
        // spočítame dáta v 4 quadrantoch rootu
        double x1 = _root.PointDownLeft.X;
        double y1 = _root.PointDownLeft.Y;
        double x2 = _root.PointUpRight.X;
        double y2 = _root.PointUpRight.Y;
        int first = _quadrantCount[0];
        int second = _quadrantCount[1];
        int third = _quadrantCount[2];
        int forth = _quadrantCount[3];
        
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
        
        if (!force)
        {
            if (_operationCount < OPERATION_TO_OPTIMALIZE)
            {
                return;
            }
        }
        
        
        var newRoot = false;
        
        if (rozdielSJ is > OPTIMALIZE_RATIO or < -OPTIMALIZE_RATIO)
        {
            newRoot = true;
            
            // ak je sever menší ako juh posunieme Pravé horné y dole, môže ale nastať situácia že y by bolo menšie ako pôvodné
            // ale musíme dbať na to aby sa neprekročili max hranice štruktúry
            // tým pádom nastavíme pôvodné Y a posunieme pravé dolné y smerom dolu
            
            // takže ak je percentuálne obsadenie severu a juhu väčšie ako 40% tak pravé horné y posunieme o 10 % hore
            if (rozdielSJ > 0)
            {
                // idem stred posúvať hore
                var tmpY1 = y2 - (vyska * (1 - CHANGE_SIZE_RATIO));
                var tmpY2 = y1 + (vyska * (1 + CHANGE_SIZE_RATIO));
                // skontrolujem či som náhodou niekedy pred tým neposúval spodnú hranicu dole
                if (Math.Abs(y1 - OriginalPointDownLeft.Y) > Double.Epsilon)
                {
                    // ak áno posuniem spodnú hranicu hore a ak je validná tak ju priradím ak nie tak posuniem hornú hranicu
                    if (tmpY1 <= OriginalPointDownLeft.Y)
                    {
                        newY1 = tmpY1;
                    }
                    else
                    {
                        newY2 = tmpY2;
                    } 
                }
                // ak nie tak posuniem hranicu hore
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
                    // ak áno posuniem spodnú hornú hore a ak je validná tak ju priradím ak nie tak posuniem dolnú hranicu
                    if (tmpY2 >= OriginalPointUpRight.Y)
                    {
                        newY2 = tmpY2;
                    }
                    else
                    {
                        newY1 = tmpY1;
                    } 
                }
                // ak nie tak posuniem hranicu hore
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
                // idem stred posúvať doprava
                var tmpX1 = x2 - (sirka * (1 - CHANGE_SIZE_RATIO));
                var tmpX2 = x1 + (sirka * (1 + CHANGE_SIZE_RATIO));
                // skontrolujem či som náhodou niekedy pred tým neposúval spodnú hranicu dole
                if (Math.Abs(x1 - OriginalPointDownLeft.X) > Double.Epsilon)
                {
                    // ak áno posuniem spodnú hranicu v pravo a ak je validná tak ju priradím ak nie tak posuniem hornú hranicu
                    if (tmpX1 <= OriginalPointDownLeft.X)
                    {
                        newX1 = tmpX1;
                    }
                    else
                    {
                        newX2 = tmpX2;
                    } 
                }
                // ak nie tak posuniem hornú hranicu v pravo
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
                // skontrolujem či som náhodou niekedy pred tým neposúval hornú hranicu hranicu v ľavo
                if (Math.Abs(x2 - OriginalPointUpRight.X) > Double.Epsilon)
                {
                    // ak áno posuniem spodnú hornú v ľavo a ak je validná tak ju priradím ak nie tak posuniem dolnú hranicu
                    if (tmpX2 >= OriginalPointUpRight.X)
                    {
                        newX2 = tmpX2;
                    }
                    else
                    {
                        newX1 = tmpX1;
                    } 
                }
                // ak nie tak posuniem hranicu v ľavo
                else
                {
                    newX1 = tmpX1;
                }
            }
        }

        if (!force)
        {
            if (_operationCount < OPERATION_TO_OPTIMALIZE)
            {
                return;
            }
        }
        _operationCount = 0;

        // ak sa root nezmenil nemá zmysel pokračovať
        if (!newRoot)
        {
            return;
        }
        
        // ak nie je optimalizácia zapnutá tak nepokračujem
        if (!force)
        {
            if (!OptimalizationOn)
            {
                return;
            }
        }

        List<QuadTreeNodeData<TKey, TValue>> tmpData = new();
        Stack<QuadTreeNodeLeaf<TKey, TValue>> stack = new();
        stack.Push(_root);
        while (stack.Count != 0)
        {
            var current = stack.Pop();
            tmpData.AddRange(current.GetArrayListData());
            if (current.LeafsInitialised)
            {
                var tmpLeafs = current.GetLeafs();
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
        
        _quadrantCount = new int[4];
        
        foreach (var data in tmpData)
        {
            // znovu zozbieram data pre budúcu optimalizáciu
            CalculateOptimalizationQuadrant(data, false);
            
            Insert(data.PointDownLeft.X, data.PointDownLeft.Y, data.PointUpRight.X,
                data.PointUpRight.Y, data.Data);
        }

        _optimalize = true;
        
        // Znovu spočítame zdravie 
        // najskôr sa zaktualizujú premenné
        first = _quadrantCount[0];
        second = _quadrantCount[1];
        third = _quadrantCount[2];
        forth = _quadrantCount[3];
        sever = second + third;
        juh = first + forth;
        vychod = third + forth;
        zapad = first + second;
        
        percentoSever = sever/ (sever + juh);
        percentoJuh = 1 - percentoSever;
        percentoVychod = vychod / (vychod + zapad);
        percentoZapad = 1 - percentoVychod;

        rozdielSJ = percentoSever - percentoJuh;
        rozdielVZ = percentoVychod - percentoZapad;
        
        // vypočítame zdravie
        Health = 1 - (Math.Abs(rozdielSJ) + Math.Abs(rozdielVZ)) / 2;
    }

    /// <summary>
    /// Zmení aktuálnu hĺbku stromu na novú zadanú
    /// </summary>
    /// <param name="newDepth"> novej hĺbky stromu</param>
    /// <exception cref="Exception"> ak je hĺbka stromu zadaná nesprávna</exception>
    public void SetQuadTreeDepth(int newDepth)
    {
        
        if (newDepth <= 0)
        {
            throw new Exception("Min depth is 1");
        }

        // ak sa hĺbka nezmenila tak nebudem pokračovať
        if (newDepth == _maxDepth)
        {
            return;
        }

        _maxDepth = newDepth;
        
        Stack<WrapClass> stack = new();
        stack.Push(new(_root, 0));

        _optimalize = false;

        while (stack.Count != 0) 
        {
            // vytiahneme zo staku node ktorý je obalený v pomocnej triede
            var current = stack.Pop();
            
            // skontrolujem či nie je null
            if (current.Node is not null)
            {
                // pozrieme sa či má inicializované listy
                if (current.Node.LeafsInitialised)
                {
                    // skontrolujem hlbku
                    if (current.ModeOrDepth < _maxDepth)
                    {
                        // ak je hĺbka menšia vložím listy do staku a pokračujem v cykle
                        var tmpLeafs = current.Node.GetLeafs();
                        foreach (var leaf in tmpLeafs)
                        {
                            stack.Push(new(leaf, current.ModeOrDepth + 1));
                        }
                    }
                    else
                    {
                        // ak je hĺbka rovnaká alebo väčšia ako požadovaná tak skontrolujem či môžem zmazať leafs
                        if (!current.Node.CanLeafsBeRemoved())
                        {
                            // ak nemôžem tak ich pridáme do staku a pri ich vkladaní zvýšime im ich hĺbku
                            // keď som pridal tak môžem začať cyklus od znova
                            var tmpLeafs = current.Node.GetLeafs();
                            foreach (var leaf in tmpLeafs)
                            {
                                stack.Push(new(leaf, current.ModeOrDepth + 1));
                            }
                        }
                        else
                        {
                            // ak môžem tak ich vymažem a pridám parenta do staku
                            stack.Push(new(current.Node.Parent, current.ModeOrDepth - 1));
                            // Ak je hĺbka väčšia ako požadovaná tak dáta vložím do parenta
                            if (!current.Node.DataIsEmpty() && current.ModeOrDepth > _maxDepth)
                            {
                                current.Node.Parent.AddData(current.Node.GetArrayListData());
                                current.Node.ClearData();
                            }
                        } 
                        
                    }
                }
                else
                {
                    // ak nemá inicializované listy tak skontrolujem hĺbku
                    if (current.ModeOrDepth > _maxDepth)
                    {
                        // ak je hĺbka väčšia
                        if (!current.Node.DataIsEmpty())
                        {
                            // ak mám dáta, vložím ich k parentovi a u seba zmažem
                            current.Node.Parent.AddData(current.Node.GetArrayListData());
                            current.Node.ClearData();  
                        }
                        
                        // pozriem sa či parent môže zmazať listy
                        var parent = current.Node.Parent;
                        if (parent.CanLeafsBeRemoved())
                        {
                            stack.Push(new (parent, current.ModeOrDepth-1));
                        }
                        // ak nie tak pokračujem v cykle
                    }
                    else if (current.ModeOrDepth < _maxDepth)
                    {
                        // ak je menšia a ja mám dáta
                        // v cykle budem ich prechádzať a vkladať do listov pomocou importu
                        var tmpData = current.Node.GetArrayListData();
                        current.Node.ClearData();
                        foreach (var quadTreeNodeData in tmpData)
                        {
                            Count--; // toto je tu preto, lebo v inserte sa zvýši tak aby sme mali reálne počty dát
                            Insert(current.Node, quadTreeNodeData, current.ModeOrDepth);
                        }
                        
                        //môžem pokračovať ďalším prvkom z poľa
                    }
                    // ak je menšia a ja mám dáta
                }
                
            }
            // ak je null tak pokračujem v cykle
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
        // pridáme do staku root
        stack.Push(_root);
        while (stack.Count != 0)
        {
            var current = stack.Pop();
            newCount += current.DataCount();
            if (current.LeafsInitialised)
            {
                var tmpLeafs = current.GetLeafs();
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
        // pridáme do staku root
        stack.Push(_root);
        while (stack.Count != 0)
        {
            var current = stack.Pop();
            newList.AddRange(current.GetArrayListData().Select(data => data.Data));
            if (current.LeafsInitialised)
            {
                var tmpLeafs = current.GetLeafs();
                foreach (var leaf in tmpLeafs)
                {
                    stack.Push(leaf);
                }
            }
        }

        return newList;
    }

    /// <summary>
    /// Vypočítava naplnenie jednotlivých kvadrantov používaných v optimalizácii
    /// </summary>
    /// <param name="node"> node ktorý sa bude vkladať strome do niektorého z kvadrantov</param>
    /// <param name="delete"> ak True tak odpočítavame hodnoty v kvadrantoch inak pripočítavame hodnoty v kvadrantoch</param>
    private void CalculateOptimalizationQuadrant(QuadTreeNode<TKey, TValue> node, bool delete)
    {
        QuadTreeNodeLeaf<TKey, TValue> quadrantTree = new(new(_root.PointDownLeft.X, _root.PointDownLeft.Y),
            new(_root.PointUpRight.X, _root.PointUpRight.Y));
        var quadrantLeafs = quadrantTree.TestInitLeafs();
        for (int i = 0; i < quadrantLeafs.Length; i++)
        {
            if (quadrantLeafs[i].ContainNode(node))
            {
                if (delete)
                {
                    _quadrantCount[i]--;
                }
                else
                {
                    _quadrantCount[i]++;
                }
                return;
            }
        }
    }

    /// <summary>
    /// Skontroluje či sa vkladané dáta môžu vložiť do stromu
    /// </summary>
    /// <returns>true ak ano a false ak nie</returns>
    public bool QuadTreeCanContain(QuadTreeNode<TKey, TValue> node)
    {
        return QuadTreeCanContain(node.PointDownLeft.X, node.PointDownLeft.Y, node.PointUpRight.X, node.PointUpRight.Y);
    }

    /// <summary>
    /// Skontroluje či sa vkladané dáta môžu vložiť do stromu
    /// </summary>
    /// <returns>true ak ano a false ak nie</returns>
    public bool QuadTreeCanContain(double xDownLeft, double yDownLeft, double xUpRight, double yUpRight)
    {
        return _originalRoot.ContainsPoints(new(xDownLeft, yDownLeft),
            new(xUpRight, yUpRight));
    }

    /// <summary>
    /// Metoda vyhradená iba na testovanie!!!!
    /// </summary>
    /// <returns>Root</returns>
        public QuadTreeNodeLeaf<TKey, TValue> TestGetRoot()
    {
        return _root;
    }
}