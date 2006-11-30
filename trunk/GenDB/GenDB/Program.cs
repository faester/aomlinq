#define RECREATE_DB
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

            Translator t = Translator.GetTranslator (typeof(C));
            int elements = 5;
            C[] cs = new C[elements];

            for (int i = 0; i < elements; i++)
            {
                cs[i] = new C();
                t.ToPropertyValueString(cs[i]);
            }
            genDB.SubmitChanges();

            Console.WriteLine("Cached objects: {0}", IBOCache.Instance.Count);

            for (int i = 0; i < elements; i++)
            {
                cs[i] = null;
            }

            object obj = t.ToObjectRepresentation("1");

            ObjectDumper.PrintOut(obj);

            ObjectDumper.PrintOut("Hello World");

            GC.Collect();
            Console.WriteLine("Cached objects: {0}", IBOCache.Instance.Count);
            
            Console.WriteLine("Press Return to end..");
            Console.ReadLine();
        }
    }
}
