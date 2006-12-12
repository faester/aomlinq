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
            public string name = null;
            public Person spouse = null;
            public char ch = 'a';
        }

        public class Student : Person { 
            static long test = 100;
            public long id = nextID++;
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
                gdb.DeleteDatabase();
                gdb.CreateDatabase();
            }

            Person spouse = new Person();
            spouse.name = "In your dreams...";
            Student s = new Student();
            s.name = "Morten";
            s.id = 839;
            s.Enlisted = new DateTime (2006, 12, 31);
            s.spouse = spouse;
            Type t = typeof(Student);

            TypeSystem.RegisterType(t);

            DelegateTranslator dtrans = TypeSystem.GetTranslator (t);
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
            IBOCache.Instance.FlushToDB();

            Console.WriteLine("Commit duration: {0}" , DateTime.Now - then);
            Console.ReadLine();
        }

    }
}
