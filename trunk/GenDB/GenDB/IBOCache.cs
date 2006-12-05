using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace GenDB
{
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
        Dictionary<long, WeakReference> cachedObjects = new Dictionary<long, WeakReference>();

        /// <summary>
        /// Adds the given obj to the cache. The DBTag element
        /// must be set prior to calling this method.
        /// </summary>
        /// <param name="obj"></param>
        public void Add(IBusinessObject obj)
        {
            if (obj.DBTag == null) { throw new NullReferenceException ("DBTag of obj not set"); }
            WeakReference wr = new WeakReference(obj);
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
            WeakReference wr = cachedObjects[id.EntityPOID];
            if (!wr.IsAlive) { throw new Exception("Internal error in cache: Object has been reclaimed by garbagecollector, but was requested from cache."); }
            retrieved++;
            return (IBusinessObject)wr.Target;
        }

        public IBusinessObject Get(long id)
        {
            WeakReference wr;
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
            //foreach (CacheEntry wr in cachedObjects.Values)
            //{
            //    if (wr.IsDirty)
            //    {
            //        IBusinessObject ibo = (IBusinessObject)wr.Target;
            //        Console.WriteLine("Writing object {0} to cache" , ibo.DBTag.EntityPOID);
            //        Translator.UpdateDBWith((IBusinessObject) wr.Target);
            //        wr.ClearDirtyBit();
            //    }
            //}
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
