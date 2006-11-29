using System;
using System.Collections.Generic;
using System.Text;

namespace GenDB
{
    internal sealed class IBOCache
    {
        static IBOCache instance = new IBOCache();

        public static IBOCache Instance 
        {
            get { return instance; }
        }

        private IBOCache() { /* empty */ } 
        Dictionary<long, WeakReference> cachedObjects = new Dictionary<long, WeakReference>();

        /// <summary>
        /// Adds the given obj to the cache. The DBTag element
        /// must be set prior to calling this method.
        /// </summary>
        /// <param name="obj"></param>
        public void Add(IBusinessObject obj)
        {
            if (obj.DBTag == null) { throw new NullReferenceException ("DBTag of obj not set"); }
            WeakReference wr = new WeakReference (obj);
            cachedObjects[obj.DBTag.EntityPOID] = wr;
        }

        /// <summary>
        /// Returns the IBusinessObject identified
        /// by the given DBTag id.
        /// </summary>
        /// <param name="id"></param>
        public IBusinessObject Get(DBTag id)
        {
            if (id == null) { throw new NullReferenceException("id"); }
            WeakReference wr = cachedObjects[id.EntityPOID];
            if (!wr.IsAlive) { throw new Exception("Internal error in cache: Object has been reclaimed by garbagecollector, but was requested from cache."); }
            return (IBusinessObject)wr.Target;
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
