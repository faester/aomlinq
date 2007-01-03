#if DEBUG
#define RECREATE_DB
using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.Query;
using System.Xml.XLinq;
using System.Data.DLinq;
using System.Data.SqlClient;
using System.Expressions;

namespace GenDB
{
    

    [TestFixture]
    public class TestTableQuery
    {
        #region configuration

        public enum Sex { MALE, FEMALE };
        static int nextID = 0;
        Table<Person> tp;

        public class Car : AbstractBusinessObject
        {
            string brand = "Volvo";

            public string Brand
            {
                get { return brand; }
                set { brand = value; }
            }
        }

        public class Person : AbstractBusinessObject
        {
            Sex sex;
            public Person() { /* empty */ }

            Car car = new Car();
            public Car Car
            {
                get { return car; }
                set { car = value; }
            }

            BOList<Person> others = new BOList<Person>();
            [Volatile]
            public BOList<Person> Others
            {
                get { return others; }
                set { others = value; }
            }

            public Sex Sex
            {
                get { return sex; }
                set { sex = value; }
            }

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


        [SetUp]
        public void TestQuery()
        {
            //Configuration.RebuildDatabase = true;
            int objCount = 10;
            tp = new Table<Person>();

            for (short i = 0; i < objCount; i++)
            {
                Car c = new Car();
                c.Brand += i.ToString();
                Person p = new Person{ Name = "Navn " + i };
                p.Car = c;
                if (i % 2 == 0) { p.Sex = Sex.FEMALE; }
                p.Age = i;
                tp.Add (p);
            }

            Person s_p = new Person{ Name = "SpousePerson"};
            s_p.Age = 9;
            tp.Add (s_p);
            Person p_p = new Person {Name = "NormalPerson", Spouse=s_p, Age=1};
            tp.Add (p_p);
            
            Configuration.SubmitChanges();
        }

        #endregion

        #region Tests

        [Test]
        public void TestNumbersAndProperties()
        {
            //Assert.Less(0,propertyEqualsNumber(tp,3),"there should be at least one Person with Age = 3");
            //Assert.Less(0,propertyLessThanNumber(tp,3),"there should be at least one Person with Age < 3");
            //Assert.Less(0,propertyLargerThanNumber(tp,3),"there should be at least one Person with Age > 3");
            //Assert.Less(0,propertyNotEqualsNumber(tp,3),"there should be at least one Person with Age != 3");
            //Assert.AreEqual(0,propertyLessThanNumber(tp,0),"there should not be any Persons with Age < 0");
            //Assert.AreEqual(0,propertyLargerThanNumber(tp,11),"there should not be any Persons with Age > 10");
        }

        [Test]
        public void EqualityExists()
        {
            Assert.IsTrue(propertyEqualsNumber(tp,3));
        }

        [Test]
        public void EqualityNotExist()
        {
            Assert.IsFalse(propertyEqualsNumber(tp,-1));
        }

        #endregion

        #region TestHelpers

        private bool propertyEqualsNumber(Table<Person> t, int n)
        {
            var v = from col in t
                    where col.Age == n
                    select col;
            if(v.Count>0)
                return true;
            else
                return false;
        }

        private int propertyLessThanNumber(Table<Person> t, int n)
        {
            var v = from col in t
                    where col.Age < n
                    select col;
            return v.Count;
        }

        private int propertyLargerThanNumber(Table<Person> t, int n)
        {
            var v = from col in t
                    where col.Age > n
                    select col;
            return v.Count;
        }

        private int propertyNotEqualsNumber(Table<Person> t, int n)
        {
            var v = from col in t
                    where col.Age != n
                    select col;
            return v.Count;
        }

        #endregion

    }
}
#endif