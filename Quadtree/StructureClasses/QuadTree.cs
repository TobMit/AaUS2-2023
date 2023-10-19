using System.Drawing;
using System.Runtime.InteropServices.JavaScript;
using Quadtree.StructureClasses.Node;

namespace Quadtree.StructureClasses;

public class QuadTree<T>
{
    private QuadTreeNodeLeaf<T> root;
    private const int HODNOTA = 100000;
    private int max_depth;

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
        
        root = new(new(QuadTreeRound(pX), QuadTreeRound(pY)),
            new(QuadTreeRound(pX + width), QuadTreeRound(pY + height)));
        
        this.max_depth = pMaxDepth;
    }

    /// <summary>
    /// Insert do QaudTree struktur x ma obmedzenie &lt;-180.00000, 180.00000&gt; y ma obmedzenie &lt;-90.00000,90.00000&gt;
    /// </summary>
    /// <param name="pX"></param>
    /// <param name="pY"></param>
    /// <param name="pData"></param>
    /// <exception cref="Exception">Ak su zle suradnice</exception>
    public void Insert(double xDownLeft, double yDownLeft, double xUpRight, double yUpLeft, T pData)
    {
        if (!root.ContainsPoints(new(QuadTreeRound(xDownLeft), QuadTreeRound(yDownLeft)), 
                new(QuadTreeRound(xUpRight), QuadTreeRound(yUpLeft))))
        {
            throw new Exception("Coordinates exceed parameter size");
        }
        QuadTreeNodeLeaf<T> current = root;
        QuadTreeNodeData<T> currentDataNode = new(new(QuadTreeRound(xDownLeft), QuadTreeRound(yDownLeft)), 
          new (QuadTreeRound(xUpRight), QuadTreeRound(yDownLeft)), pData);
        
        int depth = 0;
        while (current != null)
        {
            // 1 pozrieme sa či sa nejaký polygón nachádza v danom uzle
            if (current.DataIsEmpty())
            {
                current.AddData(currentDataNode);
                current = null;
            }
            // ak nie je prázdny
            //Skontrolujeme či nie je naplnená hlbka
            else if (depth == max_depth)
            {
                current.AddData(currentDataNode);
                current = null;
            }
            // môžu nastať 2 prípady
            // 1 skontrolujeme či sa objekt nezmestí do nejakého poduzla
                // môžu nastať 3 situácie
                // poduzol existuje a porovnáme do ktorého sa zmestí
                    // Ak sa zmestí tak current = poduzol
                // poduzol neexistuje
                    // vytvoríme predbežné poduzly a s ními skontrolujeme či sa zmestí
                    // ak sa zmestí tak poduzly vytvoríme a current = poduzol do ktorého sa zmesti a pokračujeme v cykle
            // Ak sa nezmestí do žiadného poduzla tak skontrolujeme počet objektov v danom uzle
                // ak ich tam je viack ako 1 to znamená že objekty ktoré sú tam sa už nikde nedajú presunúť
            // Ak je tam jeden objekt tak 
                // skontrolujeme či tento objekt sa nezmesti do nejakého poduzla
                // ak sa zmestí do niektorého poduzla tak aktuálny objekt vložíme
                // potom z daného objektu vymažeme nový objekt, potom zmeníme premennu currentData na tento nový objekt
                // a current = poduzol do ktorého sa zmesti a pokračujeme v cykle
        }
        
    }

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