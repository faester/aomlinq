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
            entityPOID = target.EntityPOID;
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
            clone = (IBusinessObject)ObjectUtilities.MakeClone(wr.Target);
        }
    }
}
