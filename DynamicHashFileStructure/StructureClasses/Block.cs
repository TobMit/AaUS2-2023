namespace DynamicHashFileStructure.StructureClasses;

public class Block<TData> : IRecord<Block<TData>> where TData : IComparable<TData>, IRecord<TData>
{
    private static int _blockFactor = -1;
    private TData[] _records;
    private int _validRecords;
    
    public Block(int blockFactor, TData data)
    {
        _blockFactor = blockFactor;
        _records = new TData[blockFactor];
        for (int i = 0; i < _records.Length; i++)
        {
            _records[i] = data;
        }
    }

    /// <summary>
    /// Privátny konštruktor, ktorý sa používa pri načítavaní bloku z disku
    /// </summary>
    /// <param name="blockFactor">Block faktor</param>
    /// <param name="records">Vytvorené dáta z disku</param>
    private Block(int blockFactor, TData[] records)
    {
        _blockFactor = blockFactor;
        _records = records;
    }
    
    // todo add record function
    // todo remove record function with moving invalid records to the back of the array
    // todo add get record function

    public static int GetSize()
    {
        // toto tu je pre to, lebo vieme zavolať túto funkciu ešte pred inicializovaného blokovacieho faktoru,
        // čo by nám sôsobilo nesprávny výpočt veľkosti
        if (_blockFactor == -1)
        {
            throw new OverflowException("Blok nie je inicializovaný, najskôr inicializujte blok dát");
        }
        return _blockFactor * TData.GetSize() + sizeof(int); // posledný sizeof(int) je pre _validRecords
    }

    public byte[] GetBytes()
    {
        //todo add other fields
        List<Byte> bytes = new List<byte>();
        bytes.AddRange(BitConverter.GetBytes(_validRecords));

        for (int i = 0; i < _records.Length; i++)
        {
            bytes.AddRange(_records[i].GetBytes());
            //byte[] recordBytes = _records[i].GetBytes();
            //recordBytes.CopyTo(Data, offset);
            //offset += recordBytes.Length;
        }
        return bytes.ToArray();
    }

    public static Block<TData> FromBytes(byte[] bytes)
    {
        // toto tu je pre to, lebo vieme zavolať túto funkciu ešte pred inicializovaného blokovacieho faktoru,
        // čo by nám sôsobilo nesprávny výpočt veľkosti
        if (_blockFactor == -1)
        {
            throw new OverflowException("Blok nie je inicializovaný, najskôr inicializujte blok dát");
        }
        
        int validRecords = BitConverter.ToInt32(bytes, 0);
        int offset = sizeof(int);
        
        TData[] records = new TData[_blockFactor];
        for (int i = 0; i < validRecords; i++)
        {
            // skopírujem iba potrebnú časť z bitového poľa
            byte[] recordBytes = new byte[TData.GetSize()];
            Array.Copy(bytes, offset, recordBytes, 0, TData.GetSize());
            records[i] = TData.FromBytes(recordBytes);
        }
        
        return new Block<TData>(_blockFactor, records);
    }
    
    /// <summary>
    /// Skontrolujem či je už daný blok plný
    /// </summary>
    /// <returns>True ak je plný, inak false</returns>
    public bool IsFull()
    {
        return _validRecords == _blockFactor;
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
}