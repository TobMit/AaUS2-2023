using System.Collections;
using DynamicHashFileStructure.StructureClasses.HelperClasses;
using DynamicHashFileStructure.StructureClasses.Nodes;

namespace DynamicHashFileStructure.StructureClasses;

public class DynamicHashFile<TKey, TData> where TData : IRecordData<TKey>
{

    private const int PrimaryFileBlockSize = 5;
    private const int PreplnovaciFileBlockSize = 8;
    
    public int Count { get; private set; }
    
    private NodeIntern<TData> _root;

    private FileManager<TData> _fileManager;
    private FileManager<TData> _filePreplnovaciManager;

    public DynamicHashFile(string primaryFile, string preplnovakFile)
    {
        _root = new(null);
        _fileManager = new(PrimaryFileBlockSize, primaryFile);
        _filePreplnovaciManager = new(PreplnovaciFileBlockSize, preplnovakFile);
        Count = 0;

        InitTree();
    }
    public DynamicHashFile()
    {
        _root = new(null);
        _fileManager = new(PrimaryFileBlockSize, "primaryData.bin");
        _filePreplnovaciManager = new(PreplnovaciFileBlockSize, "secondaryData.bin");
        Count = 0;

        InitTree();
    }

    private void InitTree()
    {
        //Block<TData> initBlock = new(PrimaryFileBlockSize);
        //_root.LeftSon = new NodeExtern<TData>(0, _root);
        //_root.RightSon = new NodeExtern<TData>(_fileManager.GetFreeBlock().First, _root);
        //_fileManager.WriteBlock(0, initBlock);
        //_fileManager.WriteBlock(1, initBlock);
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
            // vložím do staku root a prechádzam vnútroný stak
            stackNode.Push(_root);
            // id vrcholu = 0
            int index = 0;
            
            // vytiahnem data zo staku
            var dataToInsert = stackData.Pop();
            // zýskam hash
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
                    if (!bitArray[index]) // 0 ak je lavý 1 ak je pravý (čiže false a true)
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
                        // skontrolujem či som už mynul všetky bity
                        if (index >= bitArray.Length-1)
                        {
                            // ak áno tak vkladám do preplňovacieho bloku
                            
                            if (externNode.CountPreplnovaciBlock > 0)
                            {
                                // načítam si blok aby som získal adresu z preplňovacieho bloku
                                var mainBlock = _fileManager.GetBlock(externNode.Address);
                                Block<TData>? block = null;
                                
                                int current = mainBlock.NextDataBlock;
                                int lastCurrent = mainBlock.NextDataBlock; // keď mám blok ktorý nemá daľší blok ktorý pokračuje tak potrebujem vedieť jeho adresu
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
                                    throw new Exception("Toto nemalo nastat, chyba v Inserte pri prehľadávanií preplňovacích blokov, block je null!");
                                }
                                if (block.IsFull())
                                {
                                    // tak najdeme nový block
                                    var tmpPair = _filePreplnovaciManager.GetFreeBlock();
                                    externNode.CountPreplnovaciBlock++;
                                    // do pôvodne načítaného bloku pridame linkovaciu adresu a zapíšeme ho
                                    block.NextDataBlock = tmpPair.First;
                                    _filePreplnovaciManager.WriteBlock(lastCurrent, block);
                                    current = tmpPair.First;
                                    block = tmpPair.Second;
                                }
                                // zapíšeme dáta do bloku a uložíme
                                block.AddRecord(dataToInsert.Second);
                                _filePreplnovaciManager.WriteBlock(current, block);
                                Count++;
                                externNode.CountPreplnovaciData++;

                            }
                            // ak nemá tak pridám nový preplňovaci blok
                            else
                            {
                                // vytvorím nový blok z preplňovacieho súboru a uložíme tam dáta
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
                            // načítam blok postuplne odstránim dáta ktoré sú v bloku a vložím ich do staku
                            var block = _fileManager.GetBlock(externNode.Address);
                            var listRecordov = block.GetArrayRecords();
                            block.ClearRecords();
                            externNode.CountPrimaryData = 0;
                            // zapíšem tento prázdny blok do súboru
                            _fileManager.WriteBlock(externNode.Address, block);
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
                            // zamžem voľný blok
                            _fileManager.RemoveBlock(externNode.Address);
                            externNode.Address = -1;
                            // týmto cyklus skončil ale pokračuje sa od znovu s novímy dátami (musím vložiť aj poledné dáta)   
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
    /// <returns>Najedné dáta</returns>
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
            // zýskam vrchol
            var node = stackNode.Pop();
            // budem skontrolujem si typ vrcholu
            // je interny
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
                    // toho syna vložim do staku
                    stackNode.Push(internNode.LeftSon);
                    index++;
                }
                else
                {
                    if (internNode.RightSon is null)
                    {
                        throw new ArgumentException("Nenašiel sa záznam");
                    }
                    // toho syna vložim do staku
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
                        // načítam blok a skontrolujem dáta vňom
                        block = _filePreplnovaciManager.GetBlock(current);
                        for (int i = 0; i < block.Count(); i++)
                        {
                            // skontrolujem či je to to čo hľadám
                            if (block.GetRecord(i).CompareTo(key) == 0)
                            {
                                returnData = block.GetRecord(i);
                                current = -1; // aby sa ukončil cyklus
                                break;
                            }
                        }
                        // ak je stále return data null, to znamená že som stále nenašiel záznam a musim pokračovať daľším preplňujúcim blokom
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
            // zýskam vrchol
            var node = stackNode.Pop();
            // budem skontrolujem si typ vrcholu
            // je interny
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
                    // toho syna vložim do staku
                    stackNode.Push(internNode.LeftSon);
                    index++;
                }
                else
                {
                    if (internNode.RightSon is null)
                    {
                        throw new ArgumentException("Nenašiel sa záznam");
                    }
                    // toho syna vložim do staku
                    stackNode.Push(internNode.RightSon);
                    index++;
                }
            }
            // Ak je externý
            else
            {
                // zoberiem blok
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
                            // skontrolujem či je to to čo hľadám
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
                        // ak je stále return data null, to znamená že som stále nenašiel záznam a musim pokračovať daľším preplňujúcim blokom
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
        
        // robiť zosipanie dát
        // todo pridať do dokumentácie, to že teraz ako to mám tak má výhodu pri inom zreťazený
        if (lastNode is null)
        {
            throw new Exception("Toto sa nemalo stať, chyba v Remove pri presípani a zmenšovaní bloku");
        }
        
        // zosipanie robýme iba v tedy ak máme preplňovaci blok
        if (lastNode.CountPreplnovaciBlock > 0)
        {
            // spočítame či sa oplati robyť presipanie, presipanie sa oplatí robyť v tedy ak sa odstráni toľko dát že vieme vyprázdniť blok

            bool presipanie = false;
            // môžu nastať 2 prípady
            // máme práve 1 preplňujúci blok
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
                // do listu si načítam všetký data a popri tom mažem aj bloky v ktorych sa nachádzali
                List<TData> tmpData = new();
                var block = _fileManager.GetBlock(lastNode.Address);
                tmpData.AddRange(block.GetArrayRecords());
                block.ClearRecords();
                int current = block.NextDataBlock;
                _fileManager.RemoveBlock(lastNode.Address);
                
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
                        tmpPair.Second.AddRecord(tmpData[tmpData.Count - 1]);
                        tmpData.RemoveAt(tmpData.Count - 1);
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
                                tmpPairPreplnovak.Second.AddRecord(tmpData[tmpData.Count - 1]);
                                tmpData.RemoveAt(tmpData.Count - 1);
                                lastNode.CountPreplnovaciData++;
                            }
                            else
                            {
                                break;
                            }
                        }
                        // ak potrebujem daľší blok tak ho zýskam
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
                    // ak je niektorý z potomkov extern node a má adresu ktorá je väčšia ako 0 ale má 0 prvkov tak ho vymažem
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
                
