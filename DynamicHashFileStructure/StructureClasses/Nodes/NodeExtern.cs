namespace DynamicHashFileStructure.StructureClasses.Nodes;

public class NodeExtern<TData> : Node<TData> where TData : IRecord
{
    public int Address { get; set; }
    public int Count { get; set; }
    
    public NodeExtern()
    {
        Address = -1;
        Count = 0;
    }
    
    public NodeExtern(int address, NodeIntern<TData> pParent)
    {
        Address = address;
        Count = 0;
        Parent = pParent;
    }
}