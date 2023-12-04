namespace DynamicHashFileStructure.StructureClasses.Nodes;

public abstract class Node<TData> where TData : IRecord
{
    public NodeIntern<TData> Parent { get; set; }
}