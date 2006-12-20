#define RECREATE_DB
using System;
using System.Collections.Generic;
using System.Text;
using System.Query;
using System.Xml.XLinq;
using System.Data.DLinq;
using System.Data.SqlClient;
using System.Expressions;
using GenDB.DB;

namespace GenDB
{
    public enum Sex { MALE, FEMALE };

    class Program
    {
        static int nextID = 0;

        public class Person : AbstractBusinessObject
        {
            Sex sex;

            public Sex Sex
            {
                get { return sex; }
                set { sex = value; }
            }

            //int[] arr = new int[30];

            //public int[] Arr
            //{
            //    get { return arr; }
            //    set { arr = value; }
            //}
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

            int age;

            public int Age
            {
                get { return age; }
                set { age = value; }
            }
        }

        public class Student : Person { 
            public static long test = 100; //This should not be persisted since it is static
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

            //Expression<Func<Person, bool>> where1 = (Person p) => /* p.Spouse == null && */ p.Name == "Svend";
            //ExpressionRunner wroom = new ExpressionRunner();
            //wroom.LookIn(where1);
            // ******

            int objCount = 100;

            Table<Person> tp = new Table<Person>();

            for (short i = 0; i < objCount; i++)
            {
                Person p = new Person{ Name = "Navn " + i };
                if (i % 2 == 0) { p.Sex = Sex.FEMALE; }
                p.Age = i;
                tp.Add (p);
            }

            IEntityType etPerson = TypeSystem.GetEntityType (typeof(Person));
            IProperty propertyName = etPerson.GetProperty ("Name");
            IProperty propertyAge = etPerson.GetProperty("Age");

            OP_Equals nc1 = new OP_Equals(new CstProperty(propertyName), new CstString("Navn 1"));
            OP_LessThan nc2 = new OP_LessThan(new CstProperty(propertyAge), new CstLong(5));
            //OP_Equals nc2 = new OP_Equals(new CstProperty (propertyName), new CstString("Navn 5"));
            IWhereable wc = new ExprOr(nc1, nc2);
            MSWhereStringBuilder mswsb = new MSWhereStringBuilder();
            mswsb.Visit(wc);
                
            IBOCache.FlushToDB();

            var es = from epp in tp
                     //where epp.Age != 83
                     //where epp.Name != "Navn 3"
                     //where epp.Sex == Sex.FEMALE
                     where epp.Name == "Navn 6" || epp.Age == 7
                     select epp;

            //foreach (IEntity e in Configuration.GenDB.Where(wc))
            //{
            //    DelegateTranslator trans = TypeSystem.GetTranslator(e.EntityType.EntityTypePOID);
            //    IBusinessObject ibo = trans.Translate (e);
            //    Person pers = (Person)ibo;
            //    //pers.Name = "Knud Lavert";
            //    ObjectUtilities.PrintOut(ibo);
            //}

            foreach(Person p in es)
            {
                ObjectUtilities.PrintOut (p);
            }

            IBOCache.FlushToDB();
            
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


