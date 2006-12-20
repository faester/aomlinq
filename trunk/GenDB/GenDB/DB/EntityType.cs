using System;
using System.Collections.Generic;
using System.Text;

namespace GenDB.DB
{
    internal class EntityType : IEntityType
    {
        string name;
        string assemblyDescription;

        bool persistent;
        long entityTypePOID;

        IEntityType superEntityType;

        Dictionary<long, IProperty> properties;

        public IProperty GetProperty(string propertyname)
        {
            foreach (IProperty p in properties.Values)
            {
                if (p.PropertyName == propertyname)
                {
                    return p;
                }
            }
            return null;
        }


        public string AssemblyDescription
        {
            get { return assemblyDescription; }
            set { assemblyDescription = value; }
        }

        public IEnumerable<IProperty> DeclaredProperties
        {
            get
            {
                if (properties == null) { return null; }
                else { return properties.Values; }
            }
        }

        public IEnumerable<IProperty> GetAllProperties
        {
            get
            {
                if (DeclaredProperties != null)
                {
                    foreach (IProperty p in DeclaredProperties)
                    {
                        yield return p;
                    }
                }
                if (superEntityType != null)
                {
                    foreach (IProperty p in superEntityType.GetAllProperties)
                    {
                        yield return p;
                    }
                }
                else
                {
                    yield break;
                }
            }
        }

        /// <summary>
        /// TODO: Might be better to store all properties in each 
        /// EntityType to avoid nested look up and thus increase 
        /// performance. 
        /// </summary>
        /// <param name="propertyPOID"></param>
        /// <returns></returns>
        public IProperty GetProperty(long propertyPOID)
        {
            IProperty result;
            if (!properties.TryGetValue(propertyPOID, out result))
            {
                if (superEntityType == null) { throw new Exception("No such property in IEntityType '" + name + "': " + propertyPOID); }
                result = superEntityType.GetProperty(propertyPOID);
            }
            return result;
        }

        /// <summary>
        /// Adds property to this entity type. 
        /// Insertion of duplicates are not checked.
        /// </summary>
        /// <param name="property"></param>
        public void AddProperty(IProperty property)
        {
            if (properties == null) { properties = new Dictionary<long, IProperty>(); }
            properties.Add(property.PropertyPOID, property);
        }

        public bool ExistsInDatabase
        {
            get { return persistent; }
            set { persistent = value; }
        }

        public long EntityTypePOID
        {
            get { return entityTypePOID; }
            set { entityTypePOID = value; }
        }

        public IEntityType SuperEntityType
        {
            get { return superEntityType; }
            set { superEntityType = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
    }
}
