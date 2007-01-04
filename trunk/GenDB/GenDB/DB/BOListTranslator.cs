using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

/* Om instantiering af generiske typer via reflection:
 * http://msdn2.microsoft.com/en-us/library/b8ytshk6.aspx
 */

namespace GenDB.DB
{
    //class OneElement<T>
    //{
    //    T theElement;

    //    public T TheElement
    //    {
    //        get { return theElement; }
    //        set { theElement = value; }
    //    }

    //}

    /// <summary>
    /// The list translator does not instantiate the collection it self. 
    /// This responsibility is left to the different collections.
    /// TODO: Everything ;)
    /// </summary>
    class BOListTranslator : IIBoToEntityTranslator
    {
        Type typeOfBOList = typeof(BOList<>);
        IEntityType genericBOListEntityType = null;


        //public void HowTheFuck()
        //{
        //    OneElement<t> e = new OneElement<t>();

        //    Type theType = typeof(int);
        //    object o = typeOfBOList.MakeGenericType(theType);
        //}

        public IBusinessObject Translate(IEntity ie)
        {
            IBusinessObject res = IBOCache.Get(ie.EntityPOID);
            if (res != null)
            {
                return res;
            }
            else
            {
                IProperty elementTypeProperty = ie.EntityType.GetProperty(TypeSystem.COLLECTION_ELEMENT_TYPE_PROPERTY_NAME);
                long elementEntityTypePOID = ie.GetPropertyValue(elementTypeProperty).LongValue;
                IEntityType elementEntityType = TypeSystem.GetEntityType (elementEntityTypePOID);

                Type elementType = TypeSystem.GetClrType(elementEntityType);

                return (IBusinessObject)typeOfBOList.MakeGenericType(elementType);
            }
        }

        public IEntity Translate(IBusinessObject ibo)
        {
            Type t = ibo.GetType();
            if (t.GetGenericTypeDefinition() != typeOfBOList)
            {
                throw new NotTranslatableException("Internal error. BOList translator was invoked on wrong type. (Should be BOList<>)", ibo.GetType());
            }
            Type elementType = t.GetGenericArguments()[0];

            if (!TypeSystem.IsTypeKnown (typeOfBOList))
            {
                genericBOListEntityType = BOListEntityType();
                TypeSystem.RegisterType(genericBOListEntityType);
            }
            else if (genericBOListEntityType == null)
            {
                genericBOListEntityType = TypeSystem.GetEntityType(typeOfBOList);
            }

            if (!TypeSystem.IsTypeKnown (elementType))
            {
                TypeSystem.RegisterType (elementType);
            }

            IEntityType elementEntityType = TypeSystem.GetEntityType(elementType);

            IEntity e = null;

            //if (ibo.DBTag != null)
            //{
            //    e = Configuration.GenDB.GetEntity(ibo.DBTag.EntityPOID);
            //}
            //else
            //{
            e = Configuration.GenDB.NewEntity();
            IProperty elementTypeProperty = genericBOListEntityType.GetProperty(TypeSystem.COLLECTION_ELEMENT_TYPE_PROPERTY_NAME);
            IPropertyValue pv = Configuration.GenDB.NewPropertyValue();
            pv.LongValue = elementEntityType.EntityTypePOID;
            e.EntityType = genericBOListEntityType;
            pv.Property = elementTypeProperty;
            pv.Entity = e;
            e.StorePropertyValue(pv);
            if (ibo.DBTag != null) { e.EntityPOID = ibo.DBTag.EntityPOID; }
            else { DBTag.AssignDBTagTo(ibo, e.EntityPOID); }
            //}

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
            IEntityType res = Configuration.GenDB.NewEntityType(typeOfBOList.FullName);
            res.IsList = true;
            res.AssemblyDescription = typeOfBOList.Assembly.FullName;
            
            res.Name = typeOfBOList.FullName;

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
