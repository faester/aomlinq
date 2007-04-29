using System;
using System.Collections.Generic;
using System.Text;
using System.Query;
using System.Diagnostics;

namespace RefTests
{
    class Program
    {
        static GenDB.DataContext gcon = null;

        static void Main(string[] args)
        {
            gcon = GenDB.DataContext.Instance;
            gcon.DatabaseName = "reftest";

            int instances = 40000;
            int toSelect = 20000;
            bool create = false;
            for (int i = 0; i < 5; i++)
            {
                DLinqTest(create, instances, toSelect);
                GenDBTest(create, instances, toSelect);
                Console.WriteLine("-------------------------");
            }
        }


        private static IEnumerable<PerfPerson> CreatePersons(int instances)
        {
            for (int i = 0; i < instances; i++)
            {
                if (i % 1000 == 0) { Console.WriteLine(i); }
                PerfPerson p = new PerfPerson();
                p.PersonID = i;
                p.Name = i.ToString();
                Car c = new Car();
                c.Id = i;
                c.Name = "Car" + i;
                c.Gears = i;
                p.Age = i;
                p.Car = c;
                p.GenDBCar = c;
                yield return p;
            }
        }

        public static void DLinqTest(bool create, int instances, int toSelect)
        {
            DLinqContext dcon = new DLinqContext("server=(local);Integrated Security=SSPI");
            if (create)
            {
                if (dcon.DatabaseExists())
                {
                    dcon.DeleteDatabase();
                }
                dcon.CreateDatabase();

                foreach (PerfPerson p in CreatePersons(instances))
                {
                    dcon.Persons.Add(p);
                }

                dcon.SubmitChanges();
            }
            else
            {
                IEnumerable<PerfPerson> ps = from p in dcon.Persons
                                             where p.Car.Gears < toSelect //&& p.Car.Name == "Car10"
                                            select p;

                int count = 0;
                Stopwatch sw = new Stopwatch();
                sw.Start();
                foreach (PerfPerson p in ps)
                {
                    count++;
                    bool b = p.PersonID == p.Car.Gears;
                    if (!b) { throw new Exception("KSDLJKASDÆALKJ"); }
                }
                if (count != toSelect) { throw new Exception("Not enought persons returned."); }
                Console.WriteLine("DLinq: {0} ms with instances = {1} and toSelect = {2}",
                    sw.ElapsedMilliseconds,
                    instances,
                    toSelect);
            }
        }

        private static void GenDBTest(bool create, int instances, int toSelect)
        {
            if (!gcon.DatabaseExists()) { gcon.CreateDatabase(); }

            GenDB.Table<PerfPerson> table = gcon.GetTable<PerfPerson>();
            if (create)
            {
                table.Clear();
                gcon.SubmitChanges();
            }


            if (create)
            {
                foreach (PerfPerson p in CreatePersons(instances))
                {
                    table.Add(p);
                }
                gcon.SubmitChanges();
            }
            else
            {

                IEnumerable<PerfPerson> ps = from p in table
                                             where p.GenDBCar.Gears < toSelect //&& p.Car.Name == "Car10"
                                            select p;
                Stopwatch sw = new Stopwatch();
                sw.Start();

                int count = 0;
                foreach (PerfPerson p in ps)
                {
                    bool b = p.Age == p.GenDBCar.Gears;
                    if (!b)
                    {
                        throw new Exception("KSDLJKASDÆALKJ");
                    }
                    count++;
                }

                if (count != toSelect) { throw new Exception("Not enought persons returned."); }
                Console.WriteLine("GenDB: {0} ms with instances = {1} and toSelect = {2}",
                    sw.ElapsedMilliseconds,
                    instances,
                    toSelect);

            }
        }

    }
}
