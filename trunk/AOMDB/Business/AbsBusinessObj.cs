using System;
using System.Collections.Generic;
using System.Text;
using Persistence;

namespace Business
{
    public class AbsBusinessObj : IBusinessObject
    {
        DBTag tag;
        bool dirty = true;

        public bool IsDirty
        {
            get { return dirty; }
            set { dirty = value; }
        }

        public DBTag DatabaseID
        {
            get { return tag; }
            set { tag = value; }
        }
    }
}
