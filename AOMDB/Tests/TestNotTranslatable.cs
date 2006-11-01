using System;
using System.Collections.Generic;
using System.Text;
using Translation;
using NUnit.Framework;
using Business;
using Persistence;
using AOM; 

namespace Tests
{
    [TestFixture]
    public class TestNotTranslatable
    {
        public class NotTranslatable : IBusinessObject
        {
            object o;
            DBTag tag; 
            public DBTag DatabaseID 
            {
                set { tag = value; }
                get { return tag; }
            }

            public bool IsDirty
            {
                get { return true; }
                set {} 
            }
        }

        [Test, ExpectedException(typeof(NotTranslatableException))]
        public void ExceptionThrown()
        {
            BO2AOMTranslator<NotTranslatable> trans = new BO2AOMTranslator<NotTranslatable>();
        }
    }
}
