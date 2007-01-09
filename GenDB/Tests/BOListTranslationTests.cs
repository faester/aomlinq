using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using GenDB;

namespace Tests
{
    class Person : AbstractBusinessObject
    {
        string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        int age;

        public int Age
        {
            get { return age; }
            set { age = value; }
        }
    }


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