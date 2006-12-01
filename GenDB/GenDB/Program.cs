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
        class D : B
        {
            public IBusinessObject ibo; 
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

            Translator t = Translator.GetCreateTranslator(typeof(C));
            int elements = 0;
            C[] cs = new C[elements];

            for (int i = 0; i < elements; i++)
            {
                cs[i] = new C();
                t.GetEntityPOID(cs[i]);
            }
            genDB.SubmitChanges();

            Console.WriteLine("Cached objects: {0}", IBOCache.Instance.Count);

            for (int i = 0; i < elements; i++)
            {
                cs[i] = null;
            }

            D d = new D { ibo = new C()};
            d.ibo = d;
            Translator dt = Translator.GetCreateTranslator(d.GetType ());

            Translator.UpdateDBWith(d);
            //string idStr = dt.GetEntityPOID (d);
            //object obj = t.GetObjectFromEntityPOID(idStr);

            //ObjectDumper.PrintOut(obj);

            //ObjectDumper.PrintOut("Hello World");

            GC.Collect();
            Console.WriteLine("Cached objects: {0}", IBOCache.Instance.Count);
            
            Console.WriteLine("Press Return to end..");
            Console.ReadLine();
        }
    }
}
