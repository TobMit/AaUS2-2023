namespace Quadtree.StructureClasses;

public class QuadTree<T>
{
    private QuadTreeNode<T> root;
    private int capacity;

    public QuadTree(double pX, double pY, double width, double height, int capacity)
    {
        root = new(pX + width / 2, pY + height / 2);
        this.capacity = capacity;
    }

    public void Insert(double pX, double pY, T pData)
    {
        QuadTreeNode<T> current = root;
        QuadTreeNode<T> parent = null;
        while (current != null)
        {
            parent = current;
            
            // rozhodujem sa medzi Severom a Juhom
            if (pY > current.Y)
            {
                // rozhodujem sa Východom a Západom
                if (pX > current.X)
                {
                    if (current.NE != null)
                    {
                        current = current.NE;
                    }
                    else
                    {
                        current.NE = new(pX, pY, pData);
                        return;
                    }
                }
                else
                {
                    if (current.NW != null)
                    {
                        current = current.NW;
                    }
                    else
                    {
                        current.NW = new(pX, pY, pData);
                        return;
                    }
                }
            }
            else
            {
                // rozhodujem sa medzi Východom a Západom
                if (pX > current.X)
                {
                    if (current.SE != null)
                    {
                        current = current.SE;
                    }
                    else
                    {
                        current.SE = new(pX, pY, pData);
                        return;
                    }
                }
                else
                {
                    if (current.SW != null)
                    {
                        current = current.SW;
                    }
                    else
                    {
                        current.SW = new(pX, pY, pData);
                        return;
                    }
                }
            }
        }
    }

}