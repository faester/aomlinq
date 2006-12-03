using System;
using System.Collections.Generic;
using System.Text;
using System.Data.DLinq;

namespace GenDB
{
    [Table(Name = "EntityType")]
    internal class EntityType
    {
        [Column(Name = "Name", Id = false)]
        public string Name;

        [Column(Name = "EntityTypePOID", Id = true, AutoGen = true)]
        public long EntityTypePOID;

        //private EntitySet<Entity> _entities = new EntitySet<Entity>();
        private EntitySet<Property> _properties = new EntitySet<Property>();

        [Association(Storage = "_properties", OtherKey = "EntityTypePOID", ThisKey = "EntityTypePOID")]
        public EntitySet<Property> Properties
        {
            get { return this._properties; }
            set { this._properties.Assign(value); }
        }

        //[Association(Storage = "_entities", OtherKey = "EntityTypePOID", ThisKey = "EntityTypePOID")]
        //public EntitySet<Entity> Entities
        //{
        //    get { return this._entities; }
        //    set { this._entities.Assign(value); }
        //}
    }
}
