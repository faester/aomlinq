using System;
using System.Collections.Generic;
using System.Text;
using System.Data.DLinq;

namespace GenDB
{
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
        EntityRef<EntityTypeDL> _entityType = new EntityRef<EntityTypeDL>();

        [Association(Storage = "_values", OtherKey = "PropertyPOID")]
        public EntitySet<PropertyValue> Values
        {
            get { return this._values; }
            set { this._values.Assign(value); }
        }

        [Association(Storage = "_entityType", OtherKey = "EntityTypePOID", ThisKey = "EntityTypePOID")]
        public EntityTypeDL EntityType
        {
            get { return this._entityType.Entity; }
            set { this._entityType.Entity = value; }
        }


        [Association(Storage = "_propertyType", ThisKey = "PropertyTypePOID", OtherKey = "PropertyTypePOID")]
        public PropertyType PropertyType
        {
            get { return this._propertyType.Entity; }
            set { this._propertyType.Entity = value; }
        }
    }
}
