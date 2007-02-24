using System;
using System.Collections.Generic;
using System.Text;

namespace GenDB
{
    internal sealed class IBOCacheElement
    {
        WeakReference wr;
        int entityPOID;
        bool isCollection;

        public int EntityPOID
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
            if (target.GetType().GetInterface("IDBSaveableCollection") != null) 
            { 
                isCollection = true; 
            }
            else
            {
                clone = (IBusinessObject)ObjectUtilities.MakeClone(target);
            }

            original = target;
            wr = new WeakReference(target);
            entityPOID = target.DBIdentity;
        }

        public bool IsAlive
        {
            get { return wr.IsAlive; }
        }

        public bool IsDirty
        {
            get { 
                if (!isCollection)
                {
                    return !ObjectUtilities.TestFieldEquality(wr.Target, clone); 
                }
                else
                {
                    return (original as IDBSaveableCollection).HasBeenModified;
                }
            }
        }

        public IBusinessObject Target
        {
            get { return (IBusinessObject)wr.Target; }
        }

        public void ClearDirtyBit()
        {
            if (!isCollection) {
                clone = (IBusinessObject)ObjectUtilities.MakeClone((IBusinessObject)wr.Target);
            }
            else
            {
                (original as IDBSaveableCollection).HasBeenModified = false;
            }
        }
    }
}
