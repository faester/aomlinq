using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Query;
using GenDB.DB;

namespace GenDB.DB
{
    /*
     * Der skrives index "batches", hvilket simpelt hen er implementeret ved at 
     * sende meget lange tekststrenge med adskillige SQL-kommandoer til 
     * serveren p� en gang. 
     */
    class MsSql2005DB : IGenericDatabase
    {
        internal static string SqlSanitizeString(string s)
        {
            if (s == null) { return "null"; }
            return s.Replace("'", "''");
        }

        #region CONSTS
        /* ADO.NET opretholder en connection pool. Dette foruds�tter at forbindelser 
         * �bnes og lukkes hver gang de bruges.
         * http://msdn2.microsoft.com/en-us/library/8xx3tyca.aspx
         * 
         * Kan ogs� inds�ttes i using(){} statement
         * 
         * Connection string fungerer i denne forbindelse som n�gle.
         */

        const string TB_ENTITY_NAME = "Entity";
        const string TB_ENTITYTYPE_NAME = "EntityType";
        const string TB_PROPERTYTYPE_NAME = "PropertyType";
        const string TB_PROPERTY_NAME = "Property";
        const string TB_PROPERTYVALUE_NAME = "PropertyValue";
        const string TB_COLLECTION_ELEMENT_NAME = "CollectionElement";
        const string TB_COLLECTION_KEY_NAME = "CollectionKey";

        const bool WHERE_USING_JOINS = true;
        #endregion

        DataContext dataContext;
        int nextETID = 0;
        bool nextIDsInitialized = false;
        public int NextETID
        {
            get
            {
                if (!nextIDsInitialized)
                {
                    InitNextIDs();
                    nextIDsInitialized = true;
                }
                return nextETID++;
            }
        }
        int nextEID = 0;

        public int NextEID
        {
            get
            {
                if (!nextIDsInitialized)
                {
                    InitNextIDs();
                    nextIDsInitialized = true;
                }
                return nextEID++;
            }
        }

        int nextPTID = 0;

        public int NextPTID
        {
            get
            {
                if (!nextIDsInitialized)
                {
                    InitNextIDs();
                    nextIDsInitialized = true;
                }

                return nextPTID++;
            }
        }
        int nextPID = 0;

        public int NextPID
        {
            get
            {
                if (!nextIDsInitialized)
                {
                    InitNextIDs();
                    nextIDsInitialized = true;
                }

                return nextPID++;
            }
        }


        internal MsSql2005DB(DataContext dataContext)
        {
            this.dataContext = dataContext;
        }

        #region fields
        StringBuilder sbEntityTypeInserts = new StringBuilder(); // "Batching" queries as appended strings.
        StringBuilder sbPropertyTypeInserts = new StringBuilder(); // Stored in different stringbuilders to 
        StringBuilder sbPropertyInserts = new StringBuilder();  // ensure ordered inserts. (One migth 
        StringBuilder sbEntityInserts = new StringBuilder();  // actually suffice.)
        StringBuilder sbPropertyValueInserts = new StringBuilder();
        StringBuilder sbCollectionElementOperations = new StringBuilder();
        StringBuilder sbSetKeyInserts = new StringBuilder();


        LinkedList<string> llEntityInserts = new LinkedList<string>();
        LinkedList<string> llPropertyValueInserts = new LinkedList<string>();
        LinkedList<string> llCollectionElementInserts = new LinkedList<string>();
        LinkedList<string> llSetKeyInserts = new LinkedList<string>();

        int entityInsertCount = 0;
        int propertyValueInsertCount = 0;
        int collectionElementOperationCount = 0;
        int collectionKeyOperationCount = 0;

        //LinkedList<IEntityType> dirtyEntityTypes = new LinkedList<IEntityType>();
        //LinkedList<IEntity> dirtyEntities = new LinkedList<IEntity>();
        //LinkedList<IPropertyType> dirtyPropertyTypes = new LinkedList <IPropertyType>();
        //LinkedList<IProperty> dirtyProperties = new LinkedList<IProperty>();
        #endregion

        #region DB logic
        /// <summary>
        /// Creates the database. Throws Exception if db already exists.
        /// </summary>
        public void CreateDatabase()
        {
            Console.WriteLine("Creating database.");
            using (SqlConnection cnn = new SqlConnection(dataContext.ConnectStringWithoutDBName))
            {
                cnn.Open();
                try
                {
                    SqlCommand cmd = new SqlCommand("CREATE DATABASE "
                        + dataContext.DatabaseName
                        + " COLLATE Danish_Norwegian_CS_AS"
                        , cnn);
                    cmd.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    throw new Exception("Error creating DB. See inner exception for details.", ex);
                }
            }

            using (SqlConnection cnn = new SqlConnection(dataContext.ConnectStringWithDBName))
            {
                cnn.Open();
                SqlTransaction transaction = null;
                try
                {
                    transaction = cnn.BeginTransaction();
                    CreateTables(cnn, transaction);
                    CreateIndexes(cnn, transaction);
                    CreateSProcs(cnn, transaction);
                    CreateFunctions(cnn, transaction);
                    transaction.Commit();
                }
                catch (SqlException ex)
                {
                    transaction.Rollback();
                    try
                    {
                        SqlCommand dc = new SqlCommand("DROP DATABASE " + dataContext.DatabaseName, cnn);
                        dc.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    throw new Exception("Error creating DB (could not create tables). See inner exception for details.", ex);
                }
            }
        }

        /// <summary>
        /// Checks if the database exists.
        /// </summary>
        /// <returns></returns>
        public bool DatabaseExists()
        {
            Console.WriteLine("Checking if database exists.");
            using (SqlConnection cnn = new SqlConnection(dataContext.ConnectStringWithoutDBName))
            {
                cnn.Open();

                SqlCommand cmd = new SqlCommand("USE " + dataContext.DatabaseName, cnn);
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (SqlException)
                {
                    return false;
                }
                finally
                {
                    cnn.Close();
                }
                return true;
            }
        }

        /// <summary>
        /// Deletes database.
        /// </summary>
        public void DeleteDatabase()
        {
            using (SqlConnection cnn = new SqlConnection(dataContext.ConnectStringWithoutDBName))
            {
                cnn.Open();
                SqlCommand cmd = new SqlCommand("DROP DATABASE " + dataContext.DatabaseName, cnn);
                cmd.ExecuteNonQuery();

                cnn.Close();
            }
        }
        #endregion

        public IEntityType NewEntityType()
        {
            IEntityType res = new EntityType(NextETID);
            return res;
        }

        public IEntity NewEntity()
        {
            IEntity res = new Entity();
            res.EntityPOID = NextEID;
            return res;
        }

        public IEnumerable<IEntityType> GetAllEntityTypes()
        {
            Dictionary<int, IEntityType> entityTypes = RawEntityTypes();
            Dictionary<int, IPropertyType> propertyTypes = RawPropertyTypes();
            Dictionary<int, IProperty> properties = RawProperties(propertyTypes, entityTypes);
            foreach (IProperty property in properties.Values)
            {
                property.EntityType.AddProperty(property);
            }
            foreach (IEntityType et in entityTypes.Values)
            {
                yield return et;
            }
        }

        public IEnumerable<IPropertyType> GetAllPropertyTypes()
        {
            LinkedList<IPropertyType> res = new LinkedList<IPropertyType>();

            using (SqlConnection cnn = new SqlConnection(dataContext.ConnectStringWithDBName))
            {
                cnn.Open();
                SqlCommand cmd = new SqlCommand("SELECT Name, PropertyTypePOID, MappingType FROM " + TB_PROPERTYTYPE_NAME, cnn);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    IPropertyType tmp = new PropertyType();
                    string name = (string)reader[0];
                    short mapping = (short)reader[2];
                    MappingType mpt = (MappingType)Enum.ToObject(typeof(MappingType), mapping);
                    tmp.MappingType = mpt;
                    int ptid = int.Parse(reader[1].ToString());
                    tmp.PropertyTypePOID = ptid;
                    tmp.Name = name;
                    tmp.ExistsInDatabase = true;
                    res.AddLast(tmp);
                }
            }
            return res;
        }

