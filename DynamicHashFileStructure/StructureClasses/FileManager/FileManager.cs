namespace DynamicHashFileStructure.StructureClasses;

public class FileManager
{
    private  string _fileName;
    private int _blockSize;
    private FileStream? _fileStream;

    public int BlockCount { get; set; }

    public FileManager(int pBlockSize, string pFilenameAndAddress)
    {
        _fileName = pFilenameAndAddress;
        _blockSize = pBlockSize;
        _fileStream = new(_fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        InitFile();
    }

    /// <summary>
    /// Inicializujem súbor ak ešte neexsituje
    /// </summary>
    private void InitFile()
    {
        // skontrolujem či existuje
        if (!File.Exists(_fileName))
        {
            // ak neexsituje tak vytvorím a nastavím zakládnú veľkosť
            using (var fileStream = File.Create(_fileName))
            {
                fileStream.SetLength(_blockSize);
                BlockCount = 1;
            }
        }
        // inak zistím koľko blokov sa tam nachádza
        else
        {
            var fileInfo = new FileInfo(_fileName);
            BlockCount = (int)fileInfo.Length / _blockSize;
        }
    }
    
    /// <summary>
    /// Pridá nový blok na koniec súboru
    /// </summary>
    /// <returns>Index nového bloku, ak je neúspech tak vrátime -1</returns>
    public int AddBlock()
    {
        if (_fileStream is null)
        {
            throw new FileNotFoundException("File stream is not open.");
        }
        try
        {
            _fileStream.Seek(0, SeekOrigin.End);

            // Získanie aktuálnej pozície (nový index bloku)
            int newIndex = (int)(_fileStream.Length / _blockSize);

            // Inicializácia nového bloku na veľkosť jedného bloku
            _fileStream.SetLength(_fileStream.Length + _blockSize);

            BlockCount++;
                
            return newIndex;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Chyba pri pridávaní bloku: {ex.Message}");
            return -1; // Vrátime -1 ako neúspech
        }
    }

    /// <summary>
    /// Zapíšeme dáta do konkrétneho bloku
    /// </summary>
    /// <param name="blockIndex"></param>
    /// <param name="data"></param>
    /// <returns>True - ak všetko prebehlo správne, inak false</returns>
    public bool WriteDataToBlock(int blockIndex, byte[] data)
    {
        if (_fileStream is null)
        {
            throw new FileNotFoundException("File stream is not open.");
        }
        
        try
        {
            // Presun na začiatok bloku
            _fileStream.Seek(GetAddress(blockIndex), SeekOrigin.Begin);

            // Zápis binárnych dát
            _fileStream.Write(data, 0, data.Length);

            return true;
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Chyba pri zápise dát na blok s indexom {blockIndex}: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    ///  Čítanie dát z konkrétneho bloku
    /// </summary>
    /// <param name="blockIndex">Index bloku ktorý chceme čítať</param>
    /// <returns>Byte array prečítanách dát, môže byť aj null</returns>
    public byte[]? ReadBlock(int blockIndex)
    {
        if (_fileStream is null)
        {
            throw new FileNotFoundException("File stream is not open.");
        }
        try
        {
            // Presun na začiatok bloku
            _fileStream.Seek(GetAddress(blockIndex), SeekOrigin.Begin);

            // Čítanie binárnych dát z bloku
            byte[]? buffer = new byte[_blockSize];
            var length = _fileStream.Read(buffer, 0, _blockSize);
            return buffer;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Chyba pri čítaní bloku s indexom {blockIndex}: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Zmazanie posledného bloku
    /// </summary>
    public void DeleteLastBlock()
    {
        if (_fileStream is null)
        {
            throw new FileNotFoundException("File stream is not open.");
        }
        try
        {
            // Zmena veľkosti súboru tak, aby sa odstránil posledný blok
            _fileStream.SetLength(_fileStream.Length - _blockSize);
            BlockCount--;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Chyba pri mazaní posledného bloku: {ex.Message}");
        }
    }
    
    
    
    /// <summary>
    /// Znovu otvorí filestream
    /// </summary>
    public void OpenFile()
    {
        if (_fileStream is not null)
        {
            _fileStream.Close();
        }
        _fileStream = new FileStream(_fileName, FileMode.Open, FileAccess.ReadWrite);
    }
    
    /// <summary>
    /// Zatvoríme filestream
    /// </summary>
    public void CloseFile()
    {
        if (_fileStream is not null)
        {
            _fileStream.Close();
            _fileStream = null;
        }
    }

    /// <summary>
    /// Deštruktor automaticky zatovrí file stream;
    /// </summary>
    ~FileManager()
    {
        CloseFile();
    }
    
    /// <summary>
    /// Vypočítame adresu bloku
    /// </summary>
    /// <param name="blockIndex">Index bloku ku ktorému chceme pristúpiť</param>
    /// <returns>Adresu bloku resp offset</returns>
    private int GetAddress(int blockIndex)
    {
        return blockIndex * _blockSize;
    }
}