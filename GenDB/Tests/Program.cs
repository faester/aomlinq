﻿using System;
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

        class TestlistElement : AbstractBusinessObject
        {
            int i;

            public int I
            {
                get { return i; }
                set { i = value; }
            }
        }

        static void Main(string[] args)
        {
#if DEBUG
            try
            {
#endif
                Configuration.RebuildDatabase = true;

                Console.WriteLine("Her....");

                Configuration.DbBatchSize = 2000;
                long objcount = 1;

                //Table<BOList<TestlistElement>> tl = new Table<BOList<TestlistElement>>();

                //BOList<TestlistElement> lp = new BOList<TestlistElement>();

                //for (int i = 0; i < 10; i++)
                //{
                //    TestlistElement tle = new TestlistElement();
                //    tle.I = i;
                //    lp.Add(tle);
                //}

                //tl.Add(lp);

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

                    tp.Add(p);
                    tp.Add(s);
                    lastPerson = p;
                }

                Configuration.SubmitChanges();

                TimeSpan dur = DateTime.Now - then;
                objcount *= 2;
                Console.WriteLine("Insertion of {0} objects in {1}. {2} obj/sec", objcount, dur, objcount / dur.TotalSeconds);
                then = DateTime.Now;
                objcount = 0;
                foreach (Person ibo in tp)
                {
                    objcount++;
                    if (objcount % 500 == 0)
                    {
                        ibo.Age = (ibo.Age + 1) * 2;
                    }
                    //ObjectUtilities.PrintOut(ibo);
                }
                Configuration.SubmitChanges();
                dur = DateTime.Now - then;
                Console.WriteLine("Read {0} objects in {1}. {2} obj/sec", objcount, dur, objcount / dur.TotalSeconds);
                Console.WriteLine("Indeholder nu {0} objekter", tp.Count);

                Person[] ps = new Person[tp.Count];
                tp.CopyTo(ps, 0);
#if DEBUG
            }
            catch (NotTranslatableException ex)
            {
                Console.WriteLine(ex);
            }
#endif
            Console.ReadLine();
        }
    }
}
