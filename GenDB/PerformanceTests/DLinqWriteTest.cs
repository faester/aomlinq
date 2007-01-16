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

    public class DLinqTest
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

        public void PerformTests(int objectCount)
        {
            PerformWriteTest(objectCount);
            PerformReadTest();
            PerformClearTest();
        }

        private void PerformWriteTest(int objectsToWrite)
        {
            lastInsert = objectsToWrite;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < objectsToWrite; i++)
            {
                table.Add(new PerfTestAllPrimitiveTypes());
            }
            db.SubmitChanges();
            ewWrite.WriteInformation(objectsToWrite, sw.ElapsedMilliseconds / 1000.0);
        }

        private void PerformReadTest()
        {
            int count = 0;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            foreach(PerfTestAllPrimitiveTypes t in table)
            {
                count++;
            }
            long ms = sw.ElapsedMilliseconds;
            ewRead.WriteInformation(count, ms / 1000);
            Console.WriteLine("DLinq read: {0} objs {1} sek", count, ms / 1000.0);
        }

        private void PerformClearTest()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            table.RemoveAll(table);
            db.SubmitChanges();
            ewClear.WriteInformation(lastInsert, sw.ElapsedMilliseconds / 1000.0);
        }
    }
}
