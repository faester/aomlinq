using System;
using System.Collections.Generic;
using System.Text;
using System.Data.DLinq;

namespace GenDB
{
    [Table(Name = "Inheritance")]
    internal class Inheritance
    {
        [Column(Name = "SuperEntityTypePOID", Id = true)]
        public long SuperEntityTypePOID;

        [Column(Name = "SubEntityTypePOID", Id = true)]
        public long SubEntityTypePOID;

        private EntityRef<EntityType> _SubEntityType;
        private EntityRef<EntityType> _SuperEntityType;

        [Association(Storage = "_SubEntityType", ThisKey = "SubEntityTypePOID", OtherKey = "EntityTypePOID")]
        public EntityType SubEntityType
        {
            get { return this._SubEntityType.Entity; }
            set { this._SubEntityType.Entity = value; }
        }

        [Association(Storage = "_SuperEntityType", ThisKey = "SuperEntityTypePOID", OtherKey = "EntityTypePOID")]
        public EntityType SuperEntityType
        {
            get { return this._SuperEntityType.Entity; }
            set { this._SuperEntityType.Entity = value; }
        }
    }
}
