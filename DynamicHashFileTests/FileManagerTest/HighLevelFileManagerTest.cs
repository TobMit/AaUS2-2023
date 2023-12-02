using DynamicHashFileStructure.StructureClasses;

namespace DynamicHashFileTests.FileManagerTest;

public class HighLevelFileManagerTest
{
    private const string FileName = "testFileManager.bin";
    private FileManager<BlockTest.TestClass> _manager;

    private const int BlockFactor = 3;
    
    [SetUp]
    public void Setup()
    {
        _manager = new(BlockFactor, FileName);
    }
    
    [TearDown]
    public void tearDown()
    {
        _manager.CloseFile();
        File.Delete(FileName);
    }
    
    [Test]
    public void ConstructorTest()
    {
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(1));
        _manager.CloseFile();
        var ex = Assert.Throws<ArgumentException>(() => new FileManager<BlockTest.TestClass>(0, FileName));
        Assert.That(ex.Message, Is.EqualTo("Block faktor nemôže byť záporný alebo nulový"));
    }

    [Test]
    public void WriteAndReadBlock()
    {
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(1));
        Assert.That(_manager.BlockUsedCount, Is.EqualTo(0));
        var testRecord = new BlockTest.TestClass(1, 1.1f, 1.2, "string jeden", "sTring dva");
        
        var tmpPair = _manager.GetFreeBlock();
        Assert.That(tmpPair.First, Is.EqualTo(0));
        Assert.That(_manager.BlockUsedCount, Is.EqualTo(1));
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(1));
        tmpPair.Second.AddRecord(testRecord);
        _manager.WriteBlock(tmpPair.First, tmpPair.Second);
        
        var tmp = _manager.GetBlock(0);
        Assert.NotNull(tmp);
        Assert.True(tmp.TestEquals(tmpPair.Second));
        Assert.That(tmp.GetRecord(0), Is.EqualTo(testRecord));
        
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => _manager.WriteBlock(2, tmpPair.Second));
        Assert.That(ex.Message, Is.EqualTo("ID bloku je mimo rozsah (Parameter 'id')"));
        
        ex = Assert.Throws<ArgumentOutOfRangeException>(() => _manager.GetBlock(2));
        Assert.That(ex.Message, Is.EqualTo("ID bloku je mimo rozsah (Parameter 'id')"));
    }
    [Test]
    public void WriteAndReadNewBlock()
    {
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(1));
        var testRecord = new BlockTest.TestClass(1, 1.1f, 1.2, "string jeden", "sTring dva");

        // prvý block
        var tmpPair = _manager.GetFreeBlock();
        Assert.That(tmpPair.First, Is.EqualTo(0));
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(1));
        
        tmpPair.Second.AddRecord(testRecord);
        _manager.WriteBlock(tmpPair.First, tmpPair.Second);
        var tmp = _manager.GetBlock(0);
        Assert.NotNull(tmp);
        Assert.True(tmp.TestEquals(tmpPair.Second));
        Assert.That(tmp.GetRecord(0), Is.EqualTo(testRecord));
        
        // nový daľší block
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(1));
        Assert.That(_manager.BlockUsedCount, Is.EqualTo(1));
        tmpPair = _manager.GetFreeBlock();
        Assert.That(tmpPair.First, Is.EqualTo(1));
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(2));
        Assert.That(_manager.BlockUsedCount, Is.EqualTo(2));
        Assert.That(tmpPair.Second.Count(), Is.EqualTo(0));
        tmpPair.Second.AddRecord(testRecord);
        _manager.WriteBlock(tmpPair.First, tmpPair.Second);
        
        tmp = _manager.GetBlock(1);
        Assert.NotNull(tmp);
        Assert.True(tmp.TestEquals(tmpPair.Second));
        Assert.That(tmp.GetRecord(0), Is.EqualTo(testRecord));
        Assert.That(tmp.Count(), Is.EqualTo(1));
    }
}