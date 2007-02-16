using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using System.Query;
using GenDB;
using CommonTestObjects;

namespace BODictionaryTests
{
    [TestFixture]
    public class BODictionaryTranslationTests
    {
        const int ELEMENTS_TO_INSERT = 10;
        const int DICT_SIZE = 10;
        Table<BOList<int>> ti = null;
        Table<BOList<TestPerson>> ttp = null;
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

            ti = dataContext.CreateTable<BOList<int>>();
            ttp = dataContext.CreateTable<BOList<TestPerson>>();

            ti.Clear();
            ttp.Clear();

            dataContext.SubmitChanges();
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            dataContext.SubmitChanges();
        }


        private void InsertTest<K, V>(Func<K, V> mapping, Func<int, K> keyCreator)
        {
            Table<BODictionary<K, V>> table = dataContext.CreateTable<BODictionary<K, V>>();

            table.Clear();
            dataContext.SubmitChanges();

            for (int i = 0; i < ELEMENTS_TO_INSERT; i++)
            {
                BODictionary<K, V> dict = new BODictionary<K, V>();
                for (int j = 0; j < DICT_SIZE; j++)
                {
                    K k = keyCreator(j);
                    dict[k] = mapping(k);
                }
                table.Add(dict);
            }

            dataContext.SubmitChanges();
            GC.Collect();
            dataContext.SubmitChanges();
        }

        private void RetrieveTest<K, V>(Func<K, V> mapping, Func<K, int> keyToInt)
        {
            Table<BODictionary<K, V>> table = dataContext.CreateTable<BODictionary<K, V>>();

            int foundDicts = 0;

            foreach(BODictionary<K, V> dict in table)
            {
                foundDicts++;
                Dictionary<int, bool> foundValues = new Dictionary<int, bool>();
                for (int j = 0; j < DICT_SIZE; j++) { foundValues[j] = false; }

                foreach(KeyValuePair<K, V> kvp in dict)
                {
                    V constructedValue= mapping(kvp.Key);
                    Assert.AreEqual (constructedValue, kvp.Value, "Wrong value for key");
                    foundValues[keyToInt(kvp.Key)] = true;
                }

                for (int j = 0; j < DICT_SIZE; j++) { Assert.IsTrue(foundValues[j], "Missing key corresponding to integer shown"); }
                Assert.AreEqual(ELEMENTS_TO_INSERT, foundValues.Count, "Some wrong elemens was returned.");
            }

            Assert.AreEqual(ELEMENTS_TO_INSERT, foundDicts, "Wrong number of dictionaries returned");
        }

        [Test]
        public void TestBODictOfIntInt() 
        {
            InsertTest<int, int>(i => i, i=> i);
        }

        [Test]
        public void TestBODictOfIntIntRetrieve() 
        {
            RetrieveTest<int, int>(i => i, i => i);
        }

        [Test]
        public void TestBODictOfIntString()
        {
            InsertTest<int, string>(i => i.ToString(), i => i);
        }

        [Test]
        public void TestBODictOfIntStringRetrieve()
        {
            RetrieveTest<int, string>(i => i.ToString(), i => i);
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
                    bodict.Add(j, new TestPerson { Name = j.ToString() });
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

        [Test]
        public void TestBODictOfTestPersonInt()
        {
            this.InsertTest<TestPerson, int>((TestPerson t) => t.Age, (int i) => new TestPerson{Age = i});
        }

        [Test]
        public void TestBODictOfTestPersonIntRetrieve()
        {
            Func<TestPerson, int> m = (TestPerson t) => t.Age;
            this.RetrieveTest<TestPerson, int>(m, m);
        }

        [Test]
        public void ZTestIfBOListsAreCreated()
        {
            Assert.AreEqual(0, ti.Count, "BOList<int> created during dictionary tests");
            Assert.AreEqual(0, ttp.Count, "BOList<TestPerson> created during dictionary tests");
        }

        [Test]
        public void TestBODictSimpleQuery()
        {   
            BODictionary<int, TestPerson> bodict = new BODictionary<int, TestPerson>();
            for(int i=0;i<ELEMENTS_TO_INSERT;i++)
            {
                TestPerson tp = new TestPerson{Age = i, Name = "Name"+i};
                if(i%2==0)
                {
                    tp.Spouse = new TestPerson{Age = i*2, Name = "Spouse"+i};
                }
                bodict[i]  = tp;
            }

            var v = from b in bodict
                    where b.Value.Name == "Name2"
                    select b;

            Assert.AreEqual(1,v.Count(),"bodict should only contain 1");

            KeyValuePair<int, TestPerson> k = v.ElementAt(0);
            Assert.AreEqual(2,k.Value.Age,"TestPerson Age should equal 2");

            v = from b in bodict
                where b.Value.Spouse != null && b.Value.Spouse.Age == b.Value.Age*2
                select b;

            Assert.AreEqual(ELEMENTS_TO_INSERT/2,v.Count(),"There should be "+(ELEMENTS_TO_INSERT/2)
                +" TestPersons with twice as old Spouse");
        }
    }
}