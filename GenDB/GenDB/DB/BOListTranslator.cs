using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

/* Om instantiering af generiske typer via reflection:
 * http://msdn2.microsoft.com/en-us/library/b8ytshk6.aspx
 */

namespace GenDB.DB
{

    /// <summary>
    /// The list translator does not instantiate the collection it self. 
    /// This responsibility is left to the different collections.
    /// TODO: Everything ;)
    /// </summary>
    class BOListTranslator : IIBoToEntityTranslator
    {
        static Type typeOfBOList = typeof(BOList<>);

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
        /// <param name="entityType">entityType to use. If entityType is null, a new IEntityType instance will be created</param>
        public BOListTranslator(Type t, IEntityType entityType)
        {
            this.clrType = t;
            if (!clrType.IsGenericType || clrType.GetGenericTypeDefinition() != typeOfBOList)
            {
                throw new NotTranslatableException("Internal error. BOList translator was invoked on wrong type. (Should be BOList<>)", t);
            }
            if (entityType == null)
            {
                this.entityType = BOListEntityType();
            }
            else 
            {
                this.entityType = entityType;
            }
            elementType = clrType.GetGenericArguments()[0];
            elementIsIBusinessObject = elementType.GetInterface(typeof(IBusinessObject).FullName) != null;
            instantiator = DynamicMethodCompiler.CreateInstantiateObjectHandler (clrType);
        }

        public IBusinessObject Translate(IEntity ie)
        {
            IBusinessObject res = IBOCache.Get(ie.EntityPOID);
            if (res == null)
            {
                //IProperty elementTypeProperty = ie.EntityType.GetProperty(TypeSystem.COLLECTION_ELEMENT_TYPE_PROPERTY_NAME);
                //long elementEntityTypePOID = ie.GetPropertyValue(elementTypeProperty).LongValue;
                //IEntityType elementEntityType = TypeSystem.GetEntityType (elementEntityTypePOID);
                //Type elementType = TypeSystem.GetClrType(elementEntityType);
                res = (IBusinessObject)instantiator();
                DBTag.AssignDBTagTo(res, ie.EntityPOID);
            }
            return res;
        }

        public IEntity Translate(IBusinessObject ibo)
        {
            // Move to initializer (test that the TypeSystem knows the element type, if element is of some reference type)
            if (elementIsIBusinessObject && !TypeSystem.IsTypeKnown (elementType))
            {
                TypeSystem.RegisterType (elementType);
            }

            //IEntityType elementEntityType = TypeSystem.GetEntityType(elementType);
            IEntity e = null;
            e = Configuration.GenDB.NewEntity();

            // The mapping type for the elements are stored in this property. No other values are relevant.
            IProperty elementTypeProperty = entityType.GetProperty(TypeSystem.COLLECTION_ELEMENT_TYPE_PROPERTY_NAME);
            elementTypeProperty.MappingType = MappingType.REFERENCE;
            
            
            IPropertyValue pv = Configuration.GenDB.NewPropertyValue();
            //pv.LongValue = elementEntityType.EntityTypePOID;
            e.EntityType = entityType;
            pv.Property = elementTypeProperty;
            pv.Entity = e;
            e.StorePropertyValue(pv);
            if (ibo.DBTag != null) 
            { 
                e.EntityPOID = ibo.DBTag.EntityPOID; 
            }
            else
            { 
                DBTag.AssignDBTagTo(ibo, e.EntityPOID); 
            }

            return e;
        }

        public void SetValues(IBusinessObject ibo, IEntity ie)
        {
            throw new Exception("Not implemented.");
        }

        public void SetValues(IEntity ie, IBusinessObject ibo)
        {
            throw new Exception("Not implemented.");
        }

        public void SaveToDB(IGenericDatabase db, IBusinessObject ibo)
        {
            Type t = ibo.GetType();
            if (!ibo.GetType().IsGenericType || ibo.GetType().GetGenericTypeDefinition() != typeOfBOList)
            {
                throw new NotTranslatableException("Internal error: BOListTranslator can not translate Type ", ibo.GetType());
            }
            IEntity e = Translate(ibo);
            Configuration.GenDB.Save (e);

            IDBSaveableCollection saveable = (IDBSaveableCollection)ibo;
            saveable.SaveElementsToDB();
        }
       
        private IEntityType BOListEntityType()
        {
            IEntityType res = Configuration.GenDB.NewEntityType();
            res.IsList = true;
            res.AssemblyDescription = clrType.Assembly.FullName;
            res.Name = clrType.FullName;

            IPropertyType pt = TypeSystem.GetPropertyType(typeof(long).FullName);
            IProperty property = Configuration.GenDB.NewProperty();
            property.EntityType = res;
            property.PropertyName = TypeSystem.COLLECTION_ELEMENT_TYPE_PROPERTY_NAME;
            property.PropertyType = pt;
            property.MappingType = MappingType.LONG;
            res.AddProperty (property);
            return res;
        }
    }
}
