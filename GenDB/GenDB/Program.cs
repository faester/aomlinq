#define RECREATE_DB
using System;
using System.Collections.Generic;
using System.Text;
using System.Query;
using System.Xml.XLinq;
using System.Data.DLinq;
using System.Data.SqlClient;
using System.Expressions;
using System.Reflection;
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
            string brand;// = "Volvo";
            bool sunroof;
            int seets;
            Motor motor = new Motor();

            public string Brand
            {
                get { return brand; }
                set { brand = value; }
            }

            public bool Sunroof
            {
                get{return sunroof;}
                set{sunroof=value;}
            }

            public int Seets
            {
                get{return seets;}
                set{seets=value;}
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
            public string disco = "21";

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

            public DateTime DaysLeft()
            {
                return DateTime.Now;
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
            if (dcontext.DatabaseExists())
            {
                dcontext.DeleteDatabase(); 
            }
            if (!dcontext.DatabaseExists())
            {
                dcontext.CreateDatabase();
            }
            dcontext.Init();


            int objCount = 100;

           


            //Table<BOList<int>> tt = dcontext.CreateTable<BOList<int>>();
            //throw new Exception("dd");

            
            //BOList<Person> bolist = dcontext.BolistFactory.BOListRef <Person>();
            //for (int i = 0; i < 10; i++)
            //{
            //    Person p = new Person{Name = i.ToString() + "'"};
            //    bolist.Add (p);
            //}

            Table<BODictionary<int, int>> table = dcontext.CreateTable<BODictionary<int, int>>();
            table.Clear();
            dcontext.SubmitChanges();
            
            BODictionary<int, int> bodict = dcontext.BODictionaryFactory.BODictionaryInt<int,int>();
            for(int j=0;j<10;j++)
            {
                bodict.Add(j,j);
                bodict.Clear();
            }

            table.Add(bodict);
            dcontext.SubmitChanges();
            GC.Collect();
    


            //bodict.Remove(3);
            //foreach(KeyValuePair<int, Person> kvp in bodict)
            //{
            //    Person kp = (Person) kvp.Value;
            //    int ks = kvp.Key;
            //    Console.WriteLine("Key: {0}, Value: {1}",ks,kp.Name);
            //}

            //tbolist.Add (bolist);

            //var es = from epp in tp     
            //    //where epp.Car.Motor.HorsePower == "400"
            //    //where epp.Car.Motor.Valve == 6
            //    //where epp.Car.Sunroof==false
            //      where !(epp.Age!=4)
            //      select epp;
                //select new {Age = epp.Age, TestAggregate = tp.Average(v => v.Age)};

            //foreach(Person p in es)
            //{
            //    ObjectUtilities.PrintOut (p);
            //}
            
            //Console.WriteLine("Size of Table: {0}", es.Count);
            

            // Joining
            //int[] ages = {1, 2, 3, 979 };

            //var qw = from pers in tp
            //         join age in ages
            //         on pers.Age equals age
            //         select new {pers.Name};

            // group join
            //var gj = from person in tp
            //         join person2 in tp
            //         on new {person.Age, person.Name} equals new {person2.Age, person2.Name} into pc
            //         from person2 in pc
            //         select new {person.Name, person2.Age};

            //foreach(var thing in gj)
            //{
            //    Console.WriteLine(thing);
            //}
            
            Console.ReadLine();
        }
    }
}