using System.Text;
using DynamicHashFileStructure.StructureClasses;

namespace DynamicHashFileTests.FileManagerTest;

public class LowLevelFileManagerTest
{
    private const int BlockSize = 8;
    private const string FileName = "testFile.bin";
    private LowLevelFileManager _manager;
    
    private const string Sprava1 = "Blok - 0";
    private const string Sprava2 = "Blok - 1";
    private const string Sprava3 = "Blok - 2";
    
    [OneTimeSetUp]
    public void Setup()
    {
        //Console.WriteLine("set up");
        _manager = new(BlockSize, FileName);
    }
    
    [OneTimeTearDown]
    public void tearDown()
    {
        // zamžeme vytvorený súbor
        //Console.WriteLine("Tear down");
        _manager.CloseFile();
        File.Delete(FileName);
    }

    [Test]
    [Order(0)]
    public void ConstrustorTest()
    {
        
        Assert.That(_manager.BlockCount, Is.EqualTo(1));
        // zatvorenie managera pomocou destruktora
        _manager.CloseFile();
        _manager = new(BlockSize, FileName);
        Assert.That(_manager.BlockCount, Is.EqualTo(1));
    }
    
    [Test]
    [Order(1)]
    public void AddBlock()
    {
        Assert.That(_manager.BlockCount, Is.EqualTo(1));
        var index = _manager.AddBlock();
        Assert.That(index, Is.EqualTo(1));
        Assert.That(_manager.BlockCount, Is.EqualTo(2));
        Assert.That(_manager.GetFileSizeTest(), Is.EqualTo(2*BlockSize));
        Assert.That(_manager.GetAddressTest(index), Is.EqualTo(1*BlockSize));
        
        index = _manager.AddBlock();
        Assert.That(_manager.BlockCount, Is.EqualTo(3));
        Assert.That(_manager.GetFileSizeTest(), Is.EqualTo(3*BlockSize));
        Assert.That(_manager.GetAddressTest(index), Is.EqualTo(2*BlockSize));
        
        index = _manager.AddBlock();
        Assert.That(_manager.BlockCount, Is.EqualTo(4));
        Assert.That(_manager.GetFileSizeTest(), Is.EqualTo(4*BlockSize));
        Assert.That(_manager.GetAddressTest(index), Is.EqualTo(3*BlockSize));
        
        index = _manager.AddBlock();
        Assert.That(_manager.BlockCount, Is.EqualTo(5));
        Assert.That(_manager.GetFileSizeTest(), Is.EqualTo(5*BlockSize));
        Assert.That(_manager.GetAddressTest(index), Is.EqualTo(4*BlockSize));
        
        // otestovanie exception
        _manager.CloseFile();
        var ex = Assert.Throws<FileNotFoundException>(() => _manager.AddBlock());
        Assert.That(ex.Message, Is.EqualTo("File stream is not open."));
        _manager.OpenFile();
        
        // Na záver skontrolujem či sa nám nahodov nezmenila veľkosť
        Assert.That(_manager.BlockCount, Is.EqualTo(5));
        Assert.That(_manager.GetFileSizeTest(), Is.EqualTo(5*BlockSize));
    }

    [Test]
    [Order(2)]
    public void RemoveBlock()
    {
        Assert.That(_manager.GetFileSizeTest(), Is.EqualTo(5*BlockSize));
        Assert.That(_manager.BlockCount, Is.EqualTo(5));
        
        _manager.DeleteLastBlock();
        Assert.That(_manager.BlockCount, Is.EqualTo(4));
        Assert.That(_manager.GetFileSizeTest(), Is.EqualTo(4*BlockSize));
        _manager.DeleteLastBlock();
        Assert.That(_manager.BlockCount, Is.EqualTo(3));
        Assert.That(_manager.GetFileSizeTest(), Is.EqualTo(3*BlockSize));
        
        // otestovanie exception
        _manager.CloseFile();
        var ex = Assert.Throws<FileNotFoundException>(() => _manager.DeleteLastBlock());
        Assert.That(ex.Message, Is.EqualTo("File stream is not open."));
        _manager.OpenFile();
        
        // Na záver skontrolujem či sa nám nahodov nezmenila veľkosť
        Assert.That(_manager.BlockCount, Is.EqualTo(3));
        Assert.That(_manager.GetFileSizeTest(), Is.EqualTo(3*BlockSize)); 
    }
    
    [Test]
    [Order(3)]
    public void WriteToBlock()
    {
        byte[] code1 = Encoding.UTF8.GetBytes(Sprava1);
        byte[] code2 = Encoding.UTF8.GetBytes(Sprava2);
        byte[] code3 = Encoding.UTF8.GetBytes(Sprava3);

        var returnValue = _manager.WriteDataToBlock(0, code1);
        Assert.True(returnValue);
        returnValue = _manager.WriteDataToBlock(1, code2);
        Assert.True(returnValue);
        returnValue = _manager.WriteDataToBlock(2, code3);
        Assert.True(returnValue);
        returnValue = _manager.WriteDataToBlock(3, code3);
        Assert.False(returnValue);
        
        // otestovanie exception
        _manager.CloseFile();
        var ex = Assert.Throws<FileNotFoundException>(() => _manager.WriteDataToBlock(0, code3));
        Assert.That(ex.Message, Is.EqualTo("File stream is not open."));
        _manager.OpenFile();
        
        // Na záver skontrolujem či sa nám nahodov nezmenila veľkosť
        Assert.That(_manager.BlockCount, Is.EqualTo(3));
        Assert.That(_manager.GetFileSizeTest(), Is.EqualTo(3*BlockSize)); 
    }

    [Test]
    [Order(4)]
    public void ReadBlock()
    {
        var returnValue = _manager.ReadBlock(0);
        Assert.NotNull(returnValue);
        Assert.That(Encoding.UTF8.GetString(returnValue), Is.EqualTo(Sprava1));
        returnValue = _manager.ReadBlock(1);
        Assert.NotNull(returnValue);
        Assert.That(Encoding.UTF8.GetString(returnValue), Is.EqualTo(Sprava2));
        returnValue = _manager.ReadBlock(2);
        Assert.NotNull(returnValue);
        Assert.That(Encoding.UTF8.GetString(returnValue), Is.EqualTo(Sprava3));
        returnValue = _manager.ReadBlock(3);
        Assert.Null(returnValue);
        
        // otestovanie exception
        _manager.CloseFile();
        var ex = Assert.Throws<FileNotFoundException>(() => _manager.ReadBlock(0));
        Assert.That(ex.Message, Is.EqualTo("File stream is not open."));
        _manager.OpenFile();
        
        // Na záver skontrolujem či sa nám nahodov nezmenila veľkosť
        Assert.That(_manager.BlockCount, Is.EqualTo(3));
        Assert.That(_manager.GetFileSizeTest(), Is.EqualTo(3*BlockSize));
    }
}