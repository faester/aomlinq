using System;
using System.Collections.Generic;
using System.Text;

namespace GenDB.DB
{
    internal class Property : IProperty
    {
        IPropertyType propertyType;
        IEntityType entityType;

        int propertyPOID;
        string propertyName;
        bool existsInDatabase;

        public MappingType MappingType
        {
            get { return PropertyType.MappingType; }
        }


        public IEntityType EntityType
        {
            get { return entityType; }
            set { entityType = value; }
        }

        public bool ExistsInDatabase
        {
            get { return existsInDatabase; }
            set { existsInDatabase = value; }
        }

        public string PropertyName
        {
            get { return propertyName; }
            set { propertyName = value; }
        }

        public int PropertyPOID
        {
            get { return propertyPOID; }
            set { propertyPOID = value; }
        }

        public IPropertyType PropertyType
        {
            get { return propertyType; }
            set { propertyType = value; }
        }

        public IPropertyValue CreateNewPropertyValue(IEntity entity)
        {
            IPropertyValue pv = new PropertyValue(this, entity);
            entity.StorePropertyValue(pv);
            return pv;
        }

        public override string ToString()
        {
            return "Property {name = " + propertyName + " " + PropertyType + " }";
        }

        public override bool Equals(object obj)
        {
            if (obj == null) { return false; }
            if (!(obj is Property)) { return false; }

            int otherID = (obj as Property).PropertyPOID;

            return (otherID == this.PropertyPOID);
        }

        public override int GetHashCode()
        {
            return (propertyPOID << 16) ^ propertyPOID;
        }
    }
}
