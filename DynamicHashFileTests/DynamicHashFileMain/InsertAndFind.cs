using System.Collections;
using DynamicHashFileStructure.StructureClasses;
using DynamicHashFileTests.FileManagerTest;

namespace DynamicHashFileTests.DynamicHashFileMain;

public class InsertAndFind
{
    private class InsertClass : IRecordData<int>
    {
        
        public int ID { get; set; }

        public InsertClass(int id)
        {
            ID = id;
        }
        public int CompareTo(object? obj)
        {
            throw new NotImplementedException();
        }

        public static int GetSize()
        {
            return sizeof(int);
        }

        public byte[] GetBytes()
        {
            return BitConverter.GetBytes(ID);
        }

        public static object FromBytes(byte[] bytes)
        {
            return new InsertClass(BitConverter.ToInt32(bytes));
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
            return $"ID: {ID}";
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
        InsertClass tmpClass = new InsertClass(1);
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
}