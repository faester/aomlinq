using System;
using System.Collections.Generic;
using System.Text;
using System.Query;
using GenDB;

namespace Tests
{
    class Program
    {
        class Person : AbstractBusinessObject
        {
            string name = null;
            int age;
            string cpr = "051032-3232";
            Person spouse;

            public Person Spouse
            {
                get { return spouse; }
                set { spouse = value; }
            }

            public string Cpr
            {
                get { return cpr; }
                set { cpr = value; }
            }

            public int Age
            {
                get { return age; }
                set { age = value; }
            }

            public string Name
            {
                get { return name; }
                set { name = value; }
            }

        }

        class Student : Person
        {
            double avg = 8.0;

            public double Avg
            {
                get { return avg; }
                set { avg = value; }
            }

        }

        static void Main(string[] args)
        {
            Configuration.RebuildDatabase = true;
            Configuration.DbBatchSize = 2000;
            long objcount = 200;

            Table<Person> tp = new Table<Person>();

            DateTime then = DateTime.Now;

            Person lastPerson = null;

            for (int i = 0; i < objcount; i++)
            {
                Person p = new Person();
                Student s = new Student();

                s.Name = "Student '" + i.ToString();
                s.Avg = (double)i / objcount;
                s.Spouse = p;
                
                p.Name = "Navn " + i;
                p.Age = i;

                tp.Add (p);
                tp.Add (s);
                lastPerson = p;
            }

            Configuration.SubmitChanges();

            TimeSpan dur = DateTime.Now - then;
            objcount *= 2;
            Console.WriteLine ("Insertion of {0} objects in {1}. {2} obj/sec", objcount, dur, objcount / dur.TotalSeconds);
            then = DateTime.Now;
            objcount = 0;
            foreach (Person ibo in tp)
            {
                objcount++;
                //ObjectUtilities.PrintOut(ibo);
            }
            dur = DateTime.Now - then;
            Console.WriteLine ("Read {0} objects in {1}. {2} obj/sec", objcount, dur, objcount / dur.TotalSeconds);
            Console.ReadLine();
        }
    }
}
