using System;
using System.Collections.Generic;
using System.Text;

namespace GenDB.DB
{
    /// <summary>
    /// Implementation of the IEntityType interface.
    /// </summary>
    internal class EntityType : IEntityType
    {
        string name;
        string assemblyDescription;
        bool isList;
        bool isDictionary;


        bool persistent;
        int entityTypePOID;

        IEntityType superEntityType;

        Dictionary<int, IProperty> properties;
        Dictionary<string, int> propertyNameToIDTranslation;
        LinkedList<IProperty> allProperties = null;

        public EntityType(int entityTypePOID)
        {
            this.entityTypePOID = entityTypePOID;
        }

        public IProperty GetProperty(string propertyname)
        { 
            int id;
            if (propertyNameToIDTranslation.TryGetValue(propertyname, out id))
            {
                return properties[id];
            }
            else if (superEntityType != null)
            {
                return superEntityType.GetProperty (propertyname);
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

        private void CreateAllProperties()
        {
            allProperties = new LinkedList<IProperty>();
            if (DeclaredProperties != null)
            {
                foreach (IProperty p in DeclaredProperties)
                {
                    allProperties.AddLast(p);
                }
            }
            if (superEntityType != null)
            {
                foreach (IProperty p in superEntityType.GetAllProperties)
                {
                    allProperties.AddLast(p);
                }
            }
        }

        public IEnumerable<IProperty> GetAllProperties
        {
            get
            {
                if (allProperties == null)
                {
                    CreateAllProperties();
                }
                foreach(IProperty p in allProperties)
                {
                    yield return p;
                }
            }
        }

        public IProperty GetProperty(int propertyPOID)
        {
            IProperty result;
            if (properties != null && properties.TryGetValue(propertyPOID, out result))
            {
                return result;
            }
            else
            {
                if (superEntityType == null) { throw new Exception("No such property in IEntityType '" + name + "': " + propertyPOID); }
                return superEntityType.GetProperty(propertyPOID);
            }
        }

        public void AddProperty(IProperty property)
        {
            if (properties == null) { 
                properties = new Dictionary<int, IProperty>(); 
               propertyNameToIDTranslation = new Dictionary<string, int>();
            }
            properties.Add(property.PropertyPOID, property);
            propertyNameToIDTranslation.Add (property.PropertyName, property.PropertyPOID);
        }

        public bool ExistsInDatabase
        {
            get { return persistent; }
            set { persistent = value; }
        }

        public int EntityTypePOID
        {
            get { return entityTypePOID; }
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

        public bool IsDictionary
        {
            get { return isDictionary; }
            set { isDictionary = value; }
        }

        public bool IsList
        {
            get { return isList; }
            set { isList = value; }
        }

        public override string ToString()
        {
            return "EntityType {Name='" + Name + "'}";
        }
    }
}
