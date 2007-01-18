using System;
using System.Collections.Generic;
using System.Text;
using GenDB;
using NUnit.Framework;
using CommonTestObjects;

namespace IBOCache
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
        int testsRun = 0;
        int storeRecursiveDataType1 = 0;
        int storeRecursiveDataType2 = 0;
        int areChangesCommitted = 0;
        int areChangesPersisted = 0;
        int cacheEmptied = 0;
        int cacheEmptied2 = 0;

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

        [SetUp]
        public void Init()
        {
            testsRun++;
        }

        private void Check(int testCount)
        {
            if (testCount < (testsRun - 1))
            {
                Assert.Ignore("Tests here must run sequentially");
            }
        }

        [Test]
        public void Test1AreChangesComitted()
        {
            areChangesCommitted = testsRun;
            dt.SubmitChanges();
            foreach (CacheTestObject cto in keepInstances)
            {
                cto.Name = "NAMECHANGE";
            }
            keepInstances.Clear();
            dt.SubmitChanges();
        }

        [Test]
        public void Test2AreChangesPersisted()
        {
            Check(areChangesCommitted);
            areChangesPersisted = testsRun;
            foreach(CacheTestObject cto in table)
            {
                Assert.AreEqual("NAMECHANGE", cto.Name, "Name changes was not correctly committed");
            }
            Assert.AreEqual(instancesToCreate, table.Count);
            table.Clear();
        }

        [Test]
        public void Test3RecursiveDataType3()
        {
            Check(areChangesPersisted);
            storeRecursiveDataType1 = testsRun;
            TestPerson tp = new TestPerson(); 
            tp.Name = "Head element";

            TestPerson lastPerson = tp;

            for(int i = 0; i < 100; i++)
            {
                TestPerson newPerson = new TestPerson();
                newPerson.Name = i.ToString();
                lastPerson.Spouse = newPerson;
                lastPerson = newPerson;
            }
            lastPerson.Spouse = tp;

            Table<TestPerson> ttp = dt.CreateTable<TestPerson>();
            ttp.Add (tp);
            dt.SubmitChanges();
        }

        [Test]
        public void Test4RecursiveDataType2()
        {
            Check(storeRecursiveDataType1);
            storeRecursiveDataType2 = testsRun;   
            Table<TestPerson> ttp = dt.CreateTable<TestPerson>();

            var qs = from tps in ttp
                       where tps.Name == "0"
                       select tps;

            TestPerson head = null;
            bool first = true;

            foreach(TestPerson tp in qs)
            {
                head = tp;
                first = false;
                break;
            }

            Assert.IsFalse(first, "Ingen resultater returneret.");

            int count = 0;
            TestPerson spouse = head.Spouse;

            while(spouse != head)
            {
                count++;
                first = false;
                spouse = spouse.Spouse;
                Assert.IsTrue (count < 1000, "Test terminated.");
            }
            ttp.Clear();
            dt.SubmitChanges();
            Assert.AreEqual(100, count, "Wrong number of elements returned.");
        }

        [Test]
        public void Test5CacheEmptied()
        {
            Check(storeRecursiveDataType2);
            cacheEmptied = testsRun;
            dt.SubmitChanges();
            string msg = "(Failures here might indicate, that some other test is keeping a reference to objects. Check that this is not the case.";
            Assert.AreEqual(0, dt.UnCommittedObjectsSize, "Cache still contained uncommitted objects. " + msg);
        }

        [Test]
        public void Test6CacheEmptied2()
        {
            Check(cacheEmptied);
            table.Clear();
            dt.SubmitChanges();
            CacheTestObject hvp = null;
            for (int i = 0; i < 10; i++)
            {
                hvp = new CacheTestObject();
                table.Add (hvp);
            }
            hvp = null;
            dt.SubmitChanges();

            Assert.AreEqual(0, dt.UnCommittedObjectsSize, "Uncommitted objects still contained values");
        }


        [Test]
        public void TestDeleteThenChangeValue()
        {
            table.Clear();
            dt.SubmitChanges();

            CacheTestObject co = new CacheTestObject ();
            co.Name = "Before delete.";

            table.Add (co);
            dt.SubmitChanges();

            Table<CacheTestObject> filtered = from c in table where c.Name == "Before delete." select c;
            Assert.AreEqual(1, filtered.Count, "Test object was not persisted.");

            table.Clear();
            dt.SubmitChanges();
            Assert.AreEqual(0, table.Count, "Table wasn't emptied correctly.");

            co.Name = "After delete";

            dt.SubmitChanges();
            Assert.AreEqual(0, table.Count, "Object state change after delete caused object to be re-added to db.");
        }

        [Test]
        public void TestChangesPersisted()
        {
            int obs = 500;
            Table<ContainsAllPrimitiveTypes> tcapt = dt.CreateTable<ContainsAllPrimitiveTypes>();
            tcapt.Clear();
            dt.SubmitChanges();

            for (int i = 0; i < obs; i++)
            {
                tcapt.Add (new ContainsAllPrimitiveTypes());
            }
            
            dt.SubmitChanges();
            GC.Collect();

            int idx = 0;
            foreach(ContainsAllPrimitiveTypes c in tcapt)
            {
                c.Integer = idx++;
                c.Str = "Changed";
            }
            
            dt.SubmitChanges();
            GC.Collect();

            bool[] found = new bool[obs];

            foreach(ContainsAllPrimitiveTypes c in tcapt)
            {
                found[c.Integer] = true;
                Assert.AreEqual("Changed", c.Str, "�ndring ikke persisteret!");
            }

            for (int i = 0; i < obs; i++)
            {
                Assert.IsTrue(found[i], "Not all saved values where returned.");
            }
        }
    }


}
