using System;
using System.Collections.Generic;
using System.Text;

namespace GenDB
{
    internal sealed class IBOCacheElement
    {
        WeakReference wr;
        long entityPOID;

        public long EntityPOID
        {
            get { return entityPOID; }
        }

        IBusinessObject clone;

        public IBusinessObject Clone
        {
            get { return clone; }
            set { clone = value; }
        }

        IBusinessObject original;

        public IBusinessObject Original
        {
            get { return original; }
            set { original = value; }
        }

        private IBOCacheElement() { /* empty */ }

        public IBOCacheElement(IBusinessObject target)
        {
            original = target;
            wr = new WeakReference(target);
            clone = (IBusinessObject)ObjectUtilities.MakeClone(target);
            entityPOID = target.DBIdentity;
        }

        public bool IsAlive
        {
            get { return wr.IsAlive; }
        }

        public bool IsDirty
        {
            get { return !ObjectUtilities.TestFieldEquality(wr.Target, clone); }
        }

        public IBusinessObject Target
        {
            get { return (IBusinessObject)wr.Target; }
        }

        public void ClearDirtyBit()
        {
            clone = (IBusinessObject)ObjectUtilities.MakeClone((IBusinessObject)wr.Target);
        }
    }
}
