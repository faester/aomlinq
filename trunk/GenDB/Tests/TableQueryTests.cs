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
    //public enum Sex { MALE, FEMALE, NEUTER };



    [TestFixture]
    public class TestTableQuery
    {
        #region configuration

        public enum Sex { MALE, FEMALE, NEUTER };
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
            [Volatile]
            public long id = nextID++; // Should not be persisted due to attribute.
            private DateTime enlisted;

            public DateTime Enlisted
            {
                get { return enlisted; }
                set { enlisted = value; }
            }
        }

        [TestFixtureSetUp]
        public void Initialize()
        {
            Configuration.RebuildDatabase = true;
        }

        [SetUp]
        public void TestQuery()
        {
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
        public void EqualityExists()
        {
            // int's
            Assert.IsTrue(propertyEqualsNumber(tp,3),"Age should exist");
            Assert.IsTrue(propertyLessThanNumber(tp,3),"Age less than should exist");
            Assert.IsTrue(propertyLargerThanNumber(tp,3),"Age larger than should exist");
            Assert.IsTrue(propertyNotEqualsNumber(tp,3),"Age not equal to should exist");
            // string's
            Assert.IsTrue(propertyEqualsString(tp,"Navn 3"),"name should exist");
            // enum's
            Assert.IsTrue(propertyEqualsEnum(tp,Sex.FEMALE),"a female should exist");
        }

        [Test]
        public void EqualityNotExist()
        {
            // int's
            Assert.IsFalse(propertyEqualsNumber(tp,-1),"Age should not exist");
            Assert.IsFalse(propertyLessThanNumber(tp,0),"Age less than should not exist");
            Assert.IsFalse(propertyLargerThanNumber(tp,10),"Age larger than should not exists");
            // string's
            Assert.IsFalse(propertyEqualsString(tp,"**HubbaBubba**"),"name should not exist");
            // enum's
            Assert.IsFalse(propertyEqualsEnum(tp,Sex.NEUTER),"a nueter should not exist");
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

        private bool propertyLessThanNumber(Table<Person> t, int n)
        {
            var v = from col in t
                    where col.Age < n
                    select col;
            if(v.Count>0)
                return true;
            else
                return false;
        }

        private bool propertyLargerThanNumber(Table<Person> t, int n)
        {
            var v = from col in t
                    where col.Age > n
                    select col;
            if(v.Count>0)
                return true;
            else
                return false;
        }

        private bool propertyNotEqualsNumber(Table<Person> t, int n)
        {
            var v = from col in t
                    where col.Age != n
                    select col;
            if(v.Count>0)
                return true;
            else
                return false;
        }

        private bool propertyEqualsString(Table<Person> t, string s) 
        {
            var v = from col in t
                    where col.Name == s
                    select col;
            if(v.Count>0)
                return true;
            else
                return false;
        }

        private bool propertyEqualsEnum(Table<Person> t, Sex e)
        {
            var v = from col in t
                    where col.Sex == e
                    select col;
            if(v.Count>0)
                return true;
            else
                return false;
        }

        #endregion

    }
}
#endif