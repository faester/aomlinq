using System;
using System.Collections.Generic;
using System.Text;
using GenDB;
using NUnit.Framework;
using System.Query;
using CommonTestObjects;
using System.Expressions;

namespace TranslationTests
{
    /// <summary>
    /// Tests the semantics of translation.
    /// All classes implementing IBusinessObject should be translatable
    /// if they contain only public Properties, that either implements IBusinessObject
    /// or are of some primitive type, are of type string or of type DateTime.
    /// 
    /// If properties are public, they must have both volatileAttribute public setter and volatileAttribute public getter
    /// to be translatable. If the cstProperty should not be translated/persisted, it should 
    /// be decorated with the attribute [Volatile].
    /// 
    /// If something non-translatable is attempted to be translated, an exception will
    /// be thrown at Table&lt;T&gt;-instantion time.
    /// 
    /// Type-translatability is ensured by the compiler, since all classes must implement
    /// IBusinessObject to be volatileAttribute type parameter on table.
    /// </summary>
    [TestFixture]
    public class TranslationTests
    {
        private class TestPersonsSecretSubclass : TestPerson
        {

        }

        private class ShouldFailNoSetter : AbstractBusinessObject
        {
            int foo;

            /// <summary>
            /// TranslationTests should fail, since Foo is public and only has volatileAttribute getter 
            /// </summary>
            public int Foo
            {
                get { return foo; }
            }
        }

        private class ShouldFailNoGetter : AbstractBusinessObject
        {
            int foo = 0;

            public int Foo
            {
                set { foo = value; }
            }
        }

        private class HasVolatileProperty : AbstractBusinessObject
        {
            int vol = 0; 

            [Volatile]
            public int Vol
            {
                get { return vol; }
                set { vol = value; }
            }

            int persisted = 0; 

            public int Persisted
            {
                get { return persisted; }
                set { persisted = value; }
            }
        }

        private class PureIBusinessImpl : IBusinessObject
        {
            public const string DEFAULT_VALUE = "Keld Olsens Sengehalm A/S";

            DBIdentifier entityPOID;

            public DBIdentifier DBIdentity
            {
                get { return entityPOID; }
                set { entityPOID = value; }
            }

            string value = DEFAULT_VALUE;

            public string Value
            {
                get { return this.value; }
                set { this.value = value; }
            }
        }

        DataContext dc = null;
        Table<ShouldFailNoSetter> tableOfShouldFailNoSetter = null;
        Table<ShouldFailNoGetter> tableOfShouldFailNoGetter = null;
        Table<HasVolatileProperty> tableOfHasVolatileProperty = null;
        Table<PureIBusinessImpl> tableOfPureIBusinessImpl = null;
        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            dc = DataContext.Instance;  
            if (!dc.IsInitialized)
            {
                dc.DeleteDatabase();
                dc.CreateDatabase();
                dc.Init();
            }

            tableOfHasVolatileProperty = dc.GetTable<HasVolatileProperty>();
            tableOfPureIBusinessImpl = dc.GetTable<PureIBusinessImpl>();
        }
        
        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            dc = null;
            tableOfShouldFailNoGetter = null;
            tableOfShouldFailNoSetter = null;
            tableOfHasVolatileProperty = null;
            tableOfPureIBusinessImpl = null;
            
