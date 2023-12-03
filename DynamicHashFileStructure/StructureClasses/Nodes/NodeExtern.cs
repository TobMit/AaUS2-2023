namespace DynamicHashFileStructure.StructureClasses.Nodes;

public class NodeExtern<TData> : Node<TData> where TData : IRecord
{
    public int Address { get; set; }
    public int CountPrimaryData { get; set; }

    public int CountPreplnovaciData { get; set; }
    public int CountPreplnovaciBlock { get; set; }
    
    public NodeExtern()
    {
        Address = -1;
        CountPrimaryData = 0;
        CountPreplnovaciData = 0;
        CountPreplnovaciBlock = 0;
    }
    
    public NodeExtern(int address, NodeIntern<TData> pParent)
    {
        Address = address;
        CountPrimaryData = 0;
        Parent = pParent;
        CountPreplnovaciData = 0;
        CountPreplnovaciBlock = 0;
    }
}