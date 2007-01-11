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

        public class Motor : AbstractBusinessObject
        {
            string horsePower = "400";
            int valve = 6;
            
            public string HorsePower
            {
                get{return horsePower;}
                set{horsePower=value;}
            }

            public int Valve
            {
                get{return valve;}
                set{valve=value;}
            }
        }

        public class Car : AbstractBusinessObject
        {
            string brand = "Volvo";
            Motor motor = new Motor();

            public string Brand
            {
                get { return brand; }
                set { brand = value; }
            }

            public Motor Motor
            {
                get{return motor;}
                set{motor = value;}
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

            public Sex Sex
            {
                get { return sex; }
                set { sex = value; }
            }

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

            char letter;
            public char Letter
            {
                get {return letter;}
                set {letter = value;}
            }

            DateTime birth;
            public DateTime Birth
            {
                get {return birth;}
                set {birth = value;}
            }

            bool alive;
            public bool Alive
            {
                get {return alive;}
                set{alive = value;}
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
            DataContext dcontext = DataContext.Instance;
            dcontext.RebuildDatabase = true;
            //dcontext.Init();
            dcontext.RebuildDatabase = true;

            int objCount = 10;

            Table<Person> tp = dcontext.CreateTable<Person>();
            Person lastPerson = null;

            for (short i = 0; i < objCount; i++)
            {
                Car c = new Car();
                c.Brand += i.ToString();
                Person p = new Person{ Name = "Navn " + i };
                p.Car = c;
                if (i % 2 == 0) { p.Sex = Sex.FEMALE; }
                p.Age = i;
                p.Spouse = lastPerson;
                tp.Add (p);
                lastPerson = p;
            }

            lastPerson = null;

            DateTime t = DateTime.Now;
            Car cc = new Car();

            Person s_p = new Person{ Name = "SpousePerson", Letter = 'c', Birth = t, Alive=true, Car = cc};
            s_p.Age = 99;
            tp.Add (s_p);

            Person p_p = new Person {Name = "NormalPerson", Spouse=s_p, Age=121, Alive=true};
            tp.Add (p_p);

            //IBOCache.FlushToDB();
            dcontext.SubmitChanges();
            var es = from epp in tp     
                // where epp.Age == 3
                //where !(epp.Age <= 9)
                //where !(epp.Name != "Navn 3")
                //where epp.Sex == Sex.FEMALE || epp.Name == "Navn 3"
                //where epp.Name == "Navn 6" || epp.Age == 7
                //where epp.Spouse == s_p
                  //where epp.Car.Brand == "Volvo"
                //where epp.Name == "SpousePerson"
                //where !(epp.Letter != 'c')
                //where epp.Birth == t
                //where epp.Alive
                where epp.Name == "SpousePerson"
                //where epp.Car.Brand == "Volvo"
                //where epp.Car.Motor.HorsePower == "400"
                //where epp.Car.Motor.Valve == 6
                select epp;
                //select new {Age = epp.Age, TestAggregate = tp.Average(v => v.Age)};

            
            foreach(Person p in es)
            {
                ObjectUtilities.PrintOut (p);
            }

            Console.WriteLine("Size of Table: {0}", es.Count);

            Console.ReadLine();
        }
    }
}


