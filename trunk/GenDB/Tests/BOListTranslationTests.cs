using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using GenDB;
using TableTests;

namespace BOListTests
{
    [TestFixture]
    public class BOListTranslationTests
    {
        const int ELEMENTS_TO_INSERT = 3;
        const int LIST_LENGTH = 4;

        [TestFixtureSetUp]
        public void InitDB()
        {
            try {
            if (!Configuration.RebuildDatabase)
            {
                Configuration.RebuildDatabase = true;
            }
            }
            catch(Exception e)
            {
                Console.WriteLine("Configuration.RebuildDatabase should be set to true prior to running the tests.");
                throw e;
            }
        }

        [TestFixtureTearDown]
        public void TearDown() 
        { 
            /* empty */ 
        }

        [Test]
        public void TestBOListOfContainsAllPrimitives()
        {
            Table<BOList<ContainsAllPrimitiveTypes>> table = new Table<BOList<ContainsAllPrimitiveTypes>>();
            table.Clear();
            Configuration.SubmitChanges();

            for (int i = 0; i < ELEMENTS_TO_INSERT; i++)
            {
                BOList<ContainsAllPrimitiveTypes> bolist = BOListFactory.BOListRef<ContainsAllPrimitiveTypes>();
                for (int j = 0; j < LIST_LENGTH; j++)
                {
                    bolist.Add(new ContainsAllPrimitiveTypes { Integer = j });
                }
                table.Add (bolist);
            }
            Configuration.SubmitChanges();
            GC.Collect();

            int listCount = 0;
            foreach(BOList<ContainsAllPrimitiveTypes> blca in table)
            {
                listCount++;
                for (int i = 0; i < LIST_LENGTH; i++)
                {
                    Assert.AreEqual(i, blca[i].Integer , "Element number " + i + " had wrong Integer value.");
                }
            }
            Assert.AreEqual(ELEMENTS_TO_INSERT, listCount, "Wrong number of lists returned.");
        }

        [Test]
        public void TestBOListOfInt()
        {
            Table<BOList<int>> table = new Table<BOList<int>>();
            table.Clear();
            Configuration.SubmitChanges();

            for (int i = 0; i < ELEMENTS_TO_INSERT; i++)
            {
                BOList<int> bolist = BOListFactory.BOListInt();
                for (int j = 0; j < LIST_LENGTH; j++)
                {
                    bolist.Add(j);
                }
                table.Add (bolist);
            }
            Configuration.SubmitChanges();
            GC.Collect();

            int listCount = 0;
            foreach(BOList<int> bolist in table)
            {
                listCount++;
                for (int i = 0; i < LIST_LENGTH; i++)
                {
                    Assert.AreEqual(i, bolist[i], "Element number " + i + " had wrong Integer value.");
                }
            }
            Assert.AreEqual(ELEMENTS_TO_INSERT, listCount, "Wrong number of lists returned.");
        }

        [Test]
        public void TestBOListOfString()
        {
            Table<BOList<string>> table = new Table<BOList<string>>();
            table.Clear();
            Configuration.SubmitChanges();

            for (int i = 0; i < ELEMENTS_TO_INSERT; i++)
            {
                BOList<string> bolist = BOListFactory.BOListString();
                for (int j = 0; j < LIST_LENGTH; j++)
                {
                    bolist.Add(j.ToString());
                }
                table.Add (bolist);
            }
            Configuration.SubmitChanges();
            GC.Collect();

            int listCount = 0;
            foreach(BOList<string> bolist in table)
            {
                listCount++;
                for (int i = 0; i < LIST_LENGTH; i++)
                {
                    string s = bolist[i];
                    Assert.AreEqual(i.ToString(), s, "Wrong value returned.");
                }
            }
            Assert.AreEqual(ELEMENTS_TO_INSERT, listCount, "Wrong number of lists returned.");
        }

    }
}
