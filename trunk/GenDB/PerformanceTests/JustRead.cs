using System;
using System.Collections.Generic;
using System.Text;
using GenDB;
using System.Diagnostics;

namespace PerformanceTests
{
    class JustRead
    {
        static void Main(string[] args)
        {
            DataContext dc = DataContext.Instance;

            dc.DatabaseName = "justread";
            if (!dc.DatabaseExists())
            {
                dc.CreateDatabase();
            }

            dc.Init();
            dc.DbBatchSize = 200;

            GenDBPerfTests<PerfTestAllPrimitiveTypes> gdbtest = 
                new GenDBPerfTests<PerfTestAllPrimitiveTypes>(null, null, null);

            Table<PerfTestAllPrimitiveTypes> tpapt = DataContext.Instance.CreateTable<PerfTestAllPrimitiveTypes>();

            if (args.Length == 0)
            {
                Console.WriteLine("usage: JustRead [objects] [repetitions]");
            }

            int objCount = args.Length > 0 ? int.Parse(args[0]) : 100000;
            int repetitions = args.Length > 1 ? int.Parse(args[1]) : 10;

            int objectsInDB = tpapt.Count;
            Console.WriteLine("Objects in db: {0}", objectsInDB);
            while (objectsInDB < objCount)
            {
                Console.WriteLine("Database contains {0} objects, but should contain {1} objects. Adding 20000 objects.", objectsInDB, objCount);
                gdbtest.PerformWriteTest(20000);
                objectsInDB = tpapt.Count;
            }

            double obsSec = 0;
            long totalMS = 0;

            for (int r = 0; r < repetitions; r++)
            {
                long gms = 0;

                if (gms < 0) { break; }

                gms = gdbtest.PerformReadTest(objCount);
                Console.WriteLine("GenDB: {0} objs in test. {1} ms. {2} objs/sec", objCount, gms, gms > 0 ? (objCount * 1000) / gms : -1);
                obsSec += (objCount * 1000) / gms ;
                totalMS += gms;
            }

            Console.WriteLine();
            Console.WriteLine("Total: {0} ms, {1} objs/sec", totalMS, obsSec / repetitions );

            Console.WriteLine("Press return...");
            Console.ReadLine();
        }
    }
}

