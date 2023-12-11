using System.ComponentModel.Design;
using DynamicHashFileStructure.StructureClasses.HelperClasses;

namespace DynamicHashFileStructure.StructureClasses;

public class FileManager<TData> where TData : IRecord
{
    private string _fileName;
    private LowLevelFileManager _lowLevelFileManager;
    private int _blockTotalCount;
    public int BlockUsedCount {
        get;
        private set;
    }
    
    private int _firstFreeBlock;
    private int _lastFreeBlock;
    
    
    private readonly int _blockFactor;

    /// <summary>
    /// Správca súboru, riadi spravovanie voľných blokov, vytváranie nových blokov a podobne
    /// </summary>
    /// <param name="blockFactor">Počet záznamov v bloku</param>
    /// <param name="fileName">Adresa a cesta k súboru</param>
    public FileManager(int blockFactor, string fileName)
    {
        if (blockFactor <= 0)
        {
            throw new ArgumentException("Block faktor nemôže byť záporný alebo nulový");
        }
        _blockFactor = blockFactor;
        _fileName = fileName;
        Block<TData> firstBlock = new(blockFactor);
        _lowLevelFileManager = new(Block<TData>.GetSize(), _fileName);
        if (_lowLevelFileManager.BlockCount == 0)
        {
            _lowLevelFileManager.AddBlock();
            _lowLevelFileManager.WriteDataToBlock(0, firstBlock.GetBytes());
            _blockTotalCount = _lowLevelFileManager.BlockCount;  
            _firstFreeBlock = 0;
            _lastFreeBlock = 0;
        }
        else
        {
            _blockTotalCount = _lowLevelFileManager.BlockCount;
            // pri načítavaniu dát.bin kde už je niečo uložene
            _firstFreeBlock = -1;
            _lastFreeBlock = -1;
        }
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
        if (_lowLevelFileManager is null)
        {
            return;
        }
        _lowLevelFileManager.CloseFile();
    }
    
    /// <summary>
    /// Vráti prvý voľný blok, akonáhle je blok vrátený, zaradí sa k použitým blokom
    /// </summary>
    /// <returns>index nového bloku</returns>
    public Pair<int, Block<TData>> GetFreeBlock()
    {
        Pair<int, Block<TData>> returnPair;
        // ak je prvý voľný blok -1 tak vytvoríme nový blok lebo nie je žiaden voľný blok
        if (_firstFreeBlock <= -1)
        {
            _blockTotalCount++;
            BlockUsedCount++;
            returnPair = new(_lowLevelFileManager.AddBlock(), new Block<TData>(_blockFactor));
        }
        
        // inak, môžeme použiť voľný blok z reťazenia
        else
        {
            returnPair = new(_firstFreeBlock, (Block<TData>)Block<TData>.FromBytes(_lowLevelFileManager.ReadBlock(_firstFreeBlock), _blockFactor));
            // zmeníme na ďalší blok z reťazenia (môže byť aj -1)
            _firstFreeBlock = returnPair.Second.NextFreeBlock;
            
            // ak je prvý voľný blok -1 (čiže nie je žiaden volný blok) tak musíme zmeniť aj index posledného voľného bloku
            if (_firstFreeBlock <= -1)
            {
                _lastFreeBlock = -1;
            }
            else
            {
                // ak nie je tak načítame ten blok ktorý je za tým ktorý sme práve načítali a nastavíme mu last free block na -1
                var nextBlock = (Block<TData>)Block<TData>.FromBytes(_lowLevelFileManager.ReadBlock(_firstFreeBlock), _blockFactor);
                nextBlock.LastNextFreeBlock = -1;
                _lowLevelFileManager.WriteDataToBlock(_firstFreeBlock, nextBlock.GetBytes());
            }
            BlockUsedCount++;
        }
        
        returnPair.Second.ClearLinkReferences();
        returnPair.Second.ClearRecords();
        return returnPair;
    }