            Console.Error.WriteLine("IBOCache size is now: " + DataContext.Instance.CommittedObjectsSize);
        }

        [Test, ExpectedException(typeof(NotTranslatableException))]
        public void TestTranslateNoSetter() 
        { 
            tableOfShouldFailNoSetter = dc.GetTable<ShouldFailNoSetter>();
        }

        [Test, ExpectedException(typeof(NotTranslatableException))]
        public void TestTranslateNoGetter()
        {
            tableOfShouldFailNoGetter = dc.GetTable <ShouldFailNoGetter>();
            tableOfShouldFailNoGetter.Add ( new ShouldFailNoGetter());
        }

        [Test]
        public void CanTranslatePureIBOs()
        {
            tableOfPureIBusinessImpl.Clear();

            for (int i = 0; i < 10; i++)
            {
                tableOfPureIBusinessImpl.Add(new PureIBusinessImpl{Value = i.ToString()});
            }
            dc.SubmitChanges();

            var pis = from ps in tableOfPureIBusinessImpl
                      orderby ps.Value
                      select ps;
                      

            int idx = 0;
            foreach (var q in pis)
            {
                Assert.AreEqual(idx.ToString(), q.Value);
                idx++;
            }
        }

        private void TestFieldsShouldNotBePersistedInit()
        {
            Table<ContainsAllPrimitiveTypes> tcapt = dc.GetTable<ContainsAllPrimitiveTypes>();

            for (int i = 0; i < 100; i++)
            {
                ContainsAllPrimitiveTypes toAdd = new ContainsAllPrimitiveTypes{stringNotPersisted = "Væk!", intNotPersisted = 989};
                tcapt.Add(toAdd);
            }
            dc.SubmitChanges();
            GC.Collect();
        }

        [Test]
        public void TestFieldsShouldNotBePersisted()
        {
            TestFieldsShouldNotBePersistedInit();
            dc.SubmitChanges();
            GC.Collect();
            Table<ContainsAllPrimitiveTypes> tcapt = dc.GetTable<ContainsAllPrimitiveTypes>();

            foreach(ContainsAllPrimitiveTypes capt in tcapt)
            {
                Assert.IsFalse(capt.stringNotPersisted == "Væk!", "Value of public string field was persisted.");
                Assert.IsFalse(capt.intNotPersisted == 989, "Value of public int field was persisted.");
            }
        }

        [Test]
        public void TestIfVolatileIsTranslated()
        {
            int volSetValue = 10;
            int persistedSetvalue = 11;
            HasVolatileProperty hvp = null;
            for (int i = 0; i < 10; i++)
            {
                hvp = new HasVolatileProperty();
                hvp.Vol = volSetValue;
                hvp.Persisted = persistedSetvalue;
                tableOfHasVolatileProperty.Add (hvp);
            }
            hvp = null;
            GC.Collect();
            dc.SubmitChanges();

            Console.WriteLine("Cache size (Uncommitted = {0}, Comitted = {1})", dc.UnCommittedObjectsSize, dc.CommittedObjectsSize);

            foreach(HasVolatileProperty hvpr in tableOfHasVolatileProperty)
            {
                Assert.AreNotEqual( volSetValue, hvpr.Vol, "Vol property was persisted.");
                Assert.AreEqual(persistedSetvalue, hvpr.Persisted, "Persisted property was not persisted.");
            }
        }

        private void Populate()
        {
            Table<TestPerson> ttp = dc.GetTable<TestPerson>();
            ttp.Clear();
            dc.SubmitChanges();

            for (int i = 0; i < 100; i++)
            {
                TestPerson tp = new TestPerson();
                tp.Name = null;
                ttp.Add(tp);
            }

            dc.SubmitChanges();
            GC.Collect();

            dc.SubmitChanges();
            GC.Collect();

        }

        [Test]
        public void StringRemainsNull()
        {
            Populate();
            Table<TestPerson> ttp = dc.GetTable<TestPerson>();

            foreach(TestPerson p in ttp)
            {
                Assert.IsNull(p.Name, "Found person with name >" + p.Name + "<");
            }

        }

        private void PopulateWithPersonSubclass()
        {
            Table<TestPerson> ttp = dc.GetTable<TestPerson>();
            ttp.Clear();
            dc.SubmitChanges();

            for (int i = 0; i < 10; i ++)
            {
                TestPerson p = new TestPerson();
                p.Spouse = new TestPersonsSecretSubclass();
                p.Name = "HasSecretSpouse";
                ttp.Add(p);
            }

            dc.SubmitChanges();
        }

        [Test]
        public void FieldIsUnknownSubType()
        {
            PopulateWithPersonSubclass();
            Table<TestPerson> ttp = dc.GetTable<TestPerson>();

            var k = from ps in ttp
                    where ps.Name == "HasSecretSpouse"
                    select ps;

            Type typeSP = typeof(TestPersonsSecretSubclass);
            bool foundSome = false;

            foreach(TestPerson p in k)
            {
                foundSome = true;
                Assert.IsNotNull(p.Spouse, "Spouse was empty.");
                Assert.IsTrue(typeSP.Equals(p.Spouse.GetType()), "Spouse should be of private subtype " + typeSP);
            }

            Assert.IsTrue(foundSome, "Did not find any persons.");
        }

    }
}
