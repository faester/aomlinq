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

        SetHandler fieldSetter;
        GetHandler fieldGetter;



        public PropertyConverterLazy (Type t, FieldInfo llField, IProperty prop, DataContext dc, PropertyInfo propInfo)
        {
            lazyLoaderField = llField;

            dataContext = dc;

            propertyType = propInfo.PropertyType;

            iproperty = prop;

            propertyPOID = prop.PropertyPOID;

            fieldSetter = DynamicMethodCompiler.CreateSetHandler(t, llField);
            fieldGetter = DynamicMethodCompiler.CreateGetHandler(t, llField);

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
                fieldSetter(ibo, fieldValue);
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
            // Check if we can do with the LazyLoader object to avoid retrieving 
            // the element from db. (Might speed up saving the object holding the reference.)
            IPropertyValue pv = iproperty.CreateNewPropertyValue(e);

            LazyLoader ll = (fieldGetter(ibo) as LazyLoader);
            //if ( ll == null || (ll.IsLoaded && ll.IsNullReference))
            //{
            //    pv.RefValue = new IBOReference(true);
            //    return;
            //}

            if (ll.IsLoaded && ll.entityPOID != 0)
            {
                pv.RefValue = new IBOReference(ll.entityPOID);
                e.StorePropertyValue(pv);
                return;
            }

            // This class is only used with IBOReference properties...
            object reference = gh(ibo);
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
                    cache.Add(propibo);
                    IBOReference iboreference = new IBOReference(propibo.DBIdentity);
                    e.GetPropertyValue(iproperty).RefValue = iboreference;
                }
                else
                {
                    IBOReference iboreference = new IBOReference(propibo.DBIdentity);
                    e.GetPropertyValue(iproperty).RefValue = iboreference;
                }
            }
        }

        internal SetHandler PropertySetHandler
        {
            get { return this.setHandler; }
        }

        public PropertySetter PropertySetter
        {
            get { return ps; }
        }

        internal GetHandler PropertyGetHandler
        {
            get { return gh; }
        }

        #endregion

        #region IPropertyConverter Members

        public void CloneProperty(object source, object target)
        {
            fieldSetter(target, fieldGetter(source));
        }

        #endregion

        #region IPropertyConverter Members


        public bool CompareProperties(object a, object b)
        {
            LazyLoader la = (fieldGetter(a) as LazyLoader);
            LazyLoader lb = (fieldGetter(a) as LazyLoader);
            return ((la == null && lb == null) || (la.IsLoaded == lb.IsLoaded && la.entityPOID == lb.entityPOID));
        }

        #endregion
    }
}
