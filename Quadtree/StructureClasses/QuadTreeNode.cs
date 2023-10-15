namespace Quadtree.StructureClasses;

public class QuadTreeNode<T>
{
    /**
     * Suradnice stredu uzla
     */
    public double X { get; }
    public double Y { get; }
    public T? Data { get; set; }

    /**
     * Severo zapadny
     */
    public QuadTreeNode<T> NW { get; set; }

    /**
    * Severo vychodny
    */
    public QuadTreeNode<T> NE { get; set; }

    /**
    * Jouho zapadny
    */
    public QuadTreeNode<T> SW { get; set; }

    /**
    * Juho vychodny
    */
    public QuadTreeNode<T> SE { get; set; }

    private bool isLeaf;
    public bool IsLeaf { get => isLeaf; }

    public QuadTreeNode(double pX, double pY)
    {
        X = pX;
        Y = pY;
        Data = default;
        isLeaf = false;
    }
    
    public QuadTreeNode(double pX, double pY, T pData)
    {
        X = pX;
        Y = pY;
        isLeaf = true;
        Data = pData;
    }
}