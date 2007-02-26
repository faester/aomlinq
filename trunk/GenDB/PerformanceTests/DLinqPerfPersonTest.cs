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
        public Table<PerfPerson> Persons;
        public Table<Car> Cars;
        //public Persons<Car> TableCar;

        public DLinqPersonDB(string cnnstr)
            : base(cnnstr) { }
    }

    class DLinqPerfPersonTest<T> : QueryTest
        where T : GenDB.IBusinessObject, new()
    {
        DLinqPersonDB<T> db = null;
        Table<PerfPerson> table;

        //Persons<Car> tableCar;

        TestOutput testOutput;
        int lastInsert = 0;

        public DLinqPerfPersonTest(TestOutput testOutput, string dbName)
        {
            this.testOutput = testOutput;
            this.db = new DLinqPersonDB<T>("server=.;database=" + dbName + ";Integrated Security=SSPI");


            if (!db.DatabaseExists())
            {
                Console.WriteLine("Creating DLinq db");
                db.CreateDatabase();
            }
            table = db.Persons;
            //tableCar = db.TableCar;
        }

        public void InitTests(int objectCount)
        {
            int count = table.Count<PerfPerson>(p => true);

            int needToAdd = objectCount - count;
            if (needToAdd > 0)
            {
                Console.WriteLine("Adding {0} objects to table", needToAdd);
                for (int i = count; i < objectCount; i++)
                {
                    PerfPerson p = new PerfPerson { Name = "name" + (i + 1), Age = (i + 1) };
                    p.PersonID = i;
                    Car car = new Car();
                    car.Id = i;
                    
                    p.Cars.Add(car);
                    car.Owner = p;

                    table.Add(p);
                }
                Console.WriteLine("Submitting.");
                db.SubmitChanges();
            }
        }

        public void CleanDB()
        {
            //db.DeleteDatabase();
            //db.Cars.RemoveAll(db.Cars);
            //table.RemoveAll(table);
            //db.SubmitChanges();
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
            foreach(PerfPerson pp in v){c++;}
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
            int amount = objectCount/2;
            int half = amount/2;
            int c=0;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var v = from t in table
                    where t.Age <= amount
                    select t;                    
            var subv = from subt in v
                       where subt.Age <= half
                       select subt;
            foreach(PerfPerson pp in subv){c++;}
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
