using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using GenDB.DB;

namespace GenDB
{
    internal class PropertyConverterLazy : IPropertyConverter
    {
        FieldInfo lazyLoaderField = null;
        long propertyPOID = 0;
        PropertySetter ps = null;
        SetHandler setHandler;
        InstantiateObjectHandler instantiator;
        Type propertyType;
        IProperty iproperty;
        GetHandler gh;
        DataContext dataContext;

        public PropertyConverterLazy (Type t, FieldInfo llField, IProperty prop, DataContext dc, PropertyInfo propInfo)
        {
            lazyLoaderField = llField;

            dataContext = dc;

            propertyType = propInfo.PropertyType;

            iproperty = prop;

            propertyPOID = prop.PropertyPOID;

            instantiator = DynamicMethodCompiler.CreateInstantiateObjectHandler(llField.FieldType);

            gh = DynamicMethodCompiler.CreateGetHandler(t, propInfo);

            ps = delegate(IBusinessObject ibo, object value)
            {
                // We can cast to LazyLoader, since this is a subtype of LazyLoader<T>
                LazyLoader fieldValue = (instantiator() as LazyLoader);
                if (value == null)
                {
                    fieldValue.entityPOID = 0;
                    fieldValue.IsLoaded = true;
                }
                else
                {
                    fieldValue.entityPOID = Convert.ToInt32(value);
                    fieldValue.IsLoaded = false;
                }
            };
        }

        #region IPropertyConverter Members

        public long PropertyPOID
        {
            get
            {
                return propertyPOID;
            }
            set
            {
                this.propertyPOID = value;
            }
        }

        public Type PropertyType
        {
            get { return propertyType; }
        }

        public bool ReferenceCompare
        {
            get
            {
                return true;
            }
            set
            {
                return;
            }
        }

        public void SetEntityPropertyValue(IBusinessObject ibo, GenDB.DB.IEntity e)
        {
            // This class is only used with IBOReference properties...
            object reference = gh(ibo);
            IPropertyValue pv = iproperty.CreateNewPropertyValue(e);
            if (reference == null)
            {
                pv.RefValue = new IBOReference(true);
            }
            else
            {
                IBusinessObject propibo = (reference as IBusinessObject);
                IBOCache cache = DataContext.Instance.IBOCache;
                if (!propibo.DBIdentity.IsPersistent)
                {
                    IEntity refered = dataContext.GenDB.NewEntity();
                    cache.Add(propibo, refered.EntityPOID);
                    IBOReference iboreference = new IBOReference(propibo.DBIdentity);
                    e.GetPropertyValue(iproperty).RefValue = iboreference;
                }
                else
                {
                    IBOReference iboreference = new IBOReference(ibo.DBIdentity);
                    e.GetPropertyValue(iproperty).RefValue = iboreference;
                }
            }
        }

        public PropertySetter PropertySetter
        {
            get { return ps; }
        }

        #endregion
    }
}
