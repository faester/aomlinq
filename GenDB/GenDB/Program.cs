#define RECREATE_DB
using System;
using System.Collections.Generic;
using System.Text;
using System.Query;
using System.Xml.XLinq;
using System.Data.DLinq;
using System.Data.SqlClient;
using System.Expressions;

namespace GenDB
{

    public class ExpressionRunner : ExpressionVisitor
    {
           
        public Expression LookIn(Expression expr)
        {
            return Visit(expr);
        }

    }

    class Program
    {
        static int nextID = 0;

        public class Person : AbstractBusinessObject
        {
            bool isMale = true;
            private string name = null;

            public string Name
            {
                get { return name; }
                set { name = value; }
            }
            private Person spouse = null;

            public Person Spouse
            {
                get { return spouse; }
                set { spouse = value; }
            }
            public char ch = 'a';
        }

        public class Student : Person { 
            static long test = 100; //This should not be persisted since it is static
            [Volatile]
            public long id = nextID++; // Should not be persisted due to attribute.
            private DateTime enlisted;

            public DateTime Enlisted
            {
                get { return enlisted; }
                set { enlisted = value; }
            } 
        }

        public static void Main(string[] args)
        {
            // ******
            Expression<Func<int, bool>> exprLambda1 = x => (x & 1) == 0;
            Expression<Func<int,int,int>> exprLambda2 = (y,j) => y + j;

            Expression<Func<Person, bool>> where1 = (Person p) => /* p.Spouse == null && */ p.Name == "Svend";
            ExpressionRunner wroom = new ExpressionRunner();
            wroom.LookIn(where1);
            // ******

            Console.ReadLine();
        }

        public static void OldMain(string[] args)
        {
            IGenericDatabase gdb = Configuration.GenDB;

            Person spouse = new Person();
            spouse.Name = "In your dreams...";
            Student s = new Student();
            s.Name = "Morten";
            s.id = 839;
            s.Enlisted = new DateTime (2006, 12, 31);
            s.Spouse = spouse;
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
            IBOCache.FlushToDB();

            Console.WriteLine("Commit duration: {0}" , DateTime.Now - then);
            Console.ReadLine();

        }
    }
}
