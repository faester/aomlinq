using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Translation;
using AOM;
using Business;

namespace Tests
{
    class BOWithCollectionTranslation
    {
        private class BOWithCollection : SimpleBusinessObject
        {
            IBOCollection collection;

            public IBOCollection Collection
            {
                get { return collection; }
                set { collection = value; }
            }

            public override bool Equals(object obj)
            {
                //Null tests etc in base. 
                bool baseEquals = base.Equals(obj);
                if (!baseEquals) { return false; }
                if (!(obj is BOWithCollection)) { return false; }
                BOWithCollection other = (BOWithCollection)obj;
                bool collectionsEqual =
                    (other.Collection == null && this.Collection == null)
                || this.Collection.Equals(other.Collection);

                return other.Collection.Equals(this.Collection);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        BOWithCollection bowc = null;
        SimpleBusinessObject[] sbos;

        [SetUp]
        public void Setup()
        {
            bowc = new BOWithCollection();
            bowc.Name = "boWithCollection";
            bowc.Serial = 123987;
            bowc.Collection = new BOList();
            sbos = new SimpleBusinessObject[10];
            for (int i = 0; i < sbos.Length; i++)
            {
                sbos[i] = new SimpleBusinessObject();
                sbos[i].Name = "navn" + i.ToString();
                sbos[i].Serial = i;
                bowc.Collection.Add (sbos[i]);
            }
        }

        [TearDown]
        public void TearDown()
        {
            bowc = null;
            sbos = null;
        }

        /// <summary>
        /// Tests contents of collection field. Assumes 
        /// basic tests has been conducted elsewhere.
        /// </summary>
        [Test]
        public void CollectionContents()
        {
            BO2AOMTranslator<BOWithCollection> trans = new BO2AOMTranslator<BOWithCollection>();
            Entity e = trans.ToEntity(bowc);
            BOWithCollection copy = trans.FromEntity (e);
            Assert.IsTrue(object.ReferenceEquals(copy.Collection, bowc.Collection), "Collection in reverse translated object differed from collection in original object.");
        }
        
        /// <summary>
        /// Tests contents of collection field. Assumes 
        /// basic tests has been conducted elsewhere.
        /// </summary>
        [Test]
        public void KeepNullField()
        {
            BO2AOMTranslator<BOWithCollection> trans = new BO2AOMTranslator<BOWithCollection>();
            bowc.Collection = null;
            Entity e = trans.ToEntity(bowc);
            BOWithCollection copy = trans.FromEntity (e);
            Assert.IsNull(copy.Collection, "Collection in reverse translated object contained instance although original objects' collection was null.");
        }
    }
}
