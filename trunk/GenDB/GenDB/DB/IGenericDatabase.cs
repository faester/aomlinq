using System;
using System.Collections.Generic;
using System.Text;

namespace GenDB.DB
{
    public enum MappingType { BOOL, DATETIME, DOUBLE, LONG, STRING, REFERENCE };

    /// <summary>
    /// Representation of DB reference to another
    /// object. We need some way to represent DBNull, 
    /// since this indicates that the reference is empty.
    /// </summary>
    struct IBOReference
    {
        int entityPOID;

        public int EntityPOID
        {
            get { return entityPOID; }
        }
        bool isNullReference;

        public bool IsNullReference
        {
            get { return isNullReference; }
        }

        public IBOReference(bool isNullReference)
        {
            this.isNullReference = isNullReference;
            this.entityPOID = default(int);
        }

        public IBOReference(int entityPOID)
        {
            isNullReference = false;
            this.entityPOID = entityPOID;
        }

        public IBOReference(bool isNullReference, int entityPOID)
        {
            this.isNullReference = isNullReference;
            this.entityPOID = entityPOID;
        }

        public override string ToString()
        {
            return "IBOReference {" + (isNullReference ? " null " : " EntityPOID = " + entityPOID) + "}";
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
        int EntityTypePOID { get; set; }

        /// <summary>
        /// When using types outside the namespace/assembly, it
        /// may be neccessary to load the assembly at run-time. 
        /// This string gives a unique description of the assembly
        /// defining the type.
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
        /// Returns the cstProperty with the given propertyPOID if such a 
        /// cstProperty is associated with this IEntityType or one of its 
        /// super types. It is the responsibility of the caller to 
        /// ensure that a cstProperty with given ID actually exists. The 
        /// behaviour is unspecified if this precondition is violated.
        /// </summary>
        /// <param name="propertyPOID"></param>
        /// <returns></returns>
        IProperty GetProperty(int propertyPOID);

        /// <summary>
        /// Returns the IProperty with the name given if present
        /// in this IEntityType. Properties of subtypes will not 
        /// be returned. Null is returned if a matching cstProperty 
        /// could not be found.
        /// </summary>
        /// <param name="PropertyName"></param>
        /// <returns></returns>
        IProperty GetProperty(string propertyName);

        /// <summary>
        /// Adds a cstProperty to the entity type.
        /// </summary>
        /// <param name="cstProperty"></param>
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

        int EntityPOID { get; set; }

        IPropertyValue GetPropertyValue(IProperty property);

        void StorePropertyValue(IPropertyValue propertyValue);

        IEnumerable<IPropertyValue> AllPropertyValues { get; }
    }

    interface IPropertyType
    {
        string Name { get; set; }

        int PropertyTypePOID { get; set; }

        /// <summary>
        /// Constants should be provided by the 
        /// implementation of IGenericDatabase
        /// </summary>
        MappingType MappingType { get; set; }

        /// <summary>
        /// Used to determine if insertion should 
        /// happen using an update or insert command.
        /// </summary>
        bool ExistsInDatabase { get; set; }
    }

    interface IProperty
    {
        MappingType MappingType { get; }
        IPropertyType PropertyType { get; set; }
        IEntityType EntityType { get; set; }
        int PropertyPOID { get; set; }
        string PropertyName { get; set; }
        /// <summary>
        /// Used to determine if insertion should 
        /// happen using an update or insert command.
        /// </summary>
        bool ExistsInDatabase { get; set; }

        IPropertyValue CreateNewPropertyValue(IEntity entity);
    }

    interface IPropertyValue
    {
        IProperty Property { get; }

        IEntity Entity { get; }

        string StringValue { get; set; }

        long LongValue { get; set; }

        double DoubleValue { get; set; }

        DateTime DateTimeValue { get; set; }

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
        /// Returns all persistent cstProperty types. 
        /// </summary>
        /// <returns></returns>
        IEnumerable<IPropertyType> GetAllPropertyTypes();

        /// <summary>
        /// Returns a new IEntityType instance with 
        /// correct DBIdentity, name set and no associated
        /// properties.
        /// 
        /// The type is not persisted until it is added to the database.
        /// 
        /// (Should be called by the TypeSystem singleton only)
        /// </summary>
        /// <returns></returns>
        IEntityType NewEntityType();

        IEntity NewEntity();

        IProperty NewProperty();

        IPropertyType NewPropertyType();

        //IPropertyValue NewPropertyValue();

        /// <summary>
        /// Returns all IEntityTypes stored in the database.
        /// Is used by the TypeSystem singleton to retrieve
        /// all types upon instantiation.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IEntityType> GetAllEntityTypes();

        IBusinessObject GetEntity(int entityPOID);

        /// <summary>
        /// Clears all elements stored for the 
        /// collection with the given collectionEntityPOID
        /// </summary>
        /// <param name="collectionEntityPOID"></param>
        void ClearCollection(int collectionEntityPOID);

        /// <summary>
        /// Returns all elements pertaining to the collection 
        /// identified by the given ID.
        /// </summary>
        /// <param name="CollectionEntityPOID"></param>
        /// <returns></returns>
        IEnumerable<IGenCollectionElement> AllElements(int collectionEntityPOID);

        /// <summary>
        /// Saves the entityType as well as any unsaved 
        /// super types, properties and cstProperty types.
        /// </summary>
        /// <param name="entityType"></param>
        void Save(IEntityType entityType);

        IEnumerable<IBusinessObject> Where(IExpression expression);

        int Count(IWhereable expression);

        /// <summary>
        /// Assumes that all neccessary IProperty, IEntityType and IPropertyType 
        /// instances has been saved before this method is called.
        /// </summary>
        /// <param name="entity"></param>
        void Save(IEntity entity);

        void Save(IGenCollectionElement ce, int collectionElementPOID, MappingType mt);

        IBusinessObject GetByEntityPOID(int entityPOID);

        void CommitChanges();

        /// <summary>
        /// Commits changes in the type system.
        /// (Properties, EntityTypes and PropertyTypes)
        /// </summary>
        void CommitTypeChanges();
        void RollbackTypeTransaction();
        void RollbackTransaction();

        /// <summary>
        /// Deletes all matching entities from database. 
        /// </summary>
        /// <param name="w"></param>
        bool ClearWhere(IWhereable w);
    }

    interface IGenCollectionElement
    {
        int ElementIndex { get; set; }

        string StringValue { get; set; }

        long LongValue { get; set; }

        double DoubleValue { get; set; }

        DateTime DateTimeValue { get; set; }

        bool BoolValue { get; set; }

        IBOReference RefValue { get; set; }
    }
}
