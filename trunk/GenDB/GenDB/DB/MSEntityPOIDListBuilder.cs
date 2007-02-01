using System;
using System.Collections.Generic;
using System.Text;
using GenDB.DB;

namespace GenDB.DB
{
    class MSEntityPOIDListBuilder : MSJoinFieldWhereCondition
    {
        public MSEntityPOIDListBuilder(TypeSystem typeSys)
            : base(typeSys)
        { }

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
                Console.Out.WriteLine(res);
                return res;
            }
        }
    }
}
