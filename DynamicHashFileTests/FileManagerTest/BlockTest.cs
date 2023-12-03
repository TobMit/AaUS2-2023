using System.Diagnostics.CodeAnalysis;
using System.Text;
using DynamicHashFileStructure.StructureClasses;

namespace DynamicHashFileTests.FileManagerTest;

public class BlockTest
{
    public class TestClass : IRecordData<TestClass>, IEquatable<TestClass>
    {
        private static int MAX_STRING_LENGTH = 20;
        private static int PRIME_NUMBER = 541;
        
        public TestClass(int id, float floatValue, double doubleValue, string stringValue, string stringValue2)
        {
            Id = id;
            FloatValue = floatValue;
            DoubleValue = doubleValue;
            StringValue1 = stringValue;
            ValidString1 = stringValue.Length;
            StringValue2 = stringValue2;
            ValidString2 = stringValue2.Length;
        }

        public int Id { get; set; }
        public float FloatValue { get; set; }

        public double DoubleValue { get; set; }
        public string StringValue1 { get; set; }
        public int ValidString1 { get; set; }
        
        public string StringValue2 { get; set; }
        public int ValidString2 { get; set; }


        public int CompareTo(TestClass? other)
        {
            if (ReferenceEquals(null, other)) return -1;
            return this.Equals(other) ? 0 : -1;
        }
        
        /// <summary>
        /// Porovnáva či sú 2 pointy rovnaké.
        /// </summary>
        /// <param name="obj">Object na test rovnosti</param>
        /// <returns>True ak sú rovnaké, false ak niesú rovnaké</returns>
        public override bool Equals([NotNullWhen(true)] object? obj) => obj is TestClass && Equals((TestClass)obj);
        public bool Equals(TestClass? other)
        {
            return this.Id == other.Id && Math.Abs(this.FloatValue - other.FloatValue) < float.Epsilon && Math.Abs(this.DoubleValue - other.DoubleValue) < Double.Epsilon &&
                   this.StringValue1 == other.StringValue1 && this.StringValue2 == other.StringValue2;
        }

        public static int GetSize()
        {
            //      ID              Float           Double          Valid1              String1         Valid2          String2
            return sizeof(int) + sizeof(float) + sizeof(double) + sizeof(int) + MAX_STRING_LENGTH + sizeof(int) + MAX_STRING_LENGTH;
        }

        public byte[] GetBytes()
        {
            List<byte> bytes = new List<byte>();
            bytes.AddRange(BitConverter.GetBytes(Id));
            bytes.AddRange(BitConverter.GetBytes(FloatValue));
            bytes.AddRange(BitConverter.GetBytes(DoubleValue));
            bytes.AddRange(BitConverter.GetBytes(ValidString1));
            bytes.AddRange(Encoding.ASCII.GetBytes(StringValue1.PadRight(MAX_STRING_LENGTH)));
            bytes.AddRange(BitConverter.GetBytes(ValidString1));
            bytes.AddRange(Encoding.ASCII.GetBytes(StringValue2.PadRight(MAX_STRING_LENGTH)));
            return bytes.ToArray();
        }

        public static object FromBytes(byte[] bytes)
        {
            int offset = 0;
            int id = BitConverter.ToInt32(bytes, offset);
            offset += sizeof(int);
            float floatValue = BitConverter.ToSingle(bytes, offset);
            offset += sizeof(float);
            double doubleValue = BitConverter.ToDouble(bytes, offset);
            offset += sizeof(double);
            int validString1 = BitConverter.ToInt32(bytes, offset);
            offset += sizeof(int);
            string stringValue1 = Encoding.ASCII.GetString(bytes, offset, MAX_STRING_LENGTH).Trim();
            offset += MAX_STRING_LENGTH;
            int validString2 = BitConverter.ToInt32(bytes, offset);
            offset += sizeof(int);
            string stringValue2 = Encoding.ASCII.GetString(bytes, offset, MAX_STRING_LENGTH).Trim();
            return new TestClass(id, floatValue, doubleValue, stringValue1, stringValue2);
        }

        /// <summary>
        /// K mod hasovacia funkcia
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public byte[] GetBytesForHash()
        {
            int mod = Id % PRIME_NUMBER;
            return BitConverter.GetBytes(mod);
        }

