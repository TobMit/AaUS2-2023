using DynamicHashFileStructure.StructureClasses;

namespace DynamicHashFileTests.DynamicHashFileMain;

public class Remove
{
    private DynamicHashFile<int, InsertAndFind.InsertClass> _dynamicHashFile;
    [SetUp]
    public void Setup()
    {
        _dynamicHashFile = new DynamicHashFile<int, InsertAndFind.InsertClass>();
        for (int i = 0; i < 12; i++)
        {
            // fabrikujem kľúče tak aby mi prvý byt ostal rovnaký ale líšil sa v iných
            byte[] fabricedBytes = BitConverter.GetBytes(1);
            fabricedBytes[1] = BitConverter.GetBytes(i)[0];
            
            _dynamicHashFile.Insert(BitConverter.ToInt32(fabricedBytes), new(BitConverter.ToInt32(fabricedBytes), i));
        }
    }
    
    [TearDown]
    public void TearDown()
    {
        _dynamicHashFile.CloseFile();
        File.Delete("primaryData.bin");
        File.Delete("secondaryData.bin");
    }

    [Test]
    public void RemoveZosypanie()
    {
        _dynamicHashFile.PrintFile();
        // hodnoty kľúčov ťahám tak aby som mazal jeden záznam z každého bloku
        Assert.That(_dynamicHashFile.Count, Is.EqualTo(12));
        var tmpRemoved = _dynamicHashFile.Remove(GetFabricedKey(0));
        Assert.That(tmpRemoved.AnotherInt, Is.EqualTo(0));
        var ex = Assert.Throws<ArgumentException>(() => _dynamicHashFile.Remove(GetFabricedKey(0)));
        Assert.That(ex.Message, Is.EqualTo("Nenašiel sa záznam"));
        
        tmpRemoved = _dynamicHashFile.Remove(GetFabricedKey(1));
        Assert.That(tmpRemoved.AnotherInt, Is.EqualTo(1));
        
        tmpRemoved = _dynamicHashFile.Remove(GetFabricedKey(6));
        Assert.That(tmpRemoved.AnotherInt, Is.EqualTo(6));

        Assert.That(_dynamicHashFile.Count, Is.EqualTo(9));
        
        tmpRemoved = _dynamicHashFile.Remove(GetFabricedKey(11));
        Assert.That(tmpRemoved.AnotherInt, Is.EqualTo(11));
        
        tmpRemoved = _dynamicHashFile.Remove(GetFabricedKey(10));
        Assert.That(tmpRemoved.AnotherInt, Is.EqualTo(10));
        
        tmpRemoved = _dynamicHashFile.Remove(GetFabricedKey(9));
        Assert.That(tmpRemoved.AnotherInt, Is.EqualTo(9));

        Assert.That(_dynamicHashFile.Count, Is.EqualTo(6));
        
        tmpRemoved = _dynamicHashFile.Remove(GetFabricedKey(3));
        Assert.That(tmpRemoved.AnotherInt, Is.EqualTo(3));
        
        tmpRemoved = _dynamicHashFile.Remove(GetFabricedKey(2));
        Assert.That(tmpRemoved.AnotherInt, Is.EqualTo(2));
        
        tmpRemoved = _dynamicHashFile.Remove(GetFabricedKey(4));
        Assert.That(tmpRemoved.AnotherInt, Is.EqualTo(4));

        Assert.That(_dynamicHashFile.Count, Is.EqualTo(3));


        _dynamicHashFile.Insert(GetFabricedKey(4), new(GetFabricedKey(4), 4));
        _dynamicHashFile.PrintFile();

    }

    [Test]
    public void RemoveZmensovanie()
    {
        Assert.That(_dynamicHashFile.Count, Is.EqualTo(12));
        
        for (int i = 0; i < 12; i++)
        {
            _dynamicHashFile.Insert(GetFabricedKey2(i), new(GetFabricedKey2(i), i));
        }
        Console.WriteLine("Pred testom");
        _dynamicHashFile.PrintFile();
        for (int i = 0; i < 9; i++)
        {
            _dynamicHashFile.Remove(GetFabricedKey(i));
        }
        
        for (int i = 0; i < 9; i++)
        {
            _dynamicHashFile.Remove(GetFabricedKey2(i));
        }
        Console.WriteLine("Po mazaní");
        _dynamicHashFile.PrintFile();
        Assert.That(_dynamicHashFile.Count, Is.EqualTo(6));
        _dynamicHashFile.Remove(GetFabricedKey(9));
        Assert.That(_dynamicHashFile.Count, Is.EqualTo(5));
        _dynamicHashFile.Remove(GetFabricedKey(10));
        Assert.That(_dynamicHashFile.Count, Is.EqualTo(4));
        Console.WriteLine("Tesne pred zmenšením");
        _dynamicHashFile.PrintFile();
        _dynamicHashFile.Remove(GetFabricedKey2(11));
        //_dynamicHashFile.Remove(GetFabricedKey(11));
        Assert.That(_dynamicHashFile.Count, Is.EqualTo(3));
        _dynamicHashFile.PrintFile(); // vysledok je viditeľný v debug konozole
    }

    private int GetFabricedKey(int i)
    {
        byte[] fabricedBytes = BitConverter.GetBytes(1);
        fabricedBytes[1] = BitConverter.GetBytes(i)[0];
        return BitConverter.ToInt32(fabricedBytes);
    }
    
    private int GetFabricedKey2(int i)
    {
        byte[] fabricedBytes = BitConverter.GetBytes(2);
        fabricedBytes[1] = BitConverter.GetBytes(i)[0];
        return BitConverter.ToInt32(fabricedBytes);
    }
}