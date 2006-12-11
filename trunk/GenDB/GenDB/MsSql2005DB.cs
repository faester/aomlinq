using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Query;

namespace GenDB
{
    /*
     *  PropertyType tabellen kan helt udelades. 
     *  Der vil aldrig v�re andre typer, end dem 
     *  beskrevet i MappingType, og da der er tale 
     *  om et s�t med en endelig st�rrelse vil det 
     *  v�re bedre at g�re det til en enum, der 
     *  kun eksisterer i memory.
     *  
     *  (Den konkrete type af REFERENCE afg�res ved hj�lp af EntityType)
     */
    class MsSql2005DB : IGenericDatabase
    {
        #region static
        //TODO: Nedenst�ende skal instantieres i forhold til db-tilstand. (SELECT MAX(EntityPOID) + 1 FROM...)
        static long nextETID = 0;

        public static long NextETID
        {
            get { return MsSql2005DB.nextETID++; }
        }
        static long nextEID = 0;

        public static long NextEID
        {
            get { return MsSql2005DB.nextEID++; }
        }

        static long nextPTID = 0;

        public static long NextPTID
        {
            get { return MsSql2005DB.nextPTID++; }
        }
        static long nextPID = 0;

        public static long NextPID
        {
            get { return MsSql2005DB.nextPID++; }
        }

        #endregion

        #region CONSTS
        /* ADO.NET opretholder en connection pool. 
         * Dette foruds�tter at forbindelser �bnes 
         * og lukkes hver gang de bruges.
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
        #endregion

        #region Singleton
        static MsSql2005DB instance = new MsSql2005DB();

        public static MsSql2005DB Instance
        {
            get { return new MsSql2005DB(); }
        }
        #endregion

        private MsSql2005DB() { /* empty */ }

        #region fields
        StringBuilder sbETInserts = new StringBuilder(); // "Batching" queries as 
        StringBuilder sbPTInserts = new StringBuilder(); // appended strings.
        StringBuilder sbPInserts = new StringBuilder();  // Stored in different stringbuilders to ensure ordered inserts. (One migth actually suffice.)

        LinkedList<IEntityType> dirtyEntityTypes = new LinkedList<IEntityType>();
        LinkedList<IEntity> dirtyEntities = new LinkedList<IEntity>();
        LinkedList<IPropertyType> dirtyPropertyTypes = new LinkedList <IPropertyType>();
        LinkedList<IProperty> dirtyProperties = new LinkedList<IProperty>();
        #endregion

        #region DB logic
        /// <summary>
        /// Creates the database. Throws Exception if db already exists.
        /// </summary>
        public void CreateDatabase()
        {
            using (SqlConnection cnn = new SqlConnection(Configuration.ConnectStringWithoutDBName))
            {
                cnn.Open();
                SqlCommand cmd = new SqlCommand("CREATE DATABASE " + Configuration.DatabaseName, cnn);
                cmd.ExecuteNonQuery();
            }
            CreateTables();
            CreateIndexes();
            CreateSProcs();
        }

