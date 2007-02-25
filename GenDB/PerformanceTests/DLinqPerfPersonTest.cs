using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Query;
using System.Data.DLinq;
using System.Expressions;

namespace PerformanceTests
{
    public class DLinqPersonDB<T> : DataContext 
        where T : GenDB.IBusinessObject, new()
    {
        public Table<PerfPerson> Table;
        public DLinqPersonDB(string cnnstr)
            : base(cnnstr) { }
    }

    class DLinqPerfPersonTest<T> : QueryTest
        where T : GenDB.IBusinessObject, new()
    {
        DLinqPersonDB<T> db = null;
        Table<PerfPerson> table;
        TestOutput testOutput;
        int lastInsert = 0;

        public DLinqPerfPersonTest(TestOutput testOutput, string dbName)
        {
            this.testOutput = testOutput;

            this.db = new DLinqPersonDB<T>("server=.;database=" + dbName + ";Integrated Security=SSPI");
            //db.DeleteDatabase();
            if (!db.DatabaseExists())
            {
                Console.WriteLine("Creating DLinq db");
                db.CreateDatabase();
            }
            table = db.Table;
            //CleanDB();
        }

        public void InitTests(int objectCount)
        {
            int count = Sequence.Count(table);
            int needToAdd = objectCount - count;
            if (needToAdd > 0)
            {
                Console.WriteLine("Adding {0} objects to table", needToAdd);
                for (int i = count; i < objectCount; i++)
                {
                    PerfPerson p = new PerfPerson{Name="name"+(i+1), Age=(i+1) };
                    p.Entries .Add (new Car());
                    table.Add(p);
                }
                Console.WriteLine("Submitting.");
                db.SubmitChanges();
            }
        }

        public void CleanDB()
        {
            //db.DeleteDatabase();
            table.RemoveAll(table);
            db.SubmitChanges();
        }

        public long PerformSelectNothingTest(int objectCount)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            int c=0;
            var v = from t in table
                    where t.Name == "Albert E"
                    select t;
            foreach(PerfPerson pp in v) {c=pp.GList.Count;}
            long ms = sw.ElapsedMilliseconds;
            return ms;
        }

        public long PerformSelectOneTest(int objectCount)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            int c=0;
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
            if (c < amount) throw new Exception();
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
            int cc = 0;
            foreach(PerfPerson pp in v) 
            {
                foreach(Car car in pp.Entries)
                {
                    cc++;
                }
            }
            Console.WriteLine("Cars  {0}", cc);
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
