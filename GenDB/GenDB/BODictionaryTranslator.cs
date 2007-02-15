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
        public static readonly string MAPPING_PROPERTY_NAME = "Mapping";
        DataContext dataContext;
        InstantiateObjectHandler instantiator;
        bool elementIsIBusinessObject = true;
        IEntityType entityType;
        Type clrType; // The Type of objects translatable by this translator (some Dictionary<,> type)
        //Type elementType; // Type (typeof(V)) parameter of translatable BODictionary<K, V> V-entities.
        //Type keyType; // Type (typeof(K)) parameter of translatable BODictionary<K, V> K-entities.

        IEnumerable<PropertyConverter> fieldConverters;

        IIBoToEntityTranslator superTranslator;


        public IEntityType EntityType
        {
            get {return entityType;}
        }

        private BODictionaryTranslator() { /* empty */}


        public BODictionaryTranslator(Type t, DataContext dataContext, IEntityType bodictEntityType)
        {
            this.clrType = t;
            if (!clrType.IsGenericType || clrType.GetGenericTypeDefinition() != TypeOfBODictionary)
            { // Strictly for debugging. Should be guaranteed elsewhere.
                throw new NotTranslatableException("Internal error. BODictionary translator was invoked on wrong type. (Should be BOList<>)", t);
            }

            this.dataContext = dataContext;
            this.entityType = bodictEntityType;

            instantiator = DynamicMethodCompiler.CreateInstantiateObjectHandler (clrType);
            
            if (bodictEntityType.SuperEntityType != null)
            {
                superTranslator = dataContext.Translators.GetTranslator(bodictEntityType.SuperEntityType.EntityTypePOID);
            }
        }

        public IEntity Translate(IBusinessObject ibo)
        {
            IEntity res = dataContext.GenDB.NewEntity();

            // Drop the db-created DBIdentity if element is persistent.
            if (ibo.DBIdentity.IsPersistent)
            {
                res.EntityPOID = ibo.DBIdentity;
            }
            else
            { // Not persistent. Add it to cache/db, and assign tag
                dataContext.IBOCache.Add(ibo, res.EntityPOID);
            }

            res.EntityType = entityType;
 
            SetValues(ibo, res);

            return res;
        }

        /// <summary>
        /// No action. (Handled internally by the BOList it self)
        /// </summary>
        /// <param name="ie"></param>
        /// <param name="res"></param>
        public void SetValues(IBusinessObject ibo, IEntity ie) 
        {
            if (superTranslator != null)
            {
                superTranslator.SetValues(ibo, ie);
            }
        }

        /// <summary>
        /// No action. (Handled internally by the BOList it self)
        /// </summary>
        /// <param name="ie"></param>
        /// <param name="res"></param>
        public void SetValues(IEntity ie, IBusinessObject ibo) {             
        }

        public void SaveToDB(IBusinessObject ibo)
        {
            //throw new Exception("not implemented");
            Type t = ibo.GetType();
            if (!t.IsGenericType || t.GetGenericTypeDefinition() != TypeOfBODictionary )
            {
                throw new NotTranslatableException("Internal error: BOListTranslator can not translate Type ", ibo.GetType());
            }
            IEntity e = Translate(ibo);
            dataContext.GenDB.Save (e);

            (ibo as IDBSaveableCollection).SaveElementsToDB();
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
