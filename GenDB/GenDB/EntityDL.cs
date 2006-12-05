using System;
using System.Collections.Generic;
using System.Text;
using System.Data.DLinq;

namespace GenDB
{
    [Table(Name = "Entity")]
    internal class EntityDL
    {
        [Column(Name = "EntityPOID", Id = true, AutoGen = true)]
        public long EntityPOID;

        [Column(Name = "EntityTypePOID", Id = false)]
        public long EntityTypePOID;

        private EntityRef<EntityTypeDL> _entityType = new EntityRef<EntityTypeDL>();
        private EntitySet<PropertyValue> _propertyValues = new EntitySet<PropertyValue>();

        [Association(Storage = "_entityType", OtherKey = "EntityTypePOID", ThisKey = "EntityTypePOID")]
        public EntityTypeDL EntityType
        {
            get { return this._entityType.Entity; }
            set
            {
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
}
