using System;
using System.Collections.Generic;
using System.Text;

namespace GenDB
{
    internal class DBTag
    {
        long entityPOID;
        GenericDB connection;

        private DBTag() { /* empty */ }

        public DBTag (GenericDB connection, long entityPOID)
        {
            this.EntityPOID = entityPOID;
            this.connection = connection;
        }

        ~DBTag() 
        {
            connection.Delete(this);
        }

        public long EntityPOID
        {
            get { return entityPOID; }
            set { entityPOID = value; }
        }

    }
}
