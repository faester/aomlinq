using System;
using System.Collections.Generic;
using System.Text;
using GenDB;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class GenDBTranslationTests
    {
        Table<ContainsAllPrimitiveTypes> tableAllPrimitives = null; 

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            Configuration.RebuildDatabase = true;
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {

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
            tableAllPrimitives = new Table<ContainsAllPrimitiveTypes>();
            for (int i = 0; i < 100; i++)
            {
                // It is important not to keep references to the 
                // table elements, since they should be garbage 
                // collected, to ensure later database retrieval 
                // without regards to the cached copies.
                tableAllPrimitives.Add (new ContainsAllPrimitiveTypes());
            }
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
            Assert.IsTrue (count > 0, "No elements returned from database.");
        }
    }
}
