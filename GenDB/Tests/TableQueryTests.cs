#define RECREATE_DB
using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.Query;
using System.Xml.XLinq;
using System.Expressions;
using GenDB;

namespace TableTests
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

            BOList<Person> others = BOListFactory.BOListRef<Person>();
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
            tp.Clear();
            Configuration.SubmitChanges();

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
        public void Equality()
        {
            // int's
            Assert.IsTrue(PropertyEqualsNumber(tp,trueInt),"Age should exist");
            Assert.IsFalse(PropertyEqualsNumber(tp,falseInt),"Age should not exist");
            Assert.IsTrue(PropertyNotEqualsNumber(tp,trueInt),"Age not equal to should exist");
            // string's
            Assert.IsTrue(PropertyEqualsString(tp,trueStr),"name should exist");
            Assert.IsFalse(PropertyEqualsString(tp,falseStr),"name should not exist");
            // enum's
            Assert.IsTrue(PropertyEqualsEnum(tp,trueEnum),"a female should exist");
            Assert.IsFalse(PropertyEqualsEnum(tp,falseEnum),"a nueter should not exist");
            // Reference
            Assert.IsTrue(PropertyEqualsReference(tp,spouse),"Spouse should exist");
            Assert.IsFalse(PropertyEqualsReference(tp,johnDoe),"John Doe should not be a Spouse");
            // Null Reference
            Assert.IsTrue(PropertyEqualsReference(tp,null),"Spouse = null should exist");
        }

        [Test]
        public void EqualityLessOrLarger()
        {
            Assert.IsTrue(PropertyLessThanNumber(tp,trueInt),"Age less than should exist");
            Assert.IsTrue(PropertyLargerThanNumber(tp,trueInt),"Age larger than should exist");
            Assert.IsFalse(PropertyLessThanNumber(tp,smallInt),"Age less than should not exist");
            Assert.IsFalse(PropertyLargerThanNumber(tp,largeInt),"Age larger than should not exists");
        }

        [Test]
        public void OrElse()
        {
            Assert.IsTrue(StringOrElseNumber(tp,trueStr, falseInt),"Name(true) OR Age(false) should exist");
            Assert.IsFalse(StringOrElseNumber(tp,falseStr,falseInt),"Name(false) OR Age(false), should not exist");
        }

        [Test]
        public void AndAlso()
        {
            Assert.IsFalse(StringAndAlsoNumber(tp,falseStr, trueInt),"Name(false) AND Age(true), should not exist");
            Assert.IsTrue(StringAndAlsoNumber(tp,trueStr,trueInt),"Name(true) AND Age(true) should exist");
        }

        [Test]
        public void Not()
        {
            // GT, number
            Assert.IsTrue(NotGTExpression(tp,trueInt),"Age should exist");
            Assert.IsFalse(NotGTExpression(tp, falseInt),"Age should not exist");
            // LT, number
            Assert.IsTrue(NotLTExpression(tp,trueInt), "Age should exist");
            Assert.IsFalse(NotLTExpression(tp,largeInt), "Age should not exist");
            //GTorEQ, number
            Assert.IsTrue(NotGTorEQ(tp,trueInt), "Age should exist");
            Assert.IsFalse(NotGTorEQ(tp, falseInt), "Age should not exist");
            //LTorEQ, number
            Assert.IsTrue(NotLTorEQ(tp, trueInt), "Age should exist");
            Assert.IsFalse(NotLTorEQ(tp, largeInt), "Age should not exist");

            // NotEq, string
            Assert.IsTrue(StringNotEquals(tp, trueStr),"Name should exist");
            // NotNe, string
            Assert.IsTrue(StringNotEqualsNot(tp, trueStr),"Name should exist");
            Assert.IsFalse(StringNotEqualsNot(tp, falseStr), "Name should not exist");

            // NotEq, reference
            Assert.IsTrue(NotEqReference(tp,johnDoe),"Person should exist");
            Assert.IsFalse(NotEqReference(tp,spouse),"Person shuld not exist");
            // NotNe, reference
            Assert.IsTrue(NotNeReference(tp,spouse),"Person should exist");
            Assert.IsFalse(NotNeReference(tp,johnDoe),"Person should not exist");
        }

        [Test]
        public void LessOrEqual()
        {
            Assert.IsTrue(PropertyLessThanOrEqualsNumber(tp,trueInt),"Age should exist");
            Assert.IsFalse(PropertyLessThanOrEqualsNumber(tp,falseInt),"Age should not exist");
        }

        [Test]
        public void LargerOrEqual()
        {
            Assert.IsTrue(PropertyLargerThanOrEqualsNumber(tp,trueInt),"Age should exist");
            Assert.IsFalse(PropertyLargerThanOrEqualsNumber(tp,largeInt),"Age should not exist");
        }

        #endregion

        #region TestHelpers

        private bool PropertyEqualsNumber(Table<Person> t, int n)
        {
            var v = from col in t
                    where col.Age == n
                    select col;
            return HasAtLeastOne(v);
        }

        private bool PropertyNotEqualsNumber(Table<Person> t, int n)
        {
            var v = from col in t
                    where col.Age != n
                    select col;
            return HasAtLeastOne(v);
        }

        private bool PropertyLessThanNumber(Table<Person> t, int n)
        {
            var v = from col in t
                    where col.Age < n
                    select col;
            return HasAtLeastOne(v);
        }

        private bool PropertyLessThanOrEqualsNumber(Table<Person> t, int n)
        {
            var v = from col in t
                    where col.Age <= n
                    select col;
            return HasAtLeastOne(v);
        }

        private bool PropertyLargerThanOrEqualsNumber(Table<Person> t, int n)
        {
            var v = from col in t
                    where col.Age >= n
                    select col;
            return HasAtLeastOne(v);
        }

        private bool PropertyLargerThanNumber(Table<Person> t, int n)
        {
            var v = from col in t
                    where col.Age > n
                    select col;
            return HasAtLeastOne(v);
        }

        private bool PropertyEqualsString(Table<Person> t, string s) 
        {
            var v = from col in t
                    where col.Name == s
                    select col;
            return HasAtLeastOne(v);
        }

        private bool PropertyEqualsEnum(Table<Person> t, Sex e)
        {
            var v = from col in t
                    where col.Sex == e
                    select col;
            return HasAtLeastOne(v);
        }

        private bool StringAndAlsoNumber(Table<Person> t, string s, int n)
        {
            var v = from col in t
                    where col.Name == s && col.Age == n
                    select col;
            return HasAtLeastOne(v);
        }

        private bool StringOrElseNumber(Table<Person> t, string s, int n)
        {
            var v = from col in t
                    where col.Name == s || col.Age == n
                    select col;
            return HasAtLeastOne(v);
        }

        private bool PropertyEqualsReference(Table<Person> t, Person p)
        {
            var v = from col in t
                    where col.Spouse == p
                    select col;
            return HasAtLeastOne(v);
        }

        private bool StringNotEquals(Table<Person> t, string s)
        {
            var v = from col in t
                    where !(col.Name == s)
                    select col;
            return HasAtLeastOne(v);
        }

        private bool StringNotEqualsNot(Table<Person> t, string s)
        {
            var v = from col in t
                    where !(col.Name != s)
                    select col;
            return HasAtLeastOne(v);
        }

        private bool NotGTExpression(Table<Person> t, int n)
        {
            var v = from col in t
                    where !(col.Age > n)
                    select col;
            return HasAtLeastOne(v);
        }

        private bool NotLTExpression(Table<Person> t, int n)
        {
            var v = from col in t
                    where !(col.Age < n)
                    select col;
            return HasAtLeastOne(v);
        }

        private bool NotGTorEQ(Table<Person> t, int n)
        {
            var v = from col in t
                    where !(col.Age >= n)
                    select col;
            return HasAtLeastOne(v);
        }

        private bool NotLTorEQ(Table<Person> t, int n)
        {
            var v = from col in t
                    where !(col.Age <= n)
                    select col;
            return HasAtLeastOne(v);
        }

        private bool NotEqReference(Table<Person> t, Person p)
        {
            var v = from col in t
                    where !(col.Spouse==p)
                    select col;
            return HasAtLeastOne(v);
        }

        private bool NotNeReference(Table<Person> t, Person p)
        {
            var v = from col in t  
                    where !(col.Spouse != p)
                    select col;
            return HasAtLeastOne(v);
        }

        private bool HasAtLeastOne(Table<Person> v)
        {
            int n = v.Count;
            if(n>0) return true;
            else return false;
        }

        #endregion

    }
}