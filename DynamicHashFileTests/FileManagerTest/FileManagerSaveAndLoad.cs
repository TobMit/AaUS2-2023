using DynamicHashFileStructure.StructureClasses;

namespace DynamicHashFileTests.FileManagerTest;

public class FileManagerSaveAndLoad
{
    private const string FileName = "testFileManager.bin";
    private FileManager<BlockTest.TestClass> _manager;

    private const int BlockFactor = 3;
    
    [SetUp]
    public void Setup()
    {
        _manager = new(BlockFactor, FileName);
        for (int i = 0; i < 10; i++)
        {
            var tmp = _manager.GetFreeBlock();
            tmp.Second.AddRecord(new BlockTest.TestClass(i, i + 0.1f, i + 0.2, $"string {i}", $"sTring {i}"));
            _manager.WriteBlock(tmp.First, tmp.Second);
        }
    }
    
    [TearDown]
    public void tearDown()
    {
        _manager.CloseFile();
        File.Delete(FileName);
    }

    [Test]
    public void SaveAndLoad()
    {
        _manager.Save();
        _manager.CloseFile();
        
        _manager = new(BlockFactor, FileName);
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(10));
        _manager.Load();
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(10));
        
        for (int i = 1; i < 10; i++)
        {
            var tmpBlock = _manager.GetBlock(i);
            Assert.That(tmpBlock.GetRecord(0), Is.EqualTo(new BlockTest.TestClass(i, i + 0.1f, i + 0.2, $"string {i}", $"sTring {i}")));
        }
    }
    
    [Test]
    public void RemoveSaveAndLoadWrite()
    {
        for (int i = 1; i < 5; i++)
        {
            _manager.RemoveBlock(i);
        }
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(10));
        Assert.That(_manager.BlockUsedCount, Is.EqualTo(6));
        _manager.Save();
        _manager.CloseFile();
        
        _manager = new(BlockFactor, FileName);
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(10));
        _manager.Load();
        Assert.That(_manager.GetBlockCount(), Is.EqualTo(10));
        
        for (int i = 5; i < 10; i++)
        {
            var tmpBlock = _manager.GetBlock(i);
            Assert.That(tmpBlock.GetRecord(0), Is.EqualTo(new BlockTest.TestClass(i, i + 0.1f, i + 0.2, $"string {i}", $"sTring {i}")));
        }

        for (int i = 5 - 1; i >= 1; i--)
        {
            Assert.That(_manager.GetFreeBlock().First, Is.EqualTo(i));
        }
    }
}