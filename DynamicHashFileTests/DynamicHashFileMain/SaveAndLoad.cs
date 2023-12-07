using DynamicHashFileStructure.StructureClasses;

namespace DynamicHashFileTests.DynamicHashFileMain;

public class SaveAndLoad
{
    private class SaveClass : IRecordData<int>
    {
        public int ID { get; set; }
        public int AnotherInt { get; set; }

        public SaveClass(int id, int anotherInt)
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
            return new SaveClass(id, another);
        }

        public int CompareTo(int other)
        {
            return ID.CompareTo(other);
        }

        public static byte[] GetBytesForHash(int key)
        {
            return BitConverter.GetBytes(key % 19);
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
    
    private DynamicHashFile<int, SaveClass> _dynamicHashFile;
    [SetUp]
    public void Setup()
    {
        _dynamicHashFile = new DynamicHashFile<int, SaveClass>();
    }
    
    [TearDown]
    public void TearDown()
    {
        _dynamicHashFile.CloseFile();
        File.Delete("primaryData.bin");
        File.Delete("secondaryData.bin");
    }

    [Test]
    public void InsertAndSaveAndLoad()
    {
        for (int i = 0; i < 100; i++)
        {
            _dynamicHashFile.Insert(i, new SaveClass(i, i));
        }
        _dynamicHashFile.Save();
        _dynamicHashFile.CloseFile();
        
        _dynamicHashFile = new DynamicHashFile<int, SaveClass>();
        _dynamicHashFile.Load();
        for (int i = 0; i < 100; i++)
        {
            Assert.True(_dynamicHashFile.Find(i).CompareTo(i) == 0);
        }
        
    }
}