using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using GenDB.DB;
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


    internal class IBOCache
    {
        long retrieved = 0;

        DataContext dataContext;

        public long Retrieved
        {
            get { return retrieved; }
        }

        internal IBOCache(DataContext dataContext)
        {
            this.dataContext = dataContext;
        }

        /// <summary>
        /// Stores objects with weak references to allow garbage collection of 
        /// the cached objects.
        /// </summary>
        Dictionary<long, CacheElement> committedObjects = new Dictionary<long, CacheElement>();

        ///// <summary>
        ///// Stores regular object references. The object must not be gc'ed before 
        ///// it has been written to persistent storage.
        ///// </summary>
        Dictionary<long, IBusinessObject> uncommittedObjects = new Dictionary<long, IBusinessObject>();

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
                if (!uncommittedObjects.TryGetValue(id, out result))
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
        private void TryGC()
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

        public void FlushToDB()
        {
#if DEBUG
            Console.WriteLine("DEBUG INFORMATION FROM IBOCache.FlushToDB():");
            Stopwatch stp = new Stopwatch();
            
            stp.Start();
#endif
            CommitChangedCommitted();
            TryGC();
#if DEBUG
            stp.Stop();
            Console.WriteLine("\tCommitUncomitted took: {0}", stp.Elapsed);
            stp.Reset();

            stp.Start();
#endif
            CommitUncommitted();
#if DEBUG
            stp.Stop();
            Console.WriteLine("\tCommitChangedComitted took: {0}", stp.Elapsed);

            stp.Reset();
            stp.Start();
#endif
            //AddToUncomitted();
#if DEBUG
            stp.Stop();
            Console.WriteLine("\tMoveCommittedToUncommitted took: {0}", stp.Elapsed);

            stp.Reset();
            stp.Start();
#endif
            dataContext.GenDB.CommitChanges();
#if DEBUG
            stp.Stop();
            Console.WriteLine("\tConfiguration.GenDB.CommitChanges took: {0}", stp.Elapsed);
#endif
        }

        private void CommitUncommitted()
        {
            while (uncommittedObjects.Count > 0)
            {
                Dictionary<long, IBusinessObject> tmpUncomitted = uncommittedObjects;

                // Make a new uncomitted collection to allow clrtype2translator to add objects to the cache.
                uncommittedObjects = new Dictionary<long, IBusinessObject>();

                foreach (IBusinessObject ibo in tmpUncomitted.Values)
                {
                    IIBoToEntityTranslator trans = dataContext.Translators.GetTranslator(ibo.GetType());
                    trans.SaveToDB(dataContext.GenDB, ibo);
                    AddToCommitted(ibo);
                }
            }
        }

        private void CommitChangedCommitted()
        {
            foreach (CacheElement ce in committedObjects.Values)
            {
                if (ce.IsDirty)
                {
                    if (ce.IsAlive)
                    {
                        IBusinessObject ibo = ce.Target;
                        IIBoToEntityTranslator trans = dataContext.Translators.GetTranslator(ibo.GetType());
                        trans.SaveToDB(dataContext.GenDB, ibo);
                        //IEntity e = trans.Translate(ibo);
                        //DataContext.GenDB.Save(e);
                        ce.ClearDirtyBit();
                    }
                    else
                    {
                        //TODO: Should never happen, but need proper testing....
                        throw new Exception("Object reclaimed before if was flushed to the DB.");
                    }
                }
            }
        }

        internal void Add(IBusinessObject ibo, long entityPOID)
        {
#if DEBUG
            //TODO: Bør kun testes hvis DEBUG-flag er sat, da ansvaret varetages af oversættere.
            if (ibo.DBTag != null) { throw new Exception("Object has already had its DBTag set."); }
            if (uncommittedObjects.Contains (entityPOID))
            {
                throw new Exception("IBusinessObject already added.");
            }
#endif

            DBTag dbTag = new DBTag(this, entityPOID);
            ibo.DBTag = dbTag;

            uncommittedObjects.Add(entityPOID, ibo);
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
            uncommittedObjects.Remove(id);
        }
    }
}