        /// <summary>
        /// Checks if the database exists.
        /// </summary>
        /// <returns></returns>
        public bool DatabaseExists()
        {
            SqlConnection cnn = new SqlConnection(Configuration.ConnectStringWithoutDBName);
            cnn.Open();
            SqlCommand cmd = new SqlCommand("USE " + Configuration.DatabaseName, cnn);
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

        /// <summary>
        /// Deletes database.
        /// </summary>
        public void DeleteDatabase()
        {
            SqlConnection cnn = new SqlConnection(Configuration.ConnectStringWithoutDBName);
            cnn.Open();

            SqlCommand cmd = new SqlCommand("DROP DATABASE " + Configuration.DatabaseName, cnn);
            cmd.ExecuteNonQuery();

            cnn.Close();
        }
        #endregion
        
        public IPropertyValue NewPropertyValue()
        {
            return new MSPropertyValue();
        }

        /// <summary>
        /// Returns a new IEntityType instance with 
        /// correct EntityPOID, name set and no associated
        /// properties.
        /// 
        /// The type is not persisted until it is added to the database.
        /// </summary>
        /// <returns></returns>
        public IEntityType NewEntityType(string name)
        {
            IEntityType res = new MSEntityType();
            res.EntityTypePOID = MsSql2005DB.nextETID++;
            return res;
        }

        public IEntity NewEntity()
        {
            IEntity res = new MSEntity();
            res.EntityPOID = MsSql2005DB.nextEID++;
            return res;
        }

        public IEnumerable<IEntityType> GetAllEntityTypes()
        {
            Dictionary<long, IEntityType> entityTypes = RawEntityTypes();
            Dictionary<long, IPropertyType> propertyTypes = RawPropertyTypes();
            Dictionary<long, IProperty> properties = RawProperties(propertyTypes, entityTypes);
            foreach (IProperty property in properties.Values)
            {
                property.EntityType.AddProperty (property);
            }
            foreach (IEntityType et in entityTypes.Values)
            {
                yield return et;
            }
        }

        public IEnumerable<IPropertyType> GetAllPropertyTypes()
        {
            LinkedList<IPropertyType> res = new LinkedList<IPropertyType>();

            using (SqlConnection cnn = new SqlConnection(Configuration.ConnectStringWithDBName))
            {
                cnn.Open();
                SqlCommand cmd = new SqlCommand ("SELECT Name, PropertyTypePOID, MappingType FROM " + TB_PROPERTYTYPE_NAME, cnn);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read ())
                {
                    IPropertyType tmp = new MSPropertyType();
                    long ptid = (long)(reader[1]);
                    string name = (string)reader[0];
                    MappingType mpt = (MappingType)reader[2];
                    tmp.PropertyTypePOID = ptid;
                    tmp.Name = name;
                    tmp.ExistsInDatabase = true;
                    res.AddLast (tmp);
                }
            }
            return res;
        }

        private Dictionary<long, IPropertyType> RawPropertyTypes()
        {
            Dictionary<long, IPropertyType>  res = new Dictionary<long, IPropertyType>();
            res = GetAllPropertyTypes().ToDictionary((IPropertyType p) => p.PropertyTypePOID);
            return res;
        }

