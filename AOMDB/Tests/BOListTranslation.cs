using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Business;
using Translation;
using AOM;

namespace Tests
{
    [TestFixture]
    public class BOListTranslation
    {
        int elements = 10;
        BOList bolist;
        SimpleBusinessObject[] sbos = null;
        BO2AOMTranslator<BOList> translator;

        [SetUp]
        public void Setup()
        {
            bolist = new BOList();
            sbos = new SimpleBusinessObject[elements];
            for (int i = 0; i < elements; i++)
            {
                sbos[i] = new SimpleBusinessObject();
                sbos[i].Name = "name" + i.ToString();
                sbos[i].Serial = i;
                bolist.Add (sbos[i]);
            }
            translator = new BO2AOMTranslator<BOList>();
        }

        [TearDown]
        public void TearDown()
        {
            bolist = null;
            sbos = null;
        }

        [Test]
        public void CanTranslateToEntity()
        {
            Entity e = translator.ToEntity(bolist);
        }

        [Test]
        public void CanTranslateToBOList()
        {
            Entity e = translator.ToEntity(bolist);
            BOList copy = translator.FromEntity (e);
        }

        [Test]
        public void ReverseTranslationCorrectness()
        {
            Entity e = translator.ToEntity(bolist);
            BOList copy = translator.FromEntity (e);
            foreach (SimpleBusinessObject s in sbos)
            {
                Assert.IsTrue( copy.Contains (s), "Missing elements in BOList after translation");
            }
            Assert.AreEqual (copy.Count, sbos.Length, "Length of BOList copy differs from length of original elements");
            Assert.AreEqual (copy.Count, bolist.Count, "Length of BOList copy differs from length of original BOList");
        }
    }
}