        // operátor rovná sa
        public static bool operator ==(TestClass? a, TestClass? b)
        {
            // ak sú obe hodnoty null, vrátime true
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
            {
                return true;
            }
            // ak je jedna hodnota null, vrátime false
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
            {
                return false;
            }
            return a.Id == b.Id && Math.Abs(a.FloatValue - b.FloatValue) < float.Epsilon && Math.Abs(a.DoubleValue - b.DoubleValue) < Double.Epsilon &&
                   a.StringValue1 == b.StringValue1 && a.StringValue2 == b.StringValue2;
        }

        public static bool operator !=(TestClass? a, TestClass? b)
        {
            return !(a == b);
        }

        public override string ToString()
        {
            return $"ID: {Id}, Float: {FloatValue}, Double: {DoubleValue}, String1: {StringValue1}, String2: {StringValue2}";
        }

        public int CompareTo(object? obj)
        {
            
            return this.Equals((TestClass)obj) ? 0 : -1;
        }
    }
    
    private Block<TestClass> _block;
    private TestClass _testClass1;
    
    [SetUp]
    public void setUp()
    {
        _testClass1 = new TestClass(1, 1.1f, 1.1, "String jeden", "sTring dva");
        _block = new Block<TestClass>(3,_testClass1);
    }
    
    [Test]
    public void TestClass_GetSize()
    {
        Assert.That(_testClass1.GetBytes().Length, Is.EqualTo(TestClass.GetSize()));
    }
    
    [Test]
    public void TestClass_FromBytes()
    {
        TestClass testClass2 = (TestClass)TestClass.FromBytes(_testClass1.GetBytes());
        Assert.NotNull(testClass2);
        Assert.True(testClass2 == _testClass1);
        Assert.That(testClass2, Is.EqualTo(_testClass1));
    }
    
    [Test]
    public void Block_Constructor()
    {
        var ex = Assert.Throws<ArgumentException>(() => new Block<TestClass>(0, _testClass1));
        Assert.That(ex.Message, Is.EqualTo("Block faktor nemôže byť záporný alebo nulový"));
        
        // teraz skontrolujeme že nehodí exception
        Block<TestClass> block = new(3, _testClass1);
        Assert.NotNull(block);
    }
    
    [Test]
    public void Block_GetSize()
    {
        Assert.That(_block.GetBytes().Length, Is.EqualTo(Block<TestClass>.GetSize()));
    }
    
    [Test]
    public void Block_FromBytes()
    {
        Block<TestClass> block2 =  (Block<TestClass>)Block<TestClass>.FromBytes(_block.GetBytes());
        Assert.NotNull(block2);
        Assert.True(_block.CompareTo(block2) == 0);
    }

    [Test]
    public void Block_Record()
    {
        Assert.False(_block.IsFull());
        Assert.That(_block.GetRecord(0), Is.EqualTo(_testClass1));
        var ex = Assert.Throws<IndexOutOfRangeException>(() => _block.GetRecord(1));
        Assert.That(ex.Message, Is.EqualTo("Index je mimo rozsah bloku"));
        
        Assert.That(_block.Count(), Is.EqualTo(1));
        
        TestClass testClass2 = new TestClass(2, 2.2f, 2.2, "String dva", "sTring tri");
        TestClass testClass3 = new TestClass(3, 3.3f, 3.3, "String tri", "sTring štyri");
        
        _block.AddRecord(testClass2);
        Assert.That(_block.Count(), Is.EqualTo(2));
        _block.AddRecord(testClass3);
        Assert.That(_block.Count(), Is.EqualTo(3));
        
        Assert.True(_block.IsFull());
        Assert.That(_block.GetRecord(0), Is.EqualTo(_testClass1));
        Assert.That(_block.GetRecord(1), Is.EqualTo(testClass2));
        Assert.That(_block.GetRecord(2), Is.EqualTo(testClass3));
        
        var listRekorodov = _block.GetArrayRecords();
        Assert.That(listRekorodov.Count, Is.EqualTo(3));
        Assert.That(listRekorodov[0], Is.EqualTo(_testClass1));
        Assert.That(listRekorodov[1], Is.EqualTo(testClass2));
        Assert.That(listRekorodov[2], Is.EqualTo(testClass3));
    }
    
