using System;
using System.Collections.Generic;
using System.Text;

namespace GenDB
{
    /// <summary>
    /// DBTag objekter bruges til at gemme 
    /// databaseid'et for et objekt. DBTag
    /// elementer skal kun have een reference,
    /// der skal befinde sig som et felt i 
    /// det IBusinessObject, den gemmer id for.
    /// Derved vil DBTag objektet blive garbage-
    /// collected sammen med IBusinessObject objektet.
    /// Destructoren i DBTag sikrer så, at objectcachens
    /// interne reference til IBusinessObject-objektet
    /// fjernes, så hukommelsen kan blive genbrugt.
    /// </summary>
    public sealed class DBTag
    {
        long entityPOID;
        IBOCache cache;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="id"></param>
        /// <param name="cache"></param>
        internal static void AssignDBTagTo(IBusinessObject obj, long id, IBOCache cache)
        {
            DBTag dbtag = new DBTag(cache, id);
            obj.DBTag = dbtag;
            cache.Add(obj);
        }

        private DBTag() { /* empty */ }

        private DBTag (IBOCache cache, long entityPOID)
        {
            this.entityPOID = entityPOID;
            this.cache = cache;
        }

        /// <summary>
        /// Destructoren 
        /// </summary>
        ~DBTag() 
        {
            cache.Remove(this.EntityPOID);
        }

        public long EntityPOID
        {
            get { return entityPOID; }
        }
    }
}