                // spocitam či viem spojiť
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
                    
                    // namiesto parenta vytovrím nový externýNode a nastavím mu adresu na túto novú adresu
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
        
        // Zmenšovanie stromu ak nemá prepňovací blok
        // if (lastNode.CountPreplnovaciBlock <= 0 && lastNode.CountPrimaryData < PrimaryFileBlockSize)
        // {
        //     // budem odstraňovať node dokiaľ nenarazím na nejaký z dátami
        //     while (lastNode is not null)
        //     {
        //         // aby mi neostávali práznde bloky s adresou na pázdny blok
        //         if (lastNode.CountPrimaryData + lastNode.CountPreplnovaciData == 0)
        //         {
        //             _fileManager.RemoveBlock(lastNode.Address);
        //             lastNode.Address = -1;
        //         }
        //         
        //         if (lastNode.Parent.Parent is null)
        //         {
        //             lastNode = null;
        //             break;
        //         }
        //
        //         bool leftSon;
        //         if (lastNode.Parent.LeftSon is null)
        //         {
        //             leftSon = false;
        //         }
        //         else
        //         {
        //             leftSon = ReferenceEquals(lastNode, lastNode.Parent.LeftSon);   
        //         }
        //         if (leftSon)
        //         {
        //             // zistím či má pravého syna
        //             if (lastNode.Parent.RightSon is null)
        //             {
        //                 // tak vyhodím parenta preč
        //                 var parent = lastNode.Parent.Parent;
        //                 // musím zisťiť do ktorej vetvy sa pridať
        //                 bool leftSon1 = ReferenceEquals(lastNode.Parent, parent.LeftSon);
        //                 if (leftSon1)
        //                 {
        //                     parent.LeftSon = lastNode;
        //                     lastNode.Parent = parent;
        //                 }
        //                 else
        //                 {
        //                     parent.RightSon = lastNode;
        //                     lastNode.Parent = parent;
        //                 }
        //                 
        //             }
        //             else
        //             {
        //                 var novyPocet = lastNode.CountPrimaryData;
        //                 // skontrolujeme či je syn externalNode
        //                 if (lastNode.Parent.RightSon.GetType() == typeof(NodeExtern<TData>))
        //                 {
        //                     // ak áno tak skontrolujeme či sa vieme spojiť
        //                     var son = (NodeExtern<TData>)lastNode.Parent.RightSon;
        //                     novyPocet += son.CountPrimaryData;
        //                     novyPocet += son.CountPreplnovaciData;
        //
        //                     if (novyPocet < PrimaryFileBlockSize && son.Address >= 0) //todo check if is correct behavior
        //                     {
        //                         // spájame sa 
        //                         Block<TData> lastNodeBlock = default;
        //                         // // kontrolujeme či máme nastavenú adresu
        //                         if (lastNode.Address >= 0)
        //                         {
        //                             lastNodeBlock = _fileManager.GetBlock(lastNode.Address);
        //                         }
        //                         else
        //                         {
        //                             var tmpPair = _fileManager.GetFreeBlock();
        //                             lastNode.Address = tmpPair.First;
        //                             lastNodeBlock = tmpPair.Second;
        //
        //                         }
        //
        //                         if (son.Address >= 0)
        //                         {
        //                             var sonBlock = _fileManager.GetBlock(son.Address);
        //                             _fileManager.RemoveBlock(son.Address);
        //                             for (int i = 0; i < sonBlock.Count(); i++)
        //                             {
        //                                 lastNodeBlock.AddRecord(sonBlock.GetRecord(i));
        //                                 lastNode.CountPrimaryData++;
        //                             }
        //                         }
        //                         _fileManager.WriteBlock(lastNode.Address, lastNodeBlock);
        //                         // tak vyhodím parenta preč
        //                         var parent = lastNode.Parent.Parent;
        //                         // musím zisťiť do ktorej vetvy sa pridať
        //                         bool leftSon1 = ReferenceEquals(lastNode.Parent, parent.LeftSon);
        //                         if (leftSon1)
        //                         {
        //                             parent.LeftSon = lastNode;
        //                             lastNode.Parent = parent;
        //                         }
        //                         else
        //                         {
        //                             parent.RightSon = lastNode;
        //                             lastNode.Parent = parent;
        //                         }
        //                     }
        //                     else
        //                     {
        //                         // koniec
        //                         lastNode = null;
        //                     }
        //                 }
        //                 else
        //                 {
        //                     // inak končíme
        //                     lastNode = null;
        //                 }
        //                 
        //             }
        //         }
        //         else
        //         {
        //             if (lastNode.Parent.LeftSon is null)
        //             {
        //                 // tak vyhodím parenta preč
        //                 var parent = lastNode.Parent.Parent;
        //                 // musím zisťiť do ktorej vetvy sa pridať
        //                 bool leftSon1 = ReferenceEquals(lastNode.Parent, parent.LeftSon);
        //                 if (leftSon1)
        //                 {
        //                     parent.LeftSon = lastNode;
        //                     lastNode.Parent = parent;
        //                 }
        //                 else
        //                 {
        //                     parent.RightSon = lastNode;
        //                     lastNode.Parent = parent;
        //                 }
        //                 
        //             }
        //             else
        //             {
        //                 // vypočítame si rozdiel
        //                 var novyPocet = lastNode.CountPrimaryData;
        //                 // skontrolujeme či je syn externalNode
        //                 if (lastNode.Parent.LeftSon.GetType() == typeof(NodeExtern<TData>))
        //                 {
        //                     // ak áno tak skontrolujeme či sa vieme spojiť
        //                     var son = (NodeExtern<TData>)lastNode.Parent.LeftSon;
        //                     novyPocet += son.CountPrimaryData;
        //                     novyPocet += son.CountPreplnovaciData;
        //
        //                     if (novyPocet < PrimaryFileBlockSize && son.Address >= 0) //todo check if is correct behavior
        //                     {
        //                         // spájame sa 
        //                         Block<TData> lastNodeBlock = default;
        //                         // kontrolujeme či máme nastavenú adresu
        //                         if (lastNode.Address >= 0)
        //                         {
        //                             lastNodeBlock = _fileManager.GetBlock(lastNode.Address);
        //                         }
        //                         else
        //                         {
        //                             var tmpPair = _fileManager.GetFreeBlock();
        //                             lastNode.Address = tmpPair.First;
        //                             lastNodeBlock = tmpPair.Second;
        //
        //                         }
        //
        //                         if (son.Address >= 0)
        //                         {
        //                             var sonBlock = _fileManager.GetBlock(son.Address);
        //                             _fileManager.RemoveBlock(son.Address);
        //                             for (int i = 0; i < sonBlock.Count(); i++)
        //                             {
        //                                 lastNodeBlock.AddRecord(sonBlock.GetRecord(i));
        //                                 lastNode.CountPrimaryData++;
        //                             }
        //                         }
        //                         _fileManager.WriteBlock(lastNode.Address, lastNodeBlock);
        //                         // tak vyhodím parenta preč
        //                         var parent = lastNode.Parent.Parent;
        //                         // musím zisťiť do ktorej vetvy sa pridať
        //                         bool leftSon1 = ReferenceEquals(lastNode.Parent, parent.LeftSon);
        //                         if (leftSon1)
        //                         {
        //                             parent.LeftSon = lastNode;
        //                             lastNode.Parent = parent;
        //                         }
        //                         else
        //                         {
        //                             parent.RightSon = lastNode;
        //                             lastNode.Parent = parent;
        //                         }
        //                     }
        //                     else
        //                     {
        //                         // koniec
        //                         lastNode = null;
        //                     }
        //                 }
        //                 else
        //                 {
        //                     // inak končíme
        //                     lastNode = null;
        //                 }
        //                 
        //             }
        //         }
        //     }
        // }
        
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
}