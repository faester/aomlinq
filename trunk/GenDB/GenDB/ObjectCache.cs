using System;
using System.Collections.Generic;
using System.Text;

namespace GenDB
{
    internal sealed class ObjectCache
    {
        public const long UNDEFINED_ID = -1;
        #region Singleton pattern
        static ObjectCache instance;
    
        static ObjectCache Instance 
        {
            get 
            {
                if (instance == null) { instance = new ObjectCache (); }
                return instance;
            }
        }

        private ObjectCache() { /* empty */ }
        #endregion

        #region fields
        long nextId = 0;

        Dictionary<long, object> id2obj = new Dictionary<long, object>();
        Dictionary<object, long> obj2id = new Dictionary<object, long>();
        #endregion

        public long AddObject(object obj)
        {
            long res = nextId++;
            id2obj[res] = obj;
            obj2id[obj] = res;
            return res;
        }

        internal bool RemoveObject(object obj)
        {
            long removeId;
            if (obj2id.TryGetValue (obj, out removeId))
            {
                id2obj.Remove (removeId);
                obj2id.Remove (obj);
                return true;
            }
            else 
            {
                return false;
            }
        }

        public bool Contains(object obj)
        {
            return obj2id.ContainsKey (obj);
        }

        public bool ContainsId(long id)
        {
            return id2obj.ContainsKey (id);
        }

        public long GetObjectId(object obj)
        {
            long res;
            if (obj2id.TryGetValue (obj, out res)) {
                return res;
            }
            else
            {
                return UNDEFINED_ID;
            }
        }
    }
}
