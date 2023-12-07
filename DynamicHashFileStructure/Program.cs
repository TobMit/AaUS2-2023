﻿using DynamicHashFileStructure.StructureClasses;

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
    private static int MAX_TEST = 200000;
    private static int STARTUP_FILL_COUNT = 12000;
    private static double PROBABILIT_INSERT_DELETE = 0.55;
    private static double FILL_PROBABILITY = 0.3;
    
    private static int latestLowest = int.MaxValue;
    private static int seed = 0;
    private static int maxSeed = 10;
    //private static int maxSeed = int.MaxValue;
    public static void Main(string[] args)
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

        string primaryFile = $"primaryFile{Seed}.bin";
        string preplnovakFile = $"preplnovakFile{Seed}.bin";
        
        //File.Delete(primaryFile);
        //File.Delete(preplnovakFile);

        DynamicHashFile<int, TestClass> dhf = new(primaryFile, preplnovakFile);
        
        //dhf.PrintFile();

        if (rnd.NextDouble() < FILL_PROBABILITY)
        {
            for (int i = 0; i < STARTUP_FILL_COUNT; i++)
            {
                int index = rnd.Next(0, toInsert.Count);
                var toInsertData = toInsert[index];
                dhf.Insert( toInsertData.ID ,toInsertData);
                var tmp = dhf.Find(toInsertData.ID);
                toDelete.Add(toInsert[index]);
                toInsert.RemoveAt(index);
                if (tmp.CompareTo(toInsertData) != 0)
                {
                    Console.WriteLine("Error in startup fill");
                    return 0;
                }
            }
        }
        //652228
        
        //dhf.Find(652228);
        Console.WriteLine("Prep done, starting test");
        for (int i = 0; i < MAX_TEST; i++)
        {
            //Console.WriteLine(i);
            if (rnd.NextDouble() < PROBABILIT_INSERT_DELETE)
            {
                TestClass toInsertData = default;
                try
                {
                    // insertujeme
                    int index = rnd.Next(0, toInsert.Count);
                    toInsertData = toInsert[index];
                    dhf.Insert(toInsertData.ID, toInsertData);
                    toDelete.Add(toInsert[index]);
                    toInsert.RemoveAt(index);
                }
                catch (Exception e)
                {
                    seedOk = false;
                    latestLowest = i;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error in Insert at {i} in SEED: {Seed} \n {e.Message}");
                    Console.ForegroundColor = ConsoleColor.White;

                    return latestLowest;
                }


                //dhf.PrintFile();
            
                try
                {
                    var findData = dhf.Find(toInsertData.ID);
                    if (toInsertData.CompareTo(findData) != 0)
                    {
                        seedOk = false;
                        latestLowest = i;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Error in dint Find what should be find at {i} in SEED: {Seed}");
                        Console.ForegroundColor = ConsoleColor.White;
                        return latestLowest;
                    }
                }
                catch (Exception e)
                {
                    seedOk = false;
                    latestLowest = i;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error in Find at {i} in SEED: {Seed} \n {e.Message}");
                    Console.ForegroundColor = ConsoleColor.White;

                    return latestLowest;
                }
                //
                // try
                // {
                //     var findData652228 = dhf.Find(652228);
                //     if (findData652228.ID != 652228)
                //     {
                //         seedOk = false;
                //         latestLowest = i;
                //         Console.ForegroundColor = ConsoleColor.Red;
                //         Console.WriteLine($"Error in Insert Find 652228 at {i} in SEED: {Seed}");
                //         Console.ForegroundColor = ConsoleColor.White;
                //         return latestLowest;
                //     }
                //     else
                //     {
                //         Console.WriteLine($"Insert Find 652228 OK at {i} in SEED: {Seed}");
                //     }
                // }
                // catch (Exception e)
                // {
                //     seedOk = false;
                //     latestLowest = i;
                //     Console.ForegroundColor = ConsoleColor.Red;
                //     Console.WriteLine($"Error in Insert Find 652228 at {i} in SEED: {Seed}");
                //     Console.ForegroundColor = ConsoleColor.White;
                //     return latestLowest;
                // }
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
                    try
                    {
                        if (i == 36)
                        {
                            Console.WriteLine("tu som");
                        }
                        int index = rnd.Next(0, toDelete.Count);
                        toDeleteData = toDelete[index];
                        var removed = dhf.Remove(toDeleteData.ID);
                        toInsert.Add(toDelete[index]);
                        toDelete.RemoveAt(index);
                        if (toDeleteData.CompareTo(removed) != 0)
                        {
                            seedOk = false;
                            latestLowest = i;
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"Error in dint Find what should be find at {i} in SEED: {Seed}");
                            Console.ForegroundColor = ConsoleColor.White;

                            return latestLowest;
                        }
                        
                        
                        // kontrola či sa naozaj zmazalo
                        try
                        {
                            var findData = dhf.Find(removed.ID);
                            if (findData.CompareTo(findData) == 0)
                            {
                                seedOk = false;
                                latestLowest = i;
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"Error in Delete at {i}, intem was not removed in SEED: {Seed}");
                                Console.ForegroundColor = ConsoleColor.White;
                                return latestLowest;
                            }
                        }
                        catch (Exception e)
                        {
                            seedOk = true;
                        }
                        //
                        // try
                        // {
                        //     var findData652228 = dhf.Find(652228);
                        //     if (findData652228.ID != 652228)
                        //     {
                        //         seedOk = false;
                        //         latestLowest = i;
                        //         Console.ForegroundColor = ConsoleColor.Red;
                        //         Console.WriteLine($"Error in Delete Find 652228 at {i} in SEED: {Seed}");
                        //         Console.ForegroundColor = ConsoleColor.White;
                        //         return latestLowest;
                        //     }
                        //     else
                        //     {
                        //         Console.WriteLine($"Delete Find 652228 OK at {i} in SEED: {Seed}");
                        //     }
                        // }
                        // catch (Exception e)
                        // {
                        //     seedOk = false;
                        //     latestLowest = i;
                        //     Console.ForegroundColor = ConsoleColor.Red;
                        //     Console.WriteLine($"Error in Delete Find 652228 at {i} in SEED: {Seed}");
                        //     Console.ForegroundColor = ConsoleColor.White;
                        //     return latestLowest;
                        // }
                        
                    }
                    catch (Exception e)
                    {
                        seedOk = false;
                        latestLowest = i;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Error in Delete at {i} in SEED: {Seed}, RemovedID: {toDeleteData.ID} \n {e.Message}");
                        Console.ForegroundColor = ConsoleColor.White;

                        return latestLowest;
                    }
                    
                    
                }
            }
            
            //Console.WriteLine("-----------------------------------");
        }
        
        Console.WriteLine();
        //dhf.PrintFile();
        
        // zmažeme vytvorený file
        dhf.CloseFile();
        //File.Delete("primaryData.bin");
        //File.Delete("secondaryData.bin");
        File.Delete(primaryFile);
        File.Delete(preplnovakFile);
        
        if (seedOk)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("SEED OK: " + Seed);
            Console.ForegroundColor = ConsoleColor.White;
        }
        return int.MaxValue;
    }
}