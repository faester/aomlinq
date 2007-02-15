using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using GenDB;
using CommonTestObjects;
using System.Query;
using System.Expressions;

namespace BOListTests
{
    [TestFixture]
    public class BOListTranslationTests
    {
        const int ELEMENTS_TO_INSERT = 10;
        const int LIST_LENGTH = 10;

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

        [Test]
        public void TestBOListOfContainsAllPrimitives()
        {
            Table<BOList<ContainsAllPrimitiveTypes>> table = dataContext.CreateTable<BOList<ContainsAllPrimitiveTypes>>();
            table.Clear();
            dataContext.SubmitChanges();

            for (int i = 0; i < ELEMENTS_TO_INSERT; i++)
            {
                BOList<ContainsAllPrimitiveTypes> bolist = new BOList<ContainsAllPrimitiveTypes>();
                for (int j = 0; j < LIST_LENGTH; j++)
                {
                    bolist.Add(new ContainsAllPrimitiveTypes { Integer = j });
                }
                table.Add(bolist);
            }
            dataContext.SubmitChanges();
            GC.Collect();

            int listCount = 0;
            foreach (BOList<ContainsAllPrimitiveTypes> blca in table)
            {
                listCount++;
                for (int i = 0; i < LIST_LENGTH; i++)
                {
                    Assert.AreEqual(i, blca[i].Integer, "Element number " + i + " had wrong Integer value.");
                }
            }
            Assert.AreEqual(ELEMENTS_TO_INSERT, listCount, "Wrong number of lists returned.");
        }

        [Test]
        public void TestBOListOfInt()
        {
            Table<BOList<int>> table = dataContext.CreateTable<BOList<int>>();
            table.Clear();
            dataContext.SubmitChanges();

            for (int i = 0; i < ELEMENTS_TO_INSERT; i++)
            {
                BOList<int> bolist = new BOList<int>();
                for (int j = 0; j < LIST_LENGTH; j++)
                {
                    bolist.Add(j);
                }
                table.Add(bolist);
            }
            dataContext.SubmitChanges();
            GC.Collect();

            int listCount = 0;
            foreach (BOList<int> bolist in table)
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
            Table<BOList<string>> table = dataContext.CreateTable<BOList<string>>();
            table.Clear();
            dataContext.SubmitChanges();

            for (int i = 0; i < ELEMENTS_TO_INSERT; i++)
            {
                BOList<string> bolist = new BOList<string>();
                for (int j = 0; j < LIST_LENGTH; j++)
                {
                    bolist.Add(j.ToString());
                }
                table.Add(bolist);
            }
            dataContext.SubmitChanges();
            GC.Collect();

            int listCount = 0;
            foreach (BOList<string> bolist in table)
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

        [Test]
        public void TestBOListOfDateTime()
        {
            Table<BOList<DateTime>> table = dataContext.CreateTable<BOList<DateTime>>();
            table.Clear();
            dataContext.SubmitChanges();

            for (int i = 0; i < ELEMENTS_TO_INSERT; i++)
            {
                BOList<DateTime> bolist = new BOList<DateTime>();
                for (int j = 0; j < LIST_LENGTH; j++)
                {
                    bolist.Add(new DateTime(j));
                }
                table.Add(bolist);
            }
            dataContext.SubmitChanges();
            GC.Collect();

            int listCount = 0;
            foreach (BOList<DateTime> bolist in table)
            {
                listCount++;
                for (int i = 0; i < LIST_LENGTH; i++)
                {
                    DateTime s = bolist[i];
                    Assert.AreEqual(new DateTime(i), s, "Wrong value returned.");
                }
            }
            Assert.AreEqual(ELEMENTS_TO_INSERT, listCount, "Wrong number of lists returned.");
        }
    }
}
