using System;
using System.Collections.Generic;
using System.Text;

namespace GenDB
{
    /// <summary>
    /// DBTag objekter bruges til at gemme databaseid'et for et objekt. DBTag
    /// elementer skal kun have een reference, der skal befinde sig som et felt i 
    /// det IBusinessObject, den gemmer id for. 
    /// Derved vil DBTag objektet blive garbagecollected sammen med IBusinessObject 
    /// instansen. Destructoren i DBTag sikrer så, at objectcachens interne 
    /// reference til IBusinessObject-objektet fjernes, så hukommelsen kan blive 
    /// genbrugt.
    /// </summary>
    public sealed class DBTag
    {
        long entityPOID;
        
        /// <summary>
        /// Assigns DBTag with entityPOID == id to the obj given.
        /// The obj is also added to the cache.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="id"></param>
        internal static void AssignDBTagTo(IBusinessObject obj, long id)
        {
            DBTag dbtag = new DBTag(id);
            obj.DBTag = dbtag;
            IBOCache.Add(obj);
        }

        private DBTag() { /* empty */ }

        private DBTag (long entityPOID)
        {
            this.entityPOID = entityPOID;
        }

        /// <summary>
        /// Destructoren 
        /// </summary>
        ~DBTag() 
        {
            IBOCache.Remove(this.EntityPOID);
        }

        public long EntityPOID
        {
            get { return entityPOID; }
        }
    }
}
