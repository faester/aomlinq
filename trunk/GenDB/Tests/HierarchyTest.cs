using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using GenDB;

namespace Tests
{
    [TestFixture]
    public class HierarchyTest
    {
        public class Base : IBusinessObject
        {
            DBIdentifier dBIdentity;

            public DBIdentifier DBIdentity
            {
                get { return dBIdentity; }
                set { dBIdentity = value; }
            }
        }
    }
}
