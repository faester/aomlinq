using System;
using System.Collections.Generic;
using System.Text;
using GenDB;
using NUnit.Framework;
using CommonTestObjects;

namespace Tests
{
    [TestFixture]
    class LazyLoadTests
    {
        class HasLazyFields : AbstractBusinessObject
        {
            LazyLoader<TestPerson> personLoader = new LazyLoader<TestPerson>();

            [LazyLoad(Storage = "personLoader")]
            public TestPerson Person
            {
                get { return personLoader.Element;}
                set { personLoader.Element = value; }
            }
        }
    }
}
