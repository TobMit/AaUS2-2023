namespace DynamicHashFileStructure.StructureClasses.Nodes;

public class NodeIntern<TData> : Node<TData> where TData : IRecord
{
    public Node<TData>? LeftSon { get; set; }
    public Node<TData>? RightSon { get; set; }

    public NodeIntern(NodeIntern<TData> pParent)
    {
        Parent = pParent;
    }

    public NodeIntern(NodeIntern<TData> pParent, NodeExtern<TData> pLeftSonExtern)
    {
        Parent = pParent;
        LeftSon = pLeftSonExtern;
    }
}