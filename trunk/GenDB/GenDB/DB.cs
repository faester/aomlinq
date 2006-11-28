using System;
using System.Collections.Generic;
using System.Text;
using System.Data.DLinq;
using System.Query;

namespace GenDB
{
    [Table(Name="Inheritance")]
    internal class Inheritance
    {
        [Column(Name="SuperEntityTypePOID", Id = true)]
        public long SuperEntityTypePOID;
        
        [Column(Name="SubEntityTypePOID", Id = true)]
        public long SubEntityTypePOID;

        private EntityRef<EntityType> _SubEntityType;
        private EntityRef<EntityType> _SuperEntityType;

        [Association(Storage="_SubEntityType", ThisKey="SubEntityTypePOID", OtherKey="EntityTypePOID")]
        public EntityType SubEntityType
        {
            get { return this._SubEntityType.Entity; }
            set { this._SubEntityType.Entity = value; }
        }

        [Association(Storage="_SuperEntityType", ThisKey="SuperEntityTypePOID", OtherKey="EntityTypePOID")]
        public EntityType SuperEntityType
        {
            get { return this._SuperEntityType.Entity; }
            set { this._SuperEntityType.Entity = value; }
        }
    }

    [Table(Name = "Entity")]
    internal class Entity
    {
        [Column(Name = "EntityPOID", Id = true, AutoGen = true)]
        public long EntityPOID;

        [Column(Name = "EntityTypePOID", Id = false)]
        public long EntityTypePOID;

        private EntityRef<EntityType> _entityType = new EntityRef<EntityType>();
        private EntitySet<PropertyValue> _propertyValues = new EntitySet<PropertyValue>();

        [Association(Storage = "_entityType", OtherKey = "EntityTypePOID", ThisKey = "EntityTypePOID")]
        public EntityType EntityType
        {
            get { return this._entityType.Entity; }
            set { 
                this._entityType.Entity = value; 
            }
        }

        [Association(Storage = "_propertyValues", OtherKey = "EntityPOID", ThisKey = "EntityPOID")]
        public EntitySet<PropertyValue> PropertyValues
        {
            get { return this._propertyValues; }
            set { this._propertyValues.Assign(value); }
        }
    }

    [Table(Name = "EntityType")]
    internal class EntityType
    {
        [Column(Name = "Name", Id = false)]
        public string Name;

        [Column(Name = "EntityTypePOID", Id = true, AutoGen = true)]
        public long EntityTypePOID;

        private EntitySet<Entity> _entities = new EntitySet<Entity>();

        [Association(Storage = "_entities", OtherKey = "EntityTypePOID")]
        public EntitySet<Entity> Entities
        {
            get { return this._entities; }
            set { this._entities.Assign(value); }
        }
    }

    [Table(Name = "Property")]
    internal class Property
    {
        [Column(Name = "PropertyPOID", Id = true, AutoGen = true)]
        public long PropertyPOID;

        [Column(Name = "Name", Id = false)]
        public string Name;

        [Column(Name = "PropertyTypePOID", Id = false)]
        public long PropertyTypePOID;

        [Column(Name = "EntityTypePOID", Id = false)]
        public long EntityTypePOID;

        EntitySet<PropertyValue> _values = new EntitySet<PropertyValue>();
        EntityRef<PropertyType> _propertyType = new EntityRef<PropertyType>();
        EntityRef<EntityType> _entityType = new EntityRef<EntityType>();

        [Association(Storage = "_values", OtherKey = "PropertyPOID")]
        public EntitySet<PropertyValue> Values
        {
            get { return this._values; }
            set { this._values.Assign(value); }
        }

        [Association(Storage = "_entityType", OtherKey = "EntityTypePOID", ThisKey = "EntityTypePOID")]
        public EntityType EntityType
        {
            get { return this._entityType.Entity; }
            set { this._entityType.Entity = value; }
        }


        [Association(Storage="_propertyType", ThisKey="PropertyTypePOID", OtherKey="PropertyTypePOID")]
        public PropertyType PropertyType
        {
            get { return this._propertyType.Entity; }
            set { this._propertyType.Entity = value; }
        }
    }

    [Table(Name = "PropertyType")]
    internal class PropertyType
    {
        [Column(Name = "PropertyTypePOID", Id = true, AutoGen = true)]
        public long PropertyTypePOID;

        [Column(Name = "Name")]
        public string Name;

        EntitySet<Property> _properties = new EntitySet<Property>();

        [Association(Storage="_properties", ThisKey="PropertyTypePOID")]
        public EntitySet<Property> Properties
        {
            get { return this._properties; }
            set { this._properties.Assign (value); }
        }
    }

    [Table(Name = "PropertyValue")]
    internal class PropertyValue
    {
        [Column(Name = "TheValue", Id = false)]
        public string TheValue;

        [Column(Name = "PropertyPOID", Id = true)]
        public long PropertyPOID;

        [Column(Name = "EntityPOID", Id = true)]
        public long EntityPOID;

        private EntityRef<Property> _Property;

        private EntityRef<Entity> _Entity;

        [Association(Storage = "_Property", ThisKey = "PropertyPOID", OtherKey = "PropertyPOID")]
        public Property Property
        {
            get { return this._Property.Entity; }
            set { this._Property.Entity = value; }
        }

        [Association(Storage = "_Entity", ThisKey = "EntityPOID", OtherKey = "EntityPOID")]
        public Entity Entity
        {
            get { return this._Entity.Entity; }
            set { this._Entity.Entity = value; }
        }
    }

    internal class GenericDB : DataContext
    {
        #region singleton stuff
        private static string connectString = "server=localhost;database=generic;uid=aom;pwd=aomuser";

        public static string ConnectString
        {
            get { return GenericDB.connectString; }
            set { 
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
            get {
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

        public void Delete (DBTag dbtag)
        {
            var z = from e in Entities
                    where e.EntityPOID == dbtag.EntityPOID
                    select e;
            Entities.RemoveAll (z);

            var q = from pv in PropertyValues 
                    where pv.EntityPOID == dbtag.EntityPOID
                    select pv;
            PropertyValues.RemoveAll(q);
            
            SubmitChanges();
        }
    }
}
