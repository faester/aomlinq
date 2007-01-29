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
        TestOutput testOutput;
        int lastInsert = 0;

        public DLinqTest(TestOutput testOutput, string dbName)
        {
            this.testOutput = testOutput;

            this.db = new DLinqDB("server=.;database=" + dbName + ";Integrated Security=SSPI");
            if (!db.DatabaseExists())
            {
                Console.WriteLine("Creating DLinq db");
                db.CreateDatabase();
            }
            table = db.Table;
            db.SubmitChanges();
        }

        public void InitTests(int objectCount)
        {
            int needToAdd = objectCount - table.Count();
            if (needToAdd > 0)
            {
                Console.WriteLine("Adding {0} objects to table", needToAdd);
                for (int i = 0; i < needToAdd; i++)
                {
                    table.Add(new PerfTestAllPrimitiveTypes());
                }
                Console.WriteLine("Submitting.");
                db.SubmitChanges();
            }
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
            return ms;
        }

        public long PerformReadTest(int maxReads)
        {
            int count = 0;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            foreach (PerfTestAllPrimitiveTypes t in table)
            {
                count++;
                if (count >= maxReads) { break; }
            }
            long ms = sw.ElapsedMilliseconds;
            return ms;
        }

        public long PerformClearTest()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            table.RemoveAll(table);
            db.SubmitChanges();
            long ms = sw.ElapsedMilliseconds;
            return ms;
        }
    }
}
