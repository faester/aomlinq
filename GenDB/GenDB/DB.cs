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

        public Table<Entity> Entities;
        public Table<EntityType> EntityTypes;
        public Table<Property> Properties;
        public Table<PropertyType> PropertyTypes;
        public Table<Inheritance> Inheritance;
        public Table<PropertyValue> PropertyValues;
        private GenericDB() : base(connectString) { }

        /// <summary>
        /// Returns EntityType named 'name'. If no 
        /// such EntityType exists in the database
        /// it will be created and added to the db.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public EntityType GetCreateEntityType(string name)
        {
            EntityType et = null; // Return object
            var q = from ets in EntityTypes // Filter from table
                     where ets.Name == name // Where name matches
                     select ets;

            int count = 0; // Could also check using = q.Count(); (Would create 2 lookups)

            foreach (EntityType etEnum in q) // Should just make one pass.
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
                et = new EntityType { Name = name }; // Create new entityType
                EntityTypes.Add(et); // And add to the database
                SubmitChanges(); // Need to submit if subsequent queries are to find the added element
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
                SubmitChanges(); // Need to submit if subsequent queries are to find the added element
            }

            return res;
        }
    }
}
