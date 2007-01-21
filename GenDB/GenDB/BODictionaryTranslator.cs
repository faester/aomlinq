using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using GenDB.DB;

namespace GenDB
{
    class BODictionaryTranslator : IIBoToEntityTranslator
    {
        //public static readonly Type TypeOfBODictionary = typeof(BODictionary<>);
        
        IEntityType entityType;
        IEnumerable<FieldConverter> fieldConverters;

        public IEntityType EntityType
        {
            get {return entityType;}
        }

        public IEntity Translate(IBusinessObject ibo)
        {
            throw new Exception("not implemented");
            ////IEntityType elementEntityType = TypeSystem.GetEntityType(elementType);
            //IEntity e = null;
            //e = dataContext.GenDB.NewEntity();

            //// The mapping type for the elements are stored in this cstProperty. No other values are relevant.
            //IProperty elementTypeProperty = entityType.GetProperty(TypeSystem.COLLECTION_ELEMENT_TYPE_PROPERTY_NAME);

            //IPropertyValue pv = elementTypeProperty.CreateNewPropertyValue(e);

            //e.EntityType = entityType;
            //pv.StringValue = elementType.FullName;

            //if (ibo.DBIdentity.IsPersistent) 
            //{ 
            //    e.EntityPOID = ibo.DBIdentity; 
            //}
            //else
            //{ 
            //    dataContext.IBOCache.Add(ibo, e.EntityPOID);
            //}

            //return e;
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
            throw new Exception("not implemented");
            //Type t = ibo.GetType();
            //if (!ibo.GetType().IsGenericType || ibo.GetType().GetGenericTypeDefinition() != TypeOfBOList)
            //{
            //    throw new NotTranslatableException("Internal error: BOListTranslator can not translate Type ", ibo.GetType());
            //}
            //IEntity e = Translate(ibo);
            //dataContext.GenDB.Save (e);

            //IDBSaveableCollection saveable = (IDBSaveableCollection)ibo;
            //saveable.SaveElementsToDB();
        }

        public IBusinessObject CreateInstanceOfIBusinessObject()
        {
            throw new Exception("not implemented");
        }

        public void SetProperty(long propertyPoid, IBusinessObject ibo, object obj)
        {
            //
        }

        public IEnumerable<FieldConverter> FieldConverters 
        {
            get { return new FieldConverter[0]; }
        }
    }
}
