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

        private CacheElement () { /* empty */ }
        
        public CacheElement (IBusinessObject target)
        {
            wr = new WeakReference (target);
            clone = (IBusinessObject)ObjectUtilities.MakeClone (target);
            entityPOID = target.DBTag.EntityPOID;
        }

        public bool IsAlive
        {
            get { return wr.IsAlive; }
        }

        public bool IsDirty { 
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
        Dictionary<long, CacheElement> cachedObjects = new Dictionary<long, CacheElement>();

        /// <summary>
        /// Adds the given obj to the cache. The DBTag element
        /// must be set prior to calling this method.
        /// </summary>
        /// <param name="obj"></param>
        public void Add(IBusinessObject obj)
        {
            if (obj.DBTag == null) { throw new NullReferenceException ("DBTag of obj not set"); }
            CacheElement wr = new CacheElement(obj);
            cachedObjects[obj.DBTag.EntityPOID] = wr;
        }

        public int Count
        {
            get { return cachedObjects.Count; }
        }

        /// <summary>
        /// Returns the IBusinessObject identified
        /// by the given DBTag id.
        /// </summary>
        /// <param name="id"></param>
        public IBusinessObject Get(DBTag id)
        {
            if (id == null) { throw new NullReferenceException("id"); }
            CacheElement wr = cachedObjects[id.EntityPOID];
            if (!wr.IsAlive) { throw new Exception("Internal error in cache: Object has been reclaimed by garbagecollector, but was requested from cache."); }
            retrieved++;
            return (IBusinessObject)wr.Target;
        }

        public IBusinessObject Get(long id)
        {
            CacheElement wr;
            if (!cachedObjects.TryGetValue(id, out wr))
            {
                return null;
            }
            retrieved++;
            if (!wr.IsAlive) {throw new Exception("Internal error in cache: Object has been reclaimed by garbagecollector, but was requested from cache."); }
            return (IBusinessObject)wr.Target;
        }

        public override string ToString()
        {
            return "IBOCache. Cache size = " + instance.Count + ", cache retrieves = " + instance.Retrieved;
        }

        public void FlushToDB()
        {
            GenericDB.Instance.SubmitChanges();
            foreach (CacheElement ce in cachedObjects.Values)
            {
                if (ce.IsDirty)
                {
                    IBusinessObject ibo = ce.Target;
                    Translator.UpdateDBWith(ibo);
                    ce.ClearDirtyBit();
                }
            }
            GenericDB.Instance.SubmitChanges();
        }

        /// <summary>
        /// Removes the object identified 
        /// by the id from the cache.
        /// (Ment to be invoked only by 
        /// the DBTag destructor)
        /// </summary>
        /// <param name="id"></param>
        internal void Remove(long id)
        {
            cachedObjects.Remove (id);
        }
    }
}
