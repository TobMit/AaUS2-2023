using DynamicHashFileStructure.StructureClasses;

public class Program
{
    private class TestClass : IRecordData<TestClass>
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

        public static TestClass FromBytes(byte[] bytes)
        {
            return new TestClass()
            {
                ID = BitConverter.ToInt32(bytes)
            };
        }

        public string ToString()
        {
            return ID.ToString();
        }


        public byte[] GetBytesForHash()
        {
            // aby som nemal konflikty pri hashovani
            return BitConverter.GetBytes(ID);
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
    
    public static void Main(string[] args)
    {
        File.Delete("primaryData.bin");
        Random rnd = new Random(0);
        List<TestClass> toInsert = new(MAX_UNITS);
        List<TestClass> toDelete = new(MAX_UNITS);
        
        for (int i = 0; i < MAX_UNITS; i++)
        {
            toInsert.Add(new TestClass()
            {
                ID = i
            });
        }

        DynamicHashFile<TestClass> dhf = new();
        
        for (int i = 0; i < MAX_TEST; i++)
        {
            if (i == 20)
            {
                dhf.PrintFile();
                Console.WriteLine("som tu");
            }
            
            int index = rnd.Next(0, toInsert.Count);
            var toInsertData = toInsert[index];
            dhf.Insert(toInsertData);
            //toDelete.Add(toInsert[index]);
            toInsert.RemoveAt(index);
            
            Console.WriteLine(i);

            //dhf.PrintFile();
            
            try
            {
                var findData = dhf.Find(toInsertData.GetBytesForHash());
                if (toInsertData.CompareTo(findData) != 0)
                {
                    Console.WriteLine("Error at " + i);
                    break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error v find " + i);
                Console.WriteLine(e.Message);
                break;
            }
            
            Console.WriteLine("-----------------------------------");
        }
        
        Console.WriteLine();
        //dhf.PrintFile();
        
        // zmažeme vytvorený file
        dhf.CloseFile();
        File.Delete("primaryData.bin");
        
        Console.WriteLine("Koniec");
    }
}