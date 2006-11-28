using System;
using System.Collections.Generic;
using System.Text;

namespace GenDB
{
    public class DBTag
    {
        long entityPOID;
        IBOCache cache;
        
        internal static void AssignDBTagTo(IBusinessObject obj, long id, IBOCache cache)
        {
            DBTag dbtag = new DBTag(cache, id);
            obj.DBTag = dbtag;
        }

        private DBTag() { /* empty */ }

        private DBTag (IBOCache cache, long entityPOID)
        {
            this.EntityPOID = entityPOID;
            this.cache = cache;
        }

        ~DBTag() 
        {
            cache.Remove(this.EntityPOID);
        }

        public long EntityPOID
        {
            get { return entityPOID; }
            set { entityPOID = value; }
        }

    }
}
