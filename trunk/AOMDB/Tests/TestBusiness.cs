using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Business;

namespace Tests
{
    [TestFixture]
    public class TestBusinessCollections
    {
        LinkedList<IBusinessObject> testobjects;
        IBOCollection collection = null;

        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            testobjects = new LinkedList<IBusinessObject>();
            testobjects.AddLast(new SimpleBusinessObject());
            testobjects.AddLast(new SimpleBusinessObject());
            testobjects.AddLast(new SimpleBusinessObject());
            testobjects.AddLast(new SimpleBusinessObject());
            testobjects.AddLast(new SimpleBusinessObject());
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            testobjects = null;
        }

        [SetUp]
        public void SetUp()
        {
            /* empty */
        }

        [TearDown]
        public void TearDown()
        {
            collection = null;
        }

        private void StoreValues()
        {
            foreach (IBusinessObject t in testobjects)
            {
                collection.Add(t);
            }
        }

        /// <summary>
        /// Tests that all testobjects are present in 
        /// collection. collection must have values 
        /// assigned prior to calling this method.
        /// </summary>
        /// <returns></returns>
        private bool AllObjectsPresent()
        {
            foreach (IBusinessObject t in testobjects)
            {
                if (!collection.Contains(t))
                {
                    return false;
                }
            }
            return true;
        }

        private void CollectionSetList()
        {
            collection = new BOList();
        }

        private void CollectionSetSet()
        {
            collection = new BOSet();
        }

        [Test]
        public void BOListCanAdd()
        {
            CollectionSetList();
            StoreValues();
        }

        [Test]
        public void BOSetCanAdd()
        {
            CollectionSetSet();
            StoreValues();
        }

        [Test]
        public void BOListAddsCorrectly()
        {
            CollectionSetList();
            StoreValues();
            Assert.IsTrue(AllObjectsPresent());
        }

        [Test]
        public void BOSetAddsCorrectly()
        {
            CollectionSetSet();
            StoreValues();
            Assert.IsTrue(AllObjectsPresent());
        }

        private void TestClear()
        {
            StoreValues();
            collection.Clear();
            foreach (IBOCollection c in collection)
            {
                Assert.Fail("Collection of type '" + collection.GetType().FullName + "' contained objects after Clear() was called");
            }
            Assert.IsTrue(collection.Count == 0, "Storage count was not 0 after clear");
        }

        [Test]
        public void BOSetClear()
        {
            CollectionSetSet();
            TestClear();
        }
        [Test]
        public void BOListClear()
        {
            CollectionSetList();
            TestClear();
        }

        [Test]
        public void BOListCount()
        {
            CollectionSetList();
            StoreValues();
            Assert.IsTrue(collection.Count == testobjects.Count, "Count returned incorrect value");
        }

        private void TestEnumerator()
        {
            StoreValues();
            int idx = 0;
            foreach (IBusinessObject bo in testobjects)
            {
                idx++;
                Assert.IsTrue(testobjects.Contains(bo), "Found element not present in test objects");
            }

            Assert.IsTrue(idx == testobjects.Count, "Enumerator returned wrong number of elements.");

        }

        [Test]
        public void BOSetEnumerator()
        {
            CollectionSetSet();
            TestEnumerator();
        }

        [Test]
        public void BOListEnumerator()
        {
            CollectionSetList();
            TestEnumerator();
        }

        private void TestIterator()
        {
            collection.Reset();
            int idx = 0;
            while (collection.MoveNext ()) 
            {
                idx++;
            }
            Assert.AreEqual(testobjects.Count, idx, "IEnumerable did return wrong element count for " + collection.GetType().FullName);
        }

        [Test]
        public void BOListIterator()
        {
            CollectionSetList();
            StoreValues();
            TestIterator();
        }

        [Test]
        public void BOSetIterator()
        {
            CollectionSetSet();
            StoreValues();
            TestIterator();
        }

        private void TestContains()
        {
            StoreValues();
            Assert.IsTrue (AllObjectsPresent());
            SimpleBusinessObject testObj = new SimpleBusinessObject();
            testObj.Serial = 2312398;
            Assert.IsFalse(collection.Contains(testObj), "Contains returned true for non-contained element (Collection type was " + collection.GetType().FullName + ")");
            collection.Add (testObj);
            Assert.IsTrue(collection.Contains(testObj), "Contains returned false for contained element (Collection type was " + collection.GetType().FullName + ")");
        }

        [Test]
        public void BOListContains()
        {
            CollectionSetList();
            TestContains();
        }

        [Test]
        public void BOSetContains()
        {
            CollectionSetSet();
            TestContains();
        }
    }
}
