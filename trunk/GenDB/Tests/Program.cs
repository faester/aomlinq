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
            string name;

            public string Name
            {
                get { return name; }
                set { name = value; }
            }

        }

        static void Main(string[] args)
        {
            Configuration.RebuildDatabase = true;
            long objcount = 4000;

            DateTime then = DateTime.Now;
            GenTable gt = new GenTable();
            for (int i = 0; i < objcount; i++)
            {
                Person p = new Person();
                p.Name = "Navn " + i.ToString();
                gt.Add (p);
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
            Console.ReadLine();
        }
    }
}