        private Dictionary<long, IProperty> RawProperties(
            IDictionary<long, IPropertyType> propertyTypes,
            IDictionary<long, IEntityType> entityTypes
            )
        {
            Dictionary<long, IProperty>  res = new Dictionary<long, IProperty>();
            using (SqlConnection cnn = new SqlConnection(Configuration.ConnectStringWithDBName))
            {
                cnn.Open();
                SqlCommand cmd = new SqlCommand ("SELECT PropertyName, PropertyPOID, PropertyTypePOID, EntityTypePOID FROM " + TB_PROPERTY_NAME, cnn);
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read ())
                {
                    IProperty tmp = new MSProperty();
                    long pid = long.Parse (reader[1].ToString());
                    long tid = long.Parse (reader[2].ToString());
                    long etid = long.Parse (reader[3].ToString());
                    string name = reader[0].ToString();
                    tmp.PropertyPOID = pid;
                    tmp.PropertyName = name;
                    tmp.EntityType = entityTypes[etid];
                    tmp.PropertyType = propertyTypes[tid];
                    tmp.ExistsInDatabase = true;
                    res.Add (pid, tmp);
                }
            }
            return res;
        }

        /// <summary>
        /// EntityTypes without Properties and PropertyTypes
        /// </summary>
        /// <returns></returns>
        private Dictionary<long, IEntityType> RawEntityTypes()
        {
            Dictionary<long, IEntityType> res = new Dictionary<long, IEntityType>();
            using(SqlConnection cnn = new SqlConnection (Configuration.ConnectStringWithDBName))
            {
                cnn.Open();
                SqlCommand cmd = new SqlCommand ("SELECT EntityTypePOID, Name FROM " + TB_ENTITYTYPE_NAME, cnn);
                SqlDataReader reader = cmd.ExecuteReader();
                while(reader.Read()){
                    long id = long.Parse(reader[0].ToString());
                    string name = reader[1].ToString();
                    IEntityType et = new MSEntityType();
                    et.Name = name;
                    et.EntityTypePOID = id;
                    et.ExistsInDatabase = true;
                    res.Add (id, et);
                }
                reader.Close();
                cmd.CommandText = "SELECT EntityTypePOID, SuperEntityTypePOID FROM " + TB_ENTITYTYPE_NAME;
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    long id = long.Parse(reader[0].ToString());
                    long superId = long.Parse(reader[1].ToString());
                    res[id].SuperEntityType = res[superId];
                }
            }

            return res;
        }

        public IPropertyType NewPropertyType()
        {
            IPropertyType res = new MSPropertyType();
            res.PropertyTypePOID = NextPTID;
            return res;
        }

        public IProperty NewProperty()
        {
            MSProperty res =  new MSProperty();
            res.PropertyPOID = NextPID;
            return res;
        }

        public IEntity GetEntity(long entityPOID)
        {
            using (SqlConnection cnn = new SqlConnection (Configuration.ConnectStringWithoutDBName))
            {
                cnn.Open();

                SqlCommand cmd = new SqlCommand ("SELECT e.EntityTypePOID, propertyPOID, LongValue, DateTimeValue, BoolValue, DateTimeValue, CharValue FROM EntityPOID e LEFT JOIN PropertyValue pv ON e.EntityPOID = pv.EntityPOID");
                SqlDataReader reader = cmd.ExecuteReader();

                IEntity result = new MSEntity(); // We do not set EntityPOID (use NewEntity()) , since id is retrieved from DB.
                bool found = false;
                long propertyPOID = 0;
                long entityTypePOID = 0;

                while (reader.Read ())
                {
                    if (!found) {
                        found = true;
                        //entityPOID = (long)reader[0]; // Got that from 
                        entityTypePOID = (long)reader[1];
                        result.EntityPOID = entityPOID;
                        result.EntityType = TypeSystem.Instance.GetEntityType(entityTypePOID);
                    }
                    //result.EntityType.Get
                }

                if (!reader.IsClosed) { reader.Close(); }
                if (!found) { throw new Exception("Request for unknown entityPOID: " + entityPOID);}
                throw new Exception("Implementation incomplete.");
            }
        }

        public IPropertyType GetPropertyType(long propertyTypePOID)
        {
            throw new Exception("Not implemented");
        }

        public IPropertyType GetPropertyType(string name)
        {
            throw new Exception("Not implemented");
        }


        public IProperty GetProperty(long propertyPOID)
        {
            throw new Exception("Not implemented");
        }

        public IProperty GetProperty(IEntityType entityType, IPropertyType propertyType)
        {
            throw new Exception("Not implemented");
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
            throw new Exception("Not implemented");
        }

        public void CommitChanges()
        {
            CommitTypeChanges();
            throw new Exception("Not implemented");
        }

        public void CommitTypeChanges()
        {
            SqlCommand cmd = new SqlCommand();
            using(SqlConnection cnn =  new SqlConnection(Configuration.ConnectStringWithDBName))
            {
                cnn.Open ();
                cmd.Connection = cnn;
                if (sbETInserts.Length != 0)
                {
                    cmd.CommandText = sbETInserts.ToString();
                    cmd.ExecuteNonQuery();
                }
                if (sbPTInserts.Length != 0)
                {
                    cmd.CommandText = sbPTInserts.ToString();
                    cmd.ExecuteNonQuery();
                }
                if (sbPInserts.Length != 0)
                {
                    cmd.CommandText = sbPInserts.ToString();
                    cmd.ExecuteNonQuery();
                }
            }
            ClearDirtyLists();
            ClearInsertStringBuilders();
        }

        public void RollbackTransaction()
        {
            throw new Exception("Not implemented");
        }

        public void RollbackTypeTransaction()
        {
            foreach (IEntityType et in dirtyEntityTypes) { et.ExistsInDatabase = false; }
            foreach (IEntity e in dirtyEntities) { e.ExistsInDatabase = false; }
            foreach (IProperty p in dirtyProperties) { p.ExistsInDatabase = false; }
            foreach (IPropertyType pt in dirtyPropertyTypes) { pt.ExistsInDatabase = false; }
            ClearDirtyLists();
            ClearInsertStringBuilders();
        }

        #region Private methods.
        private void ClearDirtyLists()
        {
            dirtyEntities.Clear();
            dirtyEntityTypes.Clear();
            dirtyProperties.Clear();
            dirtyPropertyTypes.Clear();
        }

        private void ClearInsertStringBuilders()
        {
            sbETInserts = new StringBuilder();
            sbPTInserts = new StringBuilder();
            sbPInserts = new StringBuilder();
        }
        private void InternalSaveEntityType(IEntityType et)
        {
            if (et == null || et.ExistsInDatabase) { return; }
            InternalSaveEntityType (et.SuperEntityType); 

            sbETInserts.Append (" INSERT INTO ");
            sbETInserts.Append (TB_ENTITYTYPE_NAME);
            sbETInserts.Append (" (EntityTypePOID, Name, SuperEntityTypePOID) VALUES (");
            sbETInserts.Append (et.EntityTypePOID);
            sbETInserts.Append (", '");
            sbETInserts.Append (et.Name);
            sbETInserts.Append ("',");
            if (et.SuperEntityType == null) { sbETInserts.Append("null"); }
            else { sbETInserts.Append(et.SuperEntityType.EntityTypePOID); }
            sbETInserts.Append (") ");
            if (et.DeclaredProperties != null)
            {
                foreach (IProperty p in et.DeclaredProperties)
                {
                    InternalSaveProperty(p);
                }
            }
            dirtyEntityTypes.AddLast (et);
            et.ExistsInDatabase = true;
        }

        private void InternalSaveProperty(IProperty p)
        {
            InternalSavePropertyType(p.PropertyType);
            sbPInserts.Append("INSERT INTO ");
            sbPInserts.Append(TB_PROPERTY_NAME);
            sbPInserts.Append(" (PropertyPOID, PropertyTypePOID, EntityTypePOID, PropertyName) VALUES (");
            sbPInserts.Append(p.PropertyPOID);
            sbPInserts.Append(',');
            sbPInserts.Append(p.PropertyType.PropertyTypePOID);
            sbPInserts.Append(',');
            sbPInserts.Append(p.EntityType.EntityTypePOID);
            sbPInserts.Append(",'");
            sbPInserts.Append(p.PropertyName);
            sbPInserts.Append("') ");
        }

        private void InternalSavePropertyType(IPropertyType pt)
        {
            if (pt.ExistsInDatabase) { return; }
            sbPTInserts.Append (" INSERT INTO ");
            sbPTInserts.Append (TB_PROPERTYTYPE_NAME);
            sbPTInserts.Append (" (PropertyTypePOID, Name, MappingType) VALUES (");
            sbPTInserts.Append (pt.PropertyTypePOID);
            sbPTInserts.Append(",'");
            sbPTInserts.Append(pt.Name);
            sbPTInserts.Append ("',");
            sbPTInserts.Append ((short)pt.MappedType);
            sbPTInserts.Append (")");
            dirtyPropertyTypes .AddLast(pt);
            pt.ExistsInDatabase = true;
        }
        /// <summary>
        /// Creates indexes. (Pending task.)
        /// </summary>
        private void CreateIndexes()
        {
            LinkedList<string> iCC = new LinkedList<string>(); //Table create commands
            ExecuteNonQueries(iCC);
        }

        private void CreateTables()
        {
            LinkedList<string> tCC = new LinkedList<string>(); //Table create commands

            tCC.AddLast("CREATE TABLE " + TB_ENTITYTYPE_NAME + " (EntityTypePOID int primary key, SuperEntityTypePOID int references " + TB_ENTITYTYPE_NAME + " (EntityTypePOID) , Name VARCHAR(max)); ");
            tCC.AddLast("CREATE TABLE " + TB_PROPERTYTYPE_NAME + " (PropertyTypePOID int primary key, Name VARCHAR(max), MappingType smallint); ");
            tCC.AddLast("CREATE TABLE " + TB_ENTITY_NAME + " (EntityPOID int primary key, EntityTypePOID int references " + TB_ENTITYTYPE_NAME + " (EntityTypePOID)); ");
            tCC.AddLast("CREATE TABLE " + TB_PROPERTY_NAME + " (PropertyPOID int primary key, PropertyTypePOID int references " + TB_PROPERTYTYPE_NAME + "(PropertyTypePOID), EntityTypePOID int references entityType (entityTypePOID) on delete cascade on update cascade, PropertyName VARCHAR(max)); ");
            tCC.AddLast("CREATE TABLE "
                + TB_PROPERTYVALUE_NAME + " ( "
                + " PropertyPOID int not null references " + TB_PROPERTY_NAME + " (PropertyPOID) ON DELETE CASCADE, "
                + " EntityPOID int not null references " + TB_ENTITY_NAME + " (EntityPOID) ON DELETE CASCADE, "
                + " LongValue BIGINT, " // Also stores referenceids. Null is in this case empty reference. 
                + " BoolValue BIT, "
                + " StringValue VARCHAR(MAX), "
                + " DateTimeValue DATETIME, "
                + " CharValue CHAR(1))"
                );

            ExecuteNonQueries(tCC);
        }

        private void CreateSProcs()
        {
                string sp_UP_INS_ENTITY =   "CREATE PROCEDURE sp_UP_INS_ENTITY "
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
                                            "	@LongValue AS INT," +
                                            "	@CharValue AS CHAR(1)," +
                                            "	@StringValue AS VARCHAR(max)," +
                                            "	@DateTimeValue AS DateTime," +
                                            "	@BoolValue AS BIT" +
                                            " AS " +
                                            "	IF EXISTS (SELECT * FROM PropertyValue WHERE EntityPOID = @EntityPOID AND PropertyPOID = @PropertyPoid) " +
                                            "	BEGIN " +
                                            "		UPDATE PropertyValue SET " +
                                            "			LongValue = @LongValue," +
                                            "			CharValue = @CharValue," +
                                            "			StringValue = @StringValue ," +
                                            "			DateTimeValue = @DateTimeValue ," +
                                            "			BoolValue = @BoolValue " +
                                            "		WHERE " +
                                            "			EntityPOID = @EntityPOID AND PropertyPOID = @PropertyPOID" +
                                            "	END " +
                                            "	ELSE" +
                                            "	BEGIN" +
                                            "		INSERT INTO " +
                                            "		PropertyValue (EntityPOID, PropertyPOID, LongValue, CharValue , StringValue , DateTimeValue , BoolValue )" +
                                            "		VALUES (@EntityPOID, @PropertyPOID, @LongValue, @CharValue,	@StringValue, @DateTimeValue, @BoolValue)" +
                                            "	END";

	

                ExecuteNonQueries(new string[] { sp_UP_INS_ENTITY, sp_SET_PROPERTYVALUE });
        }

        /// <summary>
        /// Executes a series of command strings. Must be non queries.
        /// </summary>
        /// <param name="cmdStrings"></param>
        private void ExecuteNonQueries(IEnumerable<string> cmdStrings)
        {
            SqlConnection cnn = new SqlConnection(Configuration.ConnectStringWithDBName);
            cnn.Open();

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = cnn;

            cmd.CommandText = "BEGIN TRANSACTION";
            cmd.ExecuteNonQuery();
            foreach (string cmdStr in cmdStrings)
            {
                cmd.CommandText = cmdStr;
                cmd.ExecuteNonQuery();
            }
            cmd.CommandText = " COMMIT ";
            cmd.ExecuteNonQuery();
            cnn.Close();
        }
        #endregion
    }

    internal class MSPropertyValue : IPropertyValue
    {
        private IProperty property;
        private IEntity entity;
        private string stringValue = null;
        private DateTime dateTimeValue = default(DateTime);
        bool existsInDatabase = false;
        private long longValue = default(long);
        private bool boolValue = false;
        private char charValue = default(char);
        private IBOReference refValue = new IBOReference(true, 0);
        private double doubleValue = default(double);

        public double DoubleValue
        {
            get { return doubleValue; }
            set { doubleValue = value; }
        }

        public IBOReference RefValue
        {
            get { return refValue; }
            set { refValue = value; }
        }

        public char CharValue
        {
            get { return charValue; }
            set { charValue = value; }
        }


        public bool BoolValue
        {
            get { return boolValue; }
            set { boolValue = value; }
        }

        public long LongValue
        {
            get { return longValue; }
            set { longValue = value; }
        }

        public bool ExistsInDatabase
        {
            get { return existsInDatabase; }
            set { existsInDatabase = value; }
        }

        public DateTime DateTimeValue
        {
            get { return dateTimeValue; }
            set { dateTimeValue = value; }
        }

        public string StringValue
        {
            get { return stringValue; }
            set { stringValue = value; }
        }

        public IEntity Entity
        {
            get { return entity; }
            set { entity = value; }
        }

        public IProperty Property
        {
            get { return property; }
            set { property = value; }
        }

        public override string ToString()
        {
            string value = null;
            switch (property.MappingType)
            {
                case MappingType.BOOL: value = BoolValue.ToString(); break;
                case MappingType.DATETIME: value = DateTimeValue.ToString(); break;
                case MappingType.DOUBLE: value = DoubleValue.ToString(); break;
                case MappingType.LONG: value = LongValue.ToString(); break;
                case MappingType.REFERENCE: value = this.RefValue.ToString(); break;
                case MappingType.STRING : value = this.StringValue; break;
            }
            return property.MappingType.ToString() + " = " + value;

        }
    }

    internal class MSEntityType : IEntityType
    {
        string name;

        bool persistent;
        long entityTypePOID;

        IEntityType superEntityType;

        Dictionary<long, IProperty> properties;

        public IEnumerable<IProperty> DeclaredProperties
        {
            get {
                if (properties == null) { return null; }
                else { return properties.Values; }
            }
        }

        public IEnumerable<IProperty> GetAllProperties
        {
            get {
                if (DeclaredProperties != null)
                {
                    foreach (IProperty p in DeclaredProperties)
                    {
                        yield return p;
                    }
                    if (superEntityType != null)
                    {
                        foreach (IProperty p in superEntityType.GetAllProperties)
                        {
                            yield return p;
                        }
                    }
                }
                else
                {
                    yield break;
                }
            }
        }

        /// <summary>
        /// TODO: Might be better to store all properties in each 
        /// MSEntityType to avoid nested look up and thus increase 
        /// performance. 
        /// </summary>
        /// <param name="propertyPOID"></param>
        /// <returns></returns>
        public IProperty GetProperty(long propertyPOID)
        {
            IProperty result;
            if (!properties.TryGetValue (propertyPOID, out result))
            {
                if (superEntityType == null) { throw new Exception("No such property in IEntityType '" + name + "': " + propertyPOID); }
                result = superEntityType.GetProperty(propertyPOID);
            }
            return result;
        }

        /// <summary>
        /// Adds property to this entity type. 
        /// Insertion of duplicates are not checked.
        /// </summary>
        /// <param name="property"></param>
        public void AddProperty(IProperty property)
        {
            if (properties == null) { properties = new Dictionary<long, IProperty>(); }
            properties.Add(property.PropertyPOID, property);
        }

        public bool ExistsInDatabase
        {
            get { return persistent; }
            set { persistent = value; }
        }

        public long EntityTypePOID
        {
            get { return entityTypePOID; }
            set { entityTypePOID = value; }
        }

        public IEntityType SuperEntityType
        {
            get { return superEntityType; }
            set { superEntityType = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
    }

    internal class MSEntity : IEntity
    {
        IEntityType entityType;
        long entityPOID;
        Dictionary<long, IPropertyValue> propertyValues = new Dictionary<long, IPropertyValue>();
        bool existsInDatabase;

        public bool ExistsInDatabase
        {
            get { return existsInDatabase; }
            set { existsInDatabase = value; }
        }

        public long EntityPOID
        {
            get { return entityPOID; }
            set { entityPOID = value; }
        }

        public IEntityType EntityType
        {
            get { return entityType; }
            set { entityType = value; }
        }

        public IPropertyValue GetPropertyValue(IProperty property)
        {
            return propertyValues[property.PropertyPOID];
        }

        public void StorePropertyValue(IPropertyValue propertyValue)
        {
            this.propertyValues[propertyValue.Property.PropertyPOID] = propertyValue;
        }

        public override string ToString()
        {
            string res = "MSEntity { " + this.EntityType.ToString();
            foreach (IPropertyValue pv in propertyValues.Values)
            {
                res += "\n" + pv.ToString();
            }
            res += "\n}";
            return res;
        }
    }

    internal class MSProperty : IProperty
    {
        IPropertyType propertyType;
        IEntityType entityType;
        MappingType mappingType;

        long propertyPOID;
        string propertyName;
        bool existsInDatabase;

        public MappingType MappingType
        {
            get { return mappingType; }
            set { mappingType = value; }
        } 


        public IEntityType EntityType
        {
            get { return entityType; }
            set { entityType = value; }
        }

        public bool ExistsInDatabase
        {
            get { return existsInDatabase; }
            set { existsInDatabase = value; }
        }

        public string PropertyName
        {
            get { return propertyName; }
            set { propertyName = value; }
        }

        public long PropertyPOID
        {
            get { return propertyPOID; }
            set { propertyPOID = value; }
        }

        public IPropertyType PropertyType
        {
            get { return propertyType; }
            set { propertyType = value; }
        }

        public override string ToString()
        {
            return "MSProperty {" + this.mappingType + " name = " + propertyName + " " + PropertyType + " }";
        }
    }

    public class MSPropertyType : IPropertyType
    {
        string name;
        long propertyTypePOID;
        MappingType mappedType;
        bool existsInDatabase;

        public bool ExistsInDatabase
        {
            get { return existsInDatabase; }
            set { existsInDatabase = value; }
        }

        public MappingType MappedType
        {
            get { return mappedType; }
            set { mappedType = value; }
        }

        public long PropertyTypePOID
        {
            get { return propertyTypePOID; }
            set { propertyTypePOID = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
    }
}
