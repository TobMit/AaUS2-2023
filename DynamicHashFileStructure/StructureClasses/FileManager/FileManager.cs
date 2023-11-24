namespace DynamicHashFileStructure.StructureClasses;

public class FileManager<TData> where TData : IComparable<TData>, IRecord<TData>
{
    private string _fileName;
    private LowLevelFileManager _lowLevelFileManager;
    private int _blockCount;
    
    
    private readonly int _blockFactor;

    /// <summary>
    /// Spráca súboru, riadi sparovanie volnych blokov, vytváranie nových blokov a podobne
    /// </summary>
    /// <param name="blockFactor">Počet zázanomov v bloku</param>
    /// <param name="fileName">Adresa a cesta k súboru</param>
    public FileManager(int blockFactor, string fileName)
    {
        if (blockFactor <= 0)
        {
            throw new ArgumentException("Block faktor nemôže byť záporný alebo nulový");
        }
        _blockFactor = blockFactor;
        _fileName = fileName;
        _lowLevelFileManager = new(Block<TData>.GetSize(blockFactor), _fileName);
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
    
    /// <summary>
    /// Vráti index nového bloku
    /// </summary>
    /// <returns>index nového bloku</returns>
    public int GetNewBlockIndex()
    {
        _blockCount++;
        return _lowLevelFileManager.AddBlock();
    }
    
    /// <summary>
    /// Zapíše do konkrétneho bloku
    /// </summary>
    /// <param name="id">id bloku do kotrého chceme zapisovať</param>
    /// <param name="block">blok dát</param>
    public void WriteBlock(int id, Block<TData> block)
    {
        if (id < 0 || id >= _blockCount)
        {
            throw new ArgumentOutOfRangeException(nameof(id),"ID bloku je mimo rozsah");
        }
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
    
    /// <summary>
    /// Získame blok zo súboru
    /// </summary>
    /// <param name="id">ID čítaného bloku</param>
    /// <returns>blok ktorý čítame</returns>
    public Block<TData> GetBlock(int id)
    {
        if (id < 0 || id >= _blockCount)
        {
            throw new ArgumentOutOfRangeException(nameof(id),"ID bloku je mimo rozsah");
        }
        var data = _lowLevelFileManager.ReadBlock(id);
        return Block<TData>.FromBytes(data);
    }
    
    /// <summary>
    /// Vráti celkový počet blokov
    /// </summary>
    /// <returns></returns>
    public int GetBlockCount()
    {
        return _blockCount;
    }
}