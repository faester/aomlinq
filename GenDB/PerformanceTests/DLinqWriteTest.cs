using System;
using System.Collections.Generic;
using System.Text;
using System.Data.DLinq;
using System.Diagnostics;
using System.Query;
using System.Expressions;

namespace PerformanceTests
{
    class DingelDangelDlinq : System.Data.DLinq.DataContext
    {
        public DingelDangelDlinq(string cnnstr) : base(cnnstr) { }

        public Table<ContainsAllPrimitiveTypes> TheTable;
    }

    class DLinqTest
    {
        DingelDangelDlinq db = new DingelDangelDlinq("server=.;database=knud");
        ExcelWriter ewWrite;
        ExcelWriter ewRead;
        ExcelWriter ewClear;
        int lastInsert = 0;

        public DLinqTest(ExcelWriter ewWrite, ExcelWriter ewRead, ExcelWriter ewClear)
        {
            this.ewWrite = ewWrite;
            this.ewRead = ewRead;
            this.ewClear = ewClear;
            if (!db.DatabaseExists())
            {
                db.CreateDatabase();
            }
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
                db.TheTable.Add(new ContainsAllPrimitiveTypes());
            }
            db.SubmitChanges();
            ewWrite.WriteInformation(objectsToWrite, sw.ElapsedMilliseconds);
        }

        private void PerformReadTest()
        {
            int count = 0;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            foreach(ContainsAllPrimitiveTypes t in db.TheTable)
            {
                count++;
            }
            ewRead.WriteInformation(count, sw.ElapsedMilliseconds);
        }

        private void PerformClearTest()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            db.TheTable.RemoveAll(db.TheTable);
            db.SubmitChanges();
            ewClear.WriteInformation(lastInsert, sw.ElapsedMilliseconds);
        }
    }
}
