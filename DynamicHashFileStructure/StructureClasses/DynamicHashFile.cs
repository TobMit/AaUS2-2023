using System.Collections;
using DynamicHashFileStructure.StructureClasses.Nodes;

namespace DynamicHashFileStructure.StructureClasses;

public class DynamicHashFile<TKey, TData> where TData : IRecordData<TKey>
{

    private const int PrimaryFileBlockSize = 3;
    
    public int Count { get; private set; }
    
    private NodeIntern<TData> _root;

    private FileManager<TData> _fileManager;

    public DynamicHashFile()
    {
        _root = new(null);
        _fileManager = new(PrimaryFileBlockSize, "primaryData.bin");

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

    public void Insert(TData data)
    {
        Stack<TData> stackData = new();
        // vložím data do staku
        stackData.Push(data);
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
            var hash = dataToInsert.GetBytesForHash();
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
                            var tmpBlock = _fileManager.GetFreeBlock();
                            _fileManager.WriteBlock(tmpBlock.First, tmpBlock.Second);
                            internNode.LeftSon = new NodeExtern<TData>(tmpBlock.First, internNode);
                        }
                        // vložím do staku toho syna ktorý pokračuje podla kľúča
                        stackNode.Push(internNode.LeftSon);
                        index++;
                    }
                    else
                    {
                        if (internNode.RightSon == null)
                        {
                            var tmpBlock = _fileManager.GetFreeBlock();
                            _fileManager.WriteBlock(tmpBlock.First, tmpBlock.Second);
                            internNode.RightSon = new NodeExtern<TData>(tmpBlock.First, internNode);
                        }
                        // vložím do staku toho syna ktorý pokračuje podla kľúča
                        stackNode.Push(internNode.RightSon);
                        index++;
                    }
                    // todo ak došli kľúče tak predpokladám že tam je 0 a idem vždy podľa toho syna
                }
                // 2. vrchol je externý
                else
                {
                    var externNode = (NodeExtern<TData>) node;
                    // skontrolujem koľko má uložených dát
                    // ak má menej ako je BF
                    if (externNode.Count < PrimaryFileBlockSize) // todo kým sa nedorieši preplňovací blok
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
                        block.AddRecord(dataToInsert);
                        // zapíšem blok
                        _fileManager.WriteBlock(externNode.Address, block);
                        // zvýšim počet dát
                        externNode.Count++;
                    }
                    // ak je plný
                    else
                    {
                        // todo toto sa dá ešte zlepšiť tým že nepôjdem od rooto
                        // todo ak majú úplne rovnaký hash tak potom riešime cez preplňovací blok
                        
                        // načítam blok postuplne odstránim dáta ktoré sú v bloku a vložím ich do staku
                        var block = _fileManager.GetBlock(externNode.Address);
                        var listRecordov = block.GetArrayRecords();
                        block.ClearRecords();
                        externNode.Count = 0;
                        // zapíšem tento prázdny blok do súboru
                        _fileManager.WriteBlock(externNode.Address, block);
                        foreach (var record in listRecordov)
                        {
                            stackData.Push(record);
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
                        // todo skontrolovať či nie je hash rovnaký potom preplňovací blok
                    }
                }
            }
        }
    }

    //todo lepšie spravť ten kľúč resp hash
    public TData Find(Byte[] keyHash)
    {
        TData returnData = default;
        
        BitArray bitArray = new(keyHash);
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
                    if (block.GetRecord(i).GetBytesForHash().SequenceEqual(keyHash))
                    {
                        // ak áno tak vrátim
                        returnData = block.GetRecord(i);
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
    
    public void CloseFile()
    {
        _fileManager.CloseFile();
    }


    public void PrintFile()
    {
        for (int i = 0; i < _fileManager.GetBlockCount(); i++)
        {
            Console.WriteLine($"Block {i}");
            var block = _fileManager.GetBlock(i);
            Console.WriteLine(block.ToString());
        }
    }
}