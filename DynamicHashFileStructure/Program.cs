using DynamicHashFileStructure.StructureClasses;

public class Program
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

        public string ToString()
        {
            return ID.ToString();
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
    
    private static bool parallel = false;
    
    private static int MAX_UNITS = 1000000;
    private static int MAX_TEST = 100000;
    
    private static int latestLowest = int.MaxValue;
    private static int seed = 0;
    private static int maxSeed = 10;
    //private static int maxSeed = int.MaxValue;
    public static void Main(string[] args)
    {
        
        if (parallel) // not suported for now
        {
            Parallel.For(seed, maxSeed, (iSeed) =>
            {
                try
                {
                    int result = TestInstance(iSeed);
                    if (result < 30)
                    {
                        Console.WriteLine($"Nájdený SEED: {iSeed}");
                        return;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"---------------------------Error v seede: {iSeed} \n {e.Message}");
                    return;
                }
            });
        }
        else
        {
            for (int i = seed; i < maxSeed; i++)
            {
                latestLowest = TestInstance(i);
                
                if (latestLowest <= 30)
                {
                    Console.WriteLine($"Nájdený SEED: {i}");
                    return;
                }
            }
        }
    }

    public static int TestInstance(int Seed)
    {
        Console.WriteLine($"SEED: {Seed}");
        
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

        DynamicHashFile<int, TestClass> dhf = new();
        
        for (int i = 0; i < MAX_TEST; i++)
        {
            
            // if (i == 1000)
            // {
                // Console.WriteLine("som tu");
            // }
            
            int index = rnd.Next(0, toInsert.Count);
            var toInsertData = toInsert[index];
            dhf.Insert( toInsertData.ID ,toInsertData);
            //toDelete.Add(toInsert[index]);
            toInsert.RemoveAt(index);
            
            //Console.WriteLine(i);

            //dhf.PrintFile();
            
            try
            {
                var findData = dhf.Find(toInsertData.ID);
                if (toInsertData.CompareTo(findData) != 0)
                {
                    seedOk = false;
                    latestLowest = i;
                    Console.WriteLine($"Error in dint Find what should be find at {i} in SEED: {Seed}");
                    return latestLowest;
                }
            }
            catch (Exception e)
            {
                seedOk = false;
                latestLowest = i;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error in Find at {i} in SEED: {Seed} \n {e.Message}");
                return latestLowest;
            }
            
            //Console.WriteLine("-----------------------------------");
        }
        
        Console.WriteLine();
        //dhf.PrintFile();
        
        // zmažeme vytvorený file
        dhf.CloseFile();
        File.Delete("primaryData.bin");
        
        if (seedOk)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("SEED OK: " + Seed);
            Console.ForegroundColor = ConsoleColor.White;
        }
        return int.MaxValue;
    }
}