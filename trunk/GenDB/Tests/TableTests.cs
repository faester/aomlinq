using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using GenDB;
using System.Query;
using System.Expressions;
using CommonTestObjects;

namespace TableTests
{
    [TestFixture]
    public class TableTests
    {
        Table<TestPerson> tpt = null;
        TestPerson personToRemove = new TestPerson { Name = "I am the one to remove." };
        DataContext dataContext = DataContext.Instance;
        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            try {
                if(!dataContext.RebuildDatabase)
                {
                    dataContext.RebuildDatabase = true;
                }
            }
            catch(Exception e)
            {
                Console.Error.WriteLine (dataContext.RebuildDatabase);
                Console.Error.WriteLine("Database must be rebuild prior to calling these tests.");
                throw e;
            }
        }

        [SetUp]
        public void SetUp()
        {
            tpt = dataContext.CreateTable<TestPerson>();

            tpt.Clear();
            dataContext.SubmitChanges();

            tpt.Add (new TestPerson {Name = "Per", Age = 10});
            tpt.Add (new TestPerson {Name = "Per", Age = 21});
            tpt.Add (new TestPerson {Name = "Per"});
            tpt.Add (new TestPerson {Name = "Per"});
            tpt.Add (new TestPerson {Name = "Poul"});
            tpt.Add (new TestPerson {Name = "Konrad", Age = 30});
            tpt.Add (new TestPerson {Name = "Jørgen", Age = 50});
            tpt.Add (new TestPerson {Name = "Svend"});
            tpt.Add (personToRemove);

            dataContext.SubmitChanges();
        }

        [Test]
        public void TestCanClearOnUnPersistedType()
        {
            Table<NeverStoreThisClassToDB> tNeverStore = dataContext.CreateTable<NeverStoreThisClassToDB>();
            tNeverStore.Clear();
            
            Table<ContainsAllPrimitiveTypes> tapt = dataContext.CreateTable<ContainsAllPrimitiveTypes>();
            tapt.Clear();

            dataContext.SubmitChanges();
        }

        [Test]
        public void TestCanClearOnPersistedType()
        {
            tpt.Clear();
            dataContext.SubmitChanges();

            Assert.AreEqual(0, tpt.Count, "Table is not empty after clear.");
        }

        [Test]
        public void TestContains()
        {
            Table<TestPerson> tpt = dataContext.CreateTable<TestPerson>();
            TestPerson p1 = new TestPerson();
            TestPerson p2 = new TestPerson();
            TestPerson p3 = new TestPerson();

            tpt.Add (p1);
            tpt.Add (p2);

            dataContext.SubmitChanges();

            Assert.IsTrue (tpt.Contains(p1), "Wrong result. False negative");
            Assert.IsFalse (tpt.Contains(p3), "Wrong result. False positive");
            Assert.IsFalse (tpt.Contains (null), "Wrong result. ");
        }

        [Test, ExpectedException(typeof(NullReferenceException))]
        public void TestInsertNull()
        {
            Table<TestPerson> tpt = dataContext.CreateTable<TestPerson>();
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
            Assert.AreEqual (9, c, "Error in unfiltered result.");

            Table<TestPerson> filtered = from person in tpt where person.Name == "Per" select person;
            Assert.AreEqual (4, filtered.Count, "Filtered table returned wrong number of instances.");
        }

        [Test]
        public void TestConditionsUnions()
        {
            Table<TestPerson> pouls = from person in tpt where person.Name == "Per" select person;
            Assert.AreEqual(4, pouls.Count, "Filtered table returned wrong number of instances.");

            Table<TestPerson> oldPouls = from poul in pouls where poul.Age > 0 select poul;
            Assert.AreEqual (2, oldPouls.Count, "Wrong number of elements in result.");
            Console.WriteLine(oldPouls);
        }
        
        [Test]
        public void TestRemove()
        {
            Assert.IsTrue(tpt.Remove(personToRemove), "Database reported, that it didn't remove person");
            Assert.IsFalse(tpt.Remove(new TestPerson{Name = "This person does not exist in db"}), "Table falsely returned, that it did remove unknown person.");

            dataContext.SubmitChanges();

            Assert.IsFalse (tpt.Contains(personToRemove), "Table still contained removed person after remove was comitted.");
        }
    }
}
