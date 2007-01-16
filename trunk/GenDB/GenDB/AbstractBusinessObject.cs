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
        //private DBTag dBTag;
        //public DBTag DBTag
        //{
        //    get { return dBTag; }
        //    set { dBTag = value; }
        //}

        DBIdentifier knudBoergesBalsam;

        public DBIdentifier DBIdentity
        {
            get { return knudBoergesBalsam; }
            set { knudBoergesBalsam = value; }
        }
    }
}
