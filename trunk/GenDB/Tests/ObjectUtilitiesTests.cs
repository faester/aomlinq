#if DEBUG
using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace GenDB
{
    [TestFixture]
    public class TestObjectUtilities
    {
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
            et2.TheString = ""; // Ensure string are compared by property. 
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
            Assert.IsFalse(ObjectUtilities.TestFieldEquality (et1, et2));
        }

        [Test]
        public void TestFalsePositivesInt()
        {
            et1.I = 312;
            et2.I = 231;
            Assert.IsFalse(ObjectUtilities.TestFieldEquality (et1, et2));
        }

        [Test]
        public void TestFalsePositivesObject()
        {
            et1.TheObject = new Object();
            Assert.IsFalse(ObjectUtilities.TestFieldEquality (et1, et2));
            et2.TheObject = new Object();
            Assert.IsFalse(ObjectUtilities.TestFieldEquality (et1, et2));
        }

        [Test]
        public void TestFalsePositivesDifferentTypes()
        {
            Assert.IsFalse(ObjectUtilities.TestFieldEquality (et1, "string"));
        }

        [Test]
        public void TestHandlesNull()
        {
            Assert.IsTrue(ObjectUtilities.TestFieldEquality (null, null), "null == null should return true");
            Assert.IsFalse(ObjectUtilities.TestFieldEquality ("string", null), "Comparing something to null should return false. (null as second param)");
            Assert.IsFalse(ObjectUtilities.TestFieldEquality (null, "string") ,"Comparing something to null should return false. (null as first param)");
        }
    }
}
#endif