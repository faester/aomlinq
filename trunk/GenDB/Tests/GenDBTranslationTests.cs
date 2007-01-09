#if DEBUG
using System;
using System.Collections.Generic;
using System.Text;
using GenDB;
using NUnit.Framework;
using System.Query;

namespace Tests
{
    [TestFixture]
    public class GenDBTranslationTests
    {
        private const int ELEMENTS_TO_STORE = 100;
        Table<ContainsAllPrimitiveTypes> tableAllPrimitives = null; 

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            //Assert.IsTrue(Configuration.RebuildDatabase, "Database must be rebuild for these tests to be accurate.");
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
            Configuration.SubmitChanges();
            // Try to empty the cache, or at least ensure, 
            // that something is actually retrieved later on.
            System.GC.Collect();
        }

        private void InitTableAllPrimitives()
        {
            Configuration.SubmitChanges();
            GC.Collect();
            tableAllPrimitives = new Table<ContainsAllPrimitiveTypes>();
            tableAllPrimitives.Clear();

            for (int i = 0; i < ELEMENTS_TO_STORE; i++)
            {
                // It is important not to keep references to the 
                // table elements, since they should be garbage 
                // collected, to ensure later database retrieval 
                // without regards to the cached copies.
                ContainsAllPrimitiveTypes capt = new ContainsAllPrimitiveTypes();
                capt.Boo = (i % 2) == 0;
                capt.Lng = i % 2;
                capt.Integer = i % 2;
                capt.Str = i % 2 == 0 ? "1" : "2";
                capt.Ch = i % 2 == 0 ? '1' : '2';
                capt.Dt = i % 2 == 0 ? new DateTime(0) : new DateTime(1);
                capt.Fl = i % 2;
                capt.Dbl = i % 2;
                tableAllPrimitives.Add (capt);
            }
        }

        [TearDown]
        public void TearDown()
        {
            Table<ContainsAllPrimitiveTypes> t_capt = new Table<ContainsAllPrimitiveTypes>();
            Configuration.SubmitChanges();
            System.GC.Collect();
            Configuration.SubmitChanges();
            t_capt.Clear();
            Configuration.SubmitChanges();
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
            Assert.AreEqual(count, ELEMENTS_TO_STORE, "Returned unexpected number of results");
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

            Assert.AreEqual (count, ELEMENTS_TO_STORE / 2, "Incorrect number of elements returned.");
        }

        [Test]
        public void TestIntFilter()
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

            Assert.AreEqual (count, ELEMENTS_TO_STORE / 2, "Incorrect number of elements returned.");
        }
    }
}
#endif 