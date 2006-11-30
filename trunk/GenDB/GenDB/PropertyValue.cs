using System;
using System.Collections.Generic;
using System.Text;
using System.Data.DLinq;

namespace GenDB
{
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
}
