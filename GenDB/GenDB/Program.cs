#define RECREATE_DB
using System;
using System.Collections.Generic;
using System.Text;
using System.Query;
using System.Xml.XLinq;
using System.Data.DLinq;
using System.Data.SqlClient;
using System.Expressions;
using GenDB.DB;

namespace GenDB
{
    public enum Sex { MALE, FEMALE, NEUTER };

    class Program
    {
        static int nextID = 0;

        public class Car : AbstractBusinessObject
        {
            string brand = "Volvo";

            public string Brand
            {
                get { return brand; }
                set { brand = value; }
            }
        }

        public class Person : AbstractBusinessObject
        {
            Sex sex;

            public Person() { /* empty */ }

            Car car = new Car();

            public Car Car
            {
                get { return car; }
                set { car = value; }
            }


            BOList<Person> others = new BOList<Person>();
            [Volatile]
            public BOList<Person> Others
            {
                get { return others; }
                set { others = value; }
            }

            public Sex Sex
            {
                get { return sex; }
                set { sex = value; }
            }

            //int[] arr = new int[30];

            //public int[] Arr
            //{
            //    get { return arr; }
            //    set { arr = value; }
            //}
            private string name = null;

            public string Name
            {
                get { return name; }
                set { name = value; }
            }
            private Person spouse = null;

            public Person Spouse
            {
                get { return spouse; }
                set { spouse = value; }
            }

            int age;

            public int Age
            {
                get { return age; }
                set { age = value; }
            }
        }

        public class Student : Person { 
            public static long test = 100; //This should not be persisted since it is static
            [Volatile]
            public long id = nextID++; // Should not be persisted due to attribute.
            private DateTime enlisted;

            public DateTime Enlisted
            {
                get { return enlisted; }
                set { enlisted = value; }
            }
        }

        public static void Main(string[] args)
        {
            Configuration.RebuildDatabase = true;

            int objCount = 10;

            Table<Person> tp = new Table<Person>();

            for (short i = 0; i < objCount; i++)
            {
                Car c = new Car();
                c.Brand += i.ToString();
                Person p = new Person{ Name = "Navn " + i };
                p.Car = c;
                if (i % 2 == 0) { p.Sex = Sex.FEMALE; }
                p.Age = i;
                tp.Add (p);
            }

            Person s_p = new Person{ Name = "SpousePerson"};
            s_p.Age = 99;
            tp.Add (s_p);

            Person p_p = new Person {Name = "NormalPerson", Spouse=s_p, Age=121};
            tp.Add (p_p);

            IBOCache.FlushToDB();
            
            var es = from epp in tp
                     //where !(epp.Age >= 3)
                     //where epp.Name == "Navn 3"
                     //where epp.Sex == Sex.FEMALE || epp.Name == "Navn 3"
                     //where epp.Name == "Navn 6" || epp.Age == 7
                     where epp.Spouse != s_p
                     //where epp.Age == tp.Max(ep => ep.Age)
                     select epp;

            foreach(Person p in es)
            {
                ObjectUtilities.PrintOut (p);
            }
            
            Console.WriteLine("Size of Table: {0}", es.Count);

            //foreach(int n in es)
            //{
            //    Console.WriteLine(n);
            //}

            Console.ReadLine();
        }
    }
}


