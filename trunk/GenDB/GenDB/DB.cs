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

        public EntityType GetEntityType(string name)
        {
            EntityType et = null; // Return object
            var q = from ets in EntityTypes // Filter from table
                     where ets.Name == name // Where name matches
                     select ets;

            int count = 0; // Could also check using = q.Count(); (Will create 2 lookups)

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
            }

            return et;
        }

        public PropertyValue SetPropertyValue(Entity e, Property p, string value)
        {
            var pvs = from pv in PropertyValues
                      where pv.Entity == e && pv.Property == p
                      select pv;

            foreach (PropertyValue val in pvs) // Since PK is (EntityPOID, PropertyPOID) this will (should) contain max 1 element
            {
                val.TheValue = value;
                return val; // Return if found
            }

            // No match found

            // Create new property
            PropertyValue res = new PropertyValue { Entity = e, Property = p, TheValue = value };

            // Add it to table (TODO: Is this desired semantics?)
            PropertyValues.Add(res);

            return res;
        }
    }
}
