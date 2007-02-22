using System;
using System.Collections.Generic;
using System.Text;
using GenDB;
using NUnit.Framework;
using CommonTestObjects;
using System.Query;
using System.Expressions;

namespace QueryToSqlTranslationTests
{
    [TestFixture]
    public class TableQueryTests2
    {
        private const int ELEMENTS_TO_STORE = 40;
        Table<ContainsAllPrimitiveTypes> tableAllPrimitives = null;
        DataContext dataContext = DataContext.Instance;
        Table<TestPerson> ttp;
        LinkedList<TestPerson> thePersons;

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            tableAllPrimitives = null;
            ttp = null;
        }

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            if (!dataContext.IsInitialized)
            {
                dataContext.Init();
            }
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
            thePersons = new LinkedList<TestPerson>();
            ttp = dataContext.GetTable<TestPerson>();
            TestPerson lastPerson = null;

            for (int i = 0; i < 10; i++)
            {
                TestPerson tp = new TestPerson();
                tp.Name = "Name" + i.ToString();
                tp.Age = i;
                if (i % 2 == 0) tp.GoodLooking = true;
                tp.Spouse = lastPerson;
                lastPerson = tp;
                ttp.Add(tp);
                thePersons.AddLast(tp);
            }
        }

        private void InitTableAllPrimitives()
        {
            dataContext.SubmitChanges();
            GC.Collect();
            tableAllPrimitives = dataContext.GetTable<ContainsAllPrimitiveTypes>();
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
            Assert.IsTrue(tableAllPrimitives.ExprFullySqlTranslatable, "Expression included linq function. This should not be the case.");

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
            Assert.IsTrue(xs.ExprFullySqlTranslatable, "Expression included linq function. This should not be the case.");
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
            Assert.IsTrue(xs.ExprFullySqlTranslatable, "Expression included linq function. This should not be the case.");
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
            Assert.IsTrue(xs.ExprFullySqlTranslatable, "Expression included linq function. This should not be the case.");
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
            Assert.IsTrue(xs.ExprFullySqlTranslatable, "Expression included linq function. This should not be the case.");
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
            Assert.IsTrue(xs.ExprFullySqlTranslatable, "Expression included linq function. This should not be the case.");
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
            Assert.IsTrue(xs.ExprFullySqlTranslatable, "Expression included linq function. This should not be the case.");
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
            Assert.IsTrue(xs.ExprFullySqlTranslatable, "Expression included linq function. This should not be the case.");
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
            Assert.IsTrue(xs.ExprFullySqlTranslatable, "Expression included linq function. This should not be the case.");
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
                Console.WriteLine(tst.DBIdentity + " : " + capt.DBIdentity);
                Assert.IsTrue (object.ReferenceEquals (tst, capt), "Returned object did not ReferenceEquals requested object.");
                foundIt = true;
            }

            Assert.IsTrue (foundIt, "Did not find the added value.");
            Assert.IsTrue(res.ExprFullySqlTranslatable, "Expression included linq function. This should not be the case.");
        }

        [Test]
        public void TestDBVsAppTranslation()
       {
            foreach(TestPerson p in thePersons)
            {
                p.Name = "Kn�skade";
            }

            var ns = from ps in ttp
                     where ps.Name == "Name1" || ps.Name == "Name2"
                     select ps;

            foreach(TestPerson p in ns)
            {
                Assert.IsTrue(p.Name == "Name1" || p.Name == "Name2", "Error in returned values. Application object has changed state since last commit.");
            }

       }

        [Test]
        public void TestReferenceFieldPropertyFilter1()
        {
            var qs = from persons in ttp
                     where persons.Spouse.Spouse.Name != "Name1"
                     select persons;

            int realCount = thePersons.Count<TestPerson>(p => p != null && p.Spouse != null && p.Spouse.Spouse != null && p.Spouse.Spouse.Name != "Name1");
            int count = 0;


            foreach (var person in qs)
            {
                count++;
                TestPerson spouse = person.Spouse;
                TestPerson spouseSpouse = spouse == null ? null : spouse.Spouse;

                string spouseName = spouse != null ? spouse.Name : "N/A (No Spouse)";
                string spouseSpouseName = spouseSpouse != null ? spouseSpouse.Name : "N/A (No Spouse)";

                Assert.AreNotEqual("Name1", spouseSpouseName, "Spouse spouse name was ALL WRONG!");
            }
            Assert.IsTrue(qs.ExprFullySqlTranslatable, "Expression included linq function. This should not be the case.");
            Assert.AreEqual(realCount, count, "Wrong number of elements returned.");
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
                Assert.AreNotEqual("Name1", spouseSpouseName, "Spouse spouse name was ALL WRONG!");
                Assert.IsTrue(spouseAge > 3, "Spouse age was wrong");
            }
            Assert.IsTrue(qs.ExprFullySqlTranslatable, "Expression included linq function. This should not be the case.");
        }

        [Test]
        public void TestReferenceFieldPropertyFilter3()
        {
            dataContext.SubmitChanges();
            
            var qs = from persons in ttp
                     where persons.Spouse.Spouse.Name != "Name1" && persons.Name != "Name1" && persons.Spouse.Name != "Name1"
                     select persons;

            Assert.IsTrue(qs.ExprFullySqlTranslatable, "Expression included linq function. This should not be the case.");

            foreach (var person in qs)
            {
                TestPerson spouse = person.Spouse;
                TestPerson spouseSpouse = spouse == null ? null : spouse.Spouse;

                string spouseName = spouse != null ? spouse.Name : "N/A (No Spouse)";
                string spouseSpouseName = spouseSpouse != null ? spouseSpouse.Name : "N/A (No Spouse)";

                Assert.AreNotEqual("Name1", person.Name, "Person name was ALL WRONG!");
                Assert.AreNotEqual("Name1", spouseName, "Spouse name was ALL WRONG!");
                Assert.AreNotEqual("Name1", spouseSpouseName, "Spouse spouse name was ALL WRONG!");
            }
            Assert.IsTrue(qs.ExprFullySqlTranslatable, "Expression included linq function. This should not be the case.");
        }

        private void PersonRetrieveTest(Func<TestPerson, bool> discriminator)
        {
            int trueCount = thePersons.Count<TestPerson>(discriminator);
            thePersons = null;
            dataContext.SubmitChanges();

            int count = 0;

            var k = ttp.Where<TestPerson>(discriminator);

            foreach(TestPerson p in k)
            {
                Assert.IsTrue(discriminator(p), "Error in returned result");
                count++;
            }

            Assert.AreEqual (trueCount, count, "Wrong number of elements returned from table.");
            Assert.AreNotEqual(0, trueCount, "No tests should have conditions that are not met by any object in the table.");
        }

        [Test]
        public void TestReferenceFieldPropertyFilter4()
        {
            string testName = "Name3";
            dataContext.SubmitChanges();

            Func<TestPerson, bool> k = 
                (TestPerson p) => 
                    (p.Spouse != null && p.Spouse.Spouse != null && p.Spouse.Spouse.Name == testName) 
                    || p.Name == testName 
                    || (p.Spouse != null && p.Spouse.Name == testName);

            PersonRetrieveTest(k);
        }


        
        
        [Test]
        public void TestReferenceFieldBooleanPropertyFilter()
        {
            var qs_false  = from persons in ttp
                      where persons.Spouse.GoodLooking == false
                      select persons;
            //Assert.AreEqual(4, qs_false.Count,"there should be 4 ugly bastards out ther!!");

            foreach(var person in qs_false)
            {
                Assert.IsFalse(person.Spouse.GoodLooking,"this person should have been ugly");
            }

            var qs_true  = from persons in ttp
                      where persons.Spouse.GoodLooking == true
                      select persons;
            // Assert.AreEqual(5, qs_true.Count,"there should be 5 beauty's out there!!");

            foreach(var person in qs_true)
            {
                Assert.IsTrue(person.Spouse.GoodLooking, "this person should have been good-looking");
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
                Assert.IsTrue (tp.Age < 9 && tp.Name == "Name2", "Wrong result produced.");
            }

            Assert.IsTrue (foundSome, "No results produced. Test that test has meaningful condition.");
            Assert.IsTrue(es.ExprFullySqlTranslatable, "Expression included linq function. This should not be the case.");
        }
    }
}
