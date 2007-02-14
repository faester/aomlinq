using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using GenDB.DB;

namespace GenDB
{
    class BODictionaryTranslator : IIBoToEntityTranslator
    {
        public static readonly Type TypeOfBODictionary = typeof(BODictionary<,>);
        
        DataContext dataContext;
        InstantiateObjectHandler instantiator;
        bool elementIsIBusinessObject = true;
        IEntityType entityType;
        Type clrType; // The Type of objects translatable by this translator (some Dictionary<,> type)
        Type elementType; // Type (typeof(V)) parameter of translatable BODictionary<K, V> V-entities.
        Type keyType; // Type (typeof(K)) parameter of translatable BODictionary<K, V> K-entities.

        IEnumerable<PropertyConverter> fieldConverters;

        public IEntityType EntityType
        {
            get {return entityType;}
        }

        private BODictionaryTranslator() { /* empty */}

        public BODictionaryTranslator(Type t, DataContext dataContext, IEntityType bodictEntityType)
        {
            this.clrType = t;
            if (!clrType.IsGenericType || clrType.Name.Substring(0,6) != "BODict" /*clrType.GetGenericTypeDefinition() != TypeOfBODictionary*/ )
            {
                throw new NotTranslatableException("Internal error. BOList translator was invoked on wrong type. (Should be BOList<>)", t);
            }

            this.dataContext = dataContext;
            this.entityType = bodictEntityType;
            keyType = clrType.GetGenericArguments()[0];
            elementType = clrType.GetGenericArguments()[1];
            elementIsIBusinessObject = elementType.GetInterface(typeof(IBusinessObject).FullName) != null;
            instantiator = DynamicMethodCompiler.CreateInstantiateObjectHandler (clrType);
            
            bodictEntityType.GetProperty(TypeSystem.COLLECTION_ELEMENT_TYPE_PROPERTY_NAME).PropertyType.MappingType = 
                TypeSystem.FindMappingType(elementType);
            if (elementIsIBusinessObject  && ! dataContext.TypeSystem.IsTypeKnown (elementType))
            {
                dataContext.TypeSystem.RegisterType(elementType);
            }
        }

        public IEntity Translate(IBusinessObject ibo)
        {
            //throw new Exception("not implemented");
            ////IEntityType elementEntityType = TypeSystem.GetEntityType(elementType);
            IEntity e = null;
            e = dataContext.GenDB.NewEntity();

            //// The mapping type for the elements are stored in this cstProperty. 
            IProperty elementTypeProperty = entityType.GetProperty(TypeSystem.COLLECTION_ELEMENT_TYPE_PROPERTY_NAME);
            IProperty keyTypeProperty = entityType.GetProperty(TypeSystem.COLLECTION_KEY_PROPERTY_NAME);

            IPropertyValue elementValue = elementTypeProperty.CreateNewPropertyValue(e);
            IPropertyValue keyValue = keyTypeProperty.CreateNewPropertyValue(e);

            e.EntityType = entityType;
            elementValue.StringValue = elementType.FullName;
            keyValue.StringValue = keyType.FullName;

            if (ibo.DBIdentity.IsPersistent) 
            { 
                e.EntityPOID = ibo.DBIdentity; 
            }
            else
            { 
                dataContext.IBOCache.Add(ibo, e.EntityPOID);
            }

            return e;
        }

        /// <summary>
        /// No action. (Handled internally by the BOList it self)
        /// </summary>
        /// <param name="ie"></param>
        /// <param name="res"></param>
        public void SetValues(IBusinessObject ibo, IEntity ie) { /* empty */ }

        /// <summary>
        /// No action. (Handled internally by the BOList it self)
        /// </summary>
        /// <param name="ie"></param>
        /// <param name="res"></param>
        public void SetValues(IEntity ie, IBusinessObject ibo) { /* empty */ }

        public void SaveToDB(IGenericDatabase db, IBusinessObject ibo)
        {
            //throw new Exception("not implemented");
            Type t = ibo.GetType();
            if (!ibo.GetType().IsGenericType || t.Name.Substring(0,6)!="BODict" /*ibo.GetType().GetGenericTypeDefinition() != TypeOfBOList*/ )
            {
                throw new NotTranslatableException("Internal error: BOListTranslator can not translate Type ", ibo.GetType());
            }
            IEntity e = Translate(ibo);
            dataContext.GenDB.Save (e);

            IDBSaveableCollection saveable = (IDBSaveableCollection)ibo;
            saveable.SaveElementsToDB();
        }

        public IBusinessObject CreateInstanceOfIBusinessObject()
        {
           return (IBusinessObject)instantiator();
        }

        public void SetProperty(long propertyPoid, IBusinessObject ibo, object obj)
        {
            //
        }

        public IEnumerable<PropertyConverter> FieldConverters
        {
            
            get { return new PropertyConverter[0]; }
        }
        public PropertyConverter GetPropertyConverter(int propertyPOID)
        {
            throw new Exception("Not implemented");
        }

        public PropertyConverter GetPropertyConverter(IProperty property)
        {
            return GetPropertyConverter(property.PropertyPOID);
        }

    }
}
