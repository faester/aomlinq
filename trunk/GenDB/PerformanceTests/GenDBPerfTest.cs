using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;


namespace PerformanceTests
{
    class GenDBPerfTests<T> : ReadWriteClearTest
        where T : GenDB.IBusinessObject, new()
    {
        GenDB.DataContext dataContext = GenDB.DataContext.Instance;
        GenDB.Table<T> table = GenDB.DataContext.Instance.CreateTable<T>();
        int lastInsert = 0;

        public GenDBPerfTests(GenDB.DataContext dataContext)
        {
            this.dataContext = dataContext;
        }

        public void InitTests(int objectCount)
        {
            GenDB.Table<T> table = dataContext.CreateTable<T>();
            int count = table.Count;
            bool needCommit = false;
            Console.WriteLine("Contains {0} objects", count);
            while (count < objectCount)
            {
                needCommit = true;
                Console.WriteLine("Writing 20000 objects to table.");
                for (int i = 0; i < 20000; i++)
                {
                    table.Add(new T());
                }
                dataContext.SubmitChanges();
                count = table.Count;
            }

            if (needCommit)
            {
                Console.WriteLine("Committing changes.");
                dataContext.SubmitChanges();
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
                table.Add(new T());
            }
            dataContext.SubmitChanges();
            long ms = sw.ElapsedMilliseconds;
            return ms;
        }

        public long PerformReadTest(int maxReads)
        {
            int count = 0;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            bool hasAborted = false;

            foreach (T t in table)
            {
                count++;
                if (count >= maxReads)
                {
                    hasAborted = true;
                    break;
                }
            }

            long ms = sw.ElapsedMilliseconds;

            if (count < maxReads)
            {
                return -1;
            }
            else if (count > maxReads)
            {
                return -ms;
            }
            else
            {
                return ms;
            }
        }

        public long PerformClearTest()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            table.Clear();
            dataContext.SubmitChanges();
            long ms = sw.ElapsedMilliseconds;
            return ms;
        }
    }
}
