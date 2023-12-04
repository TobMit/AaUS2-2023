using System.Collections;
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
                //vyberiem zo staku vrchol
                var node = stackNode.Pop();
                // tu sa môžu stať 2 veci
                // 1. vrchol je interný
                if (node.GetType() == typeof(NodeIntern<TData>))
                {
                    var internNode = (NodeIntern<TData>) node;
                    var tmp = bitArray[index];
                    // skontrolujem do ktorého 
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
                    // ak má menej ako je BF
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
                        // zapíšem blok
                        _fileManager.WriteBlock(externNode.Address, block);
                        // zvýšim počet dát
                        externNode.CountPrimaryData++;
                        Count++;
                    }
                    // ak je plný
                    else
                    {
                        // skontrolujem či som už mynul všetky bity
                        if (index >= bitArray.Length)
                        {
                            // ak áno tak vkladám do preplňovacieho bloku
                            
                            // načítam blok a skontrolujem či má adresu na prepňlovaci blok
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
                                // zapíšem tieto dáta do tohto preplňovacieho bloku
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
                            // todo nemôže byť voľný blok
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
                            // týmto cyklus skončil ale pokračuje sa od znovu s novímy dátami (musím vložiť aj poledné dáta   
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
                        // ak áno tak vrátim
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
                                // ak áno tak vrátim
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
        
        return returnData;
    }
    
    public void CloseFile()
    {
        _fileManager.CloseFile();
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