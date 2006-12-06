using System;
using System.Collections.Generic;
using System.Text;

namespace GenDB
{
    public class AbstractBusinessObject : IBusinessObject
    {
        private DBTag dBTag;
        public DBTag DBTag
        {
            get { return dBTag; }
            set { dBTag = value; }
        }
    }
}
