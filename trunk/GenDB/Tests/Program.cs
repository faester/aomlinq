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
            }

            double d = 400.0;

            string name;

            int age;
            
            public Person spouse; 

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
            long objcount = 10;

            DateTime then = DateTime.Now;
            GenTable gt = new GenTable();

            Person lastPerson = null;

            for (int i = 0; i < objcount; i++)
            {
                Person p = new Person();
                p.spouse = lastPerson;
                p.Name = "Navn " + i.ToString();
                gt.Add (p);
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
                ObjectUtilities.PrintOut(ibo);
            }
            dur = DateTime.Now - then;
            Console.WriteLine ("Read {0} objects in {1}. {2} obj/sec", objcount, dur, objcount / dur.TotalSeconds);
            Console.WriteLine("Generic table internal timer: {0}", gt.TimeSpent.Elapsed);
            Console.ReadLine();
        }
    }
}
