using System;
using System.Collections.Generic;
using System.Text;
using GenDB;
using NUnit.Framework;
using System.Query;
using System.Expressions;
using CommonTestObjects;

namespace QueryToSqlTranslationTests
{
    [TestFixture]
    public class TableQueryTests3
    {
        private const int ELEMENTS_TO_STORE = 40;
        Table<ContainsAllPrimitiveTypes> tableAllPrimitives = null;
        DataContext dataContext = DataContext.Instance;
        Table<TestPerson> ttp ;

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            tableAllPrimitives = null;
            ttp = null;
        }

        [SetUp]
        public void TestSetup()
        {
            InitTableAllPrimitives();
            InitTableOfPersons();
            dataContext.SubmitChanges();
        }


        public void InitTableOfPersons()
        {
            ttp = dataContext.CreateTable<TestPerson>();  
            ttp.Clear();

            dataContext.SubmitChanges();
            
            Assert.AreEqual(0, ttp.Count, "Table wasn't cleared properly.");

            TestPerson lastPerson = null;

            for (int i = 0; i < 10; i++)
            {
                TestPerson tp = new TestPerson();
                tp.Name = "Name" + i.ToString();
                tp.Age = i;
                tp.Spouse = lastPerson;
                lastPerson = tp;
                ttp.Add(tp);
            }
            ttp.Add (new TestPerson{Name =" M�rkeligtnavn "});
        }

        private void InitTableAllPrimitives()
        {
            dataContext.SubmitChanges();
            GC.Collect();
            tableAllPrimitives = dataContext.CreateTable<ContainsAllPrimitiveTypes>();
            tableAllPrimitives.Clear();
            dataContext.SubmitChanges();

            for (int i = 0; i < ELEMENTS_TO_STORE; i++)
            {
                // It is important not to keep references to the 
                // table elements, since they should be garbage 
                // collected, to ensure later database retrieval 
                // without regards to the cached copies.
                ContainsAllPrimitiveTypes capt = new ContainsAllPrimitiveTypes();
                capt.Boo = (i % 2) == 0; // Has test
                capt.Lng = i % 2; // Has test
                capt.Integer = i % 2; // Has test
                capt.Str = i % 2 == 0 ? "1" : "0"; // Has test
                capt.Ch = i % 2 == 0 ? '1' : '0';  // Has test
                capt.Dt = i % 2 == 0 ? new DateTime(0) : new DateTime(1); // Has test
                capt.Fl = i % 2; // Has test
                capt.Dbl = i % 2; // Has test
                tableAllPrimitives.Add (capt);
            }
            dataContext.SubmitChanges();
        }

        [TearDown]
        public void TearDown()
        {
            tableAllPrimitives.Clear();
            Console.WriteLine(ttp);
            ttp.Clear();
            dataContext.SubmitChanges();
            tableAllPrimitives = null;
            ttp = null;
        }

        [Test]
        public void TestSubstring()
        {
            var p = from persons in ttp
                    where persons.Name.Substring(0,3) == "Nam"
                    select persons;
            Assert.Greater(p.Count, 0, "did not find enough");
        }

        [Test]
        public void TestTrim()
        {
            var p1 = from persons in ttp
                    where persons.Name.Trim() == "M�rkeligtnavn"
                    select persons;
            Assert.AreEqual(1, p1.Count, "not the correct number of persons (Trim())");

            var p2 = from persons in ttp
                    where persons.Name.TrimStart(' ') == "M�rkeligtnavn "
                    select persons;
            Assert.AreEqual(1, p2.Count, "not the correct number of persons (TrimStart())");

            var p3 = from persons in ttp
                    where persons.Name.TrimEnd(' ') == " M�rkeligtnavn"
                    select persons;
            Assert.AreEqual(1, p3.Count, "not the correct number of persons (TrimEnd())");
        }

        [Test]
        public void TestIndexOf()
        {
            var p1 = from persons in ttp
                    where persons.Name.IndexOf("3") == 4
                    select persons;
            Assert.AreEqual(1, p1.Count,"incorrect number of persons returned");

            var p2 = from persons in ttp
                    where persons.Name.LastIndexOf("3") == 4
                    select persons;
            Assert.AreEqual(1, p2.Count,"incorrect number of persons returned");
        }

        [Test]
        public void TestStartsWith()
        {
            var p = from persons in ttp
                    where persons.Name.StartsWith("Name")
                    select persons;
            Assert.AreEqual(10, p.Count, "not enough persons returned");
        }

        [Test]
        public void TestEndsWith()
        {
            var p = from persons in ttp
                    where persons.Name.EndsWith("e3")
                    select persons;
            Assert.AreEqual(1, p.Count, "incorrect number of persons returned");
        }

        [Test]
        public void TestContains()
        {
            var ps = from persons in ttp
                    where persons.Name.Contains("ame")
                    select persons;

            int count = 0;
            foreach (TestPerson p in ps)
            {
                count++;
                Console.WriteLine(p.Name);
            }
            Assert.AreEqual(count, ps.Count, "Count method of tables returned erroneous result.");
            Assert.AreEqual(10, ps.Count, "not enough persons returned");
        }

        [Test]
        public void TestLengthProperty()
        {
            var p = from persons in ttp
                    where persons.Name.Length > 5
                    select persons;

            Assert.AreEqual(1,p.Count,"incorrect number of persons returned");
        }
    }
}
