namespace AaUS2.BinaryVyhladavaciStrom;

public class BtsTree <T> where T: IComparable<T>
{
    private BtsNode<T> root;

    public BtsTree()
    {
        // consturctor empty for now
    }

    public void Insert(T data)
    {
        if (root == null)
        {
            root = new (data);
            return;
        }
        var actualNode = root;
        while (actualNode != null)
        {
            if (actualNode.Data.CompareTo(data) == 1)
            {
                if (actualNode.LeftSon != null)
                {
                    actualNode = actualNode.LeftSon;
                }
                else
                {
                    actualNode.LeftSon = new(data, actualNode);
                    return;
                }
            }
            else
            {
                if (actualNode.RightSon != null)
                {
                    actualNode = actualNode.RightSon;
                }
                else
                {
                    actualNode.RightSon = new(data, actualNode);
                    return;
                }
            }
        }

        //actualNode = new(data);
    }

    public void toString()
    {
        Console.WriteLine(root.Data);
        if (root.LeftSon != null)
        {
            Console.WriteLine(root.LeftSon.Data);
        }
        if (root.RightSon != null)
        {
            Console.WriteLine(root.RightSon.Data);
        }
    }
}