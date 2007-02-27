using System;
using System.Collections.Generic;
using System.Text;

namespace GenDB
{
    /// <summary>
    /// Use to store committed objects in the cache.
    /// Will store a clone of the object given at instantiation
    /// time to enable change tracking. 
    /// </summary>
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

        /// <summary>
        /// Contains the element given at instantiation time.
        /// </summary>
        public IBusinessObject Element
        {
            get { return original; }
            //set { original = value; }
        }

        public void ReleaseStrongReference()
        {
            original = null;
        }

        public void ReEstablishStrongReference()
        {
            original = (wr.Target as IBusinessObject);
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

        public void SetNotDirty()
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
