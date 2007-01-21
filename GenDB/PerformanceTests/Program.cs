using System;
using System.Collections.Generic;
using System.Text;
using GenDB;
using System.Diagnostics;

namespace PerformanceTests
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            DataContext dc = DataContext.Instance;

            dc.DatabaseName = "perftest";
            if (dc.DatabaseExists())
            {
                dc.DeleteDatabase();
            }
            dc.CreateDatabase(); 
              
            dc.Init();
            dc.DbBatchSize = 200;

            ExcelWriter ewGenDB_write = new ExcelWriter("tst.xls", "GenDB_write");
            ExcelWriter ewGenDB_read = new ExcelWriter("tst.xls", "GenDB_read");
            ExcelWriter ewGenDB_clear = new ExcelWriter("tst.xls", "GenDB_clear");

            GenDBPerfTests<PerfTestAllPrimitiveTypes> gdbtest = new GenDBPerfTests<PerfTestAllPrimitiveTypes>(ewGenDB_write, ewGenDB_read, ewGenDB_clear);

            ExcelWriter ewDLinqDB_write = new ExcelWriter("tst.xls", "DLinq_write");
            ExcelWriter ewDLinqDB_read = new ExcelWriter("tst.xls", "DLinq_read");
            ExcelWriter ewDLiqnDB_clear = new ExcelWriter("tst.xls", "DLinq_clear");

            DLinqTest dlinqtest = new DLinqTest(ewDLinqDB_write, ewDLinqDB_read, ewDLiqnDB_clear);

            long dlms = 0;
            long gdbms = 0;
            int repetitions = 10;

            for (int objCount = 5000; objCount < 5001; objCount += 5000)
            {
                Console.WriteLine("==========================================================");
                Console.WriteLine("Writing {0} objects", objCount);
                stopwatch.Reset();
                stopwatch.Start();
                gdbtest.PerformWriteTest(objCount);
                Console.WriteLine("GenDB used {0} ms", stopwatch.ElapsedMilliseconds);

                stopwatch.Reset();
                stopwatch.Start();
                dlinqtest.PerformWriteTest(objCount);
                Console.WriteLine("DLinq used {0} ms", stopwatch.ElapsedMilliseconds);

                Console.WriteLine();
                Console.WriteLine("Now performing read tests");


                for (int r = 0; r < repetitions; r++)
                {
                    long ms = 0;

                    stopwatch.Reset();
                    stopwatch.Start();
                    gdbtest.PerformReadTest();
                    ms = stopwatch.ElapsedMilliseconds;
                    Console.WriteLine("GenDB: {0} objs in test took {1} ms. {2} objs/sec", objCount, ms, ms > 0 ? (objCount * 1000) / ms : -1);
                    gdbms += ms;
                    stopwatch.Reset();
                    stopwatch.Start();
                    dlinqtest.PerformReadTest();
                    ms = stopwatch.ElapsedMilliseconds;
                    Console.WriteLine("DLinq: {0} objs in test took {1} ms. {2} objs/sec", objCount, ms, ms > 0 ? (objCount * 1000) / ms : -1);
                    dlms += ms;
                    Console.WriteLine("Akkumuleret læsefaktor: {0}", ((double)gdbms) / dlms);
                    Console.WriteLine();
                }

                Console.WriteLine("Clearing tables...");
                stopwatch.Reset();
                stopwatch.Start();
                gdbtest.PerformClearTest();
                long gendbms = stopwatch.ElapsedMilliseconds;
                dlinqtest.PerformClearTest();
                long dlinqms = stopwatch.ElapsedMilliseconds - gendbms ;
                Console.WriteLine("Clear times: GenDB {0} ms, DLinq {1} ms", gendbms, dlinqms);
                Console.WriteLine("ComObjSize= " + GenDB.DataContext.Instance.CommittedObjectsSize);
            }

            Console.WriteLine("Akkumuleret læsefaktor: {0}", ((double)gdbms) / dlms);
            ewGenDB_write.Dispose();

            Console.WriteLine("Press return...");
            Console.ReadLine();
        }
    }
}
