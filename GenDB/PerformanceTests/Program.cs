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

            for (int objCount = 50000; objCount <= 50000; objCount += 20000)
            {
                Console.WriteLine("==========================================================");
                Console.WriteLine("Writing {0} objects", objCount);
                long gms = gdbtest.PerformWriteTest(objCount);
                Console.WriteLine("GenDB used {0} ms", gms);

                long dms = dlinqtest.PerformWriteTest(objCount);

                Console.WriteLine("DLinq used {0} ms. (Faktor: {1})", dms, (double)gms / dms);

                Console.WriteLine();
                Console.WriteLine("Now performing read tests");

                for (int r = 0; r < repetitions; r++)
                {
                    gms = gdbtest.PerformReadTest(objCount);
                    Console.WriteLine("GenDB: {0} objs in test took {1} ms. {2} objs/sec", objCount, gms, gms > 0 ? (objCount * 1000) / gms : -1);
                    gdbms += gms;

                    dms = dlinqtest.PerformReadTest(objCount);

                    Console.WriteLine("DLinq: {0} objs in test took {1} ms. {2} objs/sec", objCount, dms, dms > 0 ? (objCount * 1000) / dms : -1);
                    dlms += dms;
                    Console.WriteLine("Læsefaktor: {0}", ((double)gms) / dms);
                    Console.WriteLine("Akkumuleret læsefaktor: {0}", ((double)gdbms) / dlms);
                    Console.WriteLine();
                }

                //Console.WriteLine("Clearing tables...");
                
                //long gendbms = gdbtest.PerformClearTest();
                //long dlinqms  = dlinqtest.PerformClearTest();

                //Console.WriteLine("Clear times: GenDB {0} ms, DLinq {1} ms", gendbms, dlinqms);
                Console.WriteLine("ComObjSize= " + GenDB.DataContext.Instance.CommittedObjectsSize);
            }

            Console.WriteLine("Akkumuleret læsefaktor: {0}", ((double)gdbms) / dlms);
            ewGenDB_write.Dispose();
            ewGenDB_clear.Dispose();
            ewGenDB_read.Dispose();

            ewDLinqDB_write.Dispose();
            ewDLiqnDB_clear.Dispose();
            ewDLinqDB_read.Dispose();

            Console.WriteLine("Press return...");
            Console.ReadLine();
        }
    }
}