        private Dictionary<int, IPropertyType> RawPropertyTypes()
        {
            Dictionary<int, IPropertyType> res = new Dictionary<int, IPropertyType>();
            res = GetAllPropertyTypes().ToDictionary((IPropertyType p) => p.PropertyTypePOID);
            return res;
        }

        private Dictionary<int, IProperty> RawProperties(
            IDictionary<int, IPropertyType> propertyTypes,
            IDictionary<int, IEntityType> entityTypes
            )
        {
            Dictionary<int, IProperty> res = new Dictionary<int, IProperty>();
            using (SqlConnection cnn = new SqlConnection(dataContext.ConnectStringWithDBName))
            {
                cnn.Open();
                SqlCommand cmd = new SqlCommand("SELECT PropertyName, PropertyPOID, PropertyTypePOID, EntityTypePOID FROM " + TB_PROPERTY_NAME, cnn);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    IProperty tmp = new Property();
                    int pid = int.Parse(reader[1].ToString());
                    int tid = int.Parse(reader[2].ToString());
                    int etid = int.Parse(reader[3].ToString());
                    string name = reader[0].ToString();
                    tmp.PropertyPOID = pid;
                    tmp.PropertyName = name;
                    tmp.EntityType = entityTypes[etid];
                    tmp.PropertyType = propertyTypes[tid];
                    tmp.ExistsInDatabase = true;
                    res.Add(pid, tmp);
                }
            }
            return res;
        }

