using System;
using System.Collections.Generic;
using System.Text;
using GenDB.DB;

namespace GenDB.DB
{
    /// <summary>
    /// Creates a query that will return all unique EntityPOID values
    /// for a given query. (Just a wrapper around the MSJoinFieldWhereCondition)
    /// </summary>
    class MSEntityPOIDListBuilder : MSJoinFieldWhereCondition
    {
        public MSEntityPOIDListBuilder(TypeSystem typeSys)
            : base(typeSys)
        { }


        /// <summary>
        /// Returns a query command, that will return a 
        /// list of distinct EntityPOIDs for the persisted
        /// objects matching the condition given at instantiation
        /// time.
        /// </summary>
        public new string WhereStr
        {
            get { 
                StringBuilder join = new StringBuilder();

                foreach (IProperty p in base.Properties)
                {
                    string pname = "p" + p.PropertyPOID;
                    join.Append (" INNER JOIN PropertyValue ");
                    join.Append (pname);
                    join.Append (" ON ");
                    join.Append (pname);
                    join.Append (".EntityPOID = e.EntityPOID AND ");
                    join.Append (pname);
                    join.Append (".PropertyPOID = ");
                    join.Append (p.PropertyPOID);
                }

                string res = "SELECT DISTINCT e.EntityPOID FROM Entity e " + join.ToString() + " WHERE " + base.WhereStr + ""; 
                return res;
            }
        }
    }
}
