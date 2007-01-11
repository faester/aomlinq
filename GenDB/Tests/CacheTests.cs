using System;
using System.Collections.Generic;
using System.Text;
using GenDB;
using NUnit.Framework;

namespace CacheTests
{
    [TestFixture]
    public class Tests
    {
        internal class CacheTestObject : AbstractBusinessObject
        {
            static int instances = 0;

            [Volatile]
            public static int Instances
            {
                get { return CacheTestObject.instances; }
            }

            string name = "";

            public string Name
            {
                get { return name; }
                set { name = value; }
            }

            public CacheTestObject() 
            {
                instances++;
            }

            ~CacheTestObject()
            {
                instances--;
            }
        } 

        int instancesToCreate = 100;
        DataContext dt = DataContext.Instance;
        Table<CacheTestObject> table = null;
        LinkedList<CacheTestObject> keepInstances = new LinkedList<CacheTestObject>();

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            table = dt.CreateTable<CacheTestObject>();
            table.Clear();

            for (int i = 0; i < instancesToCreate; i++)
            {
                CacheTestObject cto = new CacheTestObject { Name = "Name " + i.ToString() };
                table.Add (cto);
                keepInstances.AddLast(cto);
            }
        }

        [Test]
        public void Test1IsChangesComitted()
        {
            dt.SubmitChanges();
            foreach (CacheTestObject cto in keepInstances)
            {
                cto.Name = "NAMECHANGE";
            }
            keepInstances.Clear();
            dt.SubmitChanges();
        }

        [Test]
        public void Test2IsChangesPersisted()
        {
            foreach(CacheTestObject cto in table)
            {
                Assert.AreEqual("NAMECHANGE", cto.Name, "Name changes was not correctly committed");
            }
            Assert.AreEqual(instancesToCreate, table.Count);
        }

        [Test]
        public void Test3CacheEmptied()
        {
            dt.SubmitChanges();
            string msg = "(Failures here might indicate, that some other test is keeping a reference to objects. Check that this is not the case.";
            Assert.AreEqual(0, dt.CommittedObjectsSize, "Cache still contained committed objects. " + msg);
            Assert.AreEqual(0, dt.UnCommittedObjectsSize, "Cache still contained uncommitted objects. " + msg);
        }

    }
}
