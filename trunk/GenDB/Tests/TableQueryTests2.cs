using System;
using System.Collections.Generic;
using System.Text;
using GenDB;
using NUnit.Framework;
using System.Query;

namespace TableTests
{
    [TestFixture]
    public class TableQueryTests2
    {
        private const int ELEMENTS_TO_STORE = 40;
        Table<ContainsAllPrimitiveTypes> tableAllPrimitives = null;
        DataContext dataContext = DataContext.Instance;

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            try
            {
                if (!dataContext.RebuildDatabase)
                {
                    dataContext.RebuildDatabase = true;
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Database must be rebuild prior to calling these tests.");
                throw e;
            }
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            tableAllPrimitives = null;
        }

        [SetUp]
        public void TestSetup()
        {
            InitTableAllPrimitives();
            dataContext.SubmitChanges();
            // Try to empty the cache, or at least ensure, 
            // that something is actually retrieved later on.
            System.GC.Collect();
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
            //Table<ContainsAllPrimitiveTypes> t_capt = new Table<ContainsAllPrimitiveTypes>();
            //t_capt.Clear();
            //System.GC.Collect();
            //dataContext.SubmitChanges();
            //dataContext.SubmitChanges();
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
            
            Console.Error.WriteLine ("***********************************************************");
            Console.Error.WriteLine ("****** TEST LONG FILTER ***********************************");
            Console.Error.WriteLine ("***********************************************************");
            
            Console.WriteLine ("***********************************************************");
            Console.WriteLine ("****** TEST LONG FILTER ***********************************");
            Console.WriteLine ("***********************************************************");
            Console.WriteLine(dataContext.DbBatchSize);
            Console.WriteLine (tableAllPrimitives);
            Console.WriteLine ("Elements in unfiltered table: " + tableAllPrimitives.Count);

            var xs = from capts in tableAllPrimitives
                     where capts.Lng == 0
                     select capts;

            Console.WriteLine (xs);
            Console.WriteLine(xs.Count);
            Console.WriteLine ("Elements in unfiltered table: " + tableAllPrimitives.Count);
            Console.WriteLine ("UNFILTERED: " + tableAllPrimitives);

            int count = 0;
            foreach(var x in xs)
            {
                count++;
                Assert.IsTrue (x.Lng == 0, "Filter error: All Lng values should be zero.");
            }

            Console.WriteLine ("Elements in unfiltered table: " + tableAllPrimitives.Count);
            Console.WriteLine ("***********************************************************");
            Console.WriteLine ("****** TEST LONG FILTER END *******************************");
            Console.WriteLine ("***********************************************************");

            Console.Error.WriteLine ("***********************************************************");
            Console.Error.WriteLine ("****** TEST LONG FILTER END *******************************");
            Console.Error.WriteLine ("***********************************************************");
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
        public void TestReferenceFieldPropertyFilter()
        {
            Table<TestPerson> ttp = dataContext.CreateTable<TestPerson>();
            ttp.Clear();
            dataContext.SubmitChanges();

            TestPerson tp = new TestPerson{Name = "TestPerson, Mr."};
            TestPerson spouse = new TestPerson{Name = "TheSpouse"};
            tp.Spouse = spouse;

            ttp.Add (tp);
            ttp.Add (spouse);

            dataContext.SubmitChanges();
            tp = null;
            spouse = null;
            GC.Collect();

            int c = ttp.Count<TestPerson>((TestPerson p) => p.Spouse.Name == "TheSpouse");
            Assert.IsTrue (c > 0, "Returned false negative.");
            
            c = ttp.Count<TestPerson>((TestPerson p) => p.Spouse.Name == "I hope to God this name does not exist. (I did .Clear(), so it should hold");
            Assert.IsTrue (c == 0, "Returned false positive.");
        }
    }
}
