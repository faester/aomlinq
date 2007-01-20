using System;
using System.Collections.Generic;
using System.Text; 

namespace GenDB.DB
{
    public class PropertyType : IPropertyType
    {
        string name;
        int propertyTypePOID;
        MappingType mappedType;
        bool existsInDatabase;

        public bool ExistsInDatabase
        {
            get { return existsInDatabase; }
            set { existsInDatabase = value; }
        }

        public MappingType MappingType
        {
            get { return mappedType; }
            set { mappedType = value; }
        }

        public int PropertyTypePOID
        {
            get { return propertyTypePOID; }
            set { propertyTypePOID = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
    }
}
