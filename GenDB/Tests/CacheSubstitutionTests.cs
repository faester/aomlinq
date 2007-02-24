using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using GenDB;
using CommonTestObjects;

namespace Tests
{
    [TestFixture]
    public class CacheSubstitutionTests
    {
        const int NUM_ELEMENTS = 10;
        Table<TestPerson> personsTable = null;
        TestPerson[] personsArray = new TestPerson[NUM_ELEMENTS];

        [SetUp]
        public void Setup()
        {
            personsTable = DataContext.Instance.GetTable<TestPerson>(TransactionLevel.CacheChecking);
            personsTable.Clear();
            AddPersons();
        }

        private void AddPersons()
        {
            for (int i = 0; i < NUM_ELEMENTS; i++)
            {
                personsArray[i] = new TestPerson{Age = i, Name = i.ToString()};
                personsTable.Add (personsArray[i]);
            }
            DataContext.Instance.SubmitChanges();
        }

        [Test]
        public void TestFindsAppDataSatisfy()
        {
            for (int i  = 0; i< NUM_ELEMENTS; i++)
            {
                personsArray[i].Age = -1;
            }

            var k = from p in personsTable
                    where p.Age < 0
                    select p;

            int found = 0;

            foreach(TestPerson p in k)
            {
                found++;
            }

            Assert.AreEqual (NUM_ELEMENTS, found, "Wrong number of elements returned from query");
        }
    }
}
