using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Business;
using Translation;
using Persistence;
using AOM;


namespace Tests
{
    #region ReferenceFieldTranslationTests
    [TestFixture]
    public class ReferenceFieldTranslationTests
    {
        #region BOHasReferenceField 
        class BOHasReferenceField : IBusinessObject
        {
            SimpleBusinessObject sbo;

            public SimpleBusinessObject SBO
            {
                get { return sbo; }
                set { sbo = value; }
            }

            DBTag tag;

            public DBTag DatabaseID
            {
                get { return tag; }
                set { tag = value; }
            }

            public bool IsDirty
            {
                get { return true; }
                set { /* don't care */ }
            }

            public override bool Equals(object obj)
            {
                if (obj == null) { return false; }
                if (!(obj is BOHasReferenceField)) { return false; }
                BOHasReferenceField other = (BOHasReferenceField)obj;
                if (other.SBO == null && this.SBO == null) { return true; }
                if (other.SBO == null ^ this.SBO == null) { return false; } 
                return other.SBO.Equals (this.SBO);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }
        #endregion

        BOHasReferenceField testObj;
        BO2AOMTranslator<BOHasReferenceField> trans = new BO2AOMTranslator <BOHasReferenceField>();
        Entity entityCopy; 

        [SetUp]
        public void Setup()
        {
            testObj = new BOHasReferenceField ();
            testObj.SBO = new SimpleBusinessObject ();
            testObj.SBO.Name = "the pervasive Mr Tester";
            testObj.SBO.Serial = 42; 
        }

        [TearDown]
        public void TearDown()
        {
            testObj = null;
            entityCopy = null;
        }

        [Test]
        public void TestBackwardTranslationEquals()
        {
            entityCopy = trans.ToEntity(testObj);            
            BOHasReferenceField copy = trans.FromEntity (entityCopy);
            Assert.IsTrue (copy.Equals (testObj), "The back translated object did not .Equals(orignal)");
        }

        [Test]
        public void TestBackwardTranslationReference()
        {
            entityCopy = trans.ToEntity(testObj);            
            BOHasReferenceField copy = trans.FromEntity (entityCopy);
            Assert.IsTrue (object.ReferenceEquals ( copy, testObj), "Back translated object was not identical to original.");
        }

        [Test]
        public void CanTranslateNullReferenceFields()
        {
            Assert.IsNotNull(testObj, "Failure in test setup.");
            testObj.SBO = null;
            entityCopy = trans.ToEntity (testObj);
            BOHasReferenceField copy = trans.FromEntity (entityCopy );
            Assert.IsNotNull(copy, "Returned object was null, but only field should be null.");
            Assert.IsNull(copy.SBO, "Field was set to instance but should be null.");
        }

        [Test]
        public void CanTranslateNullReferenceObjects()
        {
            testObj = null;
            entityCopy = trans.ToEntity (testObj);
        }

        [Test]
        public void MultipleBacktranslationsReturnsSameObject()
        {
            entityCopy = trans.ToEntity (testObj);
            BOHasReferenceField copy1 = trans.FromEntity (entityCopy);
            BOHasReferenceField copy2 = trans.FromEntity (entityCopy);
            Console.WriteLine(
                "Orig type: {0}\n copy1 type: {1}\n copy2 type: {2}", 
                testObj.GetType().FullName, 
                copy1.GetType().FullName, 
                copy2.GetType().FullName
                );
            Assert.IsTrue(object.ReferenceEquals (copy1, copy2), "Object references was not identical.");
        }
    }
    #endregion

    #region SimpleTranslationTests
    [TestFixture]
    public class SimpleTranslationTests
    {
        SimpleBusinessObject sbo;
        Entity translated;
        BO2AOMTranslator<SimpleBusinessObject> translator;
        SimpleBusinessObject copy;

        [SetUp]
        public void Setup()
        {
            sbo = new SimpleBusinessObject();
            sbo.Name = "Testing";
            sbo.Serial = 2610;
            translator = new BO2AOMTranslator<SimpleBusinessObject>();
        }

        [TearDown]
        public void TearDown()
        {
            sbo = null;
            translator = null;
            translated = null;
            copy = null;
        }

        [Test]
        public void TestSimpleBusinessObjectEquality()
        {
            SimpleBusinessObject first = new SimpleBusinessObject();
            first.Name = "Test";
            first.Serial = 1212;
            SimpleBusinessObject copy = new SimpleBusinessObject();
            copy.Serial = first.Serial;
            copy.Name = first.Name;
           Assert.IsTrue(copy.Equals(first));
            Assert.IsFalse(copy.Equals(null));
            Assert.IsFalse(copy.Equals(new object()));

            SimpleBusinessObject notACopy = new SimpleBusinessObject();

            Assert.IsFalse(first.Equals(notACopy));
        }


        [Test]
        public void ConstructEntityTypeFromObject()
        {
            EntityType et = EntityTypeConverter.Construct(typeof(SimpleBusinessObject));
            Property p = et.GetProperty ("name");
            p = et.GetProperty ("serial");
            p = et.GetProperty ("m_tag");
        }

        [Test]
        public void TestFromBusinessObject()
        {
            translated = translator.ToEntity(sbo);
        }

        [Test]
        public void TestToBusinessObject()
        {
            copy = translator.FromEntity(translated);
        }

        /// <summary>
        /// This test uses the SimpleBusinessObject as basis
        /// for its tests. The intention is, that SimpleBusinessObject
        /// contains all simple s_types, that should be translatable.
        /// <p>
        /// If this test fails, it might suggest an internal error
        /// in the FieldConverter class. To aid debugging the field
        /// causing the error is discernible from SimpleBusinessObject's
        /// FirstEqualsFailFieldName.
        /// </p>
        /// </summary>
        [Test]
        public void TestTranslationCorrectnessEquals()
        {
            Assert.IsNotNull (sbo);
            Entity e = translator.ToEntity(sbo);
            Assert.IsNotNull(e);
            copy = translator.FromEntity (e);
            if (!sbo.Equals (copy))
            {
                Assert.Fail("Equals method returned false. Conflicting field was " + sbo.FirstEqualsFailFieldname);
            }
        }

        [Test]
        public void TestTranslationCorrectnessReferenceEquals()
        {
            Entity e = translator.ToEntity(sbo);
            Assert.IsNotNull(sbo);
            Assert.IsNotNull(e);
            copy = translator.FromEntity (e);
            Assert.IsTrue(object.ReferenceEquals(sbo, copy), "Translation returned new instance of known object");
        }
    }
    #endregion
}
