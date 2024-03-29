using System;
using System.Collections.Generic;
using System.Text;

namespace GenDB.DB
{
    /// <summary>
    /// Implementation of the IEntity interface.
    /// </summary>
    internal class Entity : IEntity
    {
        IEntityType entityType;
        int entityPOID;
        Dictionary<long, IPropertyValue> propertyValues = new Dictionary<long, IPropertyValue>();
        bool existsInDatabase;

        public int EntityPOID
        {
            get { return entityPOID; }
            set { entityPOID = value; }
        }

        public IEntityType EntityType
        {
            get { return entityType; }
            set { entityType = value; }
        }

        public IEnumerable<IPropertyValue> AllPropertyValues
        {
            get { return propertyValues.Values; }
        }

        public IPropertyValue GetPropertyValue(IProperty property)
        {
            return propertyValues[property.PropertyPOID];
        }

        public void StorePropertyValue(IPropertyValue propertyValue)
        {
            this.propertyValues[propertyValue.Property.PropertyPOID] = propertyValue;
        }

        public override string ToString()
        {
            string res = "Entity { " + this.EntityType.ToString();
            foreach (IPropertyValue pv in propertyValues.Values)
            {
                res += "\n" + pv.ToString();
            }
            res += "\n}";
            return res;
        }
    }
}
