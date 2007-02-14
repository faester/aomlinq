using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using GenDB;
using CommonTestObjects;

namespace BODictionaryTests
{
    [TestFixture]
    public class BODictionaryTranslationTests
    {
        const int ELEMENTS_TO_INSERT = 10;
        const int DICT_SIZE = 10;
         
        DataContext dataContext = DataContext.Instance;

        [TestFixtureSetUp]
        public void InitDB()
        {
            if (!dataContext.IsInitialized)
            {
                if (dataContext.DatabaseExists()) { dataContext.DeleteDatabase(); }
                dataContext.CreateDatabase();
            }
            try
            {
                if (!dataContext.DatabaseExists())
                {
                    dataContext.CreateDatabase();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Configuration.RebuildDatabase should be set to true prior to running the tests.");
                throw e;
            }
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            dataContext.SubmitChanges();
        }

        public void BODictOfContainsAllPrimitives()
        {
            
        }

        [Test]
        public void TestBODictOfInt() 
        {
            Assert.IsTrue(true);
            Table<BODictionary<int, int>> table = dataContext.CreateTable<BODictionary<int, int>>();
            table.Clear();
            dataContext.SubmitChanges();
            
            for(int i=0; i<ELEMENTS_TO_INSERT; i++)
            {
                BODictionary<int, int> bodict = new BODictionary<int,int>();
                for(int j=0;j<DICT_SIZE;j++)
                {
                    bodict.Add(i+j,j);
                }
                table.Add(bodict);
            }
            dataContext.SubmitChanges();
            GC.Collect();
        }

        [Test]
        public void TestBODictOfString()
        {
            Table<BODictionary<int, string>> table = dataContext.CreateTable<BODictionary<int, string>>();
            table.Clear();
            dataContext.SubmitChanges();

            for(int i=0;i<ELEMENTS_TO_INSERT;i++)
            {
                BODictionary<int, string> bodict = new BODictionary<int, string>();
                for(int j=0;j<DICT_SIZE;j++)
                {
                    bodict.Add(i+j,"str"+j);
                }
                table.Add(bodict);
            }
            dataContext.SubmitChanges();
            GC.Collect();
        }

        [Test]
        public void TestBODictOfStringRetrieve()
        {
            Table<BODictionary<int, string>> table = dataContext.CreateTable<BODictionary<int, string>>();

            int dictsFound = 0;

            foreach(BODictionary<int, string> dict in table)
            {
                dictsFound++;
                Dictionary<int, bool> foundValues = new Dictionary<int, bool>();
                for (int j = 0; j < DICT_SIZE; j++) { foundValues[j] = false; } // Values that should be found.

                foreach(KeyValuePair<int, string> kvp in dict)
                {
                    int k = kvp.Key;
                    string v = kvp.Value;
                    foundValues[k] = true;
                    Assert.AreEqual("str"+k, v, "Wrong key/value-correspondence");
                }

                for (int j = 0; j < DICT_SIZE; j++) { Assert.IsTrue(dict.ContainsKey(j), "Missing element: " + j); }

                Assert.AreEqual(DICT_SIZE, foundValues.Count, "Wrong keys found");
            }

            Assert.AreEqual(ELEMENTS_TO_INSERT, dictsFound, "Wrong number of dictionaries in table.");
        }

        [Test]
        public void TestBODictOfIntTestPerson()
        {
            Table<BODictionary<int, TestPerson>> table = dataContext.CreateTable<BODictionary<int, TestPerson>>();
            table.Clear();
            dataContext.SubmitChanges();

            for(int i=0;i<ELEMENTS_TO_INSERT;i++)
            {
                BODictionary<int, TestPerson> bodict = new BODictionary<int, TestPerson>();
                for(int j=0;j<DICT_SIZE;j++)
                {
                    bodict.Add(i + j, new TestPerson { Name = j.ToString() });
                }
                table.Add(bodict);
            }
            dataContext.SubmitChanges();
            GC.Collect();
        }

        [Test]
        public void TestBODictOfIntTestPersonRetrieve()
        {
            Table<BODictionary<int, TestPerson>> table = dataContext.CreateTable<BODictionary<int, TestPerson>>();

            int foundDicts = 0;

            foreach(BODictionary<int, TestPerson> dict in table)
            {
                foundDicts++;
                Dictionary <int, bool> foundValues = new Dictionary<int, bool>();
                for (int j = 0; j < DICT_SIZE; j++) { foundValues[j] = false; }

                foreach(KeyValuePair<int, TestPerson> kvp in dict)
                {
                    int k = kvp.Key;
                    TestPerson v = kvp.Value;
                    foundValues[k] = true;
                    Assert.AreEqual(k.ToString(), v.Name, "Wrong key/value-mapping");
                }

                for (int j = 0; j < DICT_SIZE; j++) { Assert.IsTrue(foundValues[j], "Did not find key " + j); }

                Assert.AreEqual(DICT_SIZE, foundValues.Count, "Wrong keys in returned dictionary");
            }

            Assert.AreEqual (ELEMENTS_TO_INSERT, foundDicts, "Wrong number of dictionaries found in table.");
        }
    }
}
