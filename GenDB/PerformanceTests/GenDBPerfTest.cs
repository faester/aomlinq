using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;


namespace PerformanceTests
{
    class GenDBPerfTests<T> where T : GenDB.IBusinessObject, new()
    {
        GenDB.DataContext dc = GenDB.DataContext.Instance;
        GenDB.Table <T> table = GenDB.DataContext.Instance.CreateTable<T>();
                ExcelWriter ewWrite;
        ExcelWriter ewRead;
        ExcelWriter ewClear;
        int lastInsert = 0;

        public GenDBPerfTests(ExcelWriter ewWrite, ExcelWriter ewRead, ExcelWriter ewClear)
        {
            this.ewWrite = ewWrite;
            this.ewRead = ewRead;
            this.ewClear = ewClear;
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
                table.Add(new T());
            }
            dc.SubmitChanges();
            ewWrite.WriteInformation(objectsToWrite, sw.ElapsedMilliseconds);
        }

        private void PerformReadTest()
        {
            int count = 0;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            foreach(T t in table)
            {
                count++;
            }
            ewRead.WriteInformation(count, sw.ElapsedMilliseconds);
        }

        private void PerformClearTest()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            table.Clear();
            dc.SubmitChanges();
            ewClear.WriteInformation(lastInsert, sw.ElapsedMilliseconds);
        }       

    }
}
