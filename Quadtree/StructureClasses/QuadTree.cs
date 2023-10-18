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
        // root = new(pX + width / 2, pY + height / 2);

        if (checkCoordinates(quadTreeRound(pX), quadTreeRound(pY)) && checkCoordinates(quadTreeRound(pX + width), quadTreeRound(pY + height)))
        {
            throw new Exception("Wrong world coordination");
        }

        root = new(new(quadTreeRound(pX), quadTreeRound(pY)),
            new(quadTreeRound(pX + width), quadTreeRound(pY + height)));
        
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
        if (!root.containsPoints(new(quadTreeRound(xDownLeft), quadTreeRound(yDownLeft)), 
                new(quadTreeRound(xUpRight), quadTreeRound(yUpLeft))))
        {
            throw new Exception("Coordinates exeed parameter size");
        }
        QuadTreeNode<T> current = root;
        QuadTreeNode<T> parent = null;
        
    }

    private static int quadTreeRound(double value)
    {
        return (int)double.Round(value * HODNOTA, 5);
    }

    /**
     * Check if coordinates is valid for this structure
     * @return true if good, if bad then false
     */
    private static bool checkCoordinates(int x, int y)
    {
        if (x < -180 * HODNOTA || x > 180 * HODNOTA || y < -90 * HODNOTA || y > 90 * HODNOTA)
        {
            return false;
        }

        return true;
    }
}