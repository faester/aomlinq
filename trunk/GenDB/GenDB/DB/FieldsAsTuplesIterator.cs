using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace GenDB.DB
{
    class OneEntityGetter : IDisposable
    {
        SqlCommand cmd = null; // Parametrized command
        SqlConnection cnn = null; // DB connection. Kept open during objects lifetime
        DataContext dataContext = null;
        bool hasBeenDisposed = false;

        public OneEntityGetter(DataContext dataContext)
        {
            this.dataContext = dataContext;
            Init();
        }

        ~OneEntityGetter()
        {
            if (!hasBeenDisposed)
            {
                Dispose();
            }
        }

        private void Init()
        {
            cnn = dataContext.CreateDBConnection();

            cmd = new SqlCommand(
                "SELECT " +
                "    e.EntityTypePOID, " + // 0
                "    PropertyPOID, " + // 1
                "    LongValue, " + // 2
                "    BoolValue, " + // 3x
                "    StringValue, " + // 4
                "    DoubleValue, " + // 5
                "    ReferenceValue, " + // 6
                "    e.EntityPOID " +
                " FROM Entity e LEFT JOIN  " +
                "      PropertyValue pv ON pv.EntityPOID = e.EntityPOID " +
                " WHERE e.EntityPOID = @entityPOID"
                ,
                cnn);
            cmd.CommandTimeout = dataContext.CommandTimeout;
            
            cmd.Parameters.Add (new SqlParameter("@entityPOID", SqlDbType.Int));
        }


        /// <summary>
        /// Returns object identified by 'entityPOID'
        /// from database. Will not check if a cached copy 
        /// already exists.
        /// </summary>
        /// <param name="entityPOID"></param>
        /// <returns></returns>
        public IBusinessObject GetByEntityPOID(int entityPOID)
        {
            IBusinessObject result = null;

            cmd.Parameters[0].Value = entityPOID;

            cnn.Open();

            SqlDataReader reader = cmd.ExecuteReader();

                bool first = true;
            int entityTypePOID = 0;
            IIBoToEntityTranslator translator = null;
            IEntityType et = null;

            while(reader.Read())
            {
                if (first)
                {
                    first = false;
                    entityTypePOID = reader.GetInt32(0);
                    et = dataContext.TypeSystem.GetEntityType(entityTypePOID);
                    translator = dataContext.Translators.GetTranslator(entityTypePOID);
                    result = translator.CreateInstanceOfIBusinessObject();
                    result.DBIdentity = new DBIdentifier(entityPOID, true);
                     dataContext.IBOCache.AddFromDB(result);
                }

                if (reader[1] != DBNull.Value) // Set values if they exist. (Otherwise the left join ensures null values in related reader fields)
                {
                    int propertyPOID = reader.GetInt32(1);
                    IProperty p = et.GetProperty(propertyPOID);
                    if (p == null) { throw new NullReferenceException("Property"); }

                    object propertyValue = null;

                    switch(p.MappingType)
                    {
                        case MappingType.BOOL: propertyValue = reader.GetBoolean(3); break;
                        case MappingType.DATETIME: propertyValue = new DateTime(reader.GetInt64(2)); break;
                        case MappingType.DOUBLE: propertyValue = reader.GetDouble(5); break;
                        case MappingType.LONG: propertyValue = reader.GetInt64(2); break;
                        case MappingType.REFERENCE:
                            if (reader[6] == DBNull.Value) {
                                propertyValue = null; 
                            } else {
                                reader.GetInt32(6);
                            }
                            break;
                        case MappingType.STRING:
                            propertyValue = reader[4] == DBNull.Value ? null : reader.GetString(4);
                            break;
                    }

                    //PropertyConverter pc = translator.GetPropertyConverter(propertyPOID);
                    //pc.PropertySetter(result, propertyValue);
                    translator.SetProperty(propertyPOID, result, propertyValue);
                }
            }

            reader.Close();
            cnn.Close();

            return result;
        }


        #region IDisposable Members

        public void Dispose()
        {
            hasBeenDisposed = true;
        }

        #endregion
    }

    class FieldsAsTuplesIterator : IEnumerable<IBusinessObject>
    {
        private class TheEnumerator : IEnumerator<IBusinessObject>, IEnumerable<IBusinessObject>
        {
            DataContext dataContext = null;
            string entityPoidListQuery;
            IBusinessObject current;
            SqlConnection cnn = null;
            SqlCommand cmd = null;
            SqlDataReader reader;

            IEntityType iet = null;
            IIBoToEntityTranslator translator = null;
            IBusinessObject result = null;
            int propertyPOID = 0;
            int entityTypePOID = 0;
            int oldEntityTypePOID = 1; // Must be different
            int entityPOID = 0;
            int oldEntityPOID = 1; // Must be different
            bool firstPass = true;
            bool returnCachedCopy = false;
            bool hasReturnedLastElement = false;

            public TheEnumerator(string entityPoidListQuery)
            {
                this.entityPoidListQuery = entityPoidListQuery;
                dataContext = DataContext.Instance;
                cnn = dataContext.CreateDBConnection();
                cnn.Open();
                cmd = new SqlCommand(
                    "SELECT " +
                    "    e.EntityTypePOID, " + // 0
                    "    PropertyPOID, " + // 1
                    "    LongValue, " + // 2
                    "    BoolValue, " + // 3x
                    "    StringValue, " + // 4
                    "    DoubleValue, " + // 5
                    "    e.EntityPOID, " + // 6
                    "    ReferenceValue " + // 7
                    " FROM Entity e INNER JOIN (" +
                   entityPoidListQuery +
                    ") ew ON ew.EntityPOID = e.EntityPOID " +
                    " LEFT JOIN PropertyValue pv ON pv.EntityPOID = e.EntityPOID "
                    + " ORDER BY e.EntityPOID "
                    ,
                    cnn);
                cmd.CommandTimeout = DataContext.Instance.CommandTimeout;
                reader = cmd.ExecuteReader();
            }

            private bool Advance()
            {
                while (reader.Read())
                {
                    entityPOID = reader.GetInt32(6);
                    if (entityPOID != oldEntityPOID || firstPass)
                    {
                        entityTypePOID = reader.GetInt32(0);
                        if (entityTypePOID != oldEntityTypePOID || firstPass)
                        {
                            translator = DataContext.Instance.Translators.GetTranslator(entityTypePOID);
                            iet = DataContext.Instance.TypeSystem.GetEntityType(entityTypePOID);
                            oldEntityTypePOID = entityTypePOID;
                        } // if
                        if (result != null)
                        {
                            current = result;
                            return true;
                        }

                        returnCachedCopy = dataContext.IBOCache.TryGet(entityPOID, out result);
                        if (!returnCachedCopy)
                        {
                            result = translator.CreateInstanceOfIBusinessObject();
                            result.DBIdentity = new DBIdentifier(entityPOID, true);
                            this.dataContext.IBOCache.AddFromDB(result);
                        }

                        oldEntityPOID = entityPOID;
                    } // if
                    if (!returnCachedCopy && reader[1] != DBNull.Value) // Does any properties exist?
                    {
                        propertyPOID = (int)reader[1];
                        object value = null;
                        switch (iet.GetProperty(propertyPOID).MappingType)
                        {
                            case MappingType.BOOL: value = reader.GetBoolean(3); break;
                            case MappingType.DATETIME: value = new DateTime(reader.GetInt64(2)); break;
                            case MappingType.DOUBLE: value = reader.GetDouble(5); break;
                            case MappingType.LONG: value = reader.GetInt64(2); break;
                            case MappingType.REFERENCE:
                                if (reader[7] == DBNull.Value)
                                {
                                    value = null;
                                    break;
                                }
                                else
                                {
                                    value = reader.GetInt32(7);
                                    break;
                                }
                            case MappingType.STRING:
                                if (reader[4] == DBNull.Value)
                                {
                                    value = null;
                                    break;
                                }
                                else
                                {
                                    value = reader.GetString(4); break;
                                    break;
                                }
                            default: throw new Exception("Could not translate the property value.");
                        } // switch
                        translator.SetProperty(propertyPOID, result, value);
                    } // if
                    firstPass = false;
                } // while

                if (result != null)
                {
                    current = result;
                }
                if (hasReturnedLastElement)
                {
                    return false;
                }
                else
                {
                    hasReturnedLastElement = true;
                    return true;
                }
            }

            #region IEnumerator<IBusinessObject> Members

            public IBusinessObject Current
            {
                get { return current; }
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {
                cmd.Cancel();
                if (!reader.IsClosed) { reader.Close(); }
                if (cnn.State != System.Data.ConnectionState.Closed) { cnn.Close(); }
            }

            #endregion

            #region IEnumerator Members

            object System.Collections.IEnumerator.Current
            {
                get { return current; }
            }

            public bool MoveNext()
            {
                return Advance();
            }

            public void Reset()
            {
                throw new Exception("The method or operation is not implemented.");
            }

            #endregion

            #region IEnumerable<IBusinessObject> Members

            IEnumerator<IBusinessObject> IEnumerable<IBusinessObject>.GetEnumerator()
            {
                return this;
            }

            #endregion

            #region IEnumerable Members
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this;
            }

            #endregion
        }

        DataContext dataContext = null;
        MSEntityPOIDListBuilder mswsb = null;

        public FieldsAsTuplesIterator(DataContext dt)
        {
            this.dataContext = dt;
            mswsb = new MSEntityPOIDListBuilder(dt.TypeSystem);
        }


        public FieldsAsTuplesIterator(DataContext dt, IExpression condition)
            : this(dt)
        {
            mswsb.Visit(condition);
        }

        public IExpression Clause
        {
            set
            {
                mswsb.Reset();
                mswsb.Visit(value);
            }
        }

        private IEnumerator<IBusinessObject> CreateEnumerator()
        {
            string entityPOIDLIST = mswsb.WhereStr;
            TheEnumerator e = new TheEnumerator(entityPOIDLIST);
            return e;
        }

        #region IEnumerable<IBusinessObject> Members

        IEnumerator<IBusinessObject> IEnumerable<IBusinessObject>.GetEnumerator()
        {
            return CreateEnumerator();
        }

        #endregion

        #region IEnumerable Members


        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return CreateEnumerator();
        }

        #endregion
    }
}
