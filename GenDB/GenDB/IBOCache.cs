using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
#if DEBUG
using System.Diagnostics;
#endif

namespace GenDB
{
    internal sealed class CacheElement
    {
        WeakReference wr;
        long entityPOID;
        IBusinessObject clone;
        IBusinessObject original;

        public IBusinessObject Original
        {
            get { return original; }
            set { original = value; }
        }

        private CacheElement() { /* empty */ }

        public CacheElement(IBusinessObject target)
        {
            original = target;
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

    internal static class IBOCache
    {
        static long retrieved = 0;

        public static long Retrieved
        {
            get { return IBOCache.retrieved; }
        }

        static IBOCache() { /* empty */ }

        /// <summary>
        /// Stores objects with weak references to allow garbage collection of 
        /// the cached objects.
        /// </summary>
        static Dictionary<long, CacheElement> committedObjects = new Dictionary<long, CacheElement>();

        ///// <summary>
        ///// Stores regular object references. The object must not be gc'ed before 
        ///// it has been written to persistent storage.
        ///// </summary>
        static Dictionary<long, IBusinessObject> uncommittedObject = new Dictionary<long, IBusinessObject>();

        /// <summary>
        /// Adds the given obj to the cache. The DBTag element
        /// must be set prior to calling this method.
        /// </summary>
        /// <param name="obj"></param>
        public static void Add(IBusinessObject obj)
        {
            if (obj.DBTag == null) { throw new NullReferenceException("DBTag of obj not set"); }
            uncommittedObject[obj.DBTag.EntityPOID] = obj;
        }


        private static void AddToCommitted(IBusinessObject obj)
        {
            CacheElement wr = new CacheElement(obj);
            committedObjects[obj.DBTag.EntityPOID] = wr;
        }
        
        public static int Count
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
        public static IBusinessObject Get(DBTag id)
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
        public static IBusinessObject Get(long id)
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

        /// <summary>
        /// Will null the regular references to the cached objects.
        /// Try to perform a garbage collection and let the DBTag destructor 
        /// remove irrelevant elements. Subsequently the strong references
        /// are set to point to the WeakReference's targets, to suppress garbage
        /// collection until next commit.
        /// </summary>
        private static void TryGC()
        {
#if DEBUG
            Console.WriteLine("Committed objects contains {0} elements", committedObjects.Count);
#endif 
            foreach (CacheElement ce in committedObjects.Values)
            {
                ce.Original = null;
            }
            GC.Collect();
            foreach (CacheElement ce in committedObjects.Values)
            {
                if (ce.IsAlive)
                {
                    ce.Original = ce.Target;
                }
            }
#if DEBUG
            Console.WriteLine("Committed objects now contains {0} elements", committedObjects.Count);
#endif
        }

        public static void FlushToDB()
        {
#if DEBUG
            Console.WriteLine("DEBUG INFORMATION FROM IBOCache.FlushToDB():");
            Stopwatch stp = new Stopwatch();
            
            stp.Start();
#endif
            CommitUncommitted();
#if DEBUG
            stp.Stop();
            Console.WriteLine("\tCommitUncomitted took: {0}", stp.Elapsed);
            stp.Reset();

            stp.Start();
#endif
            CommitChangedCommitted();
            TryGC();
#if DEBUG
            stp.Stop();
            Console.WriteLine("\tCommitChangedComitted took: {0}", stp.Elapsed);

            stp.Reset();
            stp.Start();
#endif
            MoveCommittedToUncommitted();
#if DEBUG
            stp.Stop();
            Console.WriteLine("\tMoveCommittedToUncommitted took: {0}", stp.Elapsed);

            stp.Reset();
            stp.Start();
#endif
            Configuration.GenDB.CommitChanges();
#if DEBUG
            stp.Stop();
            Console.WriteLine("\tConfiguration.GenDB.CommitChanges took: {0}", stp.Elapsed);
#endif
        }

        private static void CommitUncommitted()
        {
            foreach (IBusinessObject ibo in uncommittedObject.Values)
            {
                DelegateTranslator trans = TypeSystem.GetTranslator(ibo.GetType());
                IEntity e = trans.Translate(ibo);
                Configuration.GenDB.Save (e);
            }
        }

        private static void MoveCommittedToUncommitted()
        {
            foreach (IBusinessObject ibo in uncommittedObject.Values)
            {
                AddToCommitted(ibo);
            }
            uncommittedObject.Clear();
        }

        private static void CommitChangedCommitted()
        {
            foreach (CacheElement ce in committedObjects.Values)
            {
                if (ce.IsDirty)
                {
                    if (ce.IsAlive)
                    {
                        IBusinessObject ibo = ce.Target;
                        DelegateTranslator trans = TypeSystem.GetTranslator(ibo.GetType());
                        IEntity e = trans.Translate(ibo);
                        Configuration.GenDB.Save(e);
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
        internal static void Remove(long id)
        {
            //TODO: Need to make some kind of commit possible here.
            committedObjects.Remove(id);
            uncommittedObject.Remove(id);
        }
    }
}