    /// <summary>
    /// Blok ktorý sa vymazal sa zaradí medzi voľné bloky
    /// </summary>
    /// <param name="index">Index bloku ktorý mažeme</param>
    /// <exception cref="ArgumentOutOfRangeException">Ak sa zadá zly index</exception>
    public void RemoveBlock(int index)
    {
        if (index < 0 || index >= _blockTotalCount)
        {
            throw new ArgumentOutOfRangeException(nameof(index),"ID bloku je mimo rozsah");
        }
        
        // ak je to posledný block
        if (index == _blockTotalCount -1)
        {
            // ak je index posledný tak ho rovno zmažem a znížim počet všetkých blokov aj block used count
            _lowLevelFileManager.DeleteLastBlock();
            _blockTotalCount--;
            BlockUsedCount--;
            // current =  posledný block
            var current = _blockTotalCount - 1;
            // pokiaľ current je rôzne od -1
            while (current >= 0 && _blockTotalCount > 0)
            {
                //načítam si block
                var block = (Block<TData>)Block<TData>.FromBytes(_lowLevelFileManager.ReadBlock(current), _blockFactor);
                // ak je medzi uvoľnený block (čiže má 0 na valid records)
                if (block.Count() > 0)
                {
                    current = -1;
                    break;
                }

                if (block.NextFreeBlock == -1 && block.LastNextFreeBlock == -1)
                {
                    _lowLevelFileManager.DeleteLastBlock();
                    _blockTotalCount--;
                    _firstFreeBlock = -1;
                    _lastFreeBlock = -1;
                    break;
                }
                else if (current == _lastFreeBlock)
                {
                    // nový last free block je môj last free block (treba mu aj zmazať inštanciu na last free block
                    var newLastFreeBlock = (Block<TData>)Block<TData>.FromBytes(_lowLevelFileManager.ReadBlock(block.LastNextFreeBlock), _blockFactor);
                    newLastFreeBlock.NextFreeBlock = -1;
                    _lastFreeBlock = block.LastNextFreeBlock;
                    _lowLevelFileManager.WriteDataToBlock(block.LastNextFreeBlock, newLastFreeBlock.GetBytes());
                    
                    _lowLevelFileManager.DeleteLastBlock();
                    _blockTotalCount--;
                }
                else if (current == _firstFreeBlock)
                {
                    // ak narazím na first free block tak nastavím first free block na next free block
                    var newFirstFreeBlock = (Block<TData>)Block<TData>.FromBytes(_lowLevelFileManager.ReadBlock(block.NextFreeBlock), _blockFactor);
                    newFirstFreeBlock.LastNextFreeBlock = -1;
                    _firstFreeBlock = block.NextFreeBlock;
                    _lowLevelFileManager.WriteDataToBlock(block.NextFreeBlock, newFirstFreeBlock.GetBytes());
                    
                    _lowLevelFileManager.DeleteLastBlock();
                    _blockTotalCount--;
                }
                else
                {
                    var nextFreeBlock = (Block<TData>)Block<TData>.FromBytes(_lowLevelFileManager.ReadBlock(block.NextFreeBlock), _blockFactor);
                    var lastNextFreeBlock = (Block<TData>)Block<TData>.FromBytes(_lowLevelFileManager.ReadBlock(block.LastNextFreeBlock), _blockFactor);
                    
                    // môjmu next free bloku  nastavím môj last free block
                    nextFreeBlock.LastNextFreeBlock = block.LastNextFreeBlock;
                    _lowLevelFileManager.WriteDataToBlock(block.NextFreeBlock, nextFreeBlock.GetBytes());
                    // môjmu last free bloku nastavím môj next free block
                    lastNextFreeBlock.NextFreeBlock = block.NextFreeBlock;
                    _lowLevelFileManager.WriteDataToBlock(block.LastNextFreeBlock, lastNextFreeBlock.GetBytes());
                    // odstránim posledný block
                    
                    _lowLevelFileManager.DeleteLastBlock();
                    _blockTotalCount--;
                }

                current = _blockTotalCount - 1;
            }
        }
        else if (_firstFreeBlock == -1 && _lastFreeBlock == -1)
        {
            _firstFreeBlock = index;
            _lastFreeBlock = index;
            var block = (Block<TData>)Block<TData>.FromBytes(_lowLevelFileManager.ReadBlock(index), _blockFactor);
            block.ClearRecords(); // ak by z nejakého dôvodu boli ešte rekordy v bloku
            block.ClearLinkReferences(); // aby sa zmazali všetky referencie
            _lowLevelFileManager.WriteDataToBlock(index, block.GetBytes());
            BlockUsedCount--;
        } 
        // ak index nie je posledný
        else if (index != _blockTotalCount - 1)
        {
            var block = (Block<TData>)Block<TData>.FromBytes(_lowLevelFileManager.ReadBlock(index), _blockFactor);
            block.ClearRecords(); // ak by z nejakého dôvodu boli ešte rekordy v bloku
            block.ClearLinkReferences(); // aby sa zmazali všetky referencie

            var tmpFirst = _firstFreeBlock;
            var oldBlock = (Block<TData>)Block<TData>.FromBytes(_lowLevelFileManager.ReadBlock(tmpFirst), _blockFactor);
            
            oldBlock.LastNextFreeBlock = index;
            _firstFreeBlock = index;
            block.NextFreeBlock = tmpFirst;
            block.LastNextFreeBlock = -1;
            
            // zapíšeme do súboru
            _lowLevelFileManager.WriteDataToBlock(index, block.GetBytes());
            _lowLevelFileManager.WriteDataToBlock(tmpFirst, oldBlock.GetBytes());
            BlockUsedCount--;
        }
    }
    
