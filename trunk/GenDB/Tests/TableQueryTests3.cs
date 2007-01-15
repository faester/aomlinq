using System;
using System.Collections.Generic;
using System.Text;
using GenDB;
using NUnit.Framework;
using System.Query;
using System.Expressions;
using CommonTestObjects;

namespace QueryToSqlTranslationTests
{
    [TestFixture]
    public class TableQueryTests3
    {
        private const int ELEMENTS_TO_STORE = 40;
        Table<ContainsAllPrimitiveTypes> tableAllPrimitives = null;
        DataContext dataContext = DataContext.Instance;
        Table<TestPerson> ttp ;

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            tableAllPrimitives = null;
            ttp = null;
        }

        [SetUp]
        public void TestSetup()
        {
            InitTableAllPrimitives();
            InitTableOfPersons();
            dataContext.SubmitChanges();
        }

        public void InitTableOfPersons()
        {
            ttp = dataContext.CreateTable<TestPerson>();
            TestPerson lastPerson = null;

            for (int i = 0; i < 10; i++)
            {
                TestPerson tp = new TestPerson();
                tp.Name = "Name" + i.ToString();
                tp.Age = i;
                tp.Spouse = lastPerson;
                lastPerson = tp;
                ttp.Add(tp);
            }
        }

        private void InitTableAllPrimitives()
        {
            dataContext.SubmitChanges();
            GC.Collect();
            tableAllPrimitives = dataContext.CreateTable<ContainsAllPrimitiveTypes>();
            tableAllPrimitives.Clear();
            dataContext.SubmitChanges();

            for (int i = 0; i < ELEMENTS_TO_STORE; i++)
            {
                // It is important not to keep references to the 
                // table elements, since they should be garbage 
                // collected, to ensure later database retrieval 
                // without regards to the cached copies.
                ContainsAllPrimitiveTypes capt = new ContainsAllPrimitiveTypes();
                capt.Boo = (i % 2) == 0; // Has test
                capt.Lng = i % 2; // Has test
                capt.Integer = i % 2; // Has test
                capt.Str = i % 2 == 0 ? "1" : "0"; // Has test
                capt.Ch = i % 2 == 0 ? '1' : '0';  // Has test
                capt.Dt = i % 2 == 0 ? new DateTime(0) : new DateTime(1); // Has test
                capt.Fl = i % 2; // Has test
                capt.Dbl = i % 2; // Has test
                tableAllPrimitives.Add (capt);
            }
        }

        [TearDown]
        public void TearDown()
        {
            tableAllPrimitives.Clear();
            ttp.Clear();
            dataContext.SubmitChanges();
            tableAllPrimitives = null;
            ttp = null;
        }

        [Test]
        public void TestSubstring()
        {
            var p = from persons in ttp
                    where persons.Name.Substring(0,3) == "Nam"
                    select persons;
            Assert.Greater(p.Count,0,"did not find enough");
        }

        [Test]
        public void TestTrim()
        {
            var p1 = from persons in ttp
                    where persons.Name.Trim() == " Name3"
                    select persons;
            Assert.AreEqual(1,p1.Count,"not the correct number of persons");

            var p2 = from persons in ttp
                    where persons.Name.TrimStart(' ') == " Name3"
                    select persons;
            Assert.AreEqual(1,p2.Count,"not the correct number of persons");

            var p3 = from persons in ttp
                    where persons.Name.TrimEnd(' ') == "Name3 "
                    select persons;
            Assert.AreEqual(1,p3.Count,"not the correct number of persons");
        }

        [Test]
        public void TestIndexOf()
        {
            var p = from persons in ttp
                    where persons.Name.IndexOf("3") == 4
                    select persons;
            Assert.AreEqual(1,p.Count,"incorrect number of persons returned");
        }
    }
}
