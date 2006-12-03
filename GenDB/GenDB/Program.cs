//#define RECREATE_DB
using System;
using System.Collections.Generic;
using System.Text;
using System.Query;
using System.Xml.XLinq;
using System.Data.DLinq;

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
        }

        class Student : Person { public DateTime enlisted; }

        static void Main(string[] args)
        {
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
                genDB.CreateDatabase();
            }

            int elements = 200;
            //Translator dt = Translator.GetCreateTranslator(d.GetType ());
            //Translator.UpdateDBWith(d);
            //string idStr = dt.GetEntityPOID (d);
            //object obj = t.GetObjectFromEntityPOID(idStr);

            //ObjectDumper.PrintOut(obj);

            GenTable gt = new GenTable();

            DateTime then = DateTime.Now;

            for (int i = 0; i < elements; i++)
            {
                Person p = new Person();
                p.name = "person no " + i.ToString();
                
                Student s = new Student();
                s.name = "student no " + i.ToString();
                s.enlisted = DateTime.Now;

                gt.Add (s);
                gt.Add (p);
            }

            TimeSpan dur = DateTime.Now - then;

            Console.WriteLine("Inserts took: {0}", dur);

            Console.WriteLine("Cached objects: {0}", IBOCache.Instance.Count);

            foreach(IBusinessObject ibo in gt.GetAll())
            {
                //ObjectDumper.PrintOut(ibo);
            }

            GC.Collect();
            Console.WriteLine("Press Return to end..");
            Console.ReadLine();
        }
    }
}
