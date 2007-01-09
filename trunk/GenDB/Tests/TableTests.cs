using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using GenDB;
using System.Query;
using System.Expressions;

namespace TableTests
{
    [TestFixture]
    public class TableTests
    {
        Table<TestPerson> tpt = null;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            try {
                if(!Configuration.RebuildDatabase)
                {
                    Configuration.RebuildDatabase = true;
                }
            }
            catch(Exception e)
            {
                Console.Error.WriteLine (Configuration.RebuildDatabase);
                Console.Error.WriteLine("Database must be rebuild prior to calling these tests.");
                throw e;
            }
        }

        [SetUp]
        public void SetUp()
        {
            tpt = new Table<TestPerson>();

            tpt.Clear();
            Configuration.SubmitChanges();

            tpt.Add (new TestPerson {Name = "Per"});
            tpt.Add (new TestPerson {Name = "Per"});
            tpt.Add (new TestPerson {Name = "Per"});
            tpt.Add (new TestPerson {Name = "Per"});
            tpt.Add (new TestPerson {Name = "Poul"});
            tpt.Add (new TestPerson {Name = "Konrad"});
            tpt.Add (new TestPerson {Name = "Jørgen"});
            tpt.Add (new TestPerson {Name = "Svend"});

            Configuration.SubmitChanges();
        }

        [Test]
        public void TestClearOnUnknownType()
        {
            Table<NeverStoreThisClassToDB> tNeverStore = new Table<NeverStoreThisClassToDB>();
            tNeverStore.Clear();
            Table<ContainsAllPrimitiveTypes> tapt = new Table<ContainsAllPrimitiveTypes>();
            tapt.Clear();
        }

        [Test]
        public void TestContains()
        {
            Table<TestPerson> tpt = new Table<TestPerson>();
            TestPerson p1 = new TestPerson();
            TestPerson p2 = new TestPerson();
            TestPerson p3 = new TestPerson();

            tpt.Add (p1);
            tpt.Add (p2);

            Configuration.SubmitChanges();

            Assert.IsTrue (tpt.Contains(p1), "Wrong result. False negative");
            Assert.IsFalse (tpt.Contains(p3), "Wrong result. False positive");
            Assert.IsFalse (tpt.Contains (null), "Wrong result. ");
        }

        [Test, ExpectedException(typeof(NullReferenceException))]
        public void TestInsertNull()
        {
            Table<TestPerson> tpt = new Table<TestPerson>();
            tpt.Add(null);
        }

        [Test]
        public void TestCount()
        {
            int c = tpt.Count<TestPerson>((TestPerson p) => p.Name == "Per");
            Assert.AreEqual (4, c, "Error in filtered result.");

            c = tpt.Count<TestPerson>((TestPerson p) => p.Name == "Poul");
            Assert.AreEqual (1, c, "Error in filtered result.");

            c = tpt.Count<TestPerson>((TestPerson p) => p.Name == "I do not exist");
            Assert.AreEqual (0, c, "Error in filtered result.");

            c = tpt.Count;
            Assert.AreEqual (8, c, "Error in unfiltered result.");
        }

        [Test]
        public void TestRemove()
        {


        }

    }
}
