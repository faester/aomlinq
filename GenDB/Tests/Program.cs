using System;
using System.Collections.Generic;
using System.Text;
using System.Query;
using System.Xml.XLinq;
using System.Data.DLinq;
using GenDB;

namespace Tests
{
    class Program
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
            float f = 600f;

            public float F
            {
                get { return f; }
                set { f = value; }
            }

            char ch = 'a';

            public char Ch
            {
                get { return ch; }
                set { ch = value; }
            }

            double d = 400.0;

            public double D
            {
                get { return d; }
                set { d = value; }
            }

            string name;

            int age;

            public int Age
            {
                get { return age; }
                set { age = value; }
            }

            private Person spouse;

            public Person Spouse
            {
                get { return spouse; }
                set { spouse = value; }
            }

            private Car car;

            public Car Car
            {
                get { return car; }
                set { car = value; }
            }


            DateTime instantiated = DateTime.Now ;

            public string Name
            {
                get { return name; }
                set { name = value; }
            }

        }

        static void Main(string[] args)
        {
            Configuration.RebuildDatabase = true;
            Configuration.DbBatchSize = 2000;
            long objcount = 500;

            DateTime then = DateTime.Now;
            GenTable gt = new GenTable();

            Person lastPerson = null;

            for (int i = 0; i < objcount; i++)
            {
                Car c = new Car{Brand = "Car " + i };
                Person p = new Person();
                p.Car = c;
                p.Spouse = lastPerson;
                p.Name = "Navn " + i.ToString();
                gt.Add (p);
                gt.Add (c);
                lastPerson = p;
            }

            gt.CommitChanges();
            TimeSpan dur = DateTime.Now - then;
            Console.WriteLine ("Insertion of {0} objects in {1}. {2} obj/sec", objcount, dur, objcount / dur.TotalSeconds);
            then = DateTime.Now;
            objcount = 0;
            foreach (IBusinessObject ibo in gt.GetAll())
            {
                objcount++;
                //ObjectUtilities.PrintOut(ibo);
            }
            dur = DateTime.Now - then;
            Console.WriteLine ("Read {0} objects in {1}. {2} obj/sec", objcount, dur, objcount / dur.TotalSeconds);
            Console.WriteLine("Generic table internal timer: {0}", gt.TimeSpent.Elapsed);
            Console.ReadLine();
        }
    }
}
