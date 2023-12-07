using DynamicHashFileStructure.StructureClasses;

namespace DynamicHashFileTests.FileManagerTest;

public class HighLevelFileManagerRemoveTest
{
    private const string FileName = "testFileManager.bin";
    private FileManager<BlockTest.TestClass> _manager;

    private const int BlockFactor = 3;
    
    [SetUp]
    public void Setup()
    {
        _manager = new(BlockFactor, FileName);
        
        var testRecord = new BlockTest.TestClass(1, 1.1f, 1.2, "string jeden", "sTring dva");
        for (int i = 0; i < 10; i++)
        {
            var tmpPair = _manager.GetFreeBlock();
            testRecord.Id = i;
            tmpPair.Second.AddRecord(testRecord);
            _manager.WriteBlock(tmpPair.First, tmpPair.Second);
        }
    }
    
    [TearDown]
    public void tearDown()
    {
        _manager.CloseFile();
        File.Delete(FileName);
    }

    // testujeme či sa správne zmenšuje počet použitých blokov aj veľkosť súboru
    [Test]
    [Order(0)]
    public void RemoveFromBehing()
    {
        Assert.That(_manager.BlockUsedCount, Is.EqualTo(10));
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(10));

        for (int i = 10 - 1; i >= 0; i--)
        {
            _manager.RemoveBlock(i);
        
            Assert.That(_manager.BlockUsedCount, Is.EqualTo(i));
            Assert.That(_manager.GetBlockCount(), Is.EqualTo(i));
        }
    }
    
    
    /// <summary>
    /// Mažem od od začiatku po predposledný prvok
    /// </summary>
    [Test]
    [Order(1)]
    public void RemoveFromFront()
    {
        Assert.That(_manager.BlockUsedCount, Is.EqualTo(10));
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(10));

        // ešte nechceme aby sa zmala koniec, čo by spustilo cyklus na zmazanie súborov
        for (int i = 0; i < 9; i++)
        {
            _manager.RemoveBlock(i);
        
            Assert.That(_manager.BlockUsedCount, Is.EqualTo(10 - i - 1));
            Assert.That(_manager.GetBlockCount(), Is.EqualTo(10));
        }
    }

    /// <summary>
    /// Kontrolujem či mám spravne nalinkované dáta
    /// </summary>
    [Test]
    [Order(2)]
    public void RemoveFromFrontLinkTest()
    {
        Assert.That(_manager.BlockUsedCount, Is.EqualTo(10));
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(10));
        
        _manager.RemoveBlock(0);
        var tmpBlock = _manager.GetBlock(0);
        Assert.That(tmpBlock.NextFreeBlock, Is.EqualTo(-1));
        Assert.That(tmpBlock.LastNextFreeBlock, Is.EqualTo(-1));
        
        _manager.RemoveBlock(1);
        tmpBlock = _manager.GetBlock(0);
        Assert.That(tmpBlock.NextFreeBlock, Is.EqualTo(-1));
        Assert.That(tmpBlock.LastNextFreeBlock, Is.EqualTo(1));
        tmpBlock = _manager.GetBlock(1);
        Assert.That(tmpBlock.NextFreeBlock, Is.EqualTo(0));
        Assert.That(tmpBlock.LastNextFreeBlock, Is.EqualTo(-1));
        
        for (int i = 2; i < 9; i++)
        {
            _manager.RemoveBlock(i);
            tmpBlock = _manager.GetBlock(i - 1);
            Assert.That(tmpBlock.NextFreeBlock, Is.EqualTo( i - 2));
            Assert.That(tmpBlock.LastNextFreeBlock, Is.EqualTo(i));
            tmpBlock = _manager.GetBlock(i);
            Assert.That(tmpBlock.NextFreeBlock, Is.EqualTo(i - 1));
            Assert.That(tmpBlock.LastNextFreeBlock, Is.EqualTo(-1));
        }
    }

    /// <summary>
    /// Mažem od predposledného prvku ku poslednému
    /// </summary>
    [Test]
    [Order(3)]
    public void RemoveBeforeFirstFreeBlock()
    {
        Assert.That(_manager.BlockUsedCount, Is.EqualTo(10));
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(10));

        for (int i = 9 - 1; i >= 0; i--)
        {
            _manager.RemoveBlock(i);
        
            Assert.That(_manager.BlockUsedCount, Is.EqualTo(i + 1));
            Assert.That(_manager.GetBlockCount(), Is.EqualTo(10));
        }
    }

    /// <summary>
    /// Kontrolujem či mám spravne nalinkované dáta
    /// </summary>
    [Test]
    [Order(4)]
    public void RemoveBeforeFirstFreeBlockLinkTest()
    {
        Assert.That(_manager.BlockUsedCount, Is.EqualTo(10));
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(10));
        
        _manager.RemoveBlock(8);
        var tmpBlock = _manager.GetBlock(8);
        Assert.That(tmpBlock.NextFreeBlock, Is.EqualTo(-1));
        Assert.That(tmpBlock.LastNextFreeBlock, Is.EqualTo(-1));
        
        _manager.RemoveBlock(7);
        tmpBlock = _manager.GetBlock(8);
        Assert.That(tmpBlock.NextFreeBlock, Is.EqualTo(-1));
        Assert.That(tmpBlock.LastNextFreeBlock, Is.EqualTo(7));
        tmpBlock = _manager.GetBlock(7);
        Assert.That(tmpBlock.NextFreeBlock, Is.EqualTo(8));
        Assert.That(tmpBlock.LastNextFreeBlock, Is.EqualTo(-1));

        for (int i = 7 - 1; i >= 0; i--)
        {
            _manager.RemoveBlock(i);
            tmpBlock = _manager.GetBlock(i + 1);
            Assert.That(tmpBlock.NextFreeBlock, Is.EqualTo(i + 2));
            Assert.That(tmpBlock.LastNextFreeBlock, Is.EqualTo(i));
            tmpBlock = _manager.GetBlock(i);
            Assert.That(tmpBlock.NextFreeBlock, Is.EqualTo(i + 1));
            Assert.That(tmpBlock.LastNextFreeBlock, Is.EqualTo(-1));
        }
    }
    
    [Test]
    [Order(5)]
    public void RemoveFromBetweenTwoFreeBlocks()
    {
        Assert.That(_manager.BlockUsedCount, Is.EqualTo(10));
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(10));
        
        _manager.RemoveBlock(0);
        Assert.That(_manager.BlockUsedCount, Is.EqualTo(9));
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(10));
        var tmpBlock = _manager.GetBlock(0);
        Assert.That(tmpBlock.NextFreeBlock, Is.EqualTo(-1));
        Assert.That(tmpBlock.LastNextFreeBlock, Is.EqualTo(-1));
        
        // v teste sa na preskačku mažú bloky medzi dvoma zmazanými aby sa dobre otestovalo linkovanie
        
        _manager.RemoveBlock(5);
        Assert.That(_manager.BlockUsedCount, Is.EqualTo(8));
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(10));
        tmpBlock = _manager.GetBlock(0);
        Assert.That(tmpBlock.NextFreeBlock, Is.EqualTo(-1));
        Assert.That(tmpBlock.LastNextFreeBlock, Is.EqualTo(5));
        tmpBlock = _manager.GetBlock(5);
        Assert.That(tmpBlock.NextFreeBlock, Is.EqualTo(0));
        Assert.That(tmpBlock.LastNextFreeBlock, Is.EqualTo(-1));
        
        _manager.RemoveBlock(3);
        Assert.That(_manager.BlockUsedCount, Is.EqualTo(7));
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(10));
        tmpBlock = _manager.GetBlock(5);
        Assert.That(tmpBlock.NextFreeBlock, Is.EqualTo(0));
        Assert.That(tmpBlock.LastNextFreeBlock, Is.EqualTo(3));
        tmpBlock = _manager.GetBlock(3);
        Assert.That(tmpBlock.NextFreeBlock, Is.EqualTo(5));
        Assert.That(tmpBlock.LastNextFreeBlock, Is.EqualTo(-1));
        
        
        _manager.RemoveBlock(1);
        Assert.That(_manager.BlockUsedCount, Is.EqualTo(6));
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(10));
        tmpBlock = _manager.GetBlock(3);
        Assert.That(tmpBlock.NextFreeBlock, Is.EqualTo(5));
        Assert.That(tmpBlock.LastNextFreeBlock, Is.EqualTo(1));
        tmpBlock = _manager.GetBlock(1);
        Assert.That(tmpBlock.NextFreeBlock, Is.EqualTo(3));
        Assert.That(tmpBlock.LastNextFreeBlock, Is.EqualTo(-1));
        
        _manager.RemoveBlock(4);
        Assert.That(_manager.BlockUsedCount, Is.EqualTo(5));
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(10));
        tmpBlock = _manager.GetBlock(1);
        Assert.That(tmpBlock.NextFreeBlock, Is.EqualTo(3));
        Assert.That(tmpBlock.LastNextFreeBlock, Is.EqualTo(4));
        tmpBlock = _manager.GetBlock(4);
        Assert.That(tmpBlock.NextFreeBlock, Is.EqualTo(1));
        Assert.That(tmpBlock.LastNextFreeBlock, Is.EqualTo(-1));
    }

    /// <summary>
    /// V tomto teste vymažeme nepretržite celé pole keď odstránime koniec
    /// </summary>
    [Test]
    [Order(6)]
    public void RemoveTailOfFile()
    {
        Assert.That(_manager.BlockUsedCount, Is.EqualTo(10));
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(10));

        for (int i = 1; i < 9; i++)
        {
            _manager.RemoveBlock(i);
        }
        Assert.That(_manager.BlockUsedCount, Is.EqualTo(2));
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(10));
        
        _manager.RemoveBlock(9);
        Assert.That(_manager.BlockUsedCount, Is.EqualTo(1));
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(1));
    }
    
    /// <summary>
    /// V tomto teste vymažeme nepretržite celé pole keď odstránime koniec ale je na konci posledny free block
    /// </summary>
    [Test]
    [Order(6)]
    public void RemoveTailOfFileTest2()
    {
        Assert.That(_manager.BlockUsedCount, Is.EqualTo(10));
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(10));

        for (int i = 9-1; i > 0; i--)
        {
            _manager.RemoveBlock(i);
        }
        Assert.That(_manager.BlockUsedCount, Is.EqualTo(2));
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(10));
        
        _manager.RemoveBlock(9);
        Assert.That(_manager.BlockUsedCount, Is.EqualTo(1));
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(1));
    }
    
    /// <summary>
    /// V tomto overíme či keď sa zmazáva konice sa správne cyklus zastaví a aj nastavia linkovacie hodnoty
    /// </summary>
    [Test]
    [Order(7)]
    public void RemoveTailOfFileWithValidBlock()
    {
        Assert.That(_manager.BlockUsedCount, Is.EqualTo(10));
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(10));

        for (int i = 4; i < 9; i++)
        {
            _manager.RemoveBlock(i);
        }
        Assert.That(_manager.BlockUsedCount, Is.EqualTo(5));
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(10));
        
        _manager.RemoveBlock(1);
        Assert.That(_manager.BlockUsedCount, Is.EqualTo(4));
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(10));
        
        // skontrolujem si či som správne nalinkovaný
        var tmpBlock = _manager.GetBlock(1);
        Assert.That(tmpBlock.NextFreeBlock, Is.EqualTo(8));
        Assert.That(tmpBlock.LastNextFreeBlock, Is.EqualTo(-1));
        tmpBlock = _manager.GetBlock(8);
        Assert.That(tmpBlock.NextFreeBlock, Is.EqualTo(7));
        Assert.That(tmpBlock.LastNextFreeBlock, Is.EqualTo(1));
        
        _manager.RemoveBlock(9);
        Assert.That(_manager.BlockUsedCount, Is.EqualTo(3));
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(4));
        tmpBlock = _manager.GetBlock(1);
        Assert.That(tmpBlock.NextFreeBlock, Is.EqualTo(-1));
        Assert.That(tmpBlock.LastNextFreeBlock, Is.EqualTo(-1));
        
        // skontrolujem či blok na indexe 3 je neporušený
        tmpBlock = _manager.GetBlock(3);
        Assert.NotNull(tmpBlock);
        Assert.That(tmpBlock.Count(), Is.EqualTo(1));
        Assert.That(tmpBlock.GetRecord(0).Id, Is.EqualTo(3));
    }

    [Test]
    [Order(8)]
    public void RemoveTailOfFileWithUsedBlockInBetween()
    {
        _manager.RemoveBlock(1);
        _manager.RemoveBlock(3);
        _manager.RemoveBlock(6);
        _manager.RemoveBlock(2);
        _manager.RemoveBlock(8);
        _manager.RemoveBlock(4);
        Assert.That(_manager.BlockUsedCount, Is.EqualTo(4));
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(10));
        
        _manager.RemoveBlock(9);
        Assert.That(_manager.BlockUsedCount, Is.EqualTo(3));
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(8));
        // blok je neporuseny
        var tmpBlock = _manager.GetBlock(7);
        Assert.NotNull(tmpBlock);
        Assert.That(tmpBlock.Count(), Is.EqualTo(1));
        Assert.That(tmpBlock.GetRecord(0).Id, Is.EqualTo(7));
        
        tmpBlock = _manager.GetBlock(2);
        Assert.That(tmpBlock.NextFreeBlock, Is.EqualTo(6));
        Assert.That(tmpBlock.LastNextFreeBlock, Is.EqualTo(4));
        
        tmpBlock = _manager.GetBlock(4);
        Assert.That(tmpBlock.NextFreeBlock, Is.EqualTo(2));
        Assert.That(tmpBlock.LastNextFreeBlock, Is.EqualTo(-1));
    }

    /// <summary>
    /// Pre pre zýskavanie voľnych blokov je pripravený úsek ktorý je neprerušovaný
    /// </summary>
    [Test]
    [Order(9)]
    public void GetFreeBlockFromContinualRemovedBlock()
    {
        Assert.That(_manager.BlockUsedCount, Is.EqualTo(10));
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(10));

        for (int i = 4; i < 9; i++)
        {
            _manager.RemoveBlock(i);
        }
        Assert.That(_manager.BlockUsedCount, Is.EqualTo(5));
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(10));
        
        for (int i = 9-1; i >=4; i--)
        {
            var tmpPair = _manager.GetFreeBlock();
            //Assert.That(_manager.BlockUsedCount, Is.EqualTo(i + 2));
            Assert.That(_manager.GetBlockCount(), Is.EqualTo(10));
            Assert.That(tmpPair.First, Is.EqualTo(i));
        }
        
        var tmp = _manager.GetFreeBlock();
        Assert.That(_manager.BlockUsedCount, Is.EqualTo(11));
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(11));
        Assert.That(tmp.First, Is.EqualTo(10));
    }
    
    /// <summary>
    /// Pre pre zýskavanie voľnych blokov je prerušovaný úsek, musí sa skákať
    /// </summary>
    [Test]
    [Order(10)]
    public void GetFreeBlockFromNotContinualRemovedBlock()
    {
        Assert.That(_manager.BlockUsedCount, Is.EqualTo(10));
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(10));

        _manager.RemoveBlock(1);
        _manager.RemoveBlock(3);
        _manager.RemoveBlock(6);
        _manager.RemoveBlock(8);
        
        Assert.That(_manager.BlockUsedCount, Is.EqualTo(6));
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(10));
        
        var tmpPair = _manager.GetFreeBlock();
        Assert.That(_manager.BlockUsedCount, Is.EqualTo(7));
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(10));
        Assert.That(tmpPair.First, Is.EqualTo(8));

        var tmpBlock = _manager.GetBlock(6);
        Assert.That(tmpBlock.NextFreeBlock, Is.EqualTo(3));
        Assert.That(tmpBlock.LastNextFreeBlock, Is.EqualTo(-1));
        
        tmpPair = _manager.GetFreeBlock();
        Assert.That(_manager.BlockUsedCount, Is.EqualTo(8));
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(10));
        Assert.That(tmpPair.First, Is.EqualTo(6));
        
        tmpBlock = _manager.GetBlock(3);
        Assert.That(tmpBlock.NextFreeBlock, Is.EqualTo(1));
        Assert.That(tmpBlock.LastNextFreeBlock, Is.EqualTo(-1));
        
        tmpPair = _manager.GetFreeBlock();
        Assert.That(_manager.BlockUsedCount, Is.EqualTo(9));
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(10));
        Assert.That(tmpPair.First, Is.EqualTo(3));
        
        tmpBlock = _manager.GetBlock(1);
        Assert.That(tmpBlock.NextFreeBlock, Is.EqualTo(-1));
        Assert.That(tmpBlock.LastNextFreeBlock, Is.EqualTo(-1));
        
        tmpPair = _manager.GetFreeBlock();
        Assert.That(_manager.BlockUsedCount, Is.EqualTo(10));
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(10));
        Assert.That(tmpPair.First, Is.EqualTo(1));
        
        var tmp = _manager.GetFreeBlock();
        Assert.That(_manager.BlockUsedCount, Is.EqualTo(11));
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(11));
        Assert.That(tmp.First, Is.EqualTo(10));
    }
    
    /// <summary>
    /// Pre pre zýskavanie voľnych blokov je prerušovaný úsek, musí sa skákať
    /// </summary>
    [Test]
    [Order(11)]
    public void GetFreeBlockFromNotContinualFirstBlockIsFreeRemovedBlock()
    {
        Assert.That(_manager.BlockUsedCount, Is.EqualTo(10));
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(10));

        _manager.RemoveBlock(0);
        _manager.RemoveBlock(3);
        _manager.RemoveBlock(6);
        _manager.RemoveBlock(8);
        
        Assert.That(_manager.BlockUsedCount, Is.EqualTo(6));
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(10));
        
        var tmpPair = _manager.GetFreeBlock();
        Assert.That(_manager.BlockUsedCount, Is.EqualTo(7));
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(10));
        Assert.That(tmpPair.First, Is.EqualTo(8));
        
        tmpPair = _manager.GetFreeBlock();
        Assert.That(_manager.BlockUsedCount, Is.EqualTo(8));
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(10));
        Assert.That(tmpPair.First, Is.EqualTo(6));
        
        tmpPair = _manager.GetFreeBlock();
        Assert.That(_manager.BlockUsedCount, Is.EqualTo(9));
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(10));
        Assert.That(tmpPair.First, Is.EqualTo(3));
        
        tmpPair = _manager.GetFreeBlock();
        Assert.That(_manager.BlockUsedCount, Is.EqualTo(10));
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(10));
        Assert.That(tmpPair.First, Is.EqualTo(0));
        
        var tmp = _manager.GetFreeBlock();
        Assert.That(_manager.BlockUsedCount, Is.EqualTo(11));
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(11));
        Assert.That(tmp.First, Is.EqualTo(10));
    }

    [Test]
    [Order(12)]
    public void RemoveToEmptyThenNewBlock()
    {
        Assert.That(_manager.BlockUsedCount, Is.EqualTo(10));
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(10));

        for (int i = 10 - 1; i >= 0; i--)
        {
            _manager.RemoveBlock(i);
        
            Assert.That(_manager.BlockUsedCount, Is.EqualTo(i));
            Assert.That(_manager.GetBlockCount(), Is.EqualTo(i));
        }
        
        Assert.That(_manager.BlockUsedCount, Is.EqualTo(0));
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(0));
        
        var tmpPair = _manager.GetFreeBlock();
        Assert.That(_manager.BlockUsedCount, Is.EqualTo(1));
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(1));
        Assert.That(tmpPair.First, Is.EqualTo(0));
    }
}