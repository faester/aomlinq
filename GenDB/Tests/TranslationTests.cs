using System;
using System.Collections.Generic;
using System.Text;
using GenDB;
using NUnit.Framework;
using System.Query;

namespace Tests
{
    /// <summary>
    /// Tests the semantics of translation.
    /// All classes implementing IBusinessObject should be translatable
    /// if they contain only public Properties, that either implements IBusinessObject
    /// or are of some primitive type, are of type string or of type DateTime.
    /// 
    /// If properties are public, they must have both a public setter and a public getter
    /// to be translatable. If the property should not be translated/persisted, it should 
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
            /// Translation should fail, since Foo is public and only has a getter 
            /// </summary>
            public int Foo
            {
                get { return foo; }
            }
        }

        private class ShouldFailNoGetter : AbstractBusinessObject
        {
            int foo;

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
            DBTag dbtag;

            public DBTag DBTag
            {
                get { return dbtag; }
                set { dbtag = value; }
            }

            string value = DEFAULT_VALUE;

            public string Value
            {
                get { return this.value; }
                set { this.value = value; }
            }
        }

        DataContext dc = DataContext.Instance;
        Table<ShouldFailNoSetter> tableOfShouldFailNoSetter = null;
        Table<ShouldFailNoGetter> tableOfShouldFailNoGetter = null;
        Table<HasVolatileProperty> tableOfHasVolatileProperty = null;
        Table<PureIBusinessImpl> tableOfPureIBusinessImpl = null;
        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            tableOfHasVolatileProperty = dc.CreateTable<HasVolatileProperty>();
            tableOfPureIBusinessImpl = dc.CreateTable<PureIBusinessImpl>();
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
        }

        [Test]
        public void CanTranslatePureIBOs()
        {
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
        public void TestIfVolatileIsTranslated()
        {
            int volSetValue = 10;
            int persistedSetvalue = 11;

            for (int i = 0; i < 10; i++)
            {
                HasVolatileProperty hvp = new HasVolatileProperty();
                hvp.Vol = volSetValue;
                hvp.Persisted = persistedSetvalue;
                tableOfHasVolatileProperty.Add (hvp);
            }
            GC.Collect();
            dc.SubmitChanges();

            foreach(HasVolatileProperty hvp in tableOfHasVolatileProperty)
            {
                Assert.AreNotEqual( volSetValue, hvp.Vol, "Vol property was persisted.");
                Assert.AreEqual(persistedSetvalue, hvp.Persisted, "Persisted property was not persisted.");
            }
        }
    }
}
