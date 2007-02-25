using System;
using System.Collections.Generic;
using System.Text;
using GenDB;
using System.Data.DLinq;
using System.Diagnostics;


namespace PerformanceTests
{
    class GenDBPerfPersonTest<T> : QueryTest
        where T : GenDB.IBusinessObject, new()
    {
        GenDB.DataContext dataContext = GenDB.DataContext.Instance;
        GenDB.Table<PerfPerson> table = GenDB.DataContext.Instance.GetTable<PerfPerson>();
        int lastInsert = 0;

        public GenDBPerfPersonTest(GenDB.DataContext dataContext) {
            this.dataContext = dataContext;
        }

        public void InitTests(int objectCount)
        {
            GenDB.Table<PerfPerson> table = dataContext.GetTable<PerfPerson>();
            int count = table.Count;
            bool needCommit = false;
            
            Console.WriteLine("Contains {0} objects", count);
            while (count < objectCount)
            {
                needCommit = true;
                int toWrite = objectCount- count ;
                Console.WriteLine("Writing {0} objects to table.", toWrite);
                for (int i = count; i < objectCount; i++)
                {
                    table.Add(new PerfPerson{Name="name"+(i+1), Age=(i+1)});
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

        public void CleanDB()
        {
            table.Clear();
            dataContext.SubmitChanges();
            //dataContext.DeleteDatabase();
        }

        public long PerformSelectNothingTest(int objectCount)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            int c=0;
            var v = from t in table
                    where t.Name == "Albert E"
                    select t;
            foreach(PerfPerson pp in v) {c++;}
            long ms = sw.ElapsedMilliseconds;
            return ms;
        }

        public long PerformSelectOneTest(int objectCount)
        {
            int c=0;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var v = from t in table
                    where t.Name == "name9"
                    select t;
            foreach(PerfPerson pp in v) {c++;}
            long ms = sw.ElapsedMilliseconds;
            return ms;
        }
       
        public long PerformSelectOnePctTest(int objectCount)
        {
            int amount = objectCount/100;
            int c=0;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var v = from t in table
                    where t.Age <= amount
                    select t;
            foreach(PerfPerson pp in v) {c++;}
            long ms = sw.ElapsedMilliseconds;
            return ms;
        }
        
        public long PerformSelectTenPctTest(int objectCount)
        {
            int amount = objectCount/10;
            int c=0;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var v = from t in table
                    where t.Age <= amount
                    select t;
            foreach(PerfPerson pp in v) {c++;}
            long ms = sw.ElapsedMilliseconds;
            return ms;
        }

        public long PerformSelectHundredPctTest(int objectCount)
        {
            int amount = objectCount;
            int c=0;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var v = from t in table
                    where t.Age <= amount
                    select t;
            foreach(PerfPerson pp in v) {c++;}
            long ms = sw.ElapsedMilliseconds;
            return ms;
        }

        public long PerformSelectFiftySubFiftyPctTest(int objectCount)
        {
            int amount = objectCount/50;
            int half = amount/50;
            int c=0;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var v = from t in table
                    where t.Age <= amount
                    select t;                    
            var subv = from subt in v
                       where subt.Age <= half
                       select subt;
            foreach(PerfPerson pp in subv) {c++;}
            long ms = sw.ElapsedMilliseconds;
            return ms;
        }

        public long PerformSelectUnconditionalTest(int objectCount)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            int c=0;
            var v = from t in table
                    select t;
            foreach(PerfPerson pp in v) {c++;}
            long ms = sw.ElapsedMilliseconds;
            return ms;
        }
    }
}
