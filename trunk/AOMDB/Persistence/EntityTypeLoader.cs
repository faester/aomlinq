using System;
using System.Collections.Generic;
using System.Text;
//using System.Data.DLinq;
//using System.Query;
using System.Data.SqlClient;
using AOM;

namespace Persistence 
{
    public static class EntityTypeLoader
    {
        static bool loaded = false;

        static Dictionary<long, EntityType> entityTypes = null;
        static Dictionary<long, PropertyType> propertyTypes = null;
        static SqlConnection cnn = null;

        /// <summary>
        /// Load type objects from database. 
        /// </summary>
        public static void Load() {
            //Only load s_types once. 
            if (loaded) { return; }
            InitConnection();
            lock (cnn)
            {
                InitDictionaries();
                LoadEntityTypes();
                LoadPropertyTypes();
                LoadProperties();
                LoadInheritance();
                DisposeDictionaries();
                DisposeConnection();
                loaded = true;
            }
        }

        /// <summary>
        /// Set up connection to the database. 
        /// </summary>
        private static void InitConnection() {
            cnn = new SqlConnection(AOMConfig.CNN_STRING);
            cnn.Open();
        }

        /// <summary>
        /// Close connection
        /// </summary>
        private static void DisposeConnection() {
            cnn.Close();
            cnn = null;
        }

        private static void InitDictionaries() {
            entityTypes = new Dictionary<long,EntityType>();
            propertyTypes = new Dictionary<long,PropertyType>();
        }


        /// <summary>
        /// Disposes the cache dictionaries.
        /// </summary>
        private static void DisposeDictionaries() {
            entityTypes = null;
            propertyTypes = null;
        }


        /// <summary>
        /// Create stubs for each entity type. The properties are
        /// not yet associated. Each type is stored in the <pre>entityTypes</pre> 
        /// dictionary with its id as key for faster look up.
        /// </summary>
        private static void LoadEntityTypes()
        {
            SqlCommand etLoader = new SqlCommand ("SELECT EntityTypePOID, name FROM EntityType", cnn);
            SqlDataReader reader = etLoader.ExecuteReader();
            while(reader.Read()) {
                long id = long.Parse(reader[0].ToString());
                string name = reader[1].ToString();
                EntityType et = EntityType.AddType (id, name);
                et.IsPersistent = true;
                entityTypes[id] = et;
            }

            if (!reader.IsClosed ) { reader.Close(); }
        }

        /// <summary>
        /// Loads all PropertyType objects from db.
        /// </summary>
        private static void LoadPropertyTypes()
        {
            SqlCommand etLoader = new SqlCommand ("SELECT PropertyTypePOID, name FROM PropertyType", cnn);
            SqlDataReader reader = etLoader.ExecuteReader();
            while(reader.Read()) {
                long id = long.Parse(reader[0].ToString());
                string name = reader[1].ToString();
                PropertyType pt = new PropertyType(name, id);
                propertyTypes[id] = pt;
            }

            if (!reader.IsClosed ) { reader.Close(); }
        }

        /// <summary>
        /// Loads the unheritance structure of the <pre>EntityType</pre>s.
        /// The <pre>EntityType</pre> objects has already been stored in 
        /// a dictionary indexed by their <pre>EntityTypePOID</pre> so 
        /// this dictionary is used to look up the sub- and super-entitytype
        /// of each inheritance pair.
        /// </summary>
        private static void LoadInheritance()
        {
            SqlCommand etLoader = new SqlCommand ("SELECT SuperEntityTypePOID, SubEntityTypePOID FROM Inheritance", cnn);
            SqlDataReader reader = etLoader.ExecuteReader();
            while(reader.Read()) {
                long super = long.Parse(reader[0].ToString());
                long sub = long.Parse(reader[1].ToString());
                EntityType esub = entityTypes[sub];
                EntityType esuper = entityTypes[super];
                esub.SetSuperType(esuper);
            }

            if (!reader.IsClosed ) { reader.Close(); }
        }


        /// <summary>
        /// Loads property definitions and associtate them 
        /// with the appropriate <pre>EntityType</pre>s.
        /// </summary>
        private static void LoadProperties()
        {
            SqlCommand etLoader = new SqlCommand (
                "SELECT "
                + "  PropertyPOID, "
                + "  PropertyTypePOID, "
                + "  name, "
                + "  EntityTypePOID, "
                + "  defaultValue "
                + " FROM Property",
                cnn);

            SqlDataReader reader = etLoader.ExecuteReader();

            while(reader.Read()) {
                long propertyPOID = long.Parse(reader[0].ToString());
                long propertyTypePOID = long.Parse( reader[1].ToString());
                string name = reader[2].ToString();
                long entityTypePOID = long.Parse(reader[3].ToString());
                string defaultvalue = reader[4].ToString();
                PropertyType pt = propertyTypes[propertyTypePOID];
                EntityType et = entityTypes[entityTypePOID];
                Property p = new Property(name, pt, defaultvalue);
                p.Id = propertyPOID;
                et.AddProperty (p);
            }

            if (!reader.IsClosed ) { reader.Close(); }
        }

    }
}