        /// <summary>
        /// EntityTypes without Properties and PropertyTypes
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, IEntityType> RawEntityTypes()
        {
            Dictionary<int, IEntityType> res = new Dictionary<int, IEntityType>();
            using (SqlConnection cnn = new SqlConnection(dataContext.ConnectStringWithDBName))
            {
                cnn.Open();
                SqlCommand cmd = new SqlCommand(
                    "SELECT EntityTypePOID, Name, AssemblyDescription, IsList, IsDictionary FROM " + TB_ENTITYTYPE_NAME
                    , cnn);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string t1 = reader[0].ToString();
                    string t2 = reader[1].ToString();
                    int id = int.Parse(reader[0].ToString());

                    Boolean isList = (Boolean)reader[3];
                    Boolean isDictionary = (Boolean)reader[4];

                    string name = (string)reader[1];
                    IEntityType et = new EntityType(id);
                    et.IsList = isList;
                    et.IsDictionary = isDictionary;
                    et.Name = name;
                    et.AssemblyDescription = (string)reader[2];
                    et.ExistsInDatabase = true;
                    res.Add(id, et);
                }
                reader.Close();
                cmd.CommandText = "SELECT EntityTypePOID, SuperEntityTypePOID FROM " + TB_ENTITYTYPE_NAME;
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    int id = int.Parse(reader[0].ToString());
                    if (reader[1] != DBNull.Value)
                    {
                        int superId = int.Parse(reader[1].ToString());
                        res[id].SuperEntityType = res[superId];
                    }
                }
            }

            return res;
        }

        public IPropertyType NewPropertyType()
        {
            IPropertyType res = new PropertyType();
            res.PropertyTypePOID = NextPTID;
            return res;
        }

        public IProperty NewProperty()
        {
            Property res =  new Property();
            res.PropertyPOID = NextPID;
            return res;
        }


        public IBusinessObject GetByEntityPOID(int entityPOID)
        {
            int count = 0;
            IBusinessObject res = Get(entityPOID);
            return res;
        }

        private IBusinessObject Get(int entityPOID)
        {
            using (SqlConnection cnn = new SqlConnection(dataContext.ConnectStringWithDBName))
            {
                cnn.Open();

                SqlCommand cmd = new SqlCommand(
                    "SELECT " +
                    "    e.EntityTypePOID, " + // 0
                    "    PropertyPOID, " + // 1
                    "    LongValue, " + // 2
                    "    BoolValue, " + // 3x
                    "    StringValue, " + // 4
                    "    DoubleValue, " + // 5
                    "    e.EntityPOID, " + // 6
                    "    ReferenceValue " + // 7
                    " FROM Entity e LEFT JOIN PropertyValue pv ON e.EntityPOID = pv.EntityPOID" +
                    " WHERE e.EntityPOID = " + entityPOID +
                    " ORDER BY e.EntityPOID"
                    );
#if DEBUG
                Console.WriteLine("WHEREBUILDER CONSTRUCTED: " + whereStr);
                Console.WriteLine();
                Console.WriteLine(cmd.CommandText);
#endif

                cmd.Connection = cnn;
                cmd.CommandTimeout = dataContext.CommandTimeout;
                SqlDataReader reader = cmd.ExecuteReader();
                IEntityType iet = null;
                IIBoToEntityTranslator translator = null;
                IBusinessObject result = null;
                int propertyPOID = 0;
                int entityTypePOID = 0;
                bool firstPass = true;

                while (reader.Read())
                {
                    if (firstPass)
                    {
                        entityPOID = reader.GetInt32(6);
                        entityTypePOID = reader.GetInt32(0);
                        if (dataContext.IBOCache.TryGet(entityPOID, out result))
                        {
                            return result;
                        }
                        else
                        {
                            translator = DataContext.Instance.Translators.GetTranslator(entityTypePOID);
                            iet = DataContext.Instance.TypeSystem.GetEntityType(entityTypePOID);

                            result = translator.CreateInstanceOfIBusinessObject();
                            result.DBIdentity = new DBIdentifier(entityPOID, true);
                            this.dataContext.IBOCache.AddFromDB(result);
                        }
                    }

                    if (reader[1] != DBNull.Value) // Does any properties exist?
                    {
                        propertyPOID = reader.GetInt32(1);
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
                            case MappingType.STRING: value = reader.GetString(4); break;
                            default: throw new Exception("Could not translate the property value.");
                        } // switch
                        translator.SetProperty(propertyPOID, result, value);
                    } // if
                    firstPass = false;
                } // while

                return result;

                if (!reader.IsClosed) { reader.Close(); }

            }
        }

        public int Count(IWhereable expression)
        {
            MSWhereStringBuilder mswsb = new MSWhereStringBuilder(dataContext.TypeSystem);
            mswsb.Visit(expression);
            string whereStr = mswsb.WhereStr;
#if DEBUG
            Console.WriteLine("DB.Count with wherestring: " + whereStr);
#endif
            int res = 0;

            using (SqlConnection cnn = new SqlConnection(dataContext.ConnectStringWithDBName))
            {
                cnn.Open();

                SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Entity WHERE EntityPOID IN (" + whereStr + ")", cnn);
                res = (int)cmd.ExecuteScalar();
            }

            return res;
        }

        public bool ClearWhere(IWhereable expression)
        {
            MSWhereStringBuilder mswsb = new MSWhereStringBuilder(dataContext.TypeSystem);
            mswsb.Visit(expression);

            bool willRemove = this.Count(expression) > 0;

            if (entityInsertCount >= dataContext.DbBatchSize) { EntityInsertStringBuilderToLL(); }
            entityInsertCount++;
            string deleteString = " DELETE FROM Entity WHERE EntityPOID IN (" + mswsb.WhereStr + ") ";

            sbEntityInserts.Append(deleteString);

            return willRemove;
        }


        public IEnumerable<IBusinessObject> Where(IExpression expression)
        {
            if (WHERE_USING_JOINS)
            {
                return Where_JoiningFields(expression);
            }
            else
            {
                MSWhereStringBuilder mswsb = new MSWhereStringBuilder(dataContext.TypeSystem);
                mswsb.Reset();
                mswsb.Visit(expression);
                string whereStr = mswsb.WhereStr;
                return Where(whereStr);
            }
        }

        private IEnumerable<IBusinessObject> Where_JoiningFields(IExpression whereCondition)
        {
            MSWhereStringBuilder mswsb = new MSWhereStringBuilder(dataContext.TypeSystem);
            mswsb.Visit(whereCondition);
            string conditionWhereString = mswsb.WhereStr;
            IEnumerable<IEntityType> entityTypes = mswsb.EntityTypes;
            SqlConnection cnn = new SqlConnection(dataContext.ConnectStringWithDBName);
            cnn.Open();
            foreach (IEntityType et in entityTypes)
            {
                LinkedList<IProperty> properties = new LinkedList<IProperty>();
                IIBoToEntityTranslator translator = dataContext.Translators.GetTranslator(et.EntityTypePOID);
                StringBuilder selectPart = new StringBuilder("SELECT e.EntityTypePOID, e.EntityPOID ");
                StringBuilder joinPart = new StringBuilder(" FROM (")
                                .Append(conditionWhereString)
                                .Append(") ew INNER JOIN Entity e ");
                joinPart.Append(" ON ew.EntityPOID = e.EntityPOID ");
                foreach (IProperty p in et.GetAllProperties)
                {
                    int propertyID = p.PropertyPOID;
                    properties.AddLast(p);
                    string pAlias = "p" + propertyID;
                    string select = pAlias;
                    switch (p.MappingType)
                    {
                        case MappingType.BOOL:
                            select += ".BoolValue"; break;
                        case MappingType.DATETIME:
                        case MappingType.LONG:
                            select += ".LongValue"; break;
                        case MappingType.DOUBLE:
                            select += ".DoubleValue"; break;
                        case MappingType.REFERENCE:
                            select += ".ReferenceValue"; break;
                        case MappingType.STRING:
                            select += ".StringValue"; break;
                        default:
                            throw new Exception("Don't know how to handle mappingtype: " + p.MappingType);

                    }
                    selectPart.Append(", ");
                    selectPart.Append(select);
                    joinPart.Append(" INNER JOIN PropertyValue ");
                    joinPart.Append(pAlias);
                    joinPart.Append(" ON e.EntityPOID = ");
                    joinPart.Append(pAlias);
                    joinPart.Append(".EntityPOID AND ");
                    joinPart.Append(pAlias);
                    joinPart.Append(".PropertyPOID = ");
                    joinPart.Append(propertyID);
                }
                joinPart.Append(" \nWHERE e.EntityTypePOID = " + et.EntityTypePOID);
                //joinPart.Append (" OPTION (LOOP JOIN) ");
                string sqlStr = selectPart.ToString() + joinPart.ToString();
#if DEBUG
                Console.Error.WriteLine("****");
                Console.Error.WriteLine(sqlStr);
                Console.Error.WriteLine(" -- -- -- -- -- ");
                Console.Error.WriteLine(conditionWhereString);
#endif
                SqlCommand cmd = new SqlCommand(sqlStr, cnn);
                cmd.CommandTimeout = dataContext.CommandTimeout;
                SqlDataReader reader = cmd.ExecuteReader();
                
                while (reader.Read())
                {
                    int entityPOID = reader.GetInt32(1);
                    IBusinessObject res = null;

                    if (!dataContext.IBOCache.TryGet(entityPOID, out res))
                    {
                        res = translator.CreateInstanceOfIBusinessObject();
                        res.DBIdentity = new DBIdentifier(entityPOID, true);
                        this.dataContext.IBOCache.AddFromDB(res);

                        int idx = 1;
                        foreach (IProperty prop in properties)
                        {
                            idx++;
                            int pid = prop.PropertyPOID;
                            switch (prop.MappingType)
                            {
                                case MappingType.BOOL:
                                    translator.SetProperty(pid, res, reader.GetBoolean(idx)); break;
                                case MappingType.DATETIME:
                                    translator.SetProperty(pid, res, new DateTime(reader.GetInt64(idx))); break;
                                case MappingType.DOUBLE:
                                    translator.SetProperty(pid, res, reader.GetDouble(idx)); break;
                                case MappingType.LONG:
                                    translator.SetProperty(pid, res, reader.GetInt64(idx)); break;
                                case MappingType.REFERENCE:
                                    {
                                        if (reader[idx] == DBNull.Value)
                                        {
                                            translator.SetProperty(pid, res, null); break;
                                        }
                                        else
                                        {
                                            translator.SetProperty(pid, res, reader.GetInt32(idx)); break;
                                        }
                                    }
                                    break;
                                case MappingType.STRING:
                                    if (reader[idx] == DBNull.Value)
                                    {
                                        translator.SetProperty(pid, res, null); break;
                                    }
                                    else
                                    {
                                        translator.SetProperty(pid, res, reader.GetString(idx)); break;
                                    }
                                default:
                                    throw new Exception("Mapping type not implemented. " + prop.MappingType);
                            } // switch
                        } // foreach
                    } // if (res == null)
                    yield return res;
                } // while reader.Read
            } // foreach EntityType

            cnn.Close();
        }

        private IEnumerable<IBusinessObject> Where(string entityPoidListQuery)
        {
            using (SqlConnection cnn = new SqlConnection(dataContext.ConnectStringWithDBName))
            {
                cnn.Open();

                SqlCommand cmd = new SqlCommand(
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
                    , cnn);
#if DEBUG
                Console.WriteLine("WHEREBUILDER CONSTRUCTED: " + entityPoidListQuery);
                Console.WriteLine();
                Console.WriteLine(cmd.CommandText);
#endif

                cmd.Connection = cnn;
                cmd.CommandTimeout = dataContext.CommandTimeout;
                SqlDataReader reader = cmd.ExecuteReader();
                IEntityType iet = null;
                IIBoToEntityTranslator translator = null;
                IBusinessObject result = null;
                int propertyPOID = 0;
                int entityTypePOID = 0;
                int oldEntityTypePOID = entityTypePOID + 1; // Must be different
                int entityPOID = 0;
                int oldEntityPOID = entityPOID + 1; // Must be different
                bool firstPass = true;
                bool returnCachedCopy = false;

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
                            yield return result;
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
                            case MappingType.STRING: value = reader.GetString(4); break;
                            default: throw new Exception("Could not translate the property value.");
                        } // switch
                        translator.SetProperty(propertyPOID, result, value);
                    } // if
                    firstPass = false;
                } // while

                if (!reader.IsClosed) { reader.Close(); }

                if (result != null)
                {
                    yield return result;
                }
            }
        }


        public IEnumerable<IGenCollectionElement> AllElements(int collectionEntityPOID)
        {
            /*
             * The collection is generic, so all elements must 
             * share the same MappingType. 
             * Start by finding that:
             */
            IBusinessObject ibo = GetByEntityPOID(collectionEntityPOID);
            // TODO: Check below should be superfluous. Kept for debugging db consistency.
            if (ibo == null) { throw new Exception("Internal error in database. Request for unknown collection's elements! " + collectionEntityPOID); }

            Type elementType = ibo.GetType().GetGenericArguments()[0];

            // Find mapping type for the elements.
            MappingType mapping = dataContext.TypeSystem.FindMappingType(elementType);

            LinkedList<IGenCollectionElement> res = new LinkedList<IGenCollectionElement>();

            using (SqlConnection cnn = new SqlConnection(dataContext.ConnectStringWithDBName))
            {
                cnn.Open();
                string sqlStr = "SELECT ElementID, LongValue, BoolValue, StringValue, DoubleValue FROM " + TB_COLLECTION_ELEMENT_NAME + " WHERE EntityPOID = " + collectionEntityPOID.ToString();
                SqlCommand cmd = new SqlCommand(sqlStr, cnn);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    IGenCollectionElement element = new GenCollectionElement();
                    element.ElementIndex = (int)reader[0];
                    switch (mapping)
                    {
                        case MappingType.BOOL:
                            element.BoolValue = (bool)reader[2];
                            break;
                        case MappingType.DATETIME:
                            element.DateTimeValue = new DateTime((long)reader[1]);
                            break;
                        case MappingType.DOUBLE:
                            element.DoubleValue = (double)reader[4];
                            break;
                        case MappingType.LONG:
                            element.LongValue = (long)reader[1];
                            break;
                        case MappingType.REFERENCE:
                            {
                                if (reader[1] == DBNull.Value)
                                {
                                    element.RefValue = new IBOReference(true);
                                }
                                else
                                {
                                    element.RefValue = new IBOReference((int)(long)reader[1]);
                                }
                            }
                            break;
                        case MappingType.STRING:
                            element.StringValue = reader[3].ToString();
                            break;
                        default: throw new Exception("Can not fetch collection elements of type " + mapping.ToString());
                    }

                    res.AddLast(element);
                }
            }

            return res;
        }

        /// <summary>
        /// NOT IMPLEMENTED. (But no exception thrown to allow debugging)
        /// </summary>
        /// <param name="entityType"></param>
        public void Save(IEntityType entityType)
        {
            InternalSaveEntityType(entityType);
        }

        public void Save(IEntity entity)
        {
            InternalEntitySave(entity);
            foreach (IPropertyValue pv in entity.AllPropertyValues)
            {
                SavePropertyValue(pv);
            }
        }

        public void Save(IGenCollectionElement ce, int collectionEntityPOID, MappingType mt)
        {
            StringBuilder sb = new StringBuilder(" exec sp_SET_COLLECTION_ELEMENT ");
            sb.Append(collectionEntityPOID)
            .Append(',')
            .Append(ce.ElementIndex)
            .Append(',');

            switch (mt)
            {
                case MappingType.LONG:
                    sb.Append(ce.LongValue);
                    break;
                case MappingType.REFERENCE:
                    if (ce.RefValue.IsNullReference)
                    {
                        sb.Append("null");
                    }
                    else
                    {
                        sb.Append(ce.RefValue.EntityPOID);
                    }
                    break;
                case MappingType.DATETIME:
                    sb.Append(ce.DateTimeValue.Ticks);
                    break;
                default:
                    sb.Append("null");
                    break;
            }
            if (ce.StringValue == null)
            {
                sb.Append(",null,");
            }
            else
            {
                sb.Append(",'")
                .Append(SqlSanitizeString(ce.StringValue))
                .Append("',");
            }
            sb.Append(ce.BoolValue)
            .Append(',')
            .Append(ce.DoubleValue);

            collectionElementOperationCount++;
            sbCollectionElementOperations.Append(sb.ToString());
        }

        private void InternalEntitySave(IEntity entity)
        {
            if (entityInsertCount > dataContext.DbBatchSize)
            {
                EntityInsertStringBuilderToLL();
            }
            entityInsertCount++;
            sbEntityInserts.Append(" EXEC sp_UP_INS_ENTITY ")
                      .Append(entity.EntityPOID)
                      .Append(',')
                      .Append(entity.EntityType.EntityTypePOID)
                      .Append(';');
        }

        private void SavePropertyValue(IPropertyValue pv)
        {
            if (propertyValueInsertCount > dataContext.DbBatchSize)
            {
                PropertyValueStringBuilderToLL();
            }
            propertyValueInsertCount++;
            long longValue = 0; // DateTimes are stored as ticks to avoid problems with limited date span in SQL-server
            bool longValueIsNull = false;
            switch (pv.Property.MappingType)
            {
                case MappingType.DATETIME:
                    longValue = pv.DateTimeValue.Ticks;
                    break;
                case MappingType.LONG:
                    longValue = pv.LongValue;
                    break;
                default:
                    longValueIsNull = true;
                    break;
            }

            string stringValue = pv.StringValue != null ? pv.StringValue = SqlSanitizeString(pv.StringValue) : null;
            int boolValue = pv.BoolValue ? 1 : 0;

            sbPropertyValueInserts.Append(" EXEC sp_SET_PROPERTYVALUE ")
                       .Append(pv.Entity.EntityPOID)
                       .Append(',')
                       .Append(pv.Property.PropertyPOID)
                       .Append(',');

            if (longValueIsNull)
            {
                sbPropertyValueInserts.Append(" null ");
            }
            else
            {
                sbPropertyValueInserts.Append(longValue);
            }
            sbPropertyValueInserts.Append(',');
            if (stringValue == null)
            {
                sbPropertyValueInserts.Append(" null ");
            }
            else
            {
                sbPropertyValueInserts.Append('\'');
                sbPropertyValueInserts.Append(SqlSanitizeString(stringValue));
                sbPropertyValueInserts.Append('\'');
            }
            sbPropertyValueInserts.Append(',')
                       .Append(boolValue)
                       .Append(',')
                       .Append(pv.DoubleValue.ToString().Replace(',', '.')) // ',' -> '.' to solve localization issues.
                       .Append(',')
                       .Append(pv.RefValue.IsNullReference ? " null " : pv.RefValue.EntityPOID.ToString())
                       .Append(";");
        }

        /// <summary>
        /// Used to avoid to many inserts in one batch. (Too many will cause connection timeout.)
        /// </summary>
        private void EntityInsertStringBuilderToLL()
        {
            llEntityInserts.AddLast(sbEntityInserts.ToString());
            sbEntityInserts = new StringBuilder();
            entityInsertCount = 0;
        }

        /// <summary>
        /// Used to avoid to many inserts in one batch. (Too many will cause connection timeout.)
        /// </summary>
        private void PropertyValueStringBuilderToLL()
        {
            llPropertyValueInserts.AddLast(sbPropertyValueInserts.ToString());
            sbPropertyValueInserts = new StringBuilder();
            propertyValueInsertCount = 0;
        }

        private void CollectionElementStringBuilderToLL()
        {
            llCollectionElementInserts.AddLast(sbCollectionElementOperations.ToString());
            sbCollectionElementOperations = new StringBuilder();
            collectionElementOperationCount = 0;
        }

        private void CollectionKeyStringBuilderToLL()
        {
            llSetKeyInserts.AddLast(sbSetKeyInserts.ToString());
            sbSetKeyInserts = new StringBuilder();
            collectionKeyOperationCount = 0;
        }

        public void CommitChanges()
        {
            CommitTypeChanges();
            CommitValueChanges();
            CommitCollections();
        }

        public void CommitTypeChanges()
        {
            SqlCommand cmd = new SqlCommand();
            using (SqlConnection cnn =  new SqlConnection(dataContext.ConnectStringWithDBName))
            {
                cnn.Open();
                SqlTransaction transaction = cnn.BeginTransaction();
                try
                {
                    cmd.Connection = cnn;
                    cmd.Transaction = transaction;
                    if (sbEntityTypeInserts.Length != 0)
                    {
                        cmd.CommandText = sbEntityTypeInserts.ToString();
                        cmd.ExecuteNonQuery();
                    }
                    if (sbPropertyTypeInserts.Length != 0)
                    {
                        cmd.CommandText = sbPropertyTypeInserts.ToString();
                        cmd.ExecuteNonQuery();
                    }
                    if (sbPropertyInserts.Length != 0)
                    {
                        cmd.CommandText = sbPropertyInserts.ToString();
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (SqlException e)
                {
                    transaction.Rollback();
                    throw e;
                }
                transaction.Commit();
            }
            ClearInsertStringBuilders();
        }

        public void RollbackTransaction()
        {
            ClearValueInsertStringBuilders();
            ClearCollectionCommands();
        }

        public void RollbackTypeTransaction()
        {
            ClearInsertStringBuilders();
        }

        public void ClearCollection(int collectionEntityPOID)
        {
            ClearCollectionElements(collectionEntityPOID);
            ClearCollectionKeys(collectionEntityPOID);
        }

        #region Private methods.
        private void CommitValueChanges()
        {
            PropertyValueStringBuilderToLL();
            EntityInsertStringBuilderToLL();

            SqlCommand cmd = new SqlCommand();

            using (SqlConnection cnn =  new SqlConnection(dataContext.ConnectStringWithDBName))
            {
                cnn.Open();
                cmd.Connection = cnn;
                SqlTransaction transaction = cnn.BeginTransaction();
                cmd.Transaction = transaction;
                cmd.CommandTimeout = dataContext.CommandTimeout;
                try
                {
                    foreach (string insertCommand in llEntityInserts)
                    {
                        if (insertCommand != "")
                        {
                            cmd.CommandText = insertCommand;
                            cmd.ExecuteNonQuery();
                        }
                    }
                    foreach (string insertCommand in llPropertyValueInserts)
                    {
                        if (insertCommand != "")
                        {
                            cmd.CommandText = insertCommand;
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (SqlException e)
                {
                    throw e;
                }
                transaction.Commit();
            }

            ClearValueInsertStringBuilders();
        }

        private void CommitCollections()
        {
            if (collectionElementOperationCount > 0) { CollectionElementStringBuilderToLL(); }
            if (collectionKeyOperationCount > 0) { CollectionKeyStringBuilderToLL(); }
            SqlCommand cmd = new SqlCommand();

            using (SqlConnection cnn = new SqlConnection(dataContext.ConnectStringWithDBName))
            {
                cnn.Open();
                cmd.Connection = cnn;
                SqlTransaction transaction = cnn.BeginTransaction();
                cmd.Transaction = transaction;
                cmd.CommandTimeout = dataContext.CommandTimeout;

                try
                {
                    foreach (string insCmd in llSetKeyInserts)
                    {
                        cmd.CommandText = insCmd;
                        cmd.ExecuteNonQuery();
                    }

                    foreach (string insCmd in llCollectionElementInserts)
                    {
                        cmd.CommandText = insCmd;
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (SqlException e)
                {
                    transaction.Rollback();
                    throw e;
                }
                cmd.Transaction.Commit();
            }
            ClearCollectionCommands();
        }

        private void ClearCollectionCommands()
        {
            llCollectionElementInserts = new LinkedList<string>();
            llSetKeyInserts = new LinkedList<string>();
            sbCollectionElementOperations = new StringBuilder();
            sbSetKeyInserts = new StringBuilder();
            collectionElementOperationCount = 0;
            collectionKeyOperationCount = 0;
        }

        private void ClearCollectionElements(int collectionEntityPOID)
        {
            if (collectionElementOperationCount > dataContext.DbBatchSize)
            {
                CollectionElementStringBuilderToLL();
            }
            collectionElementOperationCount++;
            sbCollectionElementOperations.Append(" DELETE FROM ");
            sbCollectionElementOperations.Append(TB_COLLECTION_ELEMENT_NAME);
            sbCollectionElementOperations.Append(" WHERE EntityPOID = ");
            sbCollectionElementOperations.Append(collectionEntityPOID);
        }

        private void ClearCollectionKeys(int collectionEntityPOID)
        {
            if (collectionKeyOperationCount > dataContext.DbBatchSize)
            {
                CollectionKeyStringBuilderToLL();
            }
            collectionKeyOperationCount++;
            sbSetKeyInserts.Append(" DELETE FROM ");
            sbSetKeyInserts.Append(TB_COLLECTION_KEY_NAME);
            sbSetKeyInserts.Append(" WHERE EntityPOID = ");
            sbSetKeyInserts.Append(collectionEntityPOID);
        }

        private void InitNextIDs()
        {
            if (DatabaseExists())
            {
                using (SqlConnection cnn = new SqlConnection(dataContext.ConnectStringWithDBName))
                {
                    cnn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = cnn;

                    cmd.CommandText = "SELECT CASE WHEN Max(EntityTypePOID) is null THEN 0 ELSE Max(EntityTypePOID) + 1 END FROM EntityType";
                    nextETID = int.Parse(cmd.ExecuteScalar().ToString());

                    cmd.CommandText = "SELECT CASE WHEN Max(EntityPOID) is null THEN 1 ELSE Max(EntityPOID) + 1 END FROM Entity";
                    nextEID = int.Parse(cmd.ExecuteScalar().ToString());

                    cmd.CommandText = "SELECT CASE WHEN Max(PropertyPOID) is null THEN 0 ELSE Max(PropertyPOID) + 1 END FROM Property";
                    nextPID = int.Parse(cmd.ExecuteScalar().ToString());

                    cmd.CommandText = "SELECT CASE WHEN Max(PropertyTypePOID) is null THEN 0 ELSE Max(PropertyTypePOID) + 1 END FROM PropertyType";
                    nextPTID = int.Parse(cmd.ExecuteScalar().ToString());

                    cnn.Close();
                }
            }
        }

        private void ClearInsertStringBuilders()
        {
            sbEntityTypeInserts = new StringBuilder();
            sbPropertyTypeInserts = new StringBuilder();
            sbPropertyInserts = new StringBuilder();
        }

        private void ClearValueInsertStringBuilders()
        {
            sbEntityInserts = new StringBuilder();
            sbPropertyValueInserts = new StringBuilder();
            llEntityInserts.Clear();
            llPropertyValueInserts.Clear();
        }

        private void InternalSaveEntityType(IEntityType et)
        {
            if (et == null || et.ExistsInDatabase) { return; }
            et.ExistsInDatabase = true;
            InternalSaveEntityType(et.SuperEntityType);

            sbEntityTypeInserts.Append(" INSERT INTO ");
            sbEntityTypeInserts.Append(TB_ENTITYTYPE_NAME);
            sbEntityTypeInserts.Append(" (EntityTypePOID, Name, SuperEntityTypePOID, AssemblyDescription, IsList, IsDictionary) VALUES (");
            sbEntityTypeInserts.Append(et.EntityTypePOID);
            sbEntityTypeInserts.Append(", '");
            sbEntityTypeInserts.Append(et.Name);
            sbEntityTypeInserts.Append("',");
            if (et.SuperEntityType == null) { sbEntityTypeInserts.Append("null"); }
            else { sbEntityTypeInserts.Append(et.SuperEntityType.EntityTypePOID); }
            sbEntityTypeInserts.Append(",'");
            sbEntityTypeInserts.Append(et.AssemblyDescription);
            sbEntityTypeInserts.Append("',");
            sbEntityTypeInserts.Append(et.IsList ? 1 : 0);
            sbEntityTypeInserts.Append(',');
            sbEntityTypeInserts.Append(et.IsDictionary ? 1 : 0);
            sbEntityTypeInserts.Append(") ");
            if (et.DeclaredProperties != null)
            {
                foreach (IProperty p in et.DeclaredProperties)
                {
                    InternalSaveProperty(p);
                }
            }
        }

        private void InternalSaveProperty(IProperty p)
        {
            InternalSavePropertyType(p.PropertyType);
            sbPropertyInserts.Append("INSERT INTO ");
            sbPropertyInserts.Append(TB_PROPERTY_NAME);
            sbPropertyInserts.Append(" (PropertyPOID, PropertyTypePOID, EntityTypePOID, PropertyName) VALUES (");
            sbPropertyInserts.Append(p.PropertyPOID);
            sbPropertyInserts.Append(',');
            sbPropertyInserts.Append(p.PropertyType.PropertyTypePOID);
            sbPropertyInserts.Append(',');
            sbPropertyInserts.Append(p.EntityType.EntityTypePOID);
            sbPropertyInserts.Append(",'");
            sbPropertyInserts.Append(p.PropertyName);
            sbPropertyInserts.Append("') ");
        }

        private void InternalSavePropertyType(IPropertyType pt)
        {
            if (pt.ExistsInDatabase) { return; }
            sbPropertyTypeInserts.Append(" INSERT INTO ");
            sbPropertyTypeInserts.Append(TB_PROPERTYTYPE_NAME);
            sbPropertyTypeInserts.Append(" (PropertyTypePOID, Name, MappingType) VALUES (");
            sbPropertyTypeInserts.Append(pt.PropertyTypePOID);
            sbPropertyTypeInserts.Append(",'");
            sbPropertyTypeInserts.Append(pt.Name);
            sbPropertyTypeInserts.Append("',");
            short mt = (short)pt.MappingType;
            sbPropertyTypeInserts.Append(mt);
            sbPropertyTypeInserts.Append(")");
            pt.ExistsInDatabase = true;
        }
        /// <summary>
        /// Creates indexes. (Pending task.)
        /// </summary>
        private void CreateIndexes(SqlConnection cnn, SqlTransaction t)
        {
            LinkedList<string> iCC = new LinkedList<string>(); //Table create commands
            ExecuteNonQueries(iCC, cnn, t);
        }

        private void CreateTables(SqlConnection cnn, SqlTransaction t)
        {
            LinkedList<string> tCC = new LinkedList<string>(); //Table create commands

            tCC.AddLast("CREATE TABLE " + TB_ENTITYTYPE_NAME + " (EntityTypePOID int primary key, SuperEntityTypePOID int references " + TB_ENTITYTYPE_NAME + " (EntityTypePOID) , Name VARCHAR(max), AssemblyDescription VARCHAR(MAX), IsList bit not null, IsDictionary bit not null); ");
            tCC.AddLast("CREATE TABLE " + TB_PROPERTYTYPE_NAME + " (PropertyTypePOID int primary key, Name VARCHAR(max), MappingType smallint); ");
            tCC.AddLast("CREATE TABLE " + TB_ENTITY_NAME + " (EntityPOID int primary key, EntityTypePOID int references " + TB_ENTITYTYPE_NAME + " (EntityTypePOID) ON UPDATE CASCADE); ");
            tCC.AddLast("CREATE TABLE " + TB_PROPERTY_NAME + " (PropertyPOID int primary key, PropertyTypePOID int references " + TB_PROPERTYTYPE_NAME + "(PropertyTypePOID), EntityTypePOID int references " + TB_ENTITYTYPE_NAME + " (EntityTypePOID) on delete cascade on update cascade, PropertyName VARCHAR(max)); ");
            tCC.AddLast("CREATE TABLE "
                + TB_PROPERTYVALUE_NAME + " ( "
                + " PropertyPOID int not null references " + TB_PROPERTY_NAME + " (PropertyPOID) ON DELETE CASCADE, "
                + " EntityPOID int not null references " + TB_ENTITY_NAME + " (EntityPOID) ON DELETE CASCADE, "
                + " LongValue BIGINT, " // Also stores referenceids. Null is in this case empty reference. 
                + " BoolValue BIT, "
                + " StringValue VARCHAR(MAX), "
                + " DoubleValue FLOAT, "
                + " ReferenceValue INT) "
                );
            tCC.AddLast("CREATE TABLE "
                + TB_COLLECTION_ELEMENT_NAME + " ( "
                + " ElementID int not null, "
                + " EntityPOID int not null references " + TB_ENTITY_NAME + " (EntityPOID) ON DELETE CASCADE, "
                + " LongValue BIGINT, " // Also stores referenceids. Null is in this case empty reference. 
                + " BoolValue BIT, "
                + " StringValue VARCHAR(MAX), "
                + " DoubleValue FLOAT) "
                );

            tCC.AddLast("CREATE TABLE "
                + TB_COLLECTION_KEY_NAME + " ( "
                + " KeyID int not null, "
                + " EntityPOID int not null references " + TB_ENTITY_NAME + " (EntityPOID) ON DELETE CASCADE, "
                + " LongValue BIGINT, " // Also stores referenceids. Null is in this case empty reference. 
                + " BoolValue BIT, "
                + " StringValue VARCHAR(MAX), "
                + " DoubleValue FLOAT) "
                );

            tCC.AddLast("ALTER TABLE " + TB_PROPERTYVALUE_NAME + " ADD PRIMARY KEY (PropertyPOID, EntityPOID)");
            tCC.AddLast("ALTER TABLE " + TB_COLLECTION_ELEMENT_NAME + " ADD PRIMARY KEY ( EntityPOID, ElementID)");
            tCC.AddLast("ALTER TABLE " + TB_COLLECTION_KEY_NAME + " ADD PRIMARY KEY ( EntityPOID, KeyID)");
            tCC.AddLast("ALTER TABLE " + TB_COLLECTION_KEY_NAME + " ADD FOREIGN KEY (EntityPOID, KeyID) REFERENCES " + TB_COLLECTION_ELEMENT_NAME + " (EntityPOID, ElementID) ");

            tCC.AddLast(
            "CREATE TRIGGER cascade_delete_references ON " + TB_ENTITY_NAME + " AFTER DELETE AS " +
            "UPDATE PropertyValue SET ReferenceValue = NULL WHERE ReferenceValue IN (SELECT EntityPOID FROM deleted) "
            );

            ExecuteNonQueries(tCC, cnn, t);
        }

        private void CreateSProcs(SqlConnection cnn, SqlTransaction t)
        {
            string sp_UP_INS_ENTITY = "CREATE PROCEDURE sp_UP_INS_ENTITY "
                                      + "  @EntityPOID AS INT, "
                                      + "  @EntityTypePOID AS INT "
                                      + "AS "
                                      + "	IF EXISTS (SELECT * FROM Entity WHERE EntityPOID = @EntityPOID) "
                                      + "BEGIN "
                                      + "   UPDATE Entity SET "
                                      + "      EntityTypePOID = @EntityTypePOID "
                                      + "WHERE "
                                      + "      EntityPOID = @EntityPOID "
                                      + " END ELSE BEGIN "
                                      + "INSERT INTO Entity (EntityPOID, EntityTypePOID) "
                                      + " VALUES (@EntityPOID, @EntityTypePOID)"
                                      + "	END ";

            string sp_SET_PROPERTYVALUE = "CREATE PROCEDURE sp_SET_PROPERTYVALUE" +
                                        "	@EntityPOID AS INT, " +
                                        "	@PropertyPOID AS INT," +
                                        "	@LongValue AS BIGINT," +
                                        "	@StringValue AS VARCHAR(max)," +
                                        "	@BoolValue AS BIT, " +
                                        "   @DoubleValue AS FLOAT, " +
                                        "   @ReferenceValue AS INT " +
                                        " AS " +
                                        "	IF EXISTS (SELECT * FROM PropertyValue WHERE EntityPOID = @EntityPOID AND PropertyPOID = @PropertyPoid) " +
                                        "	BEGIN " +
                                        "		UPDATE PropertyValue SET " +
                                        "			LongValue = @LongValue," +
                                        "			StringValue = @StringValue ," +
                                        "			BoolValue = @BoolValue, " +
                                        "			DoubleValue = @DoubleValue, " +
                                        "			ReferenceValue = @ReferenceValue " +
                                        "		WHERE " +
                                        "			EntityPOID = @EntityPOID AND PropertyPOID = @PropertyPOID" +
                                        "	END " +
                                        "	ELSE" +
                                        "	BEGIN" +
                                        "		INSERT INTO " +
                                        "		PropertyValue (EntityPOID, PropertyPOID, LongValue, StringValue , BoolValue, DoubleValue, ReferenceValue)" +
                                        "		VALUES (@EntityPOID, @PropertyPOID, @LongValue, @StringValue, @BoolValue, @DoubleValue, @ReferenceValue)" +
                                        "	END";

            string sp_SET_COLLECTION_ELEMENT
                                        = "CREATE PROCEDURE sp_SET_COLLECTION_ELEMENT " +
                                        "	@EntityPOID AS INT, " +
                                        "	@ElementID AS INT," +
                                        "	@LongValue AS BIGINT," +
                                        "	@StringValue AS VARCHAR(max)," +
                                        "	@BoolValue AS BIT, " +
                                        "   @DoubleValue AS FLOAT " +
                                        " AS " +
                                        "	IF EXISTS (SELECT * FROM " + TB_COLLECTION_ELEMENT_NAME + " WHERE EntityPOID = @EntityPOID AND ElementID = @ElementID) " +
                                        "	BEGIN " +
                                        "		UPDATE " + TB_COLLECTION_ELEMENT_NAME + " SET " +
                                        "			LongValue = @LongValue," +
                                        "			StringValue = @StringValue ," +
                                        "			BoolValue = @BoolValue, " +
                                        "			DoubleValue = @DoubleValue " +
                                        "		WHERE " +
                                        "			EntityPOID = @EntityPOID AND ElementID = @ElementID" +
                                        "	END " +
                                        "	ELSE" +
                                        "	BEGIN" +
                                        "		INSERT INTO " + TB_COLLECTION_ELEMENT_NAME +
                                        "		(EntityPOID, ElementID, LongValue, StringValue , BoolValue, DoubleValue)" +
                                        "		VALUES (@EntityPOID, @ElementID, @LongValue, @StringValue, @BoolValue, @DoubleValue)" +
                                        "	END";

            string sp_SET_COLLECTION_KEY
                            = "CREATE PROCEDURE sp_SET_COLLECTION_KEY " +
                            "	@EntityPOID AS INT, " +
                            "	@KeyID AS INT," +
                            "	@LongValue AS BIGINT," +
                            "	@StringValue AS VARCHAR(max)," +
                            "	@BoolValue AS BIT, " +
                            "   @DoubleValue AS FLOAT " +
                            " AS " +
                            "	IF EXISTS (SELECT * FROM " + TB_COLLECTION_KEY_NAME + " WHERE EntityPOID = @EntityPOID AND KeyID = @KeyID) " +
                            "	BEGIN " +
                            "		UPDATE " + TB_COLLECTION_KEY_NAME + " SET " +
                            "			LongValue = @LongValue," +
                            "			StringValue = @StringValue ," +
                            "			BoolValue = @BoolValue, " +
                            "			DoubleValue = @DoubleValue " +
                            "		WHERE " +
                            "			EntityPOID = @EntityPOID AND KeyID = @KeyID " +
                            "	END " +
                            "	ELSE" +
                            "	BEGIN" +
                            "		INSERT INTO " + TB_COLLECTION_KEY_NAME +
                            "		(EntityPOID, KeyID, LongValue, StringValue , BoolValue, DoubleValue)" +
                            "		VALUES (@EntityPOID, @KeyID, @LongValue, @StringValue, @BoolValue, @DoubleValue)" +
                            "	END";

            string[] cmds = new string[] { sp_UP_INS_ENTITY, sp_SET_PROPERTYVALUE, sp_SET_COLLECTION_ELEMENT, sp_SET_COLLECTION_KEY };
            ExecuteNonQueries(cmds, cnn, t);
        }

        private void CreateFunctions(SqlConnection cnn, SqlTransaction t)
        {
            string cmdStr =
                " CREATE FUNCTION dbo.fn_lookup_EntityPOID " +
                " (	" +
                "	@bep int, " +
                "	@ls varchar(8000)" +
                " ) " +
                " RETURNS BIGINT" +
                " AS" +
                " BEGIN" +
                "	DECLARE @res AS BIGINT" +
                "	DECLARE @idx AS BIGINT" +
                "	DECLARE @nbep AS BIGINT" +
                "	DECLARE @propertyPOID AS BIGINT" +

                "	SET @idx = CHARINDEX('.', @ls)  " +
                "	SET @propertyPOID = CAST (SUBSTRING(@ls, 1, @idx - 1) AS BIGINT)" +
                "	SELECT @nbep = LongValue FROM PropertyValue WHERE PropertyPOID = @propertyPOID AND EntityPOID = @bep" +
                "	IF @nbep IS NULL " +
                "		SET @res = NULL" +
                "	ELSE" +
                "		IF @ls LIKE '%.%.%' " +
                "		BEGIN" +
                "			DECLARE @nls AS VARCHAR(8000)" +
                "			SET @nls = SUBSTRING(@ls, @idx + 1, LEN(@ls) - (@idx))" +
                "			RETURN dbo.fn_lookup_EntityPOID(@nbep, @nls)" +
                "			SET @res = dbo.fn_lookup_EntityPOID(@nbep, @nls)" +
                "		END" +
                "		ELSE" +
                "		BEGIN" +
                "			SET @res = @nbep" +
                "		END" +
                "		RETURN @res" +
                " END ";

            ExecuteNonQueries(new string[] { cmdStr }, cnn, t);
        }

        /// <summary>
        /// Executes a series of command strings. Must be non queries.
        /// </summary>
        /// <param name="cmdStrings"></param>
        private void ExecuteNonQueries(IEnumerable<string> cmdStrings, SqlConnection cnn, SqlTransaction transaction)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = cnn;
            cmd.Transaction = transaction;
            cmd.CommandTimeout = dataContext.CommandTimeout;

            foreach (string cmdStr in cmdStrings)
            {
                cmd.CommandText = cmdStr;
                cmd.ExecuteNonQuery();
            }
        }
        #endregion
    }

}
