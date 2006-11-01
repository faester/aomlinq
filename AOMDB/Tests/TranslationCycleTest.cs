using System;
using System.Collections.Generic;
using System.Text;
using Business;
using AOM;
using Translation;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class TranslationCycleTest
    {
        #region Test object RecursiveBO
        class RecursiveBO : SimpleBusinessObject
        {
            RecursiveBO next;

            public RecursiveBO Next
            {
                get { return next; }
                set { next = value; }
            }

            public override bool Equals(object obj)
            {
                bool sboEquals = base.Equals(obj);
                if (!sboEquals)
                {
                    return false;
                }
                if (!(obj is RecursiveBO))
                {
                    return false;
                }
                RecursiveBO other = (RecursiveBO)obj;

                return object.ReferenceEquals(other.next, this.next);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }
        #endregion

        BO2AOMTranslator<RecursiveBO> trans;
        BO2AOMTranslator<SimpleBusinessObject> sbotrans = new BO2AOMTranslator<SimpleBusinessObject>();

        int elements = 4; //Number of elements to insert in chain. Must be > 1
        RecursiveBO[] rbos;

        [SetUp]
        public void Setup()
        {
            trans = new BO2AOMTranslator<RecursiveBO>();
            rbos = new RecursiveBO[elements];
            //Construct cyclic data structure.
            for (int i = 0; i < elements; i++)
            {
                rbos[i] = new RecursiveBO();
                rbos[i].Name = "name" + i.ToString();
                rbos[i].Serial = i;
                if (i > 0)
                {
                    rbos[i - 1].Next = rbos[i];
                }
            }

            //Connect last element to first element
            rbos[elements - 1].Next = rbos[0];
        }

        [TearDown]
        public void TearDown()
        {
            rbos = null;
        }

        private bool TranslatesReferencesCorrectly()
        {
            SimpleBusinessObject sbo = new SimpleBusinessObject ();
            Entity e = sbotrans.ToEntity (sbo);
            SimpleBusinessObject sbocopy = sbotrans.FromEntity (e);
            return object.ReferenceEquals (sbo, sbocopy);
        }

        /// <summary>
        /// Translation of the test objects will only stop 
        /// if the cycle is discovered by the translation 
        /// mechanism. Hence: The crucial thing is <i>if</i> 
        /// this test completes rather than how it completes.
        /// </summary>
        [Test] //, Ignore("Problems with infinite loop")]
        public void TestTranslationStops()
        {
            if (!TranslatesReferencesCorrectly ()) 
            {
                Assert.Fail ("Reference translation is incorrect. Test skipped to avoid infinite loop!");
            }

            Entity e = trans.ToEntity(rbos[0]);
            RecursiveBO copy = trans.FromEntity(e);
            Assert.IsTrue(rbos[0].Equals(copy), "Copy did not match original head element");
        }

        /// <summary>
        /// Tests if the object chain is preserved. This can 
        /// only be the case if some kind of object cache is 
        /// maintained in the translation mechanism.
        /// </summary>
        [Test] //, Ignore("Problems with infinite loop")]
        public void TestChainRebuild()
        {
            if (!TranslatesReferencesCorrectly ()) 
            {
                Assert.Fail ("Reference translation is incorrect. Test skipped to avoid infinite loop!");
            }

            Entity e = trans.ToEntity(rbos[0]);
            RecursiveBO copy = trans.FromEntity(e);

            RecursiveBO[] copies = new RecursiveBO[elements];
            copies[0] = copy;

            int idx = 1;

            //Follow the chain
            while (idx < elements)
            {
                Assert.IsNotNull(copies[idx - 1].Next, "Null element found in translated chain");
                copies[idx] = copies[idx - 1].Next;
                idx++;
            }

            //Test that last element points to first element.
            Assert.IsTrue(object.ReferenceEquals(copies[0], copies[idx - 1].Next), "Last element in translated chain did not point to first element.");
        }
    }
}
