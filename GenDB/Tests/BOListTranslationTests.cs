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
            Table<BOList<ContainsAllPrimitiveTypes>> table = dataContext.GetTable<BOList<ContainsAllPrimitiveTypes>>();
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
            Table<BOList<int>> table = dataContext.GetTable<BOList<int>>();
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
            Table<BOList<string>> table = dataContext.GetTable<BOList<string>>();
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
            Table<BOList<DateTime>> table = dataContext.GetTable<BOList<DateTime>>();
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



        [Test]
        public void TestBOListSimpleQuery()
        {
            // ** not used
            Table<BOList<TestPerson>> table = dataContext.GetTable<BOList<TestPerson>>();
            table.Clear();
            dataContext.SubmitChanges();
            // **

            BOList<TestPerson> bolist = new BOList<TestPerson>();

            for(int i=0;i<ELEMENTS_TO_INSERT;i++)
            {
                TestPerson tp = new TestPerson{Age = i, Name = "Name"+i};
                if(i%2==0)
                {
                    tp.Spouse = new TestPerson{Age = i*2, Name = "Spouse"+i};
                }

                bolist.Add(tp);
            }

            var v = from b in bolist
                    where b.Age == 3
                    select b;

            Assert.AreEqual(1,v.Count(),"bolist should contain only 1 TestPerson");


            v = from b in bolist
                where b.Spouse != null && b.Spouse.Age == b.Age*2
                select b;
            
            Assert.AreEqual(ELEMENTS_TO_INSERT/2,v.Count(),"There should be "+(ELEMENTS_TO_INSERT/2)
                +" TestPersons with twice as old Spouse");

        }
    }
}