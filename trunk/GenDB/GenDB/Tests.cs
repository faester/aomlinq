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
                int i = 0;
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
                et2.TheString = ""; // Ensure string are compared by value. 
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
        }
    }
