using System;
using System.Collections.Generic;
using System.Text;
using GenDB;
using NUnit.Framework;
using System.Query;
using System.Expressions;

namespace TableTests
{
    [TestFixture]
    public class TableQueryTests2
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
        public void TestCanTranslateFrom()
        {
            // Testing is done by the setup method. Just want to check 
            // that the elements can be saved without exception.
        }

        [Test]
        public void TestCanRetrieve()
        {
            int count = 0;
            foreach (ContainsAllPrimitiveTypes capt in tableAllPrimitives)
            {
                count++;
            }
            Assert.AreEqual(ELEMENTS_TO_STORE, count, "Returned unexpected number of results");
        }

        [Test]
        public void TestBoolFilter()
        {
            var xs = from capts in tableAllPrimitives
                     where capts.Boo
                     select capts;

            int count = 0;
            foreach(var x in xs)
            {
                count++;
                Assert.IsTrue (x.Boo, "Filter error: All Boo values should be true.");
            }

            Assert.AreEqual (ELEMENTS_TO_STORE / 2, count, "Incorrect number of elements returned.");
        }

        [Test]
        public void TestIntFilter()
        {
            var xs = from capts in tableAllPrimitives
                     where capts.Integer == 0
                     select capts;

            int count = 0;
            foreach(var x in xs)
            {
                count++;
                Assert.IsTrue (x.Integer == 0, "Filter error: All int values should be zero.");
            }

            Assert.AreEqual (ELEMENTS_TO_STORE / 2, count, "Incorrect number of elements returned.");
        }

        [Test]
        public void TestLongFilter()
        {
            var xs = from capts in tableAllPrimitives
                     where capts.Lng == 0
                     select capts;

            int count = 0;
            foreach(var x in xs)
            {
                count++;
                Assert.IsTrue (x.Lng == 0, "Filter error: All Lng values should be zero.");
            }
            Assert.AreEqual (ELEMENTS_TO_STORE / 2, count, "Incorrect number of elements returned.");
        }

        [Test]
        public void TestStrFilter()
        {
            var xs = from capts in tableAllPrimitives
                     where capts.Str == "0"
                     select capts;

            int count = 0;
            foreach(var x in xs)
            {
                count++;
                Assert.IsTrue (x.Str == "0", "Filter error: All Str values should be \"0\".");
            }

            Assert.AreEqual (ELEMENTS_TO_STORE / 2, count, "Incorrect number of elements returned.");
        }

        [Test]
        public void TestDateTimeFilter()
        {
            DateTime filter = new DateTime(0);
            var xs = from capts in tableAllPrimitives
                     where capts.Dt == filter
                     select capts;

            int count = 0;
            foreach(var x in xs)
            {
                count++;
                Assert.IsTrue (x.Dt == filter, "Filter error: All Dt values should be " + filter + ".");
            }

            Assert.AreEqual (ELEMENTS_TO_STORE / 2 , count, "Incorrect number of elements returned.");
        }

        [Test]
        public void TestDoubleFilter()
        {
            var xs = from capts in tableAllPrimitives
                     where capts.Dbl == 0
                     select capts;

            int count = 0;
            foreach(var x in xs)
            {
                count++;
                Assert.IsTrue (x.Dbl == 0, "Filter error: All Dbl values should be 0.");
            }

            Assert.AreEqual (ELEMENTS_TO_STORE / 2 , count, "Incorrect number of elements returned.");
        }

        [Test]
        public void TestFloatFilter()
        {
            var xs = from capts in tableAllPrimitives
                     where capts.Fl == 0
                     select capts;

            int count = 0;
            foreach(var x in xs)
            {
                count++;
                Assert.IsTrue (x.Fl == 0, "Filter error: All Fl values should be 0.");
            }

            Assert.AreEqual (ELEMENTS_TO_STORE / 2 , count, "Incorrect number of elements returned.");
        }

        [Test]
        public void TestCharFilter()
        {
            var xs = from capts in tableAllPrimitives
                     where capts.Ch == '1'
                     select capts;

            int count = 0;
            foreach(var x in xs)
            {
                count++;
                Assert.IsTrue (x.Ch == '1', "Filter error: All Fl values should be 0.");
            }

            Assert.AreEqual (ELEMENTS_TO_STORE / 2 , count, "Incorrect number of elements returned.");
        }

        [Test]
        public void TestReferenceEqualsFilter()
        {
            ContainsAllPrimitiveTypes capt = new ContainsAllPrimitiveTypes();
            capt.Str = "ego";

            tableAllPrimitives.Add(capt);

            dataContext.SubmitChanges();

            var res = from capts in tableAllPrimitives
                      where capts == capt
                      select capts;

            bool foundIt = false;

            foreach(ContainsAllPrimitiveTypes tst in res)
            {
                if (foundIt) { Assert.Fail("Found more than one result."); }
                Assert.IsTrue (object.ReferenceEquals (tst, capt));
                foundIt = true;
            }

            Assert.IsTrue (foundIt, "Did not find the added value.");
        }

        [Test]
        public void TestReferenceFieldPropertyFilter1()
        {
            var qs = from persons in ttp
                     where persons.Spouse.Spouse.Name != "Name1"
                     select persons;

            foreach (var person in qs)
            {
                TestPerson spouse = person.Spouse;
                TestPerson spouseSpouse = spouse == null ? null : spouse.Spouse;

                string spouseName = spouse != null ? spouse.Name : "N/A (No Spouse)";
                string spouseSpouseName = spouseSpouse != null ? spouseSpouse.Name : "N/A (No Spouse)";

                Console.WriteLine("Person.Name = '{0}', Person.Spouse.Name = {1},  Person.Spouse.Spouse.Name = {2}", person.Name, spouseName, spouseSpouseName);
                Assert.AreNotEqual("Name1", spouseSpouseName, "Spouse spouse name was ALL WRONG!");
            }
        }
 
        [Test]
        public void TestReferenceFieldPropertyFilter2()
        {
            var qs = from persons in ttp
                     where persons.Spouse.Spouse.Name != "Name1" && persons.Spouse.Age > 3
                     select persons;

            foreach (var person in qs)
            {
                TestPerson spouse = person.Spouse;
                TestPerson spouseSpouse = spouse == null ? null : spouse.Spouse;

                string spouseName = spouse != null ? spouse.Name : "N/A (No Spouse)";
                string spouseSpouseName = spouseSpouse != null ? spouseSpouse.Name : "N/A (No Spouse)";
                int spouseAge = spouse != null ? spouse.Age : int.MaxValue;
                Console.WriteLine("Person.Name = '{0}', Person.Spouse.Name = {1},  Person.Spouse.Spouse.Name = {2}", person.Name, spouseName, spouseSpouseName);
                Assert.AreNotEqual("Name1", spouseSpouseName, "Spouse spouse name was ALL WRONG!");
                Assert.IsTrue(spouseAge > 3, "Spouse age was wrong");
            }
        }

        [Test]
        public void TestReferenceFieldPropertyFilter3()
        {
            TestPerson lastPerson = null;

            dataContext.SubmitChanges();


            var qs = from persons in ttp
                     where persons.Spouse.Spouse.Name != "Name1" && persons.Name != "Name1" && persons.Spouse.Name != "Name1"
                     select persons;

            foreach (var person in qs)
            {
                TestPerson spouse = person.Spouse;
                TestPerson spouseSpouse = spouse == null ? null : spouse.Spouse;

                string spouseName = spouse != null ? spouse.Name : "N/A (No Spouse)";
                string spouseSpouseName = spouseSpouse != null ? spouseSpouse.Name : "N/A (No Spouse)";

                Console.WriteLine("Person.Name = '{0}', Person.Spouse.Name = {1},  Person.Spouse.Spouse.Name = {2}", person.Name, spouseName, spouseSpouseName);
                Assert.AreNotEqual("Name1", person.Name, "Person name was ALL WRONG!");
                Assert.AreNotEqual("Name1", spouseName, "Spouse name was ALL WRONG!");
                Assert.AreNotEqual("Name1", spouseSpouseName, "Spouse spouse name was ALL WRONG!");
            }
        }

        [Test]
        public void TestLessThan()
        {
            var es = from epp in ttp     
                where epp.Name == "Name2" && epp.Age < 9
                select epp;

            bool foundSome = false;
            foreach (TestPerson tp in es)
            {
                foundSome = true;
                Console.WriteLine(tp.Age + " " + tp.Name);
                Assert.IsTrue (tp.Age < 9 && tp.Name == "Name2", "Wrong result produced.");
            }

            Assert.IsTrue (foundSome, "No results produced. Test that test has meaningful condition.");
        }
    }
}
