namespace DynamicHashFileStructure.StructureClasses;

public class FileManager<TData> where TData : IComparable<TData>, IRecord<TData>
{
    private const string FileName = "DynamicHashFileStructure/Files/data.bin";
    private LowLevelFileManager _lowLevelFileManager;
    private int _blockCount;
    
    
    private readonly int _blockFactor;
    
    /// <summary>
    /// Spráca súboru, riadi sparovanie volnych blokov, vytváranie nových blokov a podobne
    /// </summary>
    /// <param name="blockFactor"></param>
    public FileManager(int blockFactor)
    {
        _blockFactor = blockFactor;
        _lowLevelFileManager = new(Block<TData>.GetSize(), FileName);
        _blockCount = _lowLevelFileManager.BlockCount;
    }
    
    /// <summary>
    /// Otvorí súbor
    /// </summary>
    public void OpenFile()
    {
        _lowLevelFileManager.OpenFile();
    }
    
    /// <summary>
    /// Zatvorí súbor
    /// </summary>
    public void CloseFile()
    {
        _lowLevelFileManager.CloseFile();
    }
    
    ~FileManager()
    {
        _lowLevelFileManager.CloseFile();
    }
    
    public int GetNewBlockIndex()
    {
        return _lowLevelFileManager.AddBlock();
    }
    
    public void WriteBlock(int id, Block<TData> block)
    {
        _lowLevelFileManager.WriteDataToBlock(id, block.GetBytes());
    }
    
    // public void RemoveRecord(TData data)
    // {
    //     // získam index bloku
    //     var index = _lowLevelFileManager.AddBlock();
    //     // získam adresu bloku
    //     var address = _lowLevelFileManager.GetAddress(index);
    //     // získam blok
    //     var block = new Block<TData>(_blockFactor, data);
    //     // zapíšem blok na disk
    //     _lowLevelFileManager.WriteBlock(block, address);
    // }
    
    public Block<TData> GetRecord(int id)
    {
        var data = _lowLevelFileManager.ReadBlock(id);
        return Block<TData>.FromBytes(data);
    }
    
    public int GetBlockCount()
    {
        return _blockCount;
    }
}