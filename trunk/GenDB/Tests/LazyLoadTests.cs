using System;
using System.Collections.Generic;
using System.Text;
using GenDB;
using NUnit.Framework;
using CommonTestObjects;

namespace LazyLoadTests
{
    [TestFixture]
    public class LazyLoadTests
    {
        class HasLazyFields : AbstractBusinessObject
        {
            LazyLoader<TestPerson> personLoader = new LazyLoader<TestPerson>();

            string personName;

            public string PersonName
            {
                get { return personName; }
                set { personName = value; }
            }

            [LazyLoad(Storage = "personLoader")]
            public TestPerson Person
            {
                get { return personLoader.Element;}
                set { personLoader.Element = value; }
            }
        }

        Table<HasLazyFields> thlf = null;
        Table<TestPerson> ttp = null;
        DataContext dataContext = null;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            dataContext = DataContext.Instance;
            if (!dataContext.IsInitialized)
            {
                dataContext.Init();
            }
            thlf = dataContext.GetTable <HasLazyFields>();
            ttp = dataContext.GetTable<TestPerson>();
            thlf.Clear();
            ttp.Clear();
            DCCommit();
        }

        private void DCCommit()
        {
            try
            {
                dataContext.SubmitChanges();
            }
            catch (Exception e)
            {
                dataContext.RollbackTransaction();
                throw e;
            }
        }

        [SetUp]
        public void Setup()
        {
            for (int i = 0; i < 10; i++)
            {
                HasLazyFields hlf = new HasLazyFields();
                TestPerson tp = new TestPerson{Name = i.ToString()};
                hlf.Person = tp;
                hlf.PersonName = tp.Name;
                thlf.Add(hlf);
            }
            DCCommit();
        }

        [TearDown]
        public void TearDown()
        {
            thlf.Clear();
            ttp.Clear();
            DCCommit();
        }

        [Test]
        public void EmptyTest() { }

        [Test]
        public void Consistence()
        {
            bool foundSome = false;
            foreach(HasLazyFields h in thlf)
            {
                foundSome = true;
                Assert.IsNotNull(h, "HasLazyFields is null");
                Assert.IsNotNull(h.Person, "h.Person is null");
                Assert.IsNotNull(h.PersonName, "h.PersonName is null");
                Assert.IsNotNull(h.Person.Name, "h.Person.Name is null");
                Assert.AreEqual(h.Person.Name, h.PersonName, "Mismatch...");
            }
            Assert.IsTrue(foundSome, "Did not find any persons in table.");
        }
    }
}
