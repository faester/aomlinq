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
            ttp.Add (new TestPerson{Name =" Mærkeligtnavn "});
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
                    where persons.Name.Trim() == "Mærkeligtnavn"
                    select persons;
            Assert.AreEqual(1, p1.Count, "not the correct number of persons (Trim())");

            var p2 = from persons in ttp
                    where persons.Name.TrimStart(' ') == "Mærkeligtnavn "
                    select persons;
            Assert.AreEqual(1, p2.Count, "not the correct number of persons (TrimStart())");

            var p3 = from persons in ttp
                    where persons.Name.TrimEnd(' ') == " Mærkeligtnavn"
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
        public void TestMultipleConditionsOnOneProperty()
        {
            Func<TestPerson, bool> condition = (TestPerson pers) => pers.Name == "Name1" || pers.Name == "Name2";

            var p = from persons in ttp
                       where condition(persons)
                        select persons;

            Assert.AreEqual(2, p.Count, "Incorrect number of persons returned.");
            foreach(var q in p)
            {
                Assert.IsTrue(condition(q), "Wrong person returned. Had name '" + q.Name + "'");
            }
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

        [Test]
        public void TestMultiply()
        {
            Table<ContainsAllPrimitiveTypes> p = from ta in tableAllPrimitives
                    where ta.Integer * ta.Dbl == 0.2
                    select ta;

            foreach(ContainsAllPrimitiveTypes capt in p)
            {
                Assert.AreEqual (0.2, capt.Integer * capt.Dbl, "Error in result.");
            }

            Assert.IsTrue ( p.ExprFullySqlTranslatable , "Expression should be fully SQL-translatable");
        }

        [Test]
        public void TestPlus()
        {
            Table<ContainsAllPrimitiveTypes> p = from ta in tableAllPrimitives
                    where ta.Integer + ta.Dbl == 0.2
                    select ta;

            foreach(ContainsAllPrimitiveTypes capt in p)
            {
                Assert.AreEqual (0.2, capt.Integer + capt.Dbl, "Error in result.");
            }

            Assert.IsTrue ( p.ExprFullySqlTranslatable , "Expression should be fully SQL-translatable");
        }

        [Test]
        public void TestPlusStrings()
        {
            Table<TestPerson> p = from ta in ttp
                    where ta.Name + ta.Name == "Name1Name1"
                    select ta;

            foreach(TestPerson pers in p)
            {
                Assert.AreEqual (pers.Name, "Name1Name1", "Error in result.");
            }

            Assert.IsTrue ( p.ExprFullySqlTranslatable , "Expression should be fully SQL-translatable");
        }


        [Test]
        public void TestDiv()
        {
            Table<ContainsAllPrimitiveTypes> p = from ta in tableAllPrimitives
                    where ta.Integer / ta.Dbl == 0.2
                    select ta;

            foreach(ContainsAllPrimitiveTypes capt in p)
            {
                Assert.AreEqual (0.2, capt.Integer / capt.Dbl, "Error in result.");
            }

            Assert.IsTrue ( p.ExprFullySqlTranslatable , "Expression should be fully SQL-translatable");
        }

        [Test]
        public void TestMinus()
        {
            Table<ContainsAllPrimitiveTypes> p = from ta in tableAllPrimitives
                    where ta.Integer - ta.Dbl == 0.2
                    select ta;

            foreach(ContainsAllPrimitiveTypes capt in p)
            {
                Assert.AreEqual (0.2, capt.Integer - capt.Dbl, "Error in result.");
            }

            Assert.IsTrue ( p.ExprFullySqlTranslatable , "Expression should be fully SQL-translatable");
        }

        [Test]
        public void TestAndOperator()
        {
            /* Burde kunne oversættes alene med BoolAnd(ta.Boo, BoolNot(ta.Boo)) */
            Table<ContainsAllPrimitiveTypes> p = from ta in tableAllPrimitives
                    where ta.Boo
                    select ta;

            foreach(ContainsAllPrimitiveTypes capt in p)
            {
                Assert.IsTrue (capt.Boo && capt.Boo, "Error in result.");
            }
            
            Assert.IsTrue ( p.ExprFullySqlTranslatable , "Expression should be fully SQL-translatable");

        }

        [Test]
        public void TestOrOperator()
        {
            /* Burde kunne oversættes alene med BoolOr(ta.Boo, ta.Boo)) */
            Table<ContainsAllPrimitiveTypes> p = from ta in tableAllPrimitives
                    where ta.Boo || ta.Boo
                    select ta;

            foreach(ContainsAllPrimitiveTypes capt in p)
            {
                Assert.IsTrue (capt.Boo || capt.Boo, "Error in result.");
            }

            Assert.IsTrue ( p.ExprFullySqlTranslatable , "Expression should be fully SQL-translatable");
        }


        [Test]
        public void TestOrNotOperator()
        {
            /* Burde kunne oversættes alene med BoolOr(ta.Boo, BoolNot(ta.Boo)) */
            Table<ContainsAllPrimitiveTypes> p = from ta in tableAllPrimitives
                    where ta.Boo || !ta.Boo
                    select ta;

            foreach(ContainsAllPrimitiveTypes capt in p)
            {
                Assert.IsTrue (capt.Boo || !capt.Boo, "Error in result.");
            }

            Assert.IsTrue ( p.ExprFullySqlTranslatable , "Expression should be fully SQL-translatable");
        }

    }
}
