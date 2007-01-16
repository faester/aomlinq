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
            dc.DbBatchSize = 200;

            ExcelWriter ewGenDB_write = new ExcelWriter("tst.xls", "GenDB_write");
            ExcelWriter ewGenDB_read = new ExcelWriter("tst.xls", "GenDB_read");
            ExcelWriter ewGenDB_clear = new ExcelWriter("tst.xls", "GenDB_clear");

            GenDBPerfTests<PerfTestAllPrimitiveTypes> gdbtest = new GenDBPerfTests<PerfTestAllPrimitiveTypes>(ewGenDB_write, ewGenDB_read, ewGenDB_clear);

            ExcelWriter ewDLinqDB_write = new ExcelWriter("tst.xls", "DLinq_write");
            ExcelWriter ewDLinqDB_read = new ExcelWriter("tst.xls", "DLinq_read");
            ExcelWriter ewDLiqnDB_clear = new ExcelWriter("tst.xls", "DLinq_clear");

            DLinqTest dlinqtest = new DLinqTest(ewDLinqDB_write, ewDLinqDB_read, ewDLiqnDB_clear);

            for (int objCount = 5000; objCount <= 30000; objCount += 5000)
            {
                long ms = 0;

                stopwatch.Reset();
                stopwatch.Start();
                gdbtest.PerformTests(objCount);
                ms = stopwatch.ElapsedMilliseconds;
                Console.WriteLine("GenDB: {0} objs in test took {1} ms. {2} objs/sec", objCount , ms, ms > 0 ? (objCount * 1000) / ms : -1);

                stopwatch.Reset();
                stopwatch.Start();
                dlinqtest.PerformTests(objCount);
                ms = stopwatch.ElapsedMilliseconds;
                Console.WriteLine("DLinq: {0} objs in test took {1} ms. {2} objs/sec", objCount, ms, ms > 0 ? (objCount * 1000) / ms : -1);

            }
            ewGenDB_write.Dispose();

            Console.WriteLine("Press return...");
            Console.ReadLine();
        }
    }
}
