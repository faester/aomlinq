using System;
using System.Collections.Generic;
using System.Text;
using GenDB;

namespace LastingPersistanceTests
{
    class Car : AbstractBusinessObject
    {
        string brand;

        public string Brand
        {
            get { return brand; }
            set { brand = value; }
        }
    }

    class Person : AbstractBusinessObject
    {
        string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        int age;

        public int Age
        {
            get { return age; }
            set { age = value; }
        }

        string carsBrand;

        public string CarsBrand
        {
            get { return carsBrand; }
            set { carsBrand = value; }
        }

        LazyLoader<Car> carHolder = new LazyLoader<Car>();

        [LazyLoad("carHolder")]
        public Car Car
        {
            get { return carHolder.Element; }
            set { carHolder.Element = value; }
        }

        public override string ToString()
        {
            return "Person {Age = " + age + ", Name = " + name + ", CarsBrand = " + (carsBrand == null ? " null " : carsBrand) + "}";
        }
    }

    class DBConnected
    {
        protected DataContext dc;

        private Table<Person> persons;
        bool initialized = false;

        private void Init()
        {
            if (!dc.IsInitialized)
            {
                dc.Init();
            }
            persons = dc.GetTable<Person>();
        }

        protected Table<Person> Persons
        {
            get
            {
                if (!initialized)
                {
                    Init();
                }
                return persons;
            }
        }

        public DBConnected()
        {
            dc = DataContext.Instance;
            dc.DatabaseName = Program.DBNAME;
        }

        protected void ConstructDB()
        {
            if (dc.DatabaseExists())
            {
                dc.DeleteDatabase();
            }
            dc.CreateDatabase();
            dc.Init();

            persons = dc.GetTable<Person>();
        }

    }

    class Storer : DBConnected
    {
        public Storer()
        {
            ConstructDB();
        }

        public void StoreData()
        {
            for (int i = 0; i < 100; i++)
            {
                Person p = new Person();
                p.Name = i.ToString();
                p.Age = i;
                p.CarsBrand = null;

                if (i % 2 == 0)
                {
                    Car c = new Car();
                    c.Brand = "Brand" + i.ToString();
                    p.Car = c;
                }
                Persons.Add(p);
            }
            dc.SubmitChanges();

            //Make changes after first commit.
            foreach (Person p in Persons)
            {
                if (p.Car != null)
                {
                    p.CarsBrand = p.Car.Brand;
                }
            }

            dc.SubmitChanges();

            int idx = 0;

            foreach (Person p in Persons)
            {
                idx++;
                if (idx % 5 == 0)
                {
                    p.Car = new Car { Brand = "Five!" };
                    p.CarsBrand = p.Car.Brand;
                }
            }

            dc.SubmitChanges();
        }
    }


    class Retriever : DBConnected
    {
        int errorsFound = 0;

        public void RetrieveData()
        {
            LinkedList<Person> ps = new LinkedList<Person>();

            foreach (Person p in Persons)
            {
                ps.AddLast(p);
            }

            Console.WriteLine("Done reading in objects");

            foreach (Person p in ps)
            {
                if (p.Age.ToString() != p.Name)
                {
                    errorsFound++;
                    Console.WriteLine("Person {0} had errors in Name/Age", p);
                }

                if (p.CarsBrand != null)
                {
                    if (p.CarsBrand == "")
                    {
                        Console.WriteLine("Emptystring in CarsBrand for {0}", p);
                        errorsFound++;
                    }
                    else if (p.Car.Brand != p.CarsBrand)
                    {
                        errorsFound++;
                        Console.WriteLine("Person {0} had errors in car information", p);
                        Console.WriteLine("Car: " + p.Car);
                    }
                }
            }

            if (errorsFound == 0)
            {
                Console.WriteLine("Congratulations, no errors found.");
            }
            else
            {
                Console.WriteLine("Found {0} errors", errorsFound);
            }
        }
    }

    class Program
    {
        public static readonly string DBNAME = "persistancetests";

        static void Main(string[] args)
        {
            if (args.Length == 0) { args = new string[] { "retrieve" }; }
            if (args.Length == 0 || (args[0] != "store" && args[0] != "retrieve"))
            {
                Console.WriteLine("usage: {store | retrieve}");
                return;
            }

            if (args[0] == "store")
            {
                Storer s = new Storer();
                s.StoreData();
            }
            else
            {
                Retriever r = new Retriever();
                r.RetrieveData();
            }
        }
    }
}