    /// <summary>
    /// Zapíše do konkrétneho bloku
    /// </summary>
    /// <param name="id">id bloku do ktorého chceme zapisovať</param>
    /// <param name="block">blok dát</param>
    public void WriteBlock(int id, Block<TData> block)
    {
        if (id < 0 || id >= _blockTotalCount)
        {
            throw new ArgumentOutOfRangeException(nameof(id),"ID bloku je mimo rozsah");
        }
        _lowLevelFileManager.WriteDataToBlock(id, block.GetBytes());
    }
    
    /// <summary>
    /// Získame blok zo súboru
    /// </summary>
    /// <param name="id">ID čítaného bloku</param>
    /// <returns>blok ktorý čítame</returns>
    public Block<TData> GetBlock(int id)
    {
        if (id < 0 || id >= _blockTotalCount)
        {
            throw new ArgumentOutOfRangeException(nameof(id),"ID bloku je mimo rozsah");
        }
        var data = _lowLevelFileManager.ReadBlock(id);
        return (Block<TData>)Block<TData>.FromBytes(data, _blockFactor);
    }
    
    /// <summary>
    /// Vráti celkový počet blokov
    /// </summary>
    /// <returns></returns>
    public int GetBlockCount()
    {
        return _blockTotalCount;
    }

    /// <summary>
    /// Uloží potrebné informácie do súboru
    /// </summary>
    public void Save()
    {
        // uložíme počet blokov txt súboru pomocou streamwritera
        using (StreamWriter sw = new StreamWriter(_fileName.Substring(0, _fileName.Length - 4) + "_fileManager" + ".txt"))
        {
            sw.WriteLine(_blockTotalCount);
            sw.WriteLine(BlockUsedCount);
            sw.WriteLine(_firstFreeBlock);
            sw.WriteLine(_lastFreeBlock);
        }
    }

    /// <summary>
    /// Načíta potrebné informácie zo súboru
    /// </summary>
    public void Load()
    {
        using (StreamReader sr = new StreamReader(_fileName.Substring(0, _fileName.Length - 4) + "_fileManager" + ".txt"))
        {
            _blockTotalCount = int.Parse(sr.ReadLine()!);
            BlockUsedCount = int.Parse(sr.ReadLine()!);
            _firstFreeBlock = int.Parse(sr.ReadLine()!);
            _lastFreeBlock = int.Parse(sr.ReadLine()!);
        }

        if (_blockTotalCount == 0)
        {
            _lowLevelFileManager.SetSize(0);
        }
    }
    
}