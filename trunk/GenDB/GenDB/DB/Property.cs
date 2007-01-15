using System;
using System.Collections.Generic;
using System.Text;

namespace GenDB.DB
{
    internal class Property : IProperty
    {
        IPropertyType propertyType;
        IEntityType entityType;

        long propertyPOID;
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

        public long PropertyPOID
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
    }
}