    [Test]
    public void Block_RemoveRecordFromLast()
    {
        Assert.That(_block.Count(), Is.EqualTo(1));
        
        TestClass testClass2 = new TestClass(2, 2.2f, 2.2, "String dva", "sTring tri");
        TestClass testClass3 = new TestClass(3, 3.3f, 3.3, "String tri", "sTring štyri");
        
        _block.AddRecord(testClass2);
        Assert.That(_block.Count(), Is.EqualTo(2));
        _block.AddRecord(testClass3);
        Assert.That(_block.Count(), Is.EqualTo(3));
        
        var tmp = _block.RemoverRecord(2);
        Assert.That(tmp, Is.EqualTo(testClass3));
        Assert.That(_block.Count(), Is.EqualTo(2));
        
        tmp = _block.RemoverRecord(1);
        Assert.That(tmp, Is.EqualTo(testClass2));
        Assert.That(_block.Count(), Is.EqualTo(1));
        
        tmp = _block.RemoverRecord(0);
        Assert.That(tmp, Is.EqualTo(_testClass1));
        Assert.That(_block.Count(), Is.EqualTo(0));
        
        var ex = Assert.Throws<IndexOutOfRangeException>(() => _block.RemoverRecord(0));
        Assert.That(ex.Message, Is.EqualTo("Index je mimo rozsah bloku"));
        
        Assert.False(_block.IsFull());
    }

    [Test]
    public void Block_RemoveRecordFromBegining()
    {
        Assert.That(_block.Count(), Is.EqualTo(1));
        
        TestClass testClass2 = new TestClass(2, 2.2f, 2.2, "String dva", "sTring tri");
        TestClass testClass3 = new TestClass(3, 3.3f, 3.3, "String tri", "sTring štyri");
        
        _block.AddRecord(testClass2);
        Assert.That(_block.Count(), Is.EqualTo(2));
        _block.AddRecord(testClass3);
        Assert.That(_block.Count(), Is.EqualTo(3));
        
        var tmp = _block.RemoverRecord(0);
        Assert.That(tmp, Is.EqualTo(_testClass1));
        Assert.That(_block.Count(), Is.EqualTo(2));
        
        // toto je testClass 3 preto, lebo, sa posledné mazané miesto nahradí 
        tmp = _block.RemoverRecord(0);
        Assert.That(tmp, Is.EqualTo(testClass3));
        Assert.That(_block.Count(), Is.EqualTo(1));
        
        tmp = _block.RemoverRecord(0);
        Assert.That(tmp, Is.EqualTo(testClass2));
        Assert.That(_block.Count(), Is.EqualTo(0));
        
        var ex = Assert.Throws<IndexOutOfRangeException>(() => _block.RemoverRecord(0));
        Assert.That(ex.Message, Is.EqualTo("Index je mimo rozsah bloku"));
        
        Assert.False(_block.IsFull());
        
        _block.ClearRecords();
        Assert.That(_block.Count(), Is.EqualTo(0));
    }
    
    [Test]
    public void Block_RemoveRecordFromMiddle()
    {
        Assert.That(_block.Count(), Is.EqualTo(1));
        
        TestClass testClass2 = new TestClass(2, 2.2f, 2.2, "String dva", "sTring tri");
        TestClass testClass3 = new TestClass(3, 3.3f, 3.3, "String tri", "sTring štyri");
        
        _block.AddRecord(testClass2);
        Assert.That(_block.Count(), Is.EqualTo(2));
        _block.AddRecord(testClass3);
        Assert.That(_block.Count(), Is.EqualTo(3));
        
        var tmp = _block.RemoverRecord(1);
        Assert.That(tmp, Is.EqualTo(testClass2));
        Assert.That(_block.Count(), Is.EqualTo(2));
        
        tmp = _block.RemoverRecord(1);
        Assert.That(tmp, Is.EqualTo(testClass3));
        Assert.That(_block.Count(), Is.EqualTo(1));
        
        tmp = _block.RemoverRecord(0);
        Assert.That(tmp, Is.EqualTo(_testClass1));
        Assert.That(_block.Count(), Is.EqualTo(0));
        
        var ex = Assert.Throws<IndexOutOfRangeException>(() => _block.RemoverRecord(0));
        Assert.That(ex.Message, Is.EqualTo("Index je mimo rozsah bloku"));
        
        Assert.False(_block.IsFull());
    }
}