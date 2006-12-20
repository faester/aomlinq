using System;
using System.Collections.Generic;
using System.Text;

namespace GenDB.DB
{
    public enum MappingType { BOOL, DATETIME, DOUBLE, LONG, STRING, REFERENCE, CHAR };

    /// <summary>
    /// Representation of DB reference to another
    /// object. We need some way to represent DBNull, 
    /// since this indicates that the reference is empty.
    /// </summary>
    struct IBOReference
    {
        long entityPOID;

        public long EntityPOID
        {
            get { return entityPOID; }
            set { entityPOID = value; }
        }
        bool isNullReference;

        public bool IsNullReference
        {
            get { return isNullReference; }
            set { isNullReference = value; }
        }

        public IBOReference(bool isNullReference)
        {
            this.isNullReference = isNullReference;
            this.entityPOID = default(long);
        }

        public IBOReference(long entityPOID)
        {
            isNullReference = false;
            this.entityPOID = entityPOID;
        }

        public IBOReference(bool isNullReference, long entityPOID)
        {
            this.isNullReference = isNullReference;
            this.entityPOID = entityPOID;
        }
    }

    interface IEntityType
    {
        /// <summary>
        /// Returns super entity type if present. Otherwise
        /// null is returned.
        /// </summary>
        IEntityType SuperEntityType { get; set; }

        /// <summary>
        /// Database id for this IEntityType
        /// </summary>
        long EntityTypePOID { get; set; }

        /// <summary>
        /// When using types outside the namespace/assembly, it
        /// may be neccessary to load the assembly at run-time. 
        /// This string gives a unique description of the assembly
        /// specifying the type.
        /// </summary>
        string AssemblyDescription { get; set; }

        /// <summary>
        /// Name of this EntityType
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Returns all properties declared  
        /// for this IEntityType
        /// </summary>
        IEnumerable<IProperty> DeclaredProperties { get; }

        /// <summary>
        /// Returns all properties valid for 
        /// this entity type. That is, all 
        /// properties declared for this entity 
        /// type and all super types.
        /// </summary>
        IEnumerable<IProperty> GetAllProperties { get; }

        /// <summary>
        /// Returns the property with the given propertyPOID if such a 
        /// property is associated with this IEntityType or one of its 
        /// super types. It is the responsibility of the caller to 
        /// ensure that a property with given ID actually exists. The 
        /// behaviour is unspecified if this precondition is violated.
        /// </summary>
        /// <param name="propertyPOID"></param>
        /// <returns></returns>
        IProperty GetProperty(long propertyPOID);

        /// <summary>
        /// Returns the IProperty with the name given if present
        /// in this IEntityType. Properties of subtypes will not 
        /// be returned. Null is returned if a matching property 
        /// could not be found.
        /// </summary>
        /// <param name="PropertyName"></param>
        /// <returns></returns>
        IProperty GetProperty(string propertyName);

        /// <summary>
        /// Adds a property to the entity type.
        /// </summary>
        /// <param name="property"></param>
        void AddProperty(IProperty property);

        /// <summary>
        /// Used to determine if insertion should 
        /// happen using an update or insert command.
        /// </summary>
        bool ExistsInDatabase { get; set; }

        bool IsList { get; set; }
        bool IsDictionary { get; set; }
    }

    interface IEntity
    {
        IEntityType EntityType { get; set; }

        long EntityPOID { get; set; }

        IPropertyValue GetPropertyValue(IProperty property);

        void StorePropertyValue(IPropertyValue propertyValue);

        IEnumerable<IPropertyValue> AllPropertyValues { get; }
    }

    interface IPropertyType
    {
        string Name { get; set; }

        long PropertyTypePOID { get; set; }

        /// <summary>
        /// Constants should be provided by the 
        /// implementation of IGenericDatabase
        /// </summary>
        MappingType MappedType { get; set; }

        /// <summary>
        /// Used to determine if insertion should 
        /// happen using an update or insert command.
        /// </summary>
        bool ExistsInDatabase { get; set; }
    }

    interface IProperty
    {
        //TODO: Bør flyttes til PropertyType....
        MappingType MappingType { get; set; }
        IPropertyType PropertyType { get; set; }
        IEntityType EntityType { get; set; }
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

        string StringValue { get; set; }

        long LongValue { get; set; }

        double DoubleValue { get; set; }

        DateTime DateTimeValue { get; set; }

        char CharValue { get; set; }

        bool BoolValue { get; set; }

        IBOReference RefValue { get; set; }
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
        /// Tests if database exists.
        /// </summary>
        /// <returns></returns>
        bool DatabaseExists();

        /// <summary>
        /// Returns all persistent property types. 
        /// </summary>
        /// <returns></returns>
        IEnumerable<IPropertyType> GetAllPropertyTypes();

        /// <summary>
        /// Returns a new IEntityType instance with 
        /// correct EntityPOID, name set and no associated
        /// properties.
        /// 
        /// The type is not persisted until it is added to the database.
        /// 
        /// (Should be called by the TypeSystem singleton only)
        /// </summary>
        /// <returns></returns>
        IEntityType NewEntityType(string name);

        IEntity NewEntity();

        IProperty NewProperty();

        IPropertyType NewPropertyType();

        IPropertyValue NewPropertyValue();

        /// <summary>
        /// Returns all IEntityTypes stored in the database.
        /// Is used by the TypeSystem singleton to retrieve
        /// all types upon instantiation.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IEntityType> GetAllEntityTypes();

        IEntity GetEntity(long entityPOID);

        IPropertyType GetPropertyType(long propertyTypePOID);
        IPropertyType GetPropertyType(string name);

        IProperty GetProperty(long propertyPOID);
        IProperty GetProperty(IEntityType entityType, IPropertyType propertyType);

        /// <summary>
        /// Saves the entityType as well as any unsaved 
        /// super types, properties and property types.
        /// </summary>
        /// <param name="entityType"></param>
        void Save(IEntityType entityType);

        /// <summary>
        /// Will return all persisted elements ordered by 
        /// their EntityType. (The ordering happens to ease
        /// later translation to business objects.)
        /// </summary>
        /// <returns></returns>
        IEnumerable<IEntity> GetAllEntities();

        IEnumerable<IEntity> Where(IWhereable expression);

        /// <summary>
        /// Will return all persisted elements of the given
        /// IEntityType or one of its subclasses ordered by 
        /// their EntityType. (The ordering happens to ease
        /// later translation to business objects.)
        /// </summary>
        /// <returns></returns>
        IEnumerable<IEntity> GetAllEntitiesOfType(IEntityType type);

        /// <summary>
        /// Assumes that all neccessary IProperty, IEntityType and IPropertyType 
        /// instances has been saved before this method is called.
        /// </summary>
        /// <param name="entity"></param>
        void Save(IEntity entity);

        void CommitChanges();

        /// <summary>
        /// Commits changes in the type system.
        /// (Properties, EntityTypes and PropertyTypes)
        /// </summary>
        void CommitTypeChanges();
        void RollbackTypeTransaction();
        void RollbackTransaction();
    }

    interface IGenCollectionElement
    {
        IEntity Entity { get; set; }

        int ElementID { get; set; }

        MappingType MappingType { get; set; }

        string StringValue { get; set; }

        long LongValue { get; set; }

        double DoubleValue { get; set; }

        DateTime DateTimeValue { get; set; }

        char CharValue { get; set; }

        bool BoolValue { get; set; }

        IBOReference RefValue { get; set; }
    }
}
