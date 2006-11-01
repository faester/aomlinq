using System;
using System.Collections.Generic;
using System.Text;

namespace Translation
{
    static class ObjectCache
    {
        private static Dictionary<long, object> id2obj;
        private static Dictionary <object, long> obj2id;

        static ObjectCache()
        {
            id2obj = new Dictionary<long, object>();
            obj2id = new Dictionary<object, long>();
        }

        /// <summary>
        /// Look up ID based on OBJECT. Be ware of autoboxing!!!!
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool HasObject (object obj)
        {
            return obj2id.ContainsKey(obj);
        }

        public static bool HasId (long id)
        {
            return id2obj.ContainsKey(id);
        }

        public static object GetObjectByID(long id)
        {
            return id2obj[id];
        }

        public static long GetIDByObject(object obj)
        {
            return obj2id[obj];
        }

        public static void Store(object obj, long id)
        {
            obj2id[obj] = id;
            id2obj[id] = obj;
        }
    }
}
