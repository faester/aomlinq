using System;
using System.Collections.Generic;
using System.Text;

namespace GenDB
{
    interface IEntityType 
    {
        /// <summary>
        /// Returns super entity type if present. Otherwise
        /// null is returned.
        /// </summary>
        IEntityType SuperEntityType { get; set; } 

        /// <summary>
        /// Database id of this IEntityType
        /// </summary>
        long EntityTypePOID { get; set; }

        /// <summary>
        /// Name of this EntityType
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Returns all properties associated 
        /// with this IEntityType
        /// </summary>
        IEnumerable<IProperty> Properties { get; }

        /// <summary>
        /// Adds a property to the entity type.
        /// </summary>
        /// <param name="property"></param>
        void AddProperty (IProperty property);

        /// <summary>
        /// Used to determine if insertion should 
        /// happen using an update or insert command.
        /// </summary>
        bool ExistsInDatabase { get; set; } 
    }

    interface IEntity
    {
        IEntityType EntityType { get; set; }
        
        long EntityPOID { get; set; }
        
        string PropertyValue(IProperty property);

        /// <summary>
        /// Used to determine if insertion should 
        /// happen using an update or insert command.
        /// </summary>
        bool ExistsInDatabase { get; set; } 
    }

    interface IPropertyType
    {
        string Name { get; set; }
        
        long PropertyTypePOID { get; set; }
        
        /// <summary>
        /// Used to determine if insertion should 
        /// happen using an update or insert command.
        /// </summary>
        bool ExistsInDatabase { get; set; } 
    }

    interface IProperty
    {
        IPropertyType PropertyType { get; set; }
        long PropertyPOID { get; set; }
        string PropertyName { get; set; } 
        /// <summary>
        /// Used to determine if insertion should 
        /// happen using an update or insert command.
        /// </summary>
        bool ExistsInDatabase { get; set; } 
    }

    interface IPropertyValue 
    {
        IProperty Property { get; set; }
        IEntity Entity { get; set; }
        string Value { get; set; }

        /// <summary>
        /// Used to determine if insertion should 
        /// happen using an update or insert command.
        /// </summary>
        bool ExistsInDatabase { get; set; } 
    }

    interface IGenericDatabase
    {
        /// <summary>
        /// Creates the database. Throws Exception if db already exists.
        /// </summary>
        void CreateDatabase();

        /// <summary>
        /// Deletes database.
        /// </summary>
        void DeleteDatabase();

        /// <summary>
        /// Returns IEntityType with given EntityTypePOID if present.
        /// The associated properties should be set.
        /// Null otherwise.
        /// </summary>
        /// <param name="entityTypePOID"></param>
        IEntityType GetEntityType(long entityTypePOID);
        IEntityType GetEntityType(string name);

        /// <summary>
        /// Returns a new IEntityType instance with 
        /// correct EntityPOID, name set and no associated
        /// properties.
        /// </summary>
        /// <returns></returns>
        IEntityType NewEntityType(string name);

        /// <summary>
        /// Returns IEntity with given EntityPOID if present.
        /// Null otherwise.
        /// </summary>
        /// <param name="entityTypePOID"></param>
        IEntity GetEntity(long entityPOID);
        IEntity NewEntity();

        IPropertyType GetPropertyType(long propertyTypePOID);
        IPropertyType GetPropertyType(string name);

        IProperty GetProperty(long propertyPOID);
        IProperty GetProperty(IEntityType entityType, IPropertyType propertyType);
        
        void Save(IEntityType entityType);
        void Save(IEntity entity);
    }
}
