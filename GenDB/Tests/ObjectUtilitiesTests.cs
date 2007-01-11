using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using GenDB;

namespace Tests
{
    [TestFixture]
    public class TestObjectUtilities
    {
        DataContext dataContext = DataContext.Instance;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            try
            {
                if (!dataContext.RebuildDatabase)
                {
                    dataContext.RebuildDatabase = true;
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(dataContext.RebuildDatabase);
                Console.Error.WriteLine("Database must be rebuild prior to calling these tests.");
                throw e;
            }
        }

        class EqulityTestPrimitives
        {
            public int i = 0;
        }

        class EqualityTest
        {
            int i = 0;

            public int I
            {
                get { return i; }
                set { i = value; }
            }

            string theString = "this is a string";

            object theObject = null;

            public object TheObject
            {
                get { return theObject; }
                set { theObject = value; }
            }

            public string TheString
            {
                get { return theString; }
                set { theString = value; }
            }
        }

        EqualityTest et1 = null;
        EqualityTest et2 = null;
        EqulityTestPrimitives etp1 = null;
        EqulityTestPrimitives etp2 = null;

        [SetUp]
        public void TestEquality()
        {
            et1 = new EqualityTest();
            et1.TheString = "Knud";
            et2 = new EqualityTest();
            et2.TheString = ""; // Ensure string are compared by cstProperty. 
            et2.TheString += "K"; // Using same literal would make them 
            et2.TheString += "n"; // reference equal due to compiler 
            et2.TheString += "u"; // optimizations.
            et2.TheString += "d";

            etp1 = new EqulityTestPrimitives();
            etp2 = new EqulityTestPrimitives();
        }

        [Test]
        public void EqualitySameReferences()
        {
            Assert.IsTrue(ObjectUtilities.TestFieldEquality(et1, et1));
        }

        [Test]
        public void EqualityIdenticalReferences()
        {
            Assert.IsTrue(ObjectUtilities.TestFieldEquality(et1, et2));
        }

        [Test]
        public void EqualityIdenticalReferences2()
        {
            et1.TheObject = new Object();
            et2.TheObject = et1.TheObject;
            Assert.IsTrue(ObjectUtilities.TestFieldEquality(et1, et2));
        }

        [Test]
        public void EqualitySamePrimitives()
        {
            Assert.IsTrue(ObjectUtilities.TestFieldEquality(etp1, etp1));
        }

        [Test]
        public void EqualityIdenticalPrimitives()
        {
            Assert.IsTrue(ObjectUtilities.TestFieldEquality(etp1, etp2));
        }

        [Test]
        public void CloneEquals()
        {
            EqualityTest clone = (EqualityTest)ObjectUtilities.MakeClone(et1);
            Assert.IsTrue(ObjectUtilities.TestFieldEquality(et1, clone));
        }

        [Test]
        public void TestFalsePositivesString()
        {
            et1.TheString = "Per";
            et2.TheString = "Poul";
            Assert.IsFalse(ObjectUtilities.TestFieldEquality(et1, et2));
        }

        [Test]
        public void TestFalsePositivesInt()
        {
            et1.I = 312;
            et2.I = 231;
            Assert.IsFalse(ObjectUtilities.TestFieldEquality(et1, et2));
        }

        [Test]
        public void TestFalsePositivesObject()
        {
            et1.TheObject = new Object();
            Assert.IsFalse(ObjectUtilities.TestFieldEquality(et1, et2));
            et2.TheObject = new Object();
            Assert.IsFalse(ObjectUtilities.TestFieldEquality(et1, et2));
        }

        [Test]
        public void TestFalsePositivesDifferentTypes()
        {
            Assert.IsFalse(ObjectUtilities.TestFieldEquality(et1, "string"));
        }

        [Test]
        public void TestCompareBOList()
        {
            BOListFactory blf = dataContext.BolistFactory;
            BOList<int> boOrig = blf.BOListInt();
            BOList<int> boSame = blf.BOListInt();
            BOList<int> boOther = blf.BOListInt();

            for (int i = 0; i < 10; i++)
            {
                boOrig.Add(i);
                boSame.Add(i);
                boOther.Add(i * 29);
            }

            Assert.IsTrue(ObjectUtilities.TestFieldEquality(boOrig, boOrig), "BOList compare to self returned false.");
            Assert.IsFalse(ObjectUtilities.TestFieldEquality(boOrig, boSame), "BOList compare to other with same contents returned true.");
            Assert.IsFalse(ObjectUtilities.TestFieldEquality(boOrig, boOther), "BOList compare to completely different BOList returned true.");
            Assert.IsFalse(ObjectUtilities.TestFieldEquality(boOrig, blf.BOListLong()), "BOList compare to something else returned true.");
            Assert.IsFalse(ObjectUtilities.TestFieldEquality(boOrig, null), "BOList compare to null returned true.");
        }

        [Test]
        public void TestCompareBODictionaries()
        {
            BODictionary<int, int> boOrig = new BODictionary<int, int>();
            BODictionary<int, int> boSame = new BODictionary<int, int> ();
            BODictionary<int, int>  boOther = new BODictionary<int, int> ();

            for (int i = 0; i < 10; i++)
            {
                boOrig.Add(i, i * 2);
                boSame.Add(i, i * 2);
                boOther.Add(i * 29, -1);
            }

            Assert.IsTrue(ObjectUtilities.TestFieldEquality(boOrig, boOrig), "BODictionary compare to self returned false.");
            Assert.IsFalse(ObjectUtilities.TestFieldEquality(boOrig, boSame), "BODictionary compare to other with same contents returned true.");
            Assert.IsFalse(ObjectUtilities.TestFieldEquality(boOrig, boOther), "BODictionary compare to completely different BODictionary returned true.");
            Assert.IsFalse(ObjectUtilities.TestFieldEquality(boOrig, new TableTests.TestPerson()), "BODictionary compare to something else returned true.");
            Assert.IsFalse(ObjectUtilities.TestFieldEquality(boOrig, null), "BODictionary compare to null returned true.");
        }

        [Test]
        public void TestHandlesNull()
        {
            Assert.IsTrue(ObjectUtilities.TestFieldEquality(null, null), "null == null should return true");
            Assert.IsFalse(ObjectUtilities.TestFieldEquality("string", null), "Comparing something to null should return false. (null as second param)");
            Assert.IsFalse(ObjectUtilities.TestFieldEquality(null, "string"), "Comparing something to null should return false. (null as first param)");
        }
    }
}
