using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using GenDB.DB;

/* Om instantiering af generiske typer via reflection:
 * http://msdn2.microsoft.com/en-us/library/b8ytshk6.aspx
 */

namespace GenDB
{

    /// <summary>
    /// The list translator does not populate the collection elements. 
    /// This responsibility is left to the collections.
    /// TODO: Everything ;)
    /// </summary>
    class BOListTranslator : IIBoToEntityTranslator
    {
        public static readonly Type TypeOfBOList = typeof(BOList<>);

        DataContext dataContext;
        InstantiateObjectHandler instantiator;
        bool elementIsIBusinessObject = true;
        IEntityType entityType;
        Type clrType; // The Type of objects translatable by this translator (some BOList<> type)
        Type elementType; // Type (typeof(T)) parameter of translatable BOList<T> entities.

        public IEntityType EntityType
        {
            get { return entityType; }
        }

        private BOListTranslator() { /* empty */ }

        /// <summary>
        /// If entityType is null, a new IEntityType instance will be created
        /// </summary>
        /// <param name="t"></param>
        public BOListTranslator(Type t, DataContext dataContext, IEntityType bolistEntityType)
        {
            this.clrType = t;
            if (!clrType.IsGenericType || clrType.GetGenericTypeDefinition() != TypeOfBOList)
            {
                throw new NotTranslatableException("Internal error. BOList translator was invoked on wrong type. (Should be BOList<>)", t);
            }
            this.dataContext = dataContext;
            this.entityType = bolistEntityType;
            elementType = clrType.GetGenericArguments()[0];
            elementIsIBusinessObject = elementType.GetInterface(typeof(IBusinessObject).FullName) != null;
            instantiator = DynamicMethodCompiler.CreateInstantiateObjectHandler (clrType);
            bolistEntityType.GetProperty(TypeSystem.COLLECTION_ELEMENT_TYPE_PROPERTY_NAME).PropertyType.MappedType = 
                dataContext.TypeSystem.FindMappingType(elementType);
            if (elementIsIBusinessObject  && ! dataContext.TypeSystem.IsTypeKnown (elementType))
            {
                dataContext.TypeSystem.RegisterType(elementType);
            }
        }

        public IBusinessObject Translate(IEntity ie)
        {
            IBusinessObject res = dataContext.IBOCache.Get(ie.EntityPOID);
            if (res == null)
            {
                //IProperty elementTypeProperty = ie.EntityType.GetProperty(TypeSystem.COLLECTION_ELEMENT_TYPE_PROPERTY_NAME);
                //long elementEntityTypePOID = ie.GetPropertyValue(elementTypeProperty).LongValue;
                //IEntityType elementEntityType = TypeSystem.GetEntityType (elementEntityTypePOID);
                //Type elementType = TypeSystem.GetClrType(elementEntityType);
                res = (IBusinessObject)instantiator();
                dataContext.IBOCache.Add (res, ie.EntityPOID);
            }
            return res;
        }

        public IEntity Translate(IBusinessObject ibo)
        {
            //IEntityType elementEntityType = TypeSystem.GetEntityType(elementType);
            IEntity e = null;
            e = dataContext.GenDB.NewEntity();

            // The mapping type for the elements are stored in this cstProperty. No other values are relevant.
            IProperty elementTypeProperty = entityType.GetProperty(TypeSystem.COLLECTION_ELEMENT_TYPE_PROPERTY_NAME);
            
            IPropertyValue pv = dataContext.GenDB.NewPropertyValue();
            //pv.LongValue = elementEntityType.EntityTypePOID;
            e.EntityType = entityType;
            pv.Property = elementTypeProperty;
            pv.StringValue = elementType.FullName;
            pv.Entity = e;
            e.StorePropertyValue(pv);
            if (ibo.DBTag != null) 
            { 
                e.EntityPOID = ibo.DBTag.EntityPOID; 
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
        /// <param name="ibo"></param>
        public void SetValues(IBusinessObject ibo, IEntity ie) { /* empty */ }

        /// <summary>
        /// No action. (Handled internally by the BOList it self)
        /// </summary>
        /// <param name="ie"></param>
        /// <param name="ibo"></param>
        public void SetValues(IEntity ie, IBusinessObject ibo) { /* empty */ }

        public void SaveToDB(IGenericDatabase db, IBusinessObject ibo)
        {
            Type t = ibo.GetType();
            if (!ibo.GetType().IsGenericType || ibo.GetType().GetGenericTypeDefinition() != TypeOfBOList)
            {
                throw new NotTranslatableException("Internal error: BOListTranslator can not translate Type ", ibo.GetType());
            }
            IEntity e = Translate(ibo);
            dataContext.GenDB.Save (e);

            IDBSaveableCollection saveable = (IDBSaveableCollection)ibo;
            saveable.SaveElementsToDB();
        }

    }
}
