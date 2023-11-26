namespace DynamicHashFileStructure.StructureClasses;

public class Block<TData> : IRecord<Block<TData>> where TData : IComparable<TData>, IRecord<TData>
{
    private static int _blockFactor = -1;
    public static int BlockFactor
    {
        get => _blockFactor;
    }

    private TData[] _records;
    private int _validRecords;


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
    }

    /// <summary>
    /// Privátny konštruktor, ktorý sa používa pri načítavaní bloku z disku
    /// </summary>
    /// <param name="blockFactor">Blokovací faktor</param>
    /// <param name="records">Vytvorené dáta z disku</param>
    private Block(int blockFactor, TData[] records, int validRecords)
    {
        _blockFactor = blockFactor;
        _records = records;
        _validRecords = validRecords;
    }

    public Block(int blockFactor)
    {
        _blockFactor = blockFactor;
        _records = new TData[BlockFactor];
        _validRecords = 0;
    }

    public static int GetSize()
    {
        // toto tu je pre to, lebo vieme zavolať túto funkciu ešte pred inicializovaného blokovacieho faktoru,
        // čo by nám sôsobilo nesprávny výpočt veľkosti
        if (BlockFactor <= 0)
        {
            throw new OverflowException("Block faktor nie je nastavený!");
        }

        return BlockFactor * TData.GetSize() + sizeof(int); // posledný sizeof(int) je pre _validRecords
    }
    
    /// <summary>
    /// Vypočíta veľkosť bloku na základe blokovacieho faktoru
    /// </summary>
    /// <param name="blockFactor">blokovací faktor</param>
    /// <returns>veľosť bloku</returns>
    public static int GetSize(int blockFactor)
    {
        return blockFactor * TData.GetSize() + sizeof(int); // posledný sizeof(int) je pre _validRecords
    }

    public byte[] GetBytes()
    {
        // toto tu je pre to, lebo vieme zavolať túto funkciu ešte pred inicializovaného blokovacieho faktoru,
        // čo by nám sôsobilo nesprávny výpočt veľkosti
        if (BlockFactor <= 0)
        {
            throw new OverflowException("Block faktor nie je nastavený!");
        }
        
        //todo add other fields
        List<Byte> bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes(_validRecords));

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

    public static Block<TData> FromBytes(byte[] bytes)
    {
        // toto tu je pre to, lebo vieme zavolať túto funkciu ešte pred inicializovaného blokovacieho faktoru,
        // čo by nám sôsobilo nesprávny výpočt veľkosti
        if (BlockFactor <= 0)
        {
            throw new OverflowException("Block faktor nie je nastavený!");
        }

        
        int offset = 0;
        int validRecords = BitConverter.ToInt32(bytes, offset);
        offset = sizeof(int);

        TData[] records = new TData[BlockFactor];
        for (int i = 0; i < BlockFactor; i++)
        {
            // skopírujem iba potrebnú časť z bitového poľa
            byte[] recordBytes = new byte[TData.GetSize()];
            Array.Copy(bytes, offset, recordBytes, 0, TData.GetSize());
            records[i] = TData.FromBytes(recordBytes);
            offset += TData.GetSize();
        }

        return new Block<TData>(_blockFactor, records, validRecords);
    }

    public byte[] GetBytesForHash()
    {
        return new byte[0];
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
    /// Only for test purposes
    /// </summary>
    /// <returns>True ak sú rovnaké, inak false</returns>
    public bool TestEquals(Block<TData>? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        
        if (other._records.Length != _records.Length)
        {
            return false;
        }

        if (_validRecords != other._validRecords)
        {
            return false;
        }

        for (int i = 0; i < _validRecords; i++)
        {
            if (_records[i].CompareTo(other._records[i]) != 0)
            {
                return false;
            }
        }
        return true;
    }
}