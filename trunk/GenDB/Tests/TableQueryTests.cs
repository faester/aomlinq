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
        #region member data

        public enum Sex { MALE, FEMALE, NEUTER };
        static int nextID = 0;
        private Table<Person> tp;

        private string trueStr, falseStr;
        private int trueInt, falseInt, smallInt, largeInt;
        private Sex trueEnum, falseEnum;
        private Person spouse, johnDoe;

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

        #endregion

        #region configuration

        [TestFixtureSetUp]
        public void Initialize()
        {
            Configuration.RebuildDatabase = true;
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

            spouse = new Person{Name = "spouse", Age = 9};
            johnDoe = new Person{Name = "John Doe"};
            tp.Add(spouse);
            tp.Add(johnDoe);

            //Person p_p = new Person {Name = "NormalPerson", Spouse=spouse, Age=1};
            tp.Add (new Person {Name = "NormalPerson", Spouse=spouse, Age=1});
            
            Configuration.SubmitChanges();

            trueStr = "Navn 3";
            falseStr = "Jeppe på Bjerget";
            trueInt = 3;
            falseInt = -1;
            smallInt = 0;
            largeInt = 10;
            trueEnum = Sex.FEMALE;
            falseEnum = Sex.NEUTER;
        }

        [SetUp]
        public void StartTest()
        {
            
        }

        [TearDown]
        public void EndTest()
        {

        }

        [TestFixtureTearDown]
        public void Destroy()
        {

        }

        #endregion

        #region Tests

        [Test]
        public void EqualityExists()
        {
            // int's
            Assert.IsTrue(PropertyEqualsNumber(tp,trueInt),"Age should exist");
            Assert.IsTrue(PropertyLessThanNumber(tp,trueInt),"Age less than should exist");
            Assert.IsTrue(PropertyLargerThanNumber(tp,trueInt),"Age larger than should exist");
            Assert.IsTrue(PropertyNotEqualsNumber(tp,trueInt),"Age not equal to should exist");
            // string's
            Assert.IsTrue(PropertyEqualsString(tp,trueStr),"name should exist");
            // enum's
            Assert.IsTrue(PropertyEqualsEnum(tp,trueEnum),"a female should exist");
            // OrElse
            Assert.IsTrue(StringOrElseNumber(tp,trueStr, falseInt),"Name(true) OR Age(false) should exist");
            // AndAlso
            Assert.IsTrue(StringAndAlsoNumber(tp,trueStr,trueInt),"Name(true) AND Age(true) should exist");
            // Reference
            Assert.IsTrue(PropertyEqualsReference(tp,spouse),"Spouse should exist");
        }

        [Test]
        public void EqualityNotExist()
        {
            // int's
            Assert.IsFalse(PropertyEqualsNumber(tp,falseInt),"Age should not exist");
            Assert.IsFalse(PropertyLessThanNumber(tp,smallInt),"Age less than should not exist");
            Assert.IsFalse(PropertyLargerThanNumber(tp,largeInt),"Age larger than should not exists");
            // string's
            Assert.IsFalse(PropertyEqualsString(tp,falseStr),"name should not exist");
            // enum's
            Assert.IsFalse(PropertyEqualsEnum(tp,falseEnum),"a nueter should not exist");
            // OrElse
            Assert.IsFalse(StringOrElseNumber(tp,falseStr,falseInt),"Name(false) OR Age(false), should not exist");
            // AndAlso
            Assert.IsFalse(StringAndAlsoNumber(tp,falseStr, trueInt),"Name(false) AND Age(true), should not exist");
            // Reference
            Assert.IsFalse(PropertyEqualsReference(tp,johnDoe),"John Doe should not be a Spouse");
        }

        #endregion

        #region TestHelpers

        private bool PropertyEqualsNumber(Table<Person> t, int n)
        {
            var v = from col in t
                    where col.Age == n
                    select col;
            if(v.Count>0)
                return true;
            else
                return false;
        }

        private bool PropertyLessThanNumber(Table<Person> t, int n)
        {
            var v = from col in t
                    where col.Age < n
                    select col;
            if(v.Count>0)
                return true;
            else
                return false;
        }

        private bool PropertyLargerThanNumber(Table<Person> t, int n)
        {
            var v = from col in t
                    where col.Age > n
                    select col;
            if(v.Count>0)
                return true;
            else
                return false;
        }

        private bool PropertyNotEqualsNumber(Table<Person> t, int n)
        {
            var v = from col in t
                    where col.Age != n
                    select col;
            if(v.Count>0)
                return true;
            else
                return false;
        }

        private bool PropertyEqualsString(Table<Person> t, string s) 
        {
            var v = from col in t
                    where col.Name == s
                    select col;
            if(v.Count>0)
                return true;
            else
                return false;
        }

        private bool PropertyEqualsEnum(Table<Person> t, Sex e)
        {
            var v = from col in t
                    where col.Sex == e
                    select col;
            if(v.Count>0)
                return true;
            else
                return false;
        }

        private bool StringAndAlsoNumber(Table<Person> t, string s, int n)
        {
            var v = from col in t
                    where col.Name == s && col.Age == n
                    select col;
            if(v.Count>0)
                return true;
            else
                return false;
        }

        private bool StringOrElseNumber(Table<Person> t, string s, int n)
        {
            var v = from col in t
                    where col.Name == s || col.Age == n
                    select col;
            if(v.Count>0)
                return true;
            else
                return false;
        }

        private bool PropertyEqualsReference(Table<Person> t, Person p)
        {
            var v = from col in t
                    where col.Spouse == p
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