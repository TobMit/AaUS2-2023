using System.Text;

namespace DynamicHashFileStructure.StructureClasses;

public class Block<TData> : IRecord where TData : IRecord
{
    private static int _blockFactor = -1;
    public static int BlockFactor
    {
        get => _blockFactor;
    }

    private TData[] _records;
    private int _validRecords;

    public int NextFreeBlock { get; set; }
    public int LastNextFreeBlock { get; set; }
    
    public int NextDataBlock { get; set; }


    /// <summary>
    /// Konštruktor triedy Block, ktorý vytvorí blok s jedným recordom
    /// </summary>
    /// <param name="blockFactor">blokovací faktor</param>
    /// <param name="data">Data ktoré sa tam vložia</param>
    /// <remarks>Blokovací faktor musí byť nastavený pred tým</remarks>
    public Block(int blockFactor, TData data)
    {
        
        // blok faktor nemôže byť záporný
        if (blockFactor <= 0)
        {
            throw new ArgumentException("Block faktor nemôže byť záporný alebo nulový");
        }
        
        _blockFactor = blockFactor;
        _records = new TData[BlockFactor];
        for (int i = 0; i < _records.Length; i++)
        {
            _records[i] = data;
        }
        _validRecords = 1;
        NextFreeBlock = -1;
        LastNextFreeBlock = -1;
        NextDataBlock = -1;
    }

    /// <summary>
    /// Privátny konštruktor, ktorý sa používa pri načítavaní bloku z disku
    /// </summary>
    /// <param name="blockFactor">Blokovací faktor</param>
    /// <param name="records">Vytvorené dáta z disku</param>
    private Block(int blockFactor, TData[] records, int validRecords, int nextFreeBlock, int lastFreeFreeBlock, int nextDataBlock)
    {
        _blockFactor = blockFactor;
        _records = records;
        _validRecords = validRecords;
        NextFreeBlock = nextFreeBlock;
        LastNextFreeBlock = lastFreeFreeBlock;
        NextDataBlock = nextDataBlock;
    }

    public Block(int blockFactor)
    {
        _blockFactor = blockFactor;
        _records = new TData[BlockFactor];
        _validRecords = 0;
        NextFreeBlock = -1;
        LastNextFreeBlock = -1;
        NextDataBlock = -1;
    }

    public static int GetSize()
    {
        // toto tu je pre to, lebo vieme zavolať túto funkciu ešte pred inicializovaného blokovacieho faktoru,
        // čo by nám sôsobilo nesprávny výpočt veľkosti
        if (BlockFactor <= 0)
        {
            throw new OverflowException("Block faktor nie je nastavený!");
        }

        //   počt záznamov +          početPlatnýchZáznamov + nextFreeBlock + lastNextBlock + nextDataBlock
        return BlockFactor * TData.GetSize() + sizeof(int) + sizeof(int) + sizeof(int) + sizeof(int); 
    }

    /// <summary>
    /// Vypočíta veľkosť bloku na základe blokovacieho faktoru
    /// </summary>
    /// <param name="blockFactor">blokovací faktor</param>
    /// <returns>veľosť bloku</returns>
    public static int GetSize(int blockFactor)
    {
        //   počt záznamov +          početPlatnýchZáznamov + nextFreeBlock + lastNextBlock + nextDataBlock
        return blockFactor * TData.GetSize() + sizeof(int) + sizeof(int) + sizeof(int) + sizeof(int);
    }

    public byte[] GetBytes()
    {
        // toto tu je pre to, lebo vieme zavolať túto funkciu ešte pred inicializovaného blokovacieho faktoru,
        // čo by nám sôsobilo nesprávny výpočt veľkosti
        if (BlockFactor <= 0)
        {
            throw new OverflowException("Block faktor nie je nastavený!");
        }
        
        List<Byte> bytes = new List<byte>();
        
        bytes.AddRange(BitConverter.GetBytes(NextFreeBlock));
        bytes.AddRange(BitConverter.GetBytes(LastNextFreeBlock));
        bytes.AddRange(BitConverter.GetBytes(_validRecords));
        bytes.AddRange(BitConverter.GetBytes(NextDataBlock));

        for (int i = 0; i < _records.Length; i++)
        {
            if (i < _validRecords)
            {
                bytes.AddRange(_records[i].GetBytes());    
            }
            else
            {
                // ak nie je vytvorený alebo môže byť null tak to nahradím prázdnym polom
                bytes.AddRange(new byte[TData.GetSize()]);
            }
        }

        return bytes.ToArray();
    }

    public static object FromBytes(byte[] bytes)
    {
        // toto tu je pre to, lebo vieme zavolať túto funkciu ešte pred inicializovaného blokovacieho faktoru,
        // čo by nám sôsobilo nesprávny výpočt veľkosti
        if (BlockFactor <= 0)
        {
            throw new OverflowException("Block faktor nie je nastavený!");
        }

        
        int offset = 0;
        int nextFreeBlock = BitConverter.ToInt32(bytes, offset);
        offset = sizeof(int);
        int lastFreeBlock = BitConverter.ToInt32(bytes, offset);
        offset += sizeof(int);
        int validRecords = BitConverter.ToInt32(bytes, offset);
        offset += sizeof(int);
        int nextDataBlock = BitConverter.ToInt32(bytes, offset);
        offset += sizeof(int);

        TData[] records = new TData[BlockFactor];
        for (int i = 0; i < BlockFactor; i++)
        {
            // skopírujem iba potrebnú časť z bitového poľa
            byte[] recordBytes = new byte[TData.GetSize()];
            Array.Copy(bytes, offset, recordBytes, 0, TData.GetSize());
            records[i] = (TData)TData.FromBytes(recordBytes);
            offset += TData.GetSize();
        }

        return new Block<TData>(_blockFactor, records, validRecords, nextFreeBlock, lastFreeBlock, nextDataBlock);
    }
    
    public static object FromBytes(byte[] bytes, int blockFactor)
    {
        int offset = 0;
        int nextFreeBlock = BitConverter.ToInt32(bytes, offset);
        offset = sizeof(int);
        int lastFreeBlock = BitConverter.ToInt32(bytes, offset);
        offset += sizeof(int);
        int validRecords = BitConverter.ToInt32(bytes, offset);
        offset += sizeof(int);
        int nextDataBlock = BitConverter.ToInt32(bytes, offset);
        offset += sizeof(int);

        TData[] records = new TData[blockFactor];
        for (int i = 0; i < blockFactor; i++)
        {
            // skopírujem iba potrebnú časť z bitového poľa
            byte[] recordBytes = new byte[TData.GetSize()];
            Array.Copy(bytes, offset, recordBytes, 0, TData.GetSize());
            records[i] = (TData)TData.FromBytes(recordBytes);
            offset += TData.GetSize();
        }

        return new Block<TData>(blockFactor, records, validRecords, nextFreeBlock, lastFreeBlock, nextDataBlock);
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append($"Next free block: {NextFreeBlock}\n");
        sb.Append($"Last free block: {LastNextFreeBlock}\n");
        sb.Append($"Block factor: {_blockFactor}\n");
        sb.Append($"Valid records: {_validRecords}\n");
        sb.Append($"Next data block: {NextDataBlock}\n");
        sb.Append("Records:\n");
        for (int i = 0; i < _validRecords; i++)
        {
            sb.Append($"{_records[i].ToString()}\n");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Skontrolujem či je už daný blok plný
    /// </summary>
    /// <returns>True ak je plný, inak false</returns>
    public bool IsFull()
    {
        return _validRecords == BlockFactor;
    }

    /// <summary>
    /// Pridáme record do bloku
    /// </summary>
    /// <param name="record">Rekord ktorý sa ukladá do bloku</param>
    /// <exception cref="OverflowException">Ak je už blok plný tak hodí exception</exception>
    public void AddRecord(TData record)
    {
        if (IsFull())
        {
            throw new OverflowException("Blok je už plný");
        }

        _records[_validRecords] = record;
        _validRecords++;
    }

    /// <summary>
    /// Vráti konkrétne record z bloku
    /// </summary>
    /// <param name="index">Index bloku</param>
    /// <returns>Záznam z bloku</returns>
    /// <exception cref="IndexOutOfRangeException">Ak sme mimo zoznamu</exception>
    public TData GetRecord(int index)
    {
        if (index < 0 || index >= _validRecords)
        {
            throw new IndexOutOfRangeException("Index je mimo rozsah bloku");
        }

        return _records[index];
    }
    
    /// <summary>
    /// Vráti zoznam všetkých platných recordov v bloku
    /// </summary>
    /// <returns> List platných rekordov v bloku</returns>
    public List<TData> GetArrayRecords()
    {
        List<TData> records = new List<TData>();
        for (int i = 0; i < _validRecords; i++)
        {
            records.Add(_records[i]);
        }

        return records;
    }
    
    /// <summary>
    /// Zmaže všetky recordy z bloku
    /// </summary>
    public void ClearRecords()
    {
        _validRecords = 0;
    }

    /// <summary>
    /// Zmaže record z bolku dát a vrátiho, taktiež resunie posledný record na jeho miesto a zmenší počet validných recordov
    /// čím zabránime zbytočnému prechádzaniu cez prázdne recordy
    /// </summary>
    /// <param name="index">Index mazaného záznamu</param>
    /// <returns>Zmazaný záznam</returns>
    /// <exception cref="IndexOutOfRangeException">Ak sme dali index ktorý je mimo poľa</exception>
    public TData RemoverRecord(int index)
    {
        if (index < 0 || index >= _validRecords)
        {
            throw new IndexOutOfRangeException("Index je mimo rozsah bloku");
        }

        TData tmp = _records[index];
        _records[index] = _records[_validRecords - 1];
        _validRecords--;
        return tmp;
    }

    /// <summary>
    /// Vráti počet validných recordov v bloku
    /// </summary>
    /// <returns>Počet validných rekordov v bloku</returns>
    public int Count()
    {
        return _validRecords;
    }

    /// <summary>
    /// Zmaže (-1) referencie na iné free blocky
    /// </summary>
    public void ClearLinkReferences()
    {
        NextFreeBlock = -1;
        LastNextFreeBlock = -1;
        NextDataBlock = -1;
    }
    
    
    /// <summary>
    /// Only for test purposes
    /// </summary>
    /// <returns>True ak sú rovnaké, inak false</returns>
    public int CompareTo(object? obj)
    {
        if (ReferenceEquals(null, obj)) return -1;
        if (ReferenceEquals(this, obj)) return -1;
        
        Block<TData>? other = (Block<TData>)obj; 
        
        if (other._records.Length != _records.Length)
        {
            return -1;
        }

        if (_validRecords != other._validRecords)
        {
            return -1;
        }
        
        if (NextFreeBlock != other.NextFreeBlock)
        {
            return -1;
        }
        
        if (LastNextFreeBlock != other.LastNextFreeBlock)
        {
            return -1;
        }
        
        if (NextDataBlock != other.NextDataBlock)
        {
            return -1;
        }

        for (int i = 0; i < _validRecords; i++)
        {
            if (_records[i].CompareTo(other._records[i]) != 0)
            {
                return -1;
            }
        }
        return 0;
    }
}