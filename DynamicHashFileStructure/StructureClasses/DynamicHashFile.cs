using System.Collections;
using System.Text;
using DynamicHashFileStructure.StructureClasses.HelperClasses;
using DynamicHashFileStructure.StructureClasses.Nodes;

namespace DynamicHashFileStructure.StructureClasses;

public class DynamicHashFile<TKey, TData> where TData : IRecordData<TKey>
{

    private const int PrimaryFileBlockSize = 3;
    private const int PreplnovaciFileBlockSize = 3;
    
    public int Count { get; private set; }
    
    private NodeIntern<TData> _root;

    private FileManager<TData> _fileManager;
    private FileManager<TData> _filePreplnovaciManager;
    
    private string _primaryFileName;
    private string _preplnovakFileName;

    public DynamicHashFile(string primaryFileName, string preplnovakFileName)
    {
        _root = new(null);
        _fileManager = new(PrimaryFileBlockSize, primaryFileName);
        _filePreplnovaciManager = new(PreplnovaciFileBlockSize, preplnovakFileName);
        Count = 0;
        
        _primaryFileName = primaryFileName;
        _preplnovakFileName = preplnovakFileName;
        
    }
    public DynamicHashFile()
    {
        _root = new(null);
        
        _primaryFileName = "primaryData.bin";
        _preplnovakFileName = "secondaryData.bin";
        
        _fileManager = new(PrimaryFileBlockSize, _primaryFileName);
        _filePreplnovaciManager = new(PreplnovaciFileBlockSize, _preplnovakFileName);
        Count = 0;

    }


    
    /// <summary>
    /// Vloží dáta do dynamického hešovacieho súboru
    /// </summary>
    /// <param name="key">kľúč ktorý sa použije pri hešovacej funkcie</param>
    /// <param name="data">dáta ktoré sa vkladajú</param>
    public void Insert(TKey key ,TData data)
    {
        bool continueInsert = false;
        try
        {
            Find(key);
        }
        catch (Exception e)
        {
            continueInsert = true;
        }

        if (!continueInsert)
        {
            throw new ArgumentException("Dáta s týmto kľúčom už existujú!");
        }
        
        Stack<Pair<TKey, TData>> stackData = new();
        // vložím data do staku
        stackData.Push(new(key, data));
        // while stak nie je prázdny
        while (stackData.Count > 0)
        {
            Stack<Node<TData>> stackNode = new();
            // vložím do staku root a prechádzam vnútorný stak
            stackNode.Push(_root);
            // id vrcholu = 0
            int index = 0;
            
            // vytiahnem data zo staku
            var dataToInsert = stackData.Pop();
            // získam hash
            var hash = TData.GetBytesForHash(dataToInsert.First);
            // premením hash byte na pole bitov
            BitArray bitArray = new(hash);

            while (stackNode.Count > 0)
            {
                var node = stackNode.Pop();
                // 1. vrchol je interný
                if (node.GetType() == typeof(NodeIntern<TData>))
                {
                    var internNode = (NodeIntern<TData>) node;
                    var tmp = bitArray[index];
                    // ak niektorý syn nie je vytvorený tak ho vytvorím ako externý a tam vložím dáta na nový blok
                    if (!bitArray[index]) // 0 ak je ľavý 1 ak je pravý (čiže false a true)
                    {
                        if (internNode.LeftSon == null)
                        {
                            internNode.LeftSon = new NodeExtern<TData>(-1, internNode);
                        }
                        // vložím do staku toho syna ktorý pokračuje podla kľúča
                        stackNode.Push(internNode.LeftSon);
                        index++;
                    }
                    else
                    {
                        if (internNode.RightSon == null)
                        {
                            internNode.RightSon = new NodeExtern<TData>(-1, internNode);
                        }
                        // vložím do staku toho syna ktorý pokračuje podla kľúča
                        stackNode.Push(internNode.RightSon);
                        index++;
                    }
                }
                // 2. vrchol je externý
                else
                {
                    var externNode = (NodeExtern<TData>) node;
                    // skontrolujem koľko má uložených dát
                    
                    if (externNode.CountPrimaryData < PrimaryFileBlockSize)
                    {
                        // skontrolujem či má adresu
                        Block<TData>? block = null;
                        if (externNode.Address < 0)
                        {
                            // ak nemá tak pridám nový blok
                            var tmpBlock = _fileManager.GetFreeBlock();
                            block = tmpBlock.Second;
                            externNode.Address = tmpBlock.First;
                        }
                        // ak má tak prečítam blok
                        else
                        {
                            block = _fileManager.GetBlock(externNode.Address);
                        }
                        
                        // pridám nové dáta
                        block.AddRecord(dataToInsert.Second);
                        _fileManager.WriteBlock(externNode.Address, block);
                        externNode.CountPrimaryData++;
                        Count++;
                    }
                    // ak je plný
                    else
                    {
                        // skontrolujem či som už minul všetky bity
                        if (index >= bitArray.Length-1)
                        {
                            // ak áno tak vkladám do preplňujúceho bloku
                            
                            if (externNode.CountPreplnovaciBlock > 0)
                            {
                                // načítam si blok aby som získal adresu z preplňujúceho bloku
                                var mainBlock = _fileManager.GetBlock(externNode.Address);
                                Block<TData>? block = null;
                                
                                int current = mainBlock.NextDataBlock;
                                int lastCurrent = mainBlock.NextDataBlock; // keď mám blok ktorý nemá ďalší blok ktorý pokračuje tak potrebujem vedieť jeho adresu
                                // budeme prechádzať kým nenarazíme na posledný blok alebo pokiaľ nenarazíme na blok v ktorom je voľné miesto
                                while (current >= 0)
                                {
                                    block = _filePreplnovaciManager.GetBlock(current);
                                    
                                    // ak je plný tak pokračujeme ak nie je plný tak vyskakujeme
                                    if (block.IsFull())
                                    {
                                        lastCurrent = current;
                                        current = block.NextDataBlock;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                // ešte raz kontrolujeme či je blok plný (môže nastať situácia že je blok plný a nemá daľší blok)
                                if (block is null)
                                {
                                    throw new Exception("Toto nemalo nastať, chyba v Inserte pri prehľadávanií preplňujúcich blokov, block je null!");
                                }
                                if (block.IsFull())
                                {
                                    // tak nájdem nový block
                                    var tmpPair = _filePreplnovaciManager.GetFreeBlock();
                                    externNode.CountPreplnovaciBlock++;
                                    // do pôvodne načítaného bloku pridám linkovaciu adresu a zapíšeme ho
                                    block.NextDataBlock = tmpPair.First;
                                    _filePreplnovaciManager.WriteBlock(lastCurrent, block);
                                    current = tmpPair.First;
                                    block = tmpPair.Second;
                                }
                                // zapíšeme dáta do bloku a uložím
                                block.AddRecord(dataToInsert.Second);
                                _filePreplnovaciManager.WriteBlock(current, block);
                                Count++;
                                externNode.CountPreplnovaciData++;

                            }
                            // ak nemá tak pridám nový preplňujúci blok
                            else
                            {
                                // vytvorím nový blok z preplňujúceho súboru a uložíme tam dáta
                                var tmpPair = _filePreplnovaciManager.GetFreeBlock();
                                externNode.CountPreplnovaciBlock++;
                                tmpPair.Second.AddRecord(dataToInsert.Second);
                                externNode.CountPreplnovaciData++;
                                Count++;
                                _filePreplnovaciManager.WriteBlock(tmpPair.First, tmpPair.Second);
                                
                                // musíme vložiť do bloku v hlavnom súbore linkovaciu adresu
                                var block = _fileManager.GetBlock(externNode.Address);
                                block.NextDataBlock = tmpPair.First;
                                _fileManager.WriteBlock(externNode.Address, block);

                            }
                        }
                        else
                        {
                            // načítam blok postupne odstránim dáta ktoré sú v bloku a vložím ich do staku
                            var block = _fileManager.GetBlock(externNode.Address);
                            var listRecordov = block.GetArrayRecords();
                            block.ClearRecords();
                            externNode.CountPrimaryData = 0;
                            // zmažem tento blok
                            _fileManager.RemoveBlock(externNode.Address);
                            externNode.Address = -1;
                            foreach (var record in listRecordov)
                            {
                                stackData.Push(new(record.GetKey(), record));
                                Count--;
                            }
                        
                            stackData.Push(dataToInsert);
                            // medzi jeho parenta a tento blok vložím nový interný blok,
                            var parent = externNode.Parent;
                        
                            var tmp = bitArray[index - 1];

                            var tmpNode = new NodeIntern<TData>(parent, externNode);
                            externNode.Parent = tmpNode;
                        
                            if (!bitArray[index - 1]) // ak bol posledný bit 0 tak vložím nový intern node inak pravý
                            {
                                parent.LeftSon = tmpNode;
                            }
                            else
                            {
                                parent.RightSon = tmpNode;
                            }
                        }
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Vyhľadá dáta pomocou kľúča
    /// </summary>
    /// <param name="key">Kľúč ktorý sa používa pri hľadaní</param>
    /// <returns>Nájdene dáta</returns>
    /// <exception cref="ArgumentException">Ak sa nejaký záznam nenašiel</exception>
    public TData Find(TKey key)
    {
        
        TData returnData = default;
        
        BitArray bitArray = new(TData.GetBytesForHash(key));
        int index = 0;

        Stack<Node<TData>> stackNode = new();
        stackNode.Push(_root);
        while (stackNode.Count > 0)
        {
            var node = stackNode.Pop();
            // skontrolujem si typ vrcholu
            if (node.GetType() == typeof(NodeIntern<TData>))
            {
                // zoberiem toho syna ktorý je podľa kľúča
                // ak ten vrchol neexistuje tak hodím exception
                var internNode = (NodeIntern<TData>) node;
                if (!bitArray[index])
                {
                    if (internNode.LeftSon is null)
                    {
                        throw new ArgumentException("Nenašiel sa záznam");
                    }
                    stackNode.Push(internNode.LeftSon);
                    index++;
                }
                else
                {
                    if (internNode.RightSon is null)
                    {
                        throw new ArgumentException("Nenašiel sa záznam");
                    }
                    // toho syna vložím do staku
                    stackNode.Push(internNode.RightSon);
                    index++;
                }
            }
            // Ak je externý
            else
            {
                // zoberiem blok
                var externNode = (NodeExtern<TData>) node;
                if (externNode.Address < 0)
                {
                    throw new ArgumentException("Nenašiel sa záznam");
                }
                var block = _fileManager.GetBlock(externNode.Address);
                // prejdem všetky dáta v bloku
                for (int i = 0; i < block.Count(); i++)
                {
                    // skontrolujem či je to to čo hľadám
                    if (block.GetRecord(i).CompareTo(key) == 0)
                    {
                        returnData = block.GetRecord(i);
                        break;
                    }
                }
                
                // ak sa nenašiel v hlavnom bloku tak idem do preplňujúceho ak existuje
                if (externNode.CountPreplnovaciBlock > 0 && returnData is null)
                {
                    int current = block.NextDataBlock;
                    while (current >= 0)
                    {
                        // načítam blok a skontrolujem dáta v ňom
                        block = _filePreplnovaciManager.GetBlock(current);
                        for (int i = 0; i < block.Count(); i++)
                        {
                            if (block.GetRecord(i).CompareTo(key) == 0)
                            {
                                returnData = block.GetRecord(i);
                                current = -1;
                                break;
                            }
                        }
                        // ak je stále return data null, to znamená že som stále nenašiel záznam a musím pokračovať ďalším preplňujúcim blokom
                        if (returnData is null)
                        {
                            current = block.NextDataBlock;
                        }
                    }
                }
            }
        }
        
        
        // ak som nenašiel záznam tak hodím exception
        if (returnData is null)
        {
            throw new ArgumentException("Nenašiel sa záznam");
        }
        
        return returnData;
    }
    
    /// <summary>
    /// Zmaže dáta pomocou kľúča
    /// </summary>
    /// <param name="key">Kľúč ktorý sa používa pri hľadaní a následnom mazaní</param>
    /// <returns>Vymazané dáta</returns>
    /// <exception cref="ArgumentException">Ak sa mazaný záznam nenašiel</exception>
    public TData Remove(TKey key)
    {
        
        TData returnData = default;
        
        BitArray bitArray = new(TData.GetBytesForHash(key));
        int index = 0;


        NodeExtern<TData>? lastNode = null;
        
        Stack<Node<TData>> stackNode = new();
        stackNode.Push(_root);
        while (stackNode.Count > 0)
        {
            var node = stackNode.Pop();
            if (node.GetType() == typeof(NodeIntern<TData>))
            {
                var internNode = (NodeIntern<TData>) node;
                if (!bitArray[index])
                {
                    if (internNode.LeftSon is null)
                    {
                        throw new ArgumentException("Nenašiel sa záznam");
                    }
                    stackNode.Push(internNode.LeftSon);
                    index++;
                }
                else
                {
                    if (internNode.RightSon is null)
                    {
                        throw new ArgumentException("Nenašiel sa záznam");
                    }
                    stackNode.Push(internNode.RightSon);
                    index++;
                }
            }
            // Ak je externý
            else
            {
                var externNode = (NodeExtern<TData>) node;
                lastNode = externNode;
                if (externNode.Address < 0)
                {
                    throw new ArgumentException("Nenašiel sa záznam");
                }
                var block = _fileManager.GetBlock(externNode.Address);
                // prejdem všetky dáta v bloku
                for (int i = 0; i < block.Count(); i++)
                {
                    // skontrolujem či je to to čo hľadám
                    if (block.GetRecord(i).CompareTo(key) == 0)
                    {
                        // ak áno tak vrátim
                        returnData = block.GetRecord(i);
                        block.RemoverRecord(i);
                        externNode.CountPrimaryData--;
                        Count--;
                        _fileManager.WriteBlock(externNode.Address, block);
                        break;
                    }
                }
                
                // ak sa nenašiel v hlavnom bloku tak idem do preplňujúceho ak existuje
                if (externNode.CountPreplnovaciBlock > 0 && returnData is null)
                {
                    int current = block.NextDataBlock;
                    while (current >= 0)
                    {
                        // načítam blok a skontrolujem dáta vňom
                        block = _filePreplnovaciManager.GetBlock(current);
                        for (int i = 0; i < block.Count(); i++)
                        {
                            if (block.GetRecord(i).CompareTo(key) == 0)
                            {
                                // ak áno tak vrátim
                                returnData = block.GetRecord(i);
                                block.RemoverRecord(i);
                                externNode.CountPreplnovaciData--;
                                Count--;
                                _filePreplnovaciManager.WriteBlock(current, block);
                                current = -1; // aby sa ukončil cyklus
                                break;
                            }
                        }
                        // ak je stále return data null, to znamená že som stále nenašiel záznam a musím pokračovať ďalším preplňujúcim blokom
                        if (returnData is null)
                        {
                            current = block.NextDataBlock;
                        }
                    }
                }
            }
        }
        
        
        // ak som nenašiel tak hodím exception
        if (returnData is null)
        {
            throw new ArgumentException("Nenašiel sa záznam");
        }
        
        if (lastNode is null)
        {
            throw new Exception("Toto sa nemalo stať, chyba v Remove pri presípani a zmenšovaní bloku");
        }
        
        // zosypanie robíme iba v tedy ak máme preplňovacie bloky
        if (lastNode.CountPreplnovaciBlock > 0)
        {
            // spočítame či sa oplatí robiť presýpanie, presýpanie sa oplatí robiť v tedy ak sa odstráni toľko dát že vieme vyprázdniť blok
            bool presipanie = false;
            if (lastNode.CountPreplnovaciBlock == 1)
            {
                presipanie = (lastNode.CountPrimaryData + lastNode.CountPreplnovaciData) <= PrimaryFileBlockSize;
            }
            // máme viac preplňujúci blokov
            else
            {
                // rozdiel medzi zapísanímy dátami a dátami ktoré by sa tam vošli musí byť väčší ako veľkosť prepňujúceho bloku
                int zapisaneData = lastNode.CountPrimaryData + lastNode.CountPreplnovaciData;
                int plnaKapacita = PrimaryFileBlockSize + (PreplnovaciFileBlockSize * lastNode.CountPreplnovaciBlock);
                presipanie = plnaKapacita - zapisaneData >= PreplnovaciFileBlockSize;
            }

            if (presipanie)
            {
                // do listu si načítam všetký data a popri tom mažem aj bloky v ktorých sa nachádzali
                List<TData> tmpData = new();
                var block = _fileManager.GetBlock(lastNode.Address);
                tmpData.AddRange(block.GetArrayRecords());
                block.ClearRecords();
                int current = block.NextDataBlock;
                _fileManager.RemoveBlock(lastNode.Address);
                lastNode.Address = -1;
                
                while (current >= 0)
                {
                    block = _filePreplnovaciManager.GetBlock(current);
                    tmpData.AddRange(block.GetArrayRecords());
                    block.ClearRecords();
                    _filePreplnovaciManager.RemoveBlock(current);
                    current = block.NextDataBlock;
                }

                lastNode.CountPreplnovaciBlock = 0;
                lastNode.CountPrimaryData = 0;
                lastNode.CountPreplnovaciData = 0;
                
                // ideme na novo napĺňať
                var tmpPair = _fileManager.GetFreeBlock();
                lastNode.Address = tmpPair.First;
                for (int i = 0; i < PrimaryFileBlockSize; i++)
                {
                    // iba ak máme čo zapisovať
                    if (tmpData.Count > 0)
                    {
                        tmpPair.Second.AddRecord(tmpData[0]);
                        tmpData.RemoveAt(0);
                        lastNode.CountPrimaryData++;
                    }
                    else
                    {
                        break;
                    }
                }
                // ak máme šte dáta tak vytvárame preplňujúci súbor
                if (tmpData.Count > 0)
                {
                    var tmpPairPreplnovak = _filePreplnovaciManager.GetFreeBlock();
                    lastNode.CountPreplnovaciBlock++;
                    tmpPair.Second.NextDataBlock = tmpPairPreplnovak.First;
                    while (tmpData.Count > 0)
                    {
                        for (int i = 0; i < PreplnovaciFileBlockSize; i++)
                        {
                            // iba ak máme čo zapisovať
                            if (tmpData.Count > 0)
                            {
                                tmpPairPreplnovak.Second.AddRecord(tmpData[0]);
                                tmpData.RemoveAt(0);
                                lastNode.CountPreplnovaciData++;
                            }
                            else
                            {
                                break;
                            }
                        }
                        // ak potrebujem ďalší blok tak ho získam
                        if (tmpData.Count > 0)
                        {
                            var tmpTmpPair = _filePreplnovaciManager.GetFreeBlock();
                            lastNode.CountPreplnovaciBlock++;
                            tmpPairPreplnovak.Second.NextDataBlock = tmpTmpPair.First;
                            _filePreplnovaciManager.WriteBlock(tmpPairPreplnovak.First, tmpPairPreplnovak.Second);
                            tmpPairPreplnovak = tmpTmpPair;
                        }
                        else
                        {
                            _filePreplnovaciManager.WriteBlock(tmpPairPreplnovak.First, tmpPairPreplnovak.Second); 
                        }  
                    }
                }
                    
                _fileManager.WriteBlock(tmpPair.First, tmpPair.Second);
            }
        }

        // zmenšovanie stromu
        if (lastNode.CountPreplnovaciBlock <= 0 && lastNode.CountPrimaryData < PrimaryFileBlockSize)
        {
            // ak nemáme záznam tak môžeme vymazať blok
            if (lastNode.CountPrimaryData <=0)
            {
                if (lastNode.Address >= 0)
                {
                    _fileManager.RemoveBlock(lastNode.Address);
                    lastNode.Address = -1;
                }
            }
            
            var parent = lastNode.Parent;
            while (parent is not null)
            {
                // ak je parent root tak bloky už nemôžem spájať
                if (parent.Parent is null)
                {
                    // ak je niektorý z potomkov extern node a má adresu ktorá je väčšia alebo rovná ako 0 ale má 0 prvkov tak ho vymažem
                    if (parent.LeftSon is not null)
                    {
                        if (parent.LeftSon.GetType() == typeof(NodeExtern<TData>))
                        {
                            var leftSon = (NodeExtern<TData>)parent.LeftSon;
                            if (leftSon.Address >= 0 && leftSon.CountPrimaryData + leftSon.CountPreplnovaciData == 0)
                            {
                                _fileManager.RemoveBlock(leftSon.Address);
                                leftSon.Address = -1;
                                parent.LeftSon = null;
                            }
                        }
                    }
                    if (parent.RightSon is not null)
                    {
                        if (parent.RightSon.GetType() == typeof(NodeExtern<TData>))
                        {
                            var rightSon = (NodeExtern<TData>)parent.RightSon;
                            if (rightSon.Address >= 0 && rightSon.CountPrimaryData + rightSon.CountPreplnovaciData == 0)
                            {
                                _fileManager.RemoveBlock(rightSon.Address);
                                rightSon.Address = -1;
                                parent.RightSon = null;
                            }
                        }
                    }
                    break;
                }
                
                // spočítam či viem spojiť
                var celovyPocet = 0;
                
                bool isLeftSonExtern = false;
                bool isRightSonExtern = false;
                if (parent.LeftSon is not null)
                {
                    if (parent.LeftSon.GetType() == typeof(NodeExtern<TData>))
                    {
                        celovyPocet += ((NodeExtern<TData>) parent.LeftSon).CountPrimaryData + ((NodeExtern<TData>) parent.LeftSon).CountPreplnovaciData;
                        isLeftSonExtern = true;
                    }
                    // ak to je interný node tak nemôžem pokračovať
                    else
                    {
                        break;
                    }
                }
                if (parent.RightSon is not null)
                {
                    if (parent.RightSon.GetType() == typeof(NodeExtern<TData>))
                    {
                        celovyPocet += ((NodeExtern<TData>) parent.RightSon).CountPrimaryData + ((NodeExtern<TData>) parent.RightSon).CountPreplnovaciData;
                        isRightSonExtern = true;
                    }
                    // ak to je interný node tak nemôžem pokračovať
                    else
                    {
                        break;
                    }
                }
                
                // ak je celkový počet menší ako je BF tak spojím
                if (celovyPocet <= PrimaryFileBlockSize)
                {
                    List<TData> listData = new(celovyPocet);
                    if (isLeftSonExtern)
                    {
                        var tmpAddress = ((NodeExtern<TData>)parent.LeftSon).Address;
                        if (tmpAddress >= 0)
                        {
                            var tmpBlock = _fileManager.GetBlock(tmpAddress);
                            listData.AddRange(tmpBlock.GetArrayRecords());
                            _fileManager.RemoveBlock(tmpAddress);
                            ((NodeExtern<TData>)parent.LeftSon).Address = -1;
                        }
                    }
                    if (isRightSonExtern)
                    {
                        var tmpAddress = ((NodeExtern<TData>)parent.RightSon).Address;
                        if (tmpAddress >= 0)
                        {
                            var tmpBlock = _fileManager.GetBlock(tmpAddress);
                            listData.AddRange(tmpBlock.GetArrayRecords());
                            _fileManager.RemoveBlock(tmpAddress);
                            ((NodeExtern<TData>)parent.RightSon).Address = -1;
                        }
                    }
                    
                    // vytvorím nový blok a vložím do neho dáta
                    var tmpPair = _fileManager.GetFreeBlock();
                    for (int i = 0; i < listData.Count; i++)
                    {
                        tmpPair.Second.AddRecord(listData[i]);
                    }
                    _fileManager.WriteBlock(tmpPair.First, tmpPair.Second);
                    
                    // namiesto parenta vytvorím nový externýNode a nastavím mu adresu na túto novú adresu
                    var parentParenta = parent.Parent;
                    var tmpNode = new NodeExtern<TData>(tmpPair.First, parentParenta);
                    tmpNode.CountPrimaryData = listData.Count;

                    bool isLeftson = false;
                    if (parentParenta.LeftSon is not null)
                    {
                        if (ReferenceEquals(parent, parentParenta.LeftSon))
                        {
                            isLeftson = true;
                        }
                    }
                    
                    if (isLeftson)
                    {
                        parentParenta.LeftSon = tmpNode;
                    }
                    else
                    {
                        parentParenta.RightSon = tmpNode;
                    }

                    parent = parentParenta;
                }
                // inak ukončujem cyklus
                else
                {
                    parent = null;
                }
            }
        }
        return returnData;
    }
    
    public void CloseFile()
    {
        _fileManager.CloseFile();
        _filePreplnovaciManager.CloseFile();
    }


    public void PrintFile()
    {
        Console.WriteLine("------------  PRIMARY FILE ------------");
        for (int i = 0; i < _fileManager.GetBlockCount(); i++)
        {
            Console.WriteLine($"Block {i}");
            var block = _fileManager.GetBlock(i);
            Console.WriteLine(block.ToString());
        }
        
        Console.WriteLine("------------  PREPLNOVACI FILE ------------");
        for (int i = 0; i < _filePreplnovaciManager.GetBlockCount(); i++)
        {
            Console.WriteLine($"Block {i}");
            var block = _filePreplnovaciManager.GetBlock(i);
            Console.WriteLine(block.ToString());
        }
    }


    /// <summary>
    /// Sekvenčný výpis pre primary file
    /// </summary>
    public string GetPrimaryFileSequece()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("------------  PRIMARY FILE ------------");
        for (int i = 0; i < _fileManager.GetBlockCount(); i++)
        {
            sb.AppendLine($"Block {i}");
            var block = _fileManager.GetBlock(i);
            sb.AppendLine(block.ToString());
        }

        return sb.ToString();
    }


    /// <summary>
    /// Sekvenčný výpis pre primary file
    /// </summary>
    public string GetPreplnovaciFileSequece()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("------------  PREPLNOVACI FILE ------------");
        for (int i = 0; i < _filePreplnovaciManager.GetBlockCount(); i++)
        {
            sb.AppendLine($"Block {i}");
            var block = _filePreplnovaciManager.GetBlock(i);
            sb.AppendLine(block.ToString());
        }

        return sb.ToString();
    }

    
    /// <summary>
    /// Trieda pomocná pri ukladaní
    /// </summary>
    private class SaveClass
    {
        public Node<TData> node { get; set; }
        public List<int> bitArray { get; set; }

        public SaveClass(Node<TData> pNode, List<int> pBitArray)
        {
            node = pNode;
            bitArray = pBitArray;
        }
    }
    
    /// <summary>
    /// Uloží štruktúru stromu do súboru
    /// </summary>
    public void Save()
    {
        _fileManager.Save();
        _filePreplnovaciManager.Save();

        using (StreamWriter sw = new StreamWriter(_primaryFileName.Substring(0, _primaryFileName.Length - 4) + "_dataStructure" + ".txt"))
        {
            // zapíšem si celkový počet
            sw.WriteLine(Count);
            
            Stack<SaveClass> stack = new();
        
            stack.Push(new(_root, new()));
            
            // do staku budem postupne vkladať jednotlivé nodes a budem im postupne zvyšovať bity
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                // ak je interný vkladám do staku jeho synov
                if (current.node.GetType() == typeof(NodeIntern<TData>))
                {
                    var internNode = (NodeIntern<TData>) current.node;
                    // musím kontrolovať aj null hodnoty
                    if (internNode.LeftSon is not null)
                    {
                        var bitArrayLeft = new List<int>();
                        bitArrayLeft.AddRange(current.bitArray);
                        bitArrayLeft.Add(0);
                        stack.Push(new(internNode.LeftSon, bitArrayLeft));
                    }

                    if (internNode.RightSon is not null)
                    {
                        var bitArrayRight = new List<int>();
                        bitArrayRight.AddRange(current.bitArray);
                        bitArrayRight.Add(1);
                        stack.Push(new(internNode.RightSon, bitArrayRight));
                    }
                }
                // ak je externý tak zapisujem dáta do súboru
                else
                {
                    var externNode = (NodeExtern<TData>) current.node;
                    sw.WriteLine($"{string.Join("", current.bitArray)};{externNode.Address};{externNode.CountPrimaryData};{externNode.CountPreplnovaciData};{externNode.CountPreplnovaciBlock}");
                }
            }
        }

    }

    /// <summary>
    /// Načíta strom zo súboru
    /// </summary>
    public void Load()
    {
        _root = new(null);;
        _fileManager.Load();
        _filePreplnovaciManager.Load();

        using (StreamReader sr = new StreamReader(_primaryFileName.Substring(0, _primaryFileName.Length - 4) + "_dataStructure" + ".txt"))
        {
            // načítanie celkového počtu
            Count = int.Parse(sr.ReadLine());
            
            // až teraz môžeme načítavať zbytok
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                var split = line.Split(';');
                if (split.Length != 5)
                {
                    throw new Exception("Chyba pri načítavaní súboru");
                }
                
                var bitArray = new BitArray(split[0].Select(c => c == '1').ToArray());
                var address = int.Parse(split[1]);
                var countPrimaryData = int.Parse(split[2]);
                var countPreplnovaciData = int.Parse(split[3]);
                var countPreplnovaciBlock = int.Parse(split[4]);


                NodeIntern<TData> current = _root;
                for (int i = 0; i < bitArray.Count; i++)
                {
                    // skontrolujeme či je posledný, ak áno vkladám externý node
                    bool last = i == bitArray.Count - 1;
                    
                    // ak je 0 tak vkladám doľava inak doprava
                    if (!bitArray[i])
                    {
                        if (!last)
                        {
                            if (current.LeftSon is null)
                            {
                                current.LeftSon = new NodeIntern<TData>(current);
                            }
                            current = (NodeIntern<TData>) current.LeftSon;
                        }
                        else
                        {
                            current.LeftSon = new NodeExtern<TData>(address, current, countPrimaryData, countPreplnovaciData, countPreplnovaciBlock);
                        }
                    }
                    else
                    {
                        if (!last)
                        {
                            if (current.RightSon is null)
                            {
                                current.RightSon = new NodeIntern<TData>(current);
                            }
                            current = (NodeIntern<TData>) current.RightSon;
                        }
                        else
                        {
                            current.RightSon = new NodeExtern<TData>(address, current, countPrimaryData, countPreplnovaciData, countPreplnovaciBlock);
                        }
                    }
                }
            }
        }
        
    }
    
}