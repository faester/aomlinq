using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using GenDB;

namespace Tests
{
  

    [TestFixture]
    public class BOListTranslationTests
    {
        Table<ContainsAllPrimitiveTypes> tablePrimitive = new Table<ContainsAllPrimitiveTypes>();

        [TestFixtureSetUp]
        public void InitDB()
        {
            Configuration.RebuildDatabase = true;
        }

        [TestFixtureTearDown]
        public void TearDown() 
        { 
            /* empty */ 
        }
    }
}
