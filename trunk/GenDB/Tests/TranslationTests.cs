using System;
using System.Collections.Generic;
using System.Text;
using GenDB;
using NUnit.Framework;
using System.Query;
using CommonTestObjects;

namespace TranslationTests
{
    /// <summary>
    /// Tests the semantics of translation.
    /// All classes implementing IBusinessObject should be translatable
    /// if they contain only public Properties, that either implements IBusinessObject
    /// or are of some primitive type, are of type string or of type DateTime.
    /// 
    /// If properties are public, they must have both a public setter and a public getter
    /// to be translatable. If the cstProperty should not be translated/persisted, it should 
    /// be decorated with the attribute [Volatile].
    /// 
    /// If something non-translatable is attempted to be translated, an exception will
    /// be thrown at Table&lt;T&gt;-instantion time.
    /// 
    /// Type-translatability is ensured by the compiler, since all classes must implement
    /// IBusinessObject to be a type parameter on table.
    /// </summary>
    [TestFixture]
    public class TranslationTests
    {
        private class ShouldFailNoSetter : AbstractBusinessObject
        {
            int foo;

            /// <summary>
            /// TranslationTests should fail, since Foo is public and only has a getter 
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

            public DBIdentifier EntityPOID
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
            tableOfHasVolatileProperty = dc.CreateTable<HasVolatileProperty>();
            tableOfPureIBusinessImpl = dc.CreateTable<PureIBusinessImpl>();
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            dc = null;
            tableOfShouldFailNoGetter = null;
            tableOfShouldFailNoSetter = null;
            tableOfHasVolatileProperty = null;
            tableOfPureIBusinessImpl = null;
            GC.Collect();
        }

        [Test, ExpectedException(typeof(NotTranslatableException))]
        public void TestTranslateNoSetter() 
        { 
            tableOfShouldFailNoSetter = dc.CreateTable<ShouldFailNoSetter>();
        }

        [Test, ExpectedException(typeof(NotTranslatableException))]
        public void TestTranslateNoGetter()
        {
            tableOfShouldFailNoGetter = dc.CreateTable <ShouldFailNoGetter>();
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

        [Test]
        public void TestFieldsShouldNotBePersisted()
        {
            Table<ContainsAllPrimitiveTypes> tcapt = dc.CreateTable<ContainsAllPrimitiveTypes>();

            for (int i = 0; i < 100; i++)
            {
                ContainsAllPrimitiveTypes toAdd = new ContainsAllPrimitiveTypes{stringNotPersisted = "Væk!", intNotPersisted = 989};
                tcapt.Add(toAdd);
            }
            dc.SubmitChanges();

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
    }
}
