using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using GenDB.DB;

/* Om instantiering af generiske typer via reflection:
 * http://msdn2.microsoft.com/en-us/library/b8ytshk6.aspx
 */
// registrer generiske typeparameter, hvis ikke registreret i forvejen
namespace GenDB
{

    /// <summary>
    /// The list translator does not populate the collection elements. 
    /// This responsibility is left to the collections.
    /// TODO: Everything ;)
    /// </summary>
    class BOListTranslator : BaseTranslator
    {
        public static readonly Type TypeOfBOList = typeof(BOList<>);
        public static readonly Type TypeOfInternalList = typeof(InternalList<>);
        
        bool elementIsIBusinessObject = true;
        //IEntityType entityType;
        Type elementType; // Type (typeof(T)) parameter of translatable BOList<T> entities.

        /// <summary>
        /// If entityType is null, a new IEntityType instance will be created
        /// </summary>
        /// <param name="t"></param>
        public BOListTranslator(Type t, DataContext dataContext, IEntityType bolistEntityType)
            : base(t, bolistEntityType, dataContext)
        {
            Type genericType = t.GetGenericTypeDefinition();
            if (!t.IsGenericType || (genericType != TypeOfBOList && genericType != TypeOfInternalList))
            {
                throw new NotTranslatableException("Internal error. BOList translator was invoked on wrong type. (Should be BOList<>)", t);
            }
            elementType = t.GetGenericArguments()[0];
            elementIsIBusinessObject = elementType.GetInterface(typeof(IBusinessObject).FullName) != null;
            instantiator = DynamicMethodCompiler.CreateInstantiateObjectHandler (t);
            bolistEntityType.GetProperty(TypeSystem.COLLECTION_ELEMENT_TYPE_PROPERTY_NAME).PropertyType.MappingType = 
                TypeSystem.FindMappingType(elementType);
            if (elementIsIBusinessObject  && ! dataContext.TypeSystem.IsTypeKnown (elementType))
            {
                dataContext.TypeSystem.RegisterType(elementType);
            }
        }

        public new IEntity Translate(IBusinessObject ibo)
        {
            IEntity e = new GenDB.DB.Entity();

            // The mapping type for the elements are stored in this cstProperty. No other values are relevant.
            IProperty elementTypeProperty = EntityType.GetProperty(TypeSystem.COLLECTION_ELEMENT_TYPE_PROPERTY_NAME);

            IPropertyValue pv = elementTypeProperty.CreateNewPropertyValue(e);

            e.EntityType = EntityType;
            pv.StringValue = elementType.FullName;

            if (!ibo.DBIdentity.IsPersistent) 
            {
                dataContext.IBOCache.Add(ibo);
            }
            e.EntityPOID = ibo.DBIdentity; 
            return e;
        }

        public override void SaveToDB(IBusinessObject ibo)
        {
            Type t = ibo.GetType();
            if (!ibo.GetType().IsGenericType || (ibo.GetType().GetGenericTypeDefinition() != TypeOfBOList && ibo.GetType().GetGenericTypeDefinition()  != TypeOfInternalList)) 
            {
                throw new NotTranslatableException("Internal error: BOListTranslator can not translate Type ", ibo.GetType());
            }
            IEntity e = Translate(ibo);
            dataContext.GenDB.Save (e);

            IDBSaveableCollection saveable = (IDBSaveableCollection)ibo;
            saveable.SaveElementsToDB();
        }

        protected override PropertyInfo[] GetPropertiesToTranslate()
        {
            return new PropertyInfo[0];
        }

        public override void SetProperty(long propertyPOID, IBusinessObject obj, object propertyValue)
        {
        }

        public new bool CompareProperties(IBusinessObject a, IBusinessObject b)
        {
            if (a == null && b == null) { return true; }

            if (a == null ^ b == null) { return false; }

            return !((a as IDBSaveableCollection).HasBeenModified || (b as IDBSaveableCollection).HasBeenModified);
        }
    }
}
