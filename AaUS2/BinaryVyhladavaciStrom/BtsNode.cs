namespace AaUS2.BinaryVyhladavaciStrom;

public class BtsNode<T> where T: IComparable<T>
{
    public T Data { get; set; }
    public BtsNode<T> Parent { get; set; }
    public BtsNode<T> LeftSon { get; set; }
    public BtsNode<T> RightSon { get; set; }

    public BtsNode(T data, BtsNode<T> parent)
    {
        Data = data;
        Parent = parent;
    }
    
    public BtsNode(T data)
    {
        Data = data;
    }
}