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
        static int nextID = 0;

        public class Person : AbstractBusinessObject
        {
            //public IBusinessObject obj = null;
            public string name = null;
            public Person spouse = null;
        }

        public class Student : Person { 
            public int rnd = 0;    
            public int id = nextID++;
            private DateTime enlisted;

            public DateTime Enlisted
            {
                get { return enlisted; }
                set { enlisted = value; }
            } 
        }

        public static void Main(string[] args)
        {
            IGenericDatabase gdb = MsSql2005DB.Instance;
            if (!gdb.DatabaseExists ()) 
            {
                gdb.CreateDatabase();
            }
            else
            {
                //gdb.DeleteDatabase();
                //gdb.CreateDatabase();
            }

            Person spouse = new Person();
            spouse.name = "In your dreams...";
            Student s = new Student();
            s.name = "Morten";
            s.id = 839;
            s.Enlisted = new DateTime (2006, 12, 31);
            s.spouse = spouse;
            Type t = typeof(Student);

            TypeSystem.Instance.RegisterType(t);

            DelegateTranslator dtrans = TypeSystem.Instance.GetTranslator (t);
            IEntity e = dtrans.Translate (s);
            Console.WriteLine (e);
            
            Student copy = (Student)dtrans.Translate (e);
            Console.WriteLine("Here goes the original Student: ");
            ObjectUtilities.PrintOut(s);
            Console.WriteLine("Here goes the copy: ");
            ObjectUtilities.PrintOut(copy);
            Console.WriteLine("Reflection based equality test says: ");
            Console.WriteLine(ObjectUtilities.TestFieldEquality (s, copy));

            DateTime then = DateTime.Now;
            Configuration.GenDB.Save (e);
            Configuration.GenDB.CommitChanges();

            Console.WriteLine("Commit duration: {0}" , DateTime.Now - then);
            Console.ReadLine();
        }

        static void OldMain()
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
                Console.WriteLine("Building database.");
                genDB.CreateDatabase();
                Console.WriteLine("Database built");
            }

            GenTable genericTable = new GenTable();

            DateTime then = DateTime.Now;
            TimeSpan dur = DateTime.Now - then;

            Student lastStudent = null;

            int elements = 1000;
            int submitInterval = 2500;

            for (int i = 0; i < elements; i++)
            {
                Student s = new Student();
                s.name = "student no " + i.ToString();
                s.Enlisted = DateTime.Now;
                //s.spouse = lastStudent;

                lastStudent = s;

                genericTable.Add(s);
                if (i % submitInterval == 0)
                {
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

            IBOCache.Instance.FlushToDB();

            if (false)
            {
                EntityTypeDL et = genDB.EntityTypes.Where((EntityTypeDL e) => e.Name.EndsWith("Student")).First();
                IEnumerable<IBusinessObject> ibosss = genericTable.GetAll(et);
                foreach (IBusinessObject ibo in ibosss)
                {
                    if (retrieveCount % 2 == 0)
                    {
                        Student s = (Student)ibo;
                        s.name = "Navn nr " + retrieveCount;
                    }
                    retrieveCount++;
                    //ObjectDumper.PrintOut(ibo);
                }
            }
            else
            {
                foreach (IBusinessObject ibo in genericTable.GetAll())
                {
                    if (retrieveCount % 2 == 0)
                    {
                        Student s = (Student)ibo;
                        s.name = "Navn nr " + retrieveCount;
                    }
                    retrieveCount++;
                    //ObjectDumper.PrintOut(ibo);
                }
            }
            dur = DateTime.Now - then;
            Console.WriteLine("{0} objects retrieved in {1}. {2} obj/sec", retrieveCount, dur, retrieveCount / dur.TotalSeconds);
            Console.WriteLine("Objects retrieved from cache: {0}", IBOCache.Instance.Retrieved);
            Console.WriteLine(IBOCache.Instance);
            Console.WriteLine("Forcing garbage collection.");
            Console.WriteLine("Flushing cache and submitting DB.");
            then = DateTime.Now;
            IBOCache.Instance.FlushToDB();
            GenericDB.Instance.SubmitChanges();
            dur = DateTime.Now - then;
            Console.WriteLine("Cache flush duration: " + dur);
            Console.ReadLine();
        }
    }
}
