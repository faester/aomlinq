using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using AOM;


namespace Persistence
{
    public sealed partial class Database
    {

        /// <summary>
        /// Abstract base class of a wrapper around an
        /// sql command for executing stored procedures.
        /// </summary>
        private abstract class StoredProcedureExecuter
        {
            internal SqlCommand cmd = null;

            private StoredProcedureExecuter() { /* empty */ }

            public StoredProcedureExecuter(SqlConnection cnn)
            {
                Init(cnn);
            }

            /// <summary>
            /// Initializes command with the given connection 
            /// and adds appropriate parameters.
            /// </summary>
            /// <param name="cnn"></param>
            internal abstract void Init(SqlConnection cnn);
            public SqlTransaction Transaction { set { cmd.Transaction = value; } }

            /// <summary>
            /// Will - when appropriate - return the POID created 
            /// from the executed stored procedure.
            /// </summary>
            /// <returns></returns>
            internal long Execute()
            {
                object o = cmd.ExecuteScalar();
                return long.Parse(o.ToString());
            }
        }

        /// <summary>
        /// Convenience wrapper for execution of 
        /// stored procedure storeValueExecuter
        /// </summary>
        private sealed class StoreValueExecuter : StoredProcedureExecuter
        {
            public StoreValueExecuter(SqlConnection cnn)
                : base(cnn)
            { }

            internal override void Init(SqlConnection cnn)  

            {
                cmd = new SqlCommand(
                    "exec sp_StoreValue @entityPOID, @propertyPOID, @propertyValue",
                    cnn
                    );
                cmd.Parameters.Add(new SqlParameter("@entityPOID", SqlDbType.Int));
                cmd.Parameters.Add(new SqlParameter("@propertyPOID", SqlDbType.Int));
                cmd.Parameters.Add(new SqlParameter("@propertyVALUE", SqlDbType.VarChar));
            }

            public void Store(Entity e, Property p)
            {
                object value = e.GetPropertyValue(p);
                if (value == null) { value = DBNull.Value; }
                cmd.Parameters[0].Value = e.Id;
                cmd.Parameters[1].Value = p.Id;
                cmd.Parameters[2].Value = value;
                Execute();
            }
        }

        private sealed class StoreEntityTypeExecuter : StoredProcedureExecuter
        {
            public StoreEntityTypeExecuter(SqlConnection cnn)
                : base(cnn)
            { }

            internal override void Init(SqlConnection cnn)
            {
                cmd = new SqlCommand("exec sp_StoreEntityType @name, @entityTypePOID", cnn);
                cmd.Parameters.Add(new SqlParameter("@name", SqlDbType.VarChar, 255));
                cmd.Parameters.Add(new SqlParameter("@entityTypePOID", SqlDbType.Int));
            }

            public void Store(EntityType et)
            {
                if (et.HasUndefinedId) { cmd.Parameters[1].Value = DBNull.Value; }
                else { cmd.Parameters[1].Value = et.Id; }
                cmd.Parameters[0].Value = et.Name;
                et.Id = Execute();
                et.IsPersistent = true;
            }
        }

        private sealed class StorePropertyExecuter : StoredProcedureExecuter
        {
            public StorePropertyExecuter(SqlConnection cnn) : base(cnn) { }

            internal override void Init(SqlConnection cnn)
            {
                cmd = new SqlCommand("exec sp_StoreProperty "
                    + " @name, "
                    + " @propertyPOID, "
                    + " @propertyTypePOID, "
                    + " @entityTypePOID, "
                    + " @defaultValue ",
                    cnn
                    );
                cmd.Parameters.Add(new SqlParameter("@name", SqlDbType.VarChar));
                cmd.Parameters.Add(new SqlParameter("@propertyPOID", SqlDbType.Int));
                cmd.Parameters.Add(new SqlParameter("@propertyTypePOID", SqlDbType.Int));
                cmd.Parameters.Add(new SqlParameter("@entityTypePOID", SqlDbType.Int));
                cmd.Parameters.Add(new SqlParameter("@defaultValue", SqlDbType.VarChar));
            }


            public void Store(Property p, EntityType owner)
            {
                cmd.Parameters[0].Value = p.Name;
                if (p.HasUndefinedId) { cmd.Parameters[1].Value = DBNull.Value; }
                else { cmd.Parameters[1].Value = p.Id; }
                cmd.Parameters[2].Value = p.Type.Id;
                cmd.Parameters[3].Value = owner.Id;
                cmd.Parameters[4].Value = p.DefaultValue;
                if (p.DefaultValue == null) { cmd.Parameters[4].Value = DBNull.Value; }
                p.Id = Execute();
                p.IsPersistent = true;
            }
        }

        private sealed class StoreEntityExecuter : StoredProcedureExecuter
        {
            public StoreEntityExecuter(SqlConnection cnn) : base(cnn) { }

            internal override void Init(SqlConnection cnn)
            {
                cmd = new SqlCommand("exec sp_StoreEntity "
                        + " @entityPOID, "
                        + " @entityTypePOID ",
                        cnn);
                cmd.Parameters.Add(new SqlParameter("@entityPOID", SqlDbType.Int));
                cmd.Parameters.Add(new SqlParameter("@entityTypePOID", SqlDbType.Int));
            }


            public void Store(Entity e, Entity subEntity)
            {
                if (e.HasUndefinedId)
                {
                    cmd.Parameters[0].Value = DBNull.Value;
                }
                else
                {
                    cmd.Parameters[0].Value = e.Id;
                }

                cmd.Parameters[1].Value = e.Type.Id;
                e.Id = Execute();
                e.IsPersistent = true;
            }
        }

        private sealed class StoreInheritanceExecuter : StoredProcedureExecuter
        {
            public StoreInheritanceExecuter(SqlConnection cnn) : base(cnn) { }

            internal override void Init(SqlConnection cnn)
            {
                cmd = new SqlCommand("exec sp_StoreInheritance @superEntityTypePOID, @subEntityTypePOID", cnn);
                cmd.Parameters.Add(new SqlParameter("@superEntityTypePOID", SqlDbType.Int));
                cmd.Parameters.Add(new SqlParameter("@subEntityTypePOID", SqlDbType.Int));
            }

            public void Store(EntityType super, EntityType sub)
            {
                cmd.Parameters[0].Value = super.Id;
                cmd.Parameters[1].Value = sub.Id;
                Execute();
            }
        }

        private sealed class StorePropertyTypeExecuter : StoredProcedureExecuter
        {
            public StorePropertyTypeExecuter(SqlConnection cnn) : base(cnn) { }

            internal override void Init(SqlConnection cnn)
            {
                cmd = new SqlCommand("exec sp_StorePropertyType "
                        + "@name, "
                        + "@propertyTypePOID",
                        cnn);

                cmd.Parameters.Add(new SqlParameter("@name", SqlDbType.VarChar));
                cmd.Parameters.Add(new SqlParameter("@propertyTypePOID", SqlDbType.Int));
            }

            public void Store(ref PropertyType pt)
            {
                if (pt.HasUndefinedId) { cmd.Parameters[1].Value = DBNull.Value; }
                else { cmd.Parameters[1].Value = pt.Id; }
                Console.WriteLine ("StorePropertyTypeExecuter storing: {0}", pt.Name );
                cmd.Parameters[0].Value = pt.Name;
                pt.Id = Execute();
                pt.IsPersistent = true;
            }
        }
    }
}
