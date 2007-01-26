using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using GenDB;
using CommonTestObjects;

namespace BODictionaryTests
{
    //[TestFixture]
    public class BODictionaryTranslationTests
    {
        const int ELEMENTS_TO_INSERT = 10;
        const int DICT_SIZE = 10;
         
        DataContext dataContext = DataContext.Instance;

        //[TestFixtureSetUp]
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

        //[TestFixtureTearDown]
        public void TearDown()
        {
            dataContext.SubmitChanges();
        }

        public void BODictOfContainsAllPrimitives()
        {
            
        }

        //[Test]
        public void TestBODictOfInt() 
        {
            Assert.IsTrue(true);
            //Table<BODictionary<int, int>> table = dataContext.CreateTable<BODictionary<int, int>>();
            //table.Clear();
            //dataContext.SubmitChanges();
            
            //for(int i=0; i<ELEMENTS_TO_INSERT; i++)
            //{
            //    BODictionary<int, int> bodict = dataContext.BODictionaryFactory.BODictionaryInt<int,int>();
            //    for(int j=0;j<DICT_SIZE;j++)
            //    {
            //        bodict.Add(i+j,j);
            //    }
            //    table.Add(bodict);
            //}
            //dataContext.SubmitChanges();
            //GC.Collect();
            // collect and assert
        }

        //[Test]
        public void TestBODictOfString()
        {
            //Table<BODictionary<int,string>> table = dataContext.CreateTable<BODictionary<int, string>>();
            //table.Clear();
            //dataContext.SubmitChanges();

            //for(int i=0;i<ELEMENTS_TO_INSERT;i++)
            //{
            //    BODictionary<int, string> bodict = dataContext.BODictionaryFactory.BODictionaryString();
            //    for(int j=0;j<DICT_SIZE;j++)
            //    {
            //        bodict.Add(i+j,"str"+j);
            //    }
            //    table.Add(bodict);
            //}
            //dataContext.SubmitChanges();
            //GC.Collect();

            // collect and assert
        }
    }
}
