using System;
using System.Collections.Generic;
using System.Text;
using System.Data.DLinq;
using System.Diagnostics;
using System.Query;
using System.Expressions;

namespace PerformanceTests
{
    public class DLinqDB : DataContext
    {
        public DLinqDB(string cnnstr) : base(cnnstr) { }
        public Table<PerfTestAllPrimitiveTypes> Table;
    }

    public class DLinqTest : ReadWriteClearTest

    {
        DLinqDB db = null;
        Table<PerfTestAllPrimitiveTypes> table;
        ExcelWriter ewWrite;
        ExcelWriter ewRead;
        ExcelWriter ewClear;
        int lastInsert = 0;

        public DLinqTest(ExcelWriter ewWrite, ExcelWriter ewRead, ExcelWriter ewClear)
        {
            this.ewWrite = ewWrite;
            this.ewRead = ewRead;
            this.ewClear = ewClear;
            this.db = new DLinqDB ("server=.;database=dlinqperf;Integrated Security=SSPI");
            if (db.DatabaseExists())
            {
                Console.WriteLine("Deleting DLinq db");
                db.DeleteDatabase();
            }
            db.CreateDatabase();
            table = db.Table;
            db.SubmitChanges();
        }

        public long PerformAllTests(int objectCount)
        {
            return PerformWriteTest(objectCount) + PerformReadTest(objectCount) + PerformClearTest();
        }

        public long PerformWriteTest(int objectsToWrite)
        {
            lastInsert = objectsToWrite;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < objectsToWrite; i++)
            {
                table.Add(new PerfTestAllPrimitiveTypes());
            }
            db.SubmitChanges();
            long ms = sw.ElapsedMilliseconds;
            ewWrite.WriteInformation(objectsToWrite, sw.ElapsedMilliseconds);
            return ms;
        }

        public long PerformReadTest(int maxReads)
        {
            int count = 0;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            foreach(PerfTestAllPrimitiveTypes t in table)
            {
                count++;
                if (count >= maxReads) { break; }
            }
            long ms = sw.ElapsedMilliseconds;
            ewRead.WriteInformation(count, ms);
            return ms;
        }

        public long PerformClearTest()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            table.RemoveAll(table);
            db.SubmitChanges();
            long ms = sw.ElapsedMilliseconds;
            ewClear.WriteInformation(lastInsert, ms);
            return ms;
        }
    }
}
