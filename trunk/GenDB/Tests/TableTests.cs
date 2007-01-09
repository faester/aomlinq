using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using GenDB;
using System.Query;
using System.Expressions;

namespace Tests
{
    [TestFixture]
    public class TableTests
    {
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
            Table<Person> tpt = new Table<Person>();
            Person p1 = new Person();
            Person p2 = new Person();
            Person p3 = new Person();

            tpt.Add (p1);
            tpt.Add (p2);

            Configuration.SubmitChanges();

            Assert.IsTrue (tpt.Contains(p1), "Wrong result. False negative");
            Assert.IsFalse (tpt.Contains(p3), "Wrong result. False positive");
        }

        [Test]
        public void TestCount()
        {
            Table<Person> tpt = new Table<Person>();

            tpt.Clear();
            Configuration.SubmitChanges();

            tpt.Add (new Person {Name = "Per"});
            tpt.Add (new Person {Name = "Per"});
            tpt.Add (new Person {Name = "Per"});
            tpt.Add (new Person {Name = "Per"});
            tpt.Add (new Person {Name = "Poul"});
            tpt.Add (new Person {Name = "Konrad"});
            tpt.Add (new Person {Name = "Jørgen"});
            tpt.Add (new Person {Name = "Svend"});

            Configuration.SubmitChanges();

            int c = tpt.Count<Person>((Person p) => p.Name == "Per");
            Assert.AreEqual (4, c, "Error in filtered result.");

            c = tpt.Count<Person>((Person p) => p.Name == "Poul");
            Assert.AreEqual (1, c, "Error in filtered result.");

            c = tpt.Count<Person>((Person p) => p.Name == "I do not exist");
            Assert.AreEqual (0, c, "Error in filtered result.");

            c = tpt.Count;
            Assert.AreEqual (8, c, "Error in unfiltered result.");
        }
    }
}
