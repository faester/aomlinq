using System;
using System.Collections.Generic;
using System.Text;

namespace GenDB
{
    /// <summary>
    /// The simplest possible implementation
    /// of IBusinessObject. Can be used as a 
    /// base class for any object hierarchy.
    /// </summary>
    public abstract class AbstractBusinessObject : IBusinessObject
    {
        DBIdentifier dBIdentifier;

        public DBIdentifier DBIdentity
        {
            get { return dBIdentifier; }
            set { dBIdentifier = value; }
        }
    }
}
