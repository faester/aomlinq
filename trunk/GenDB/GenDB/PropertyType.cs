using System;
using System.Collections.Generic;
using System.Text;
using System.Data.DLinq;

namespace GenDB
{
    [Table(Name = "PropertyType")]
    internal class PropertyType
    {
        public PropertyType()
        {
            PropertyTypePOID = GenericDB.Instance.NextPropertyTypePOID;
        }

        [Column(Name = "PropertyTypePOID", Id = true, AutoGen = false)]
        public long PropertyTypePOID;

        [Column(Name = "Name")]
        public string Name;

        EntitySet<Property> _properties = new EntitySet<Property>();

        [Association(Storage = "_properties", ThisKey = "PropertyTypePOID")]
        public EntitySet<Property> Properties
        {
            get { return this._properties; }
            set { this._properties.Assign(value); }
        }
    }
}
