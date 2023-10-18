using System.Drawing;
using System.Runtime.InteropServices.JavaScript;

namespace Quadtree.StructureClasses;

public class QuadTree<T>
{
    private QuadTreeNode<T> root;
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
        QuadTreeNode<T> current = root;
        QuadTreeNode<T> parent = null;
        
        
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