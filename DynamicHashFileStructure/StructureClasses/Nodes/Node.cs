namespace DynamicHashFileStructure.StructureClasses.Nodes;

public abstract class Node<TData> where TData : IRecord<TData>
{
    public NodeIntern<TData> Parent { get; set; }
    private int _index;
}