using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace GenDB
{
    internal sealed class CacheElement
    {
        WeakReference wr;
        long entityPOID;
        IBusinessObject clone;

        private CacheElement() { /* empty */ }

        public CacheElement(IBusinessObject target)
        {
            wr = new WeakReference(target);
            clone = (IBusinessObject)ObjectUtilities.MakeClone(target);
            entityPOID = target.DBTag.EntityPOID;
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

    internal sealed class IBOCache
    {
        static IBOCache instance = new IBOCache();
        static long retrieved = 0;

        public long Retrieved
        {
            get { return IBOCache.retrieved; }
        }

        public static IBOCache Instance
        {
            get { return instance; }
        }

        private IBOCache() { /* empty */ }

        /// <summary>
        /// Stores objects with weak references to allow garbage collection of 
        /// the cached objects.
        /// </summary>
        Dictionary<long, CacheElement> committedObjects = new Dictionary<long, CacheElement>();

        ///// <summary>
        ///// Stores regular object references. The object must not be gc'ed before 
        ///// it has been written to persistent storage.
        ///// </summary>
        Dictionary<long, IBusinessObject> uncommittedObject = new Dictionary<long, IBusinessObject>();

        /// <summary>
        /// Adds the given obj to the cache. The DBTag element
        /// must be set prior to calling this method.
        /// </summary>
        /// <param name="obj"></param>
        public void Add(IBusinessObject obj)
        {
            if (obj.DBTag == null) { throw new NullReferenceException("DBTag of obj not set"); }
            uncommittedObject[obj.DBTag.EntityPOID] = obj;
        }

        private void AddToCommitted(IBusinessObject obj)
        {
            CacheElement wr = new CacheElement(obj);
            committedObjects[obj.DBTag.EntityPOID] = wr;
        }
        
        public int Count
        {
            get { return committedObjects.Count; }
        }

        /// <summary>
        /// Returns the IBusinessObject identified
        /// by the given DBTag id. Returns null if 
        /// object is not found. If DBTag is null, a
        /// NullReferenceException is thrown.
        /// </summary>
        /// <param name="id"></param>
        public IBusinessObject Get(DBTag id)
        {
            if (id == null) { throw new NullReferenceException("id"); }
            return Get(id.EntityPOID);
        }

        /// <summary>
        /// Returns the business object identified 
        /// by the given id. Will return null if id
        /// is not found.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IBusinessObject Get(long id)
        {
            CacheElement wr;
            IBusinessObject result;
            if (!committedObjects.TryGetValue(id, out wr))
            {
                if (!uncommittedObject.TryGetValue(id, out result))
                {
                    return null;
                }

            }
            else
            {
                if (!wr.IsAlive) { throw new Exception("Internal error in cache: Object has been reclaimed by garbagecollector, but was requested from cache."); }
                result = wr.Target;
            }
            retrieved++;
            return result;
        }

        public override string ToString()
        {
            return "IBOCache. Cache size = " + instance.Count + ", cache retrieves = " + instance.Retrieved;
        }

        public void FlushToDB()
        {
            GenericDB.Instance.SubmitChanges();

            CommitUncommitted();
            CommitChangedCommited();
            MoveCommittedToUncommitted();

            GenericDB.Instance.SubmitChanges();
        }

        private void CommitUncommitted()
        {
            foreach (IBusinessObject ibo in uncommittedObject.Values)
            {
                Translator.UpdateDBWith(ibo);
            }
        }

        private void MoveCommittedToUncommitted()
        {
            foreach (IBusinessObject ibo in uncommittedObject.Values)
            {
                AddToCommitted(ibo);
            }
            uncommittedObject.Clear();
        }

        private void CommitChangedCommited()
        {
            foreach (CacheElement ce in committedObjects.Values)
            {
                if (ce.IsDirty)
                {
                    if (ce.IsAlive)
                    {
                        IBusinessObject ibo = ce.Target;
                        Translator.UpdateDBWith(ibo);
                        ce.ClearDirtyBit();
                    }
                    else
                    {
                        //TODO:
                        throw new Exception("Object reclaimed before if was flushed to the DB.");
                    }
                }
            }
        }

        /// <summary>
        /// Removes the object identified 
        /// by the id from the cache. If object is 
        /// dirty its new state will be added to the database.
        /// (Ment to be invoked only by 
        /// the DBTag destructor)
        /// </summary>
        /// <param name="id"></param>
        internal void Remove(long id)
        {
            //TODO: Need to make some kind of commit possible here.
            committedObjects.Remove(id);
            uncommittedObject.Remove(id);
        }
    }
}
