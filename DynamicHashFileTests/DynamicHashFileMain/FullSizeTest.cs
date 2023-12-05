using DynamicHashFileStructure.StructureClasses;

namespace DynamicHashFileTests.DynamicHashFileMain;

public class FullSizeTest
{
    private class TestClass : IRecordData<int>
    {
        public int ID { get; set; }
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
            return new TestClass()
            {
                ID = BitConverter.ToInt32(bytes)
            };
        }

        public int CompareTo(int other)
        {
            return ID.CompareTo(other);
        }

        public override string ToString()
        {
            return $"{ID}";
        }

        public int CompareTo(object? obj)
        {
            throw new NotImplementedException();
        }


        public static byte[] GetBytesForHash(int key)
        {
            // aby som nemal konflikty pri hashovani
            return BitConverter.GetBytes(key % 7919);
            //return BitConverter.GetBytes(key % 19);
        }

        public int GetKey()
        {
            return ID;
        }

        public int CompareTo(TestClass? other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return ID.CompareTo(other.ID);
        }
    }
    
    private static int MAX_UNITS = 1000000;
    private static int MAX_TEST = 100000;
    private static int STARTUP_FILL_COUNT = 12000;
    private static double PROBABILIT_INSERT_DELETE = 0.55;
    
    private static int seed = 0;
    private static int maxSeed = 5;


    [Test]
    public void FullTestDontFill()
    {
        for (int i = seed; i < maxSeed; i++)
        {
            TestInstance(i, false);
        }
    }
    
    [Test]
    public void FullTestFill()
    {
        for (int i = seed; i < maxSeed; i++)
        {
            TestInstance(i, true);
        }
    }
    
    public void TestInstance(int Seed, bool fill)
    {
        //Console.WriteLine($"SEED: {Seed}");
        
        bool seedOk = true;
        
        Random rnd = new Random(Seed);
        List<TestClass> toInsert = new(MAX_UNITS);
        List<TestClass> toDelete = new(MAX_UNITS);
        
        for (int i = 0; i < MAX_UNITS; i++)
        {
            toInsert.Add(new TestClass()
            {
                ID = i
            });
        }

        string primaryFile = $"primaryFile{Seed}.bin";
        string preplnovakFile = $"preplnovakFile{Seed}.bin";

        DynamicHashFile<int, TestClass> dhf = new(primaryFile, preplnovakFile);

        if (fill)
        {
            Console.WriteLine("som tu");
            for (int i = 0; i < STARTUP_FILL_COUNT; i++)
            {
                int index = rnd.Next(0, toInsert.Count);
                var toInsertData = toInsert[index];
                dhf.Insert( toInsertData.ID ,toInsertData);
                toDelete.Add(toInsert[index]);
                toInsert.RemoveAt(index);
            }
        }
        Assert.That(dhf.Count, Is.EqualTo(fill ? STARTUP_FILL_COUNT : 0));
        
        for (int i = 0; i < MAX_TEST; i++)
        {
            
            if (rnd.NextDouble() < PROBABILIT_INSERT_DELETE)
            {
                // insertujeme
                int index = rnd.Next(0, toInsert.Count);
                var toInsertData = toInsert[index];
                dhf.Insert( toInsertData.ID ,toInsertData);
                toDelete.Add(toInsert[index]);
                toInsert.RemoveAt(index);
                
                Assert.That(dhf.Count, Is.EqualTo(toDelete.Count));


                //dhf.PrintFile();
            
                var findData = dhf.Find(toInsertData.ID);
                Assert.That(findData.ID, Is.EqualTo(toInsertData.ID));
                Assert.That(dhf.Count, Is.EqualTo(toDelete.Count));
            }
            else
            {
                // mažeme
                if (toDelete.Count <= 0)
                {
                    i--; // aby sme zachovali počet operácií
                }
                else
                {
                    TestClass toDeleteData = null;
                    
                    int tmpIndex = rnd.Next(0, toDelete.Count);
                    toDeleteData = toDelete[tmpIndex];
                    TestClass? removed = null;
                    Assert.DoesNotThrow(() => removed = dhf.Remove(toDeleteData.ID));
                    toInsert.Add(toDelete[tmpIndex]);
                    toDelete.RemoveAt(tmpIndex);
                    Assert.That(dhf.Count, Is.EqualTo(toDelete.Count));
                    
                    Assert.NotNull(removed);
                    Assert.That(removed.ID, Is.EqualTo(toDeleteData.ID));
                    Assert.That(dhf.Count, Is.EqualTo(toDelete.Count));

                    var ex = Assert.Throws<ArgumentException>(() => dhf.Find(removed.ID));
                    Assert.That(ex.Message, Is.EqualTo("Nenašiel sa záznam"));
                }
            }
            
        }
        
        dhf.CloseFile();
        File.Delete(primaryFile);
        File.Delete(preplnovakFile);
    }
}