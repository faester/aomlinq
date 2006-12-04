#define RECREATE_DB
using System;
using System.Collections.Generic;
using System.Text;
using System.Query;
using System.Xml.XLinq;
using System.Data.DLinq;
using System.Data.SqlClient;

namespace GenDB
{
    class Program
    {
        class AbsBO : IBusinessObject
        {
            private DBTag dBTag;
            public DBTag DBTag 
            {
                get { return dBTag;}
                set { dBTag = value; }
            }
        }

        class A : AbsBO
        {
            public string dateString = DateTime.Now.ToString();
        }
        
        class B : A 
        {
            public int year;
        }
        class C : B 
        {
            public string name = "bnavn";
        }
        class D : B
        {
            public IBusinessObject ibo; 
        }

        class Person : AbsBO
        { 
            public string name = null; 
            public Person spouse = null;
        }

        class Student : Person { public DateTime enlisted; }

        static void Main(string[] args)
        {
            int elements = 100;

            GenericDB genDB = GenericDB.Instance;
            //genDB.Log = Console.Out;
#if RECREATE_DB
            if (genDB.DatabaseExists())
            {
                Console.WriteLine("Deleting old database.");
                genDB.DeleteDatabase();
            }
#endif
            if (!genDB.DatabaseExists())
            {
                Console.WriteLine("Building database.");
                genDB.CreateDatabase();
                Console.WriteLine("Database built");
            }

            //Translator dt = Translator.GetCreateTranslator(d.GetType ());
            //Translator.UpdateDBWith(d);
            //string idStr = dt.GetEntityPOID (d);
            //object obj = t.GetObjectFromEntityPOID(idStr);

            //ObjectDumper.PrintOut(obj);

            GenTable gt = new GenTable();

            DateTime then = DateTime.Now;
                        TimeSpan dur = DateTime.Now - then;

            Student lastStudent = null;

            for (int i = 0; i < elements; i++)
            {
                Person p = new Person();
                p.name = "person no " + i.ToString();
                
                Student s = new Student();
                s.name = "student no " + i.ToString();
                s.enlisted = DateTime.Now;
                s.spouse = lastStudent;

                lastStudent = s;

                gt.Add (s);
                //gt.Add (p);
                if (i % 50 == 0) {
                    dur = DateTime.Now - then;
                    Console.WriteLine("{0} objects inserted in {1}. {2} objs/s ", i, dur, i / dur.TotalSeconds);
                    GenericDB.Instance.SubmitChanges();
                }
            }

            dur = DateTime.Now - then;

            Console.WriteLine("{0} objects inserted in {1}. {2} objs/s ", elements, dur, elements / dur.TotalSeconds);

            Console.WriteLine("Cached objects: {0}", IBOCache.Instance.Count);

            then = DateTime.Now;
            int retrieveCount = 0;

            foreach(IBusinessObject ibo in gt.GetAll())
            {
                retrieveCount++;
                //ObjectDumper.PrintOut(ibo);
            }


            dur = DateTime.Now - then;
            Console.WriteLine("{0} objects retrieved in {1}. {2} obj/sec", retrieveCount, dur, retrieveCount / dur.TotalSeconds);
            Console.WriteLine("Objects retrieved from cache: {0}", IBOCache.Instance.Retrieved);
            Console.WriteLine(IBOCache.Instance);
            Console.WriteLine("Forcing garbage collection.");
            GC.Collect();
            Console.WriteLine(IBOCache.Instance);
            Console.WriteLine("Press Return to end..");
            Console.ReadLine();
        }
    }
}
