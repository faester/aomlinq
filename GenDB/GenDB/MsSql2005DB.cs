using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace GenDB
{
    class MsSql2005DB : IGenericDatabase
    {
        #region CONSTS
        const string CNN_STRING = "server=(local);database=generic;Integrated Security=SSPI";
        const string CNN_STRING_PRE_CREATE = "server=(local);Integrated Security=SSPI";
        /* ADO.NET opretholder en connection pool. 
         * Dette forudsætter at forbindelser åbnes 
         * og lukkes hver gang de bruges.
         * http://msdn2.microsoft.com/en-us/library/8xx3tyca.aspx
         * 
         * Kan også indsættes i using(){} statement
         * 
         * Connection string fungerer i denne forbindelse som nøgle.
         */

        const string DB_NAME = "generic";
        const string TB_ENTITY_NAME = "Entity";
        const string TB_ENTITYTYPE_NAME = "EntityType";
        const string TB_PROPERTYTYPE_NAME = "PropertyType";
        const string TB_PROPERTY_NAME = "Property";
        const string TB_PROPERTYVALUE_NAME = "PropertyValue";
        #endregion

        /// <summary>
        /// Creates the database. Throws Exception if db already exists.
        /// </summary>
        public void CreateDatabase()
        {
            using (SqlConnection cnn = new SqlConnection(CNN_STRING_PRE_CREATE))
            {
                cnn.Open();
                SqlCommand cmd = new SqlCommand("CREATE DATABASE " + DB_NAME, cnn);
                cmd.ExecuteNonQuery();
            }
            CreateTables();
            CreateIndexes();
        }


        public bool DatabaseExists()
        {
            SqlConnection cnn = new SqlConnection(CNN_STRING_PRE_CREATE);
            cnn.Open();
            SqlCommand cmd = new SqlCommand("USE " + DB_NAME, cnn);
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
                Console.WriteLine("Finally executed. Hooray!");
                cnn.Close();
            }
            return true;
        }

        /// <summary>
        /// Deletes database.
        /// </summary>
        public void DeleteDatabase()
        {
            SqlConnection cnn = new SqlConnection(CNN_STRING_PRE_CREATE);
            cnn.Open();

            SqlCommand cmd = new SqlCommand("DROP DATABASE " + DB_NAME, cnn);
            cmd.ExecuteNonQuery();

            cnn.Close();
        }

        /// <summary>
        /// Returns IEntityType with given EntityTypePOID if present.
        /// The associated properties should be set.
        /// Null otherwise.
        /// </summary>
        /// <param name="entityTypePOID"></param>
        public IEntityType RetrieveEntityType(long entityTypePOID)
        {
            throw new Exception("Not implemented");
        }


        public IEntityType RetrieveEntityType(string name)
        {
            throw new Exception("Not implemented");
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
            throw new Exception("Not implemented");
        }

        /// <summary>
        /// Returns IEntity with given EntityPOID if present.
        /// Null otherwise.
        /// </summary>
        /// <param name="entityTypePOID"></param>
        public IEntity GetEntity(long entityPOID)
        {
            throw new Exception("Not implemented");
        }


        public IEntity NewEntity()
        {
            throw new Exception("Not implemented");
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


        public void Save(IEntityType entityType)
        {
            throw new Exception("Not implemented");
        }

        public void Save(IEntity entity)
        {
            throw new Exception("Not implemented");
        }

        #region Private methods.
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
            tCC.AddLast("CREATE TABLE " + TB_PROPERTYTYPE_NAME + " (PropertyTypePOID int primary key, Name VARCHAR(max), MappedType smallint); ");
            tCC.AddLast("CREATE TABLE " + TB_ENTITY_NAME + " (EntityPOID int primary key, EntityType int references " + TB_ENTITYTYPE_NAME + " (EntityTypePOID) ON DELETE CASCADE ON UPDATE CASCADE); ");
            tCC.AddLast("CREATE TABLE " + TB_PROPERTY_NAME + " (PropertyPOID int primary key, PropertyTypePOID int references " + TB_PROPERTYTYPE_NAME + "(PropertyTypePOID), name VARCHAR(max)); ");
            tCC.AddLast("CREATE TABLE "
                + TB_PROPERTYVALUE_NAME + " ( "
                + " PropertyPOID int not null references " + TB_PROPERTY_NAME + " (PropertyPOID) ON DELETE CASCADE, "
                + " EntityPOID int not null references " + TB_ENTITY_NAME + " (EntityPOID) ON DELETE CASCADE, "
                + " LongValue BIGINT, "
                + " BoolValue BIT, "
                + " StringValue VARCHAR(MAX), "
                + " DateTimeValue DATETIME)"
                );

            ExecuteNonQueries(tCC);
        }

        /// <summary>
        /// Executes a series of command strings. Must be non queries.
        /// </summary>
        /// <param name="cmdStrings"></param>
        private void ExecuteNonQueries(IEnumerable<string> cmdStrings)
        {
            SqlConnection cnn = new SqlConnection(CNN_STRING);
            cnn.Open();

            SqlCommand cmd = new SqlCommand();
            cmd.Connection = cnn;
            foreach (string cmdStr in cmdStrings)
            {
                cmd.CommandText = cmdStr;
                cmd.ExecuteNonQuery();
            }

            cnn.Close();
        }
        #endregion
    }

    internal class MSEntityType : IEntityType
    {
        string name;

        bool persistent;
        long entityTypePOID;

        IEntityType superEntityType;

        LinkedList<IProperty> properties;

        public IEnumerable<IProperty> Properties
        {
            get { return properties; }
        }


        /// <summary>
        /// Adds property to this entity type. 
        /// Insertion of duplicates are not checked.
        /// </summary>
        /// <param name="property"></param>
        public void AddProperty(IProperty property)
        {
            if (properties == null) { properties = new LinkedList<IProperty>(); }
            properties.AddLast(property);
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
    }

    internal class MSProperty : IProperty
    {
        IPropertyType propertyType;
        long propertyPOID;
        string propertyName;
        bool existsInDatabase;

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
