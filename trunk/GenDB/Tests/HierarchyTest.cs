using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using GenDB;

namespace ClassHierarchy
{
    [TestFixture]
    public class HierarchyTest
    {
        #region test class hierarcy
        public class Base : IBusinessObject
        {
            DBIdentifier dBIdentity;

            public DBIdentifier DBIdentity
            {
                get { return dBIdentity; }
                set { dBIdentity = value; }
            }

            string name = "base";

            public string Name
            {
                get { return name; }
                set { name = value; }
            }
        }

        public class A0 : Base
        {
            int integer;

            public int Integer
            {
                get { return integer; }
                set { integer = value; }
            }
        }

        public class A1 : Base
        {
            double d;

            public double D
            {
                get { return d; }
                set { d = value; }
            }
        }

        public class B : A0
        {
            string bString = "B";

            public string BString
            {
                get { return bString; }
                set { bString = value; }
            }
        }

        public class C : A0
        {
            string name = "shadow";

            public string Name
            {
                get { return name; }
                set { name = value; }
            }
        }

        public class D : A1
        {

        }
        #endregion

        Dictionary<Type, bool> typesFound = null;
        DataContext dc = DataContext.Instance;
        Table<Base> tableOfBase = null;

        private void InitTypesFound()
        {
            typesFound = new Dictionary<Type, bool>();
            typesFound[typeof(Base)] = false;
            typesFound[typeof(A0)] = false;
            typesFound[typeof(A1)] = false;
            typesFound[typeof(B)] = false;
            typesFound[typeof(C)] = false;
            typesFound[typeof(D)] = false;
        }

        private void InitTable()
        {
            tableOfBase = dc.CreateTable<Base>();
            tableOfBase.Add(new Base());
            tableOfBase.Add(new A0());
            tableOfBase.Add(new A1());
            tableOfBase.Add(new B());
            tableOfBase.Add(new C());
            tableOfBase.Add(new D());
            dc.SubmitChanges();
        }

        [TestFixtureSetUp]
        public void TestFixtureSetUp() { }

        [SetUp]
        public  void SetUp()
        {
            InitTable();
            InitTypesFound();
        }

        [TearDown]
        public void TearDown()
        {
            tableOfBase.Clear();
            dc.SubmitChanges();
        }

        [Test]
        public void TestShadowedPropertyNamesAccepted()
        {
            // Testing happens in setup method. C.Name shadows Base.Name and did cause an error earlier.
        }

        [Test]
        public void TestFindsAll()
        {
            var bs = from baseobj in tableOfBase
                     select baseobj;

            foreach (Base b in bs)
            {
                typesFound[b.GetType()] = true;
            }

            foreach(KeyValuePair<Type, bool> kvp in typesFound)
            {
                Assert.IsTrue (kvp.Value, "Did not return type: " + kvp.Key);
            }
        }

        [Test]
        public void TestDontFilterOnShadowedField()
        {
            var bs = from baseobj in tableOfBase
                     where baseobj.Name == "shadow"
                     select baseobj;

            foreach(Base b in bs)
            {
                Assert.Fail("Should return no values. Filtered on property of derived class rather than declared base class");
            }
        }

        [Test]
        public void TestFilterOnShadowedField()
        {
            Table<C> tc = dc.CreateTable<C>();

            var cs = from c in tc
                     where c.Name == "shadow"
                     select c;

            bool foundIt = false;

            foreach(C cinst in tc)
            {
                foundIt = true;
            }

            Assert.IsTrue (foundIt, "Did not find the derived class based on filter on its shadowing property.");
        }
    }
}
