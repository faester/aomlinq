using System;
using System.Collections.Generic;
using System.Text;
using System.Data.DLinq;
using System.Query;

namespace GenDB
{

    internal class GenericDB : DataContext
    {
        #region singleton stuff
        private static string connectString = "server=localhost;database=generic;uid=aom;pwd=aomuser";

        public static string ConnectString
        {
            get { return GenericDB.connectString; }
            set
            {
                if (instance == null)
                {
                    GenericDB.connectString = value;
                }
                else
                {
                    throw new Exception("Can not change connect string after initialization.");
                }
            }
        }

        private static GenericDB instance = null;

        public static GenericDB Instance
        {
            get
            {
                if (instance == null) { instance = new GenericDB(); }
                return instance;
            }
        }
        #endregion

        long nextEntityPOID = 0;

        public long NextEntityPOID
        {
            get { return nextEntityPOID++; }
        }

        long nextEntityTypePOID = 0;

        public long NextEntityTypePOID
        {
            get { return nextEntityTypePOID++; }
        }

        long nextPropertyTypePOID = 0;

        public long NextPropertyTypePOID
        {
            get { return nextPropertyTypePOID++; }
        }
        long nextPropertyPOID = 0;

        public long NextPropertyPOID
        {
            get { return nextPropertyPOID++; }
        }

        public Table<EntityDL> Entities;
        public Table<EntityTypeDL> EntityTypes;
        public Table<Property> Properties;
        public Table<PropertyType> PropertyTypes;
        public Table<Inheritance> Inheritance;
        public Table<PropertyValue> PropertyValues;
        private GenericDB() : base(connectString) 
        { 
            InitIDs();
        }

        private void InitIDs()
        {
            var entityIDs = from ids in Entities select ids.EntityPOID;
            var entityTypeIDS = from ids in EntityTypes select ids.EntityTypePOID;
            var propertyPoids = from ids in Properties select ids.PropertyPOID;
            var propertyTypePoids = from ids in PropertyTypes select ids.PropertyTypePOID;

            nextEntityPOID = entityIDs.Max() + 1;
            nextEntityTypePOID = entityTypeIDS.Max() + 1;
            nextPropertyPOID = propertyPoids.Max() + 1;
            nextPropertyPOID = propertyTypePoids.Max() + 1;
        }

        /// <summary>
        /// Returns EntityType named 'name'. If no 
        /// such EntityType exists in the database
        /// it will be created and added to the db.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public EntityTypeDL GetCreateEntityType(string name)
        {
            EntityTypeDL et = null; // Return object
            var q = from ets in EntityTypes // Filter from table
                     where ets.Name == name // Where name matches
                     select ets;

            int count = 0; // Could also check using = q.Count(); (Would create 2 lookups)

            foreach (EntityTypeDL etEnum in q) // Should just make one pass.
            {
                count++;
                if (count > 1)
                {
                    throw new Exception("Inconsistent database. Dublicates of EntityType named '" + name + "'");
                }
                et = etEnum; // Store in result value to make it possible to test if iteration continues after first element
            }

            if (count == 0) // Test if no result was found
            {
                et = new EntityTypeDL { Name = name }; // Create new entityType
                EntityTypes.Add(et); // And add to the database
                //                SubmitChanges(); // Need to submit if subsequent queries are to find the added element
            }

            return et;
        }


        /// <summary>
        /// Returns PropertyType named 'name'. If no 
        /// such PropertyType exists in the database
        /// it will be created and added to the db.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public PropertyType GetCreatePropertyType(string name)
        {
            var pts = from pt in PropertyTypes 
                      where pt.Name == name
                      select pt;
            
            int count = 0;
            PropertyType res = null;

            foreach (PropertyType pt in pts)
            {
                count++;
                if (count > 1) { throw new Exception ("Dublicate PropertyTypes with name '" + name + "' exists. "); }
                res = pt;
            }

            if (count == 0)
            {
                res = new PropertyType();
                res.Name = name;
                PropertyTypes.Add (res);
                //SubmitChanges(); // Need to submit if subsequent queries are to find the added element
            }

            return res;
        }
    }
}
