using System.Collections;
using System.Runtime.InteropServices.JavaScript;
using DynamicHashFileStructure.StructureClasses;
using DynamicHashFileTests.FileManagerTest;

namespace DynamicHashFileTests.DynamicHashFileMain;

public class InsertAndFind
{
    public class InsertClass : IRecordData<int>
    {
        
        public int ID { get; set; }
        public int AnotherInt { get; set; }

        public InsertClass(int id, int anotherInt)
        {
            ID = id;
            AnotherInt = anotherInt;
        }
        public int CompareTo(object? obj)
        {
            throw new NotImplementedException();
        }

        public static int GetSize()
        {
            return sizeof(int) + sizeof(int);
        }

        public byte[] GetBytes()
        {
            List<Byte> bytes = new List<byte>();
        
            bytes.AddRange(BitConverter.GetBytes(ID));
            bytes.AddRange(BitConverter.GetBytes(AnotherInt));
            return bytes.ToArray();
        }

        public static object FromBytes(byte[] bytes)
        {
            int id = BitConverter.ToInt32(bytes);
            int offset = sizeof(int);
            int another = BitConverter.ToInt32(bytes, offset);
            return new InsertClass(id, another);
        }

        public int CompareTo(int other)
        {
            return ID.CompareTo(other);
        }

        public static byte[] GetBytesForHash(int key)
        {
            byte[] bytes = BitConverter.GetBytes(key);
            byte[] result = new byte[1];
            result[0] = bytes[0];
            return result;
        }

        public int GetKey()
        {
            return ID;
        }

        public override string ToString()
        {
            return $"ID: {ID}, AnotherInt: {AnotherInt}";
        }
    }


    private DynamicHashFile<int, InsertClass> _dynamicHashFile;
    [SetUp]
    public void Setup()
    {
        _dynamicHashFile = new DynamicHashFile<int, InsertClass>();
    }
    
    [TearDown]
    public void TearDown()
    {
        _dynamicHashFile.CloseFile();
        File.Delete("primaryData.bin");
        File.Delete("secondaryData.bin");
    }

    [Test]
    public void InsertTest()
    {
        InsertClass tmpClass = new InsertClass(1, 1);
        _dynamicHashFile.Insert(tmpClass.GetKey(),tmpClass);
        _dynamicHashFile.Insert(tmpClass.GetKey(),tmpClass);
        _dynamicHashFile.Insert(tmpClass.GetKey(),tmpClass);
        _dynamicHashFile.Insert(tmpClass.GetKey(),tmpClass);
        _dynamicHashFile.Insert(tmpClass.GetKey(),tmpClass);
        _dynamicHashFile.Insert(tmpClass.GetKey(),tmpClass);
        _dynamicHashFile.Insert(tmpClass.GetKey(),tmpClass);
        _dynamicHashFile.Insert(tmpClass.GetKey(),tmpClass);
        _dynamicHashFile.Insert(tmpClass.GetKey(),tmpClass);
        _dynamicHashFile.Insert(tmpClass.GetKey(),tmpClass);
        _dynamicHashFile.Insert(tmpClass.GetKey(),tmpClass);
        _dynamicHashFile.Insert(tmpClass.GetKey(),tmpClass);
        
        _dynamicHashFile.PrintFile();
        
        Assert.That(_dynamicHashFile.Count, Is.EqualTo(12));
    }

    [Test]
    public void FindTest()
    {
        for (int i = 0; i < 12; i++)
        {
            // fabrikujem kľúče tak aby mi prvý byt ostal rovnaký ale lýšil sa v iných
            byte[] fabricedBytes = BitConverter.GetBytes(1);
            fabricedBytes[1] = BitConverter.GetBytes(i)[0];
            
            _dynamicHashFile.Insert(BitConverter.ToInt32(fabricedBytes), new(BitConverter.ToInt32(fabricedBytes), i));
        }
        
        _dynamicHashFile.PrintFile();
        Assert.That(_dynamicHashFile.Count, Is.EqualTo(12));
        for (int i = 0; i < 12; i++)
        {
            // fabrikujem kľúče tak aby mi prvý byt ostal rovnaký ale lýšil sa v iných
            byte[] fabricedBytes = BitConverter.GetBytes(1);
            fabricedBytes[1] = BitConverter.GetBytes(i)[0];
            
            var tmp = _dynamicHashFile.Find(BitConverter.ToInt32(fabricedBytes));
            Assert.That(tmp.AnotherInt, Is.EqualTo(i));
        }
    }
}