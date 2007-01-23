using System;
using System.Collections.Generic;
using System.Text;
using GenDB;
using System.Diagnostics;
using System.Threading;

namespace PerformanceTests
{
    class Program
    {
        public static void Main(string[] args)
        {
            DataContext dc = DataContext.Instance;
            dc.DatabaseName = "perftest";
            if (dc.DatabaseExists())
            {
                dc.DeleteDatabase();
            }
            dc.CreateDatabase();
            dc.Init();
            dc.DbBatchSize = 1;

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

            int[] objcounts = {/* 5000, 20000, 50000, */ 100000, 150000, 200000, 250000, 300000, 350000, 
                400000, 500000, 750000, 875000, 1000000};


            foreach (int objCount in objcounts)
            {
                Console.WriteLine("==========================================================");
                for (int r = 0; r < repetitions; r++)
                {
                    Console.Write("Writing {0} objects: ", objCount);
                    long gms = gdbtest.PerformWriteTest(objCount);
                    Console.WriteLine("GenDB used {0} ms.", gms);

                    Console.Write("GenDB reading objects: ");

                    gms = gdbtest.PerformReadTest(objCount);
                    Console.WriteLine("{0} objs in test took {1} ms. {2} objs/sec", objCount, gms, gms > 0 ? (objCount * 1000) / gms : -1);
                    gdbms += gms;

                    Console.Write("Clearing table: ");
                    gms = gdbtest.PerformClearTest();
                    Console.WriteLine("GenDB {0} ms, ComObjSize = {1}", gms, GenDB.DataContext.Instance.CommittedObjectsSize);
                }
                Console.WriteLine(" - - - - - - - - -");
                for (int r = 0; r < repetitions; r++)
                {
                    Console.Write("Writing {0} objects: ", objCount);
                    long dms = dlinqtest.PerformWriteTest(objCount);
                    Console.WriteLine("DLinq used {0} ms", dms);
                    
                    Console.Write("DLinq reading objects: ");
                    dms = dlinqtest.PerformReadTest(objCount);
                    Console.WriteLine("{0} objs in test took {1} ms. {2} objs/sec", objCount, dms, dms > 0 ? (objCount * 1000) / dms : -1);
                    dlms += dms;


                    Console.Write("Clearing table: ");
                    dms = dlinqtest.PerformClearTest();
                    Console.WriteLine("DLinq {0} ms", dms);
                }
                Console.WriteLine("Akkumuleret læsefaktor: {0}", ((double)gdbms) / dlms);
            }
            Console.WriteLine();
            Console.WriteLine("ComObjSize= " + GenDB.DataContext.Instance.CommittedObjectsSize);

            Console.WriteLine("Akkumuleret læsefaktor: {0}", ((double)gdbms) / dlms);
            ewGenDB_write.Dispose();
            ewGenDB_clear.Dispose();
            ewGenDB_read.Dispose();

            ewDLinqDB_write.Dispose();
            ewDLiqnDB_clear.Dispose();
            ewDLinqDB_read.Dispose();
        }
    }
}
