using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Business;
using Translation;
using AOM;

namespace Tests
{
    [TestFixture, Ignore("Known to fail.")]
    public class BOSetTranslation
    {
        int elements = 10;
        BOSet BOSet;
        SimpleBusinessObject[] sbos = null;
        BO2AOMTranslator<BOSet> translator;

        [SetUp]
        public void Setup()
        {
            BOSet = new BOSet();
            sbos = new SimpleBusinessObject[elements];
            for (int i = 0; i < elements; i++)
            {
                sbos[i] = new SimpleBusinessObject();
                sbos[i].Name = "name" + i.ToString();
                sbos[i].Serial = i;
                BOSet.Add (sbos[i]);
            }
            translator = new BO2AOMTranslator<BOSet>();
        }

        [TearDown]
        public void TearDown()
        {
            BOSet = null;
            sbos = null;
        }

        [Test]
        public void CanTranslateToEntity()
        {
            Entity e = translator.ToEntity(BOSet);
        }

        [Test]
        public void CanTranslateToBOSet()
        {
            Entity e = translator.ToEntity(BOSet);
            BOSet copy = translator.FromEntity (e);
        }

        [Test]
        public void ReverseTranslationCorrectness()
        {
            Entity e = translator.ToEntity(BOSet);
            BOSet copy = translator.FromEntity (e);
            foreach (SimpleBusinessObject s in sbos)
            {
                Assert.IsTrue( copy.Contains (s), "Missing elements in BOSet after translation");
            }
            Assert.AreEqual (copy.Count, sbos.Length, "Length of BOSet copy differs from length of original elements");
            Assert.AreEqual (copy.Count, BOSet.Count, "Length of BOSet copy differs from length of original BOSet");
        }
    }
}
