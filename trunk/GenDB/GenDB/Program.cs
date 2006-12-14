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
            Configuration.RebuildDatabase = true;

            Table<Person> table = new Table<Person>();
            table.Add(new Person {Name = "Poul"});
            table.Add(new Person {Name = "Nulle"});

            //Person[] pArray = new Person[]{
            //    new Person {Name = "Poul"},
            //    new Person {Name = "Nulle"}
            //};

            var pList = from p in table 
                        where p.Name == "Poul" 
                        select p;

            foreach(Person p in pList)
                Console.WriteLine(p.Name);

            //Expression<Func<Person, bool>> where1 = (Person p) => /* p.Spouse == null && */ p.Name == "Svend";
            //ExpressionRunner wroom = new ExpressionRunner();
            //wroom.LookIn(where1);
            // ******

            int objCount = 100;


            Table<Person> tp = new Table<Person>();

            for (int i = 0; i < objCount; i++)
            {
                Person p = new Person{ Name = "Navn " + i };
                tp.Add (p);
            }


            if (!TypeSystem.IsTypeKnown(typeof(Person)))
            {
                TypeSystem.RegisterType(typeof(Person));
            }
            IEntityType etPerson = TypeSystem.GetEntityType (typeof(Person));
            IProperty propertyName = etPerson.GetProperty ("Name");

            OP_Equals nc1 = new OP_Equals(new CstProperty (propertyName), new CstString("Navn 1"));
            //OP_Equals nc2 = new OP_Equals( new CstString("Navn 10"), new CstProperty (propertyName));
            OP_Equals nc2 = new OP_Equals(new CstProperty (propertyName), new CstString("Navn 10"));
            IWhereable wc = new ExprOr(nc1, nc2);
            MSWhereStringBuilder mswsb = new MSWhereStringBuilder();
            mswsb.Visit(wc);
            
            Configuration.GenDB.CommitChanges();

            foreach(IEntity e in Configuration.GenDB.Where (wc))
            {
                DelegateTranslator trans = TypeSystem.GetTranslator(e.EntityType.EntityTypePOID);
                IBusinessObject ibo = trans.Translate (e);
                ObjectUtilities.PrintOut(ibo);
            }

            //Console.WriteLine(mswsb.WhereStr);
            // ******
            //Expression<Func<int, bool>> exprLambda1 = x => (x & 1) == 0;
            //Expression<Func<int,int,int>> exprLambda2 = (y,j) => y + j;

            //Expression<Func<Person, bool>> where1 = (Person p) => /* p.Spouse == null && */ p.Name == "Svend";
            //ExpressionRunner wroom = new ExpressionRunner();
            //wroom.LookIn(where1);
            // ******

            Console.ReadLine();
        }
    }
}
