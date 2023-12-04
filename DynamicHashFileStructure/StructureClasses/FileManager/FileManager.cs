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
        Block<TData> firstBlock = new(blockFactor);
        _lowLevelFileManager = new(Block<TData>.GetSize(), _fileName);
        _lowLevelFileManager.WriteDataToBlock(0, firstBlock.GetBytes());
        _blockTotalCount = _lowLevelFileManager.BlockCount;
        BlockUsedCount = 0;
        _firstFreeBlock = 0;
        _lastFreeBlock = 0;
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
            returnPair = new(_firstFreeBlock, (Block<TData>)Block<TData>.FromBytes(_lowLevelFileManager.ReadBlock(_firstFreeBlock)));
            // zmeníme na daľší blok z reťazenia (môže byť aj -1)
            _firstFreeBlock = returnPair.Second.NextFreeBlock;
            
            // ak je prvý voľný blok -1 (čiže nie je žiaden volný blok) tak musíme zmeniť aj index polsedného voľného bloku
            if (_firstFreeBlock <= -1)
            {
                _lastFreeBlock = -1;
            }
            else
            {
                // ak nie je tak načítame ten blok ktorý je za tým ktorý sme práve načítali a nastavíme mu last free block na -1
                var nextBlock = (Block<TData>)Block<TData>.FromBytes(_lowLevelFileManager.ReadBlock(_firstFreeBlock));
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
        
        // skontrolujeme či toto nie je posledny blok v zozname
        if (index == _blockTotalCount - 1)
        {
            BlockUsedCount--;
            int current = index;
            // v cykle spravíme, current nastavíme na adresu toho posledného bloku
            while (current != -1) // ak je current -1 tak končíme
            {
                // zýskame ho zo súboru
                var block = (Block<TData>)Block<TData>.FromBytes(_lowLevelFileManager.ReadBlock(current));
                // posledný index zmažeme
                _lowLevelFileManager.DeleteLastBlock();
                // current nastavíme last free block zo zýkaného bloku, pri prvom mazaní z konca keď blok nemá nastavené linkovacie hodnoty, tak má tam -1 všade čo je vlastne chyba a preto musíme overovať voči _lastFreeBlock
                if (current - 1 == (block.LastNextFreeBlock != -1 ? block.LastNextFreeBlock : _lastFreeBlock))
                {
                    // nahrá sa hodnota z bloku len v tedy ak je rozdielana -1 (tento prípad nastáva prvý krát keď sa maže s konca, linkovacie hodnoty ešte niesú nastavené)
                    if (block.LastNextFreeBlock != -1)
                    {
                        current = block.LastNextFreeBlock;   
                    }
                    else
                    {
                        current = _lastFreeBlock;
                    }
                }
                // ak nie je
                else
                {
                    // tak nebudeme pokračovať v cykle
                    current = -1;
                    // ak má informáciu o predchádzajúcom voľnom bloku
                    if (block.LastNextFreeBlock != -1)
                    {
                        // načítame ten blok ktorý je pred ním a zmeníme mu adresu next free na -1
                        var prevBlock = (Block<TData>)Block<TData>.FromBytes(_lowLevelFileManager.ReadBlock(block.LastNextFreeBlock));
                        prevBlock.NextFreeBlock = -1;
                        _lowLevelFileManager.WriteDataToBlock(block.LastNextFreeBlock, prevBlock.GetBytes());
                    }
                    // ak nemá tak nič nenastavujeme
                }
                
                // nastavíme _lastBlock na hodnotu z nacitaného bloku ak pred tým nebola -1
                if (block.LastNextFreeBlock != -1)
                {
                    _lastFreeBlock = block.LastNextFreeBlock;
                }
                
                // ak je last next free blcok z bloku -1 tak nastavíme aj prvý na -1 čo znamená že je 
                if (current == -1)
                {
                    _firstFreeBlock = -1;
                    _lastFreeBlock = -1;
                }

                _blockTotalCount--;
            }
        }
        // ak nie je žiaden blok volny (last aj next je -1) tak sa tento vymazaný nastavý ako prvý a aj posledný
        else if (_firstFreeBlock == -1 && _lastFreeBlock == -1)
        {
            _firstFreeBlock = index;
            _lastFreeBlock = index;
            BlockUsedCount--;
        }
        // ak je index tohto vymazaného bloku menší ako index prvého voľného bloku
        else if (index < _firstFreeBlock)
        {
            // načítame si prvý voľný block a zmenime mu hodnotu last free block na index tohoto vymazaného bloku
            var block = (Block<TData>)Block<TData>.FromBytes(_lowLevelFileManager.ReadBlock(_firstFreeBlock));
            block.LastNextFreeBlock = index;
            _lowLevelFileManager.WriteDataToBlock(_firstFreeBlock, block.GetBytes());
            
            // nastavíme prvý voľný blok na index tohoto vymazaného bloku
            var lastFirstFreeBlock = _firstFreeBlock;
            _firstFreeBlock = index;
            
            // na záver načítam vymazávaný blok a nastavíme mu lastFree blok na -1 a next free block na hodnotu predhcázajúceho free blocku
            var removedBlock = (Block<TData>)Block<TData>.FromBytes(_lowLevelFileManager.ReadBlock(index));
            removedBlock.LastNextFreeBlock = -1;
            removedBlock.NextFreeBlock = lastFirstFreeBlock;
            _lowLevelFileManager.WriteDataToBlock(index, removedBlock.GetBytes());
            BlockUsedCount--;
        }
        // ak je index väčší ako prvý ale menši ako posledný
        else if (_firstFreeBlock < index && index < _lastFreeBlock)
        {
            // vložíme ho medzi ne a tak že budeme postupne prechádzať od prvého bloku až kým nenarazíme na blok ktorý má next free block väčší index ako tento vymazávaný
            // tam ho vložíme

            int current = _firstFreeBlock;
            int lastCurrent = -1;
            Block<TData>? block = null;
            while (current < index)
            {
                block = (Block<TData>)Block<TData>.FromBytes(_lowLevelFileManager.ReadBlock(current));
                lastCurrent = current;
                current = block.NextFreeBlock;
            }
            // teraz v premennej block nachádza block ktorý je pred tým ktorý mažeme
            // nastavíme mu next free block na index tohoto vymazaného bloku
            block.NextFreeBlock = index;
            _lowLevelFileManager.WriteDataToBlock(lastCurrent, block.GetBytes());
            
            // v premennej current máme hodnotu indexu bloku ktorý je za tým ktorý mažeme
            // načítame si tento blok a nastavíme mu last free block na index tohoto vymazaného bloku
            block = (Block<TData>)Block<TData>.FromBytes(_lowLevelFileManager.ReadBlock(current));
            block.LastNextFreeBlock = index;
            _lowLevelFileManager.WriteDataToBlock(current, block.GetBytes());
            
            // teraz už len nastavíme vymazavaný block
            block = (Block<TData>)Block<TData>.FromBytes(_lowLevelFileManager.ReadBlock(index));
            block.LastNextFreeBlock = lastCurrent;
            block.NextFreeBlock = current;
            _lowLevelFileManager.WriteDataToBlock(index, block.GetBytes());
            
            BlockUsedCount--;
        }
        else if (_lastFreeBlock < index)
        {
            // načítam last free block a nastavím mu next free block na index tohoto vymazaného bloku
            var block = (Block<TData>)Block<TData>.FromBytes(_lowLevelFileManager.ReadBlock(_lastFreeBlock));
            block.NextFreeBlock = index;
            _lowLevelFileManager.WriteDataToBlock(_lastFreeBlock, block.GetBytes());
            
            // načítam vymazávaný blok a nastavím mu lastFreeBlock na posledný _lastFreeBock a next je -1
            block = (Block<TData>)Block<TData>.FromBytes(_lowLevelFileManager.ReadBlock(index));
            block.LastNextFreeBlock = _lastFreeBlock;
            block.NextFreeBlock = -1;
            _lowLevelFileManager.WriteDataToBlock(index, block.GetBytes());
            _lastFreeBlock = index;
            
            BlockUsedCount--;
        }
        else
        {
            throw new Exception("FileManager, Remove block: Toto nemalo nastať");
        }
        
    }
    
    /// <summary>
    /// Zapíše do konkrétneho bloku
    /// </summary>
    /// <param name="id">id bloku do kotrého chceme zapisovať</param>
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
        return (Block<TData>)Block<TData>.FromBytes(data);
    }
    
    /// <summary>
    /// Vráti celkový počet blokov
    /// </summary>
    /// <returns></returns>
    public int GetBlockCount()
    {
        return _blockTotalCount;
    }
    
    
    
}