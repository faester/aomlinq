using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using GenDB.DB;

namespace GenDB
{
    /*
     * http://www.codeproject.com/csharp/csharpcasts.asp
     */

    /// <summary>
    /// Used to convert between a property of a class and an IPropertyValue 
    /// </summary>
    class PropertyConverter : GenDB.IPropertyConverter
    {
        PropertyValueSetter pvs;
        PropertySetter propertySetter;

        public PropertySetter PropertySetter
        {
            get { return propertySetter; }
        }

        SetHandler propertySetHandler;

        internal SetHandler PropertySetHandler
        {
            get { return propertySetHandler; }
        }

        GetHandler propertyGetHandler;

        internal GetHandler PropertyGetHandler
        {
            get { return propertyGetHandler; }
        }


        bool referenceCompare = false;

        public bool ReferenceCompare
        {
            get { return referenceCompare; }
            set { referenceCompare = value; }
        }

        Type clrType;

        Type propertyType;

        public Type PropertyType
        {
            get { return propertyType; }
        }

        PropertyInfo propertyInfo;
        DataContext dataContext;
        IProperty property;

        internal IProperty Property
        {
            get { return property; }
        }

        long propertyPOID;

        public long PropertyPOID
        {
            get { return propertyPOID; }
            set { propertyPOID = value; }
        }

        public PropertyConverter(Type t, PropertyInfo propInfo, IProperty property, DataContext dataContext)
        {
            this.dataContext = dataContext;
            this.propertyInfo = propInfo;
            this.propertyPOID = property.PropertyPOID;
            this.property = property;
            clrType = t;
            propertySetHandler = DynamicMethodCompiler.CreateSetHandler(t, propInfo);
            propertyGetHandler = DynamicMethodCompiler.CreateGetHandler(t, propInfo);
            propertySetter = CreatePropertySetter(propInfo);
            //pvg = CreateGetter(propInfo, property);
            pvs = CreateSetter(property);
            propertyType = propInfo.PropertyType;
            referenceCompare = !propertyType.IsPrimitive && propertyType != typeof(string) && propertyType != typeof(DateTime);
        }

        public void SetEntityPropertyValue(IBusinessObject source, IEntity target)
        {
            pvs(target, propertyGetHandler(source));
        }

        PropertyValueSetter CreateSetter(IProperty p)
        {
            switch (p.MappingType)
            {
                case MappingType.BOOL:
                    return delegate(IEntity e, object value)
                    {
                        e.GetPropertyValue(p).BoolValue = Convert.ToBoolean(value);
                    };
                case MappingType.DATETIME:
                    return delegate(IEntity e, object value)
                    {
                        e.GetPropertyValue(p).DateTimeValue = Convert.ToDateTime(value);
                    };
                case MappingType.DOUBLE:
                    return delegate(IEntity e, object value)
                    {
                        e.GetPropertyValue(p).DoubleValue = Convert.ToDouble(value);
                    };
                case MappingType.LONG:
                    return delegate(IEntity e, object value)
                    {
                        e.GetPropertyValue(p).LongValue = Convert.ToInt64(value);
                    };
                case MappingType.REFERENCE:
                    return delegate(IEntity e, object value)
                    {
                        if (value == null)
                        {
                            IBOReference reference = new IBOReference(true);
                            e.GetPropertyValue(p).RefValue = reference;
                            return;
                        }
                        IBusinessObject ibo = (IBusinessObject)value;
                        if (!ibo.DBIdentity.IsPersistent)
                        {
                            dataContext.IBOCache.Add(ibo);
                            IBOReference reference = new IBOReference(ibo.DBIdentity);
                            e.GetPropertyValue(p).RefValue = reference;
                        }
                        else
                        {
                            IBOReference reference = new IBOReference(ibo.DBIdentity);
                            e.GetPropertyValue(p).RefValue = reference;
                        }
                    };
                case MappingType.STRING:
                    return delegate(IEntity e, object value)
                    {
                        e.GetPropertyValue(p).StringValue = value == null ? null : Convert.ToString(value);
                    };
                default:
                    throw new Exception("Unknown MappingType in DelegateTranslator, CreateSetter: " + p.MappingType);
            }
        }

        private PropertySetter CreatePropertySetter(PropertyInfo propInfo)
        {
            if ((propInfo.PropertyType.IsByRef || propInfo.PropertyType.IsClass)
                && !(propInfo.PropertyType == typeof(string) || propInfo.PropertyType == typeof(DateTime))
                )
            { // Handles references other than string and DateTime
                return delegate(IBusinessObject ibo, object value)
                {
                    if (value == DBNull.Value || value == null)
                    {
                        propertySetHandler(ibo, null);
                        return;
                    }

                    int refEntityPOID = ((IConvertible)value).ToInt32(null);

                    IBusinessObject iboVal = null;

                    if (this.dataContext.IBOCache.TryGet(refEntityPOID, out iboVal))
                    {
                        propertySetHandler(ibo, iboVal);
                    }
                    else
                    {
                        IExpression where = new BoolEquals(new CstLong(refEntityPOID), CstThis.Instance);
                        // TODO: Kunne nok gøres hurtigere...
                        iboVal = dataContext.GenDB.GetByEntityPOID(refEntityPOID);
                        propertySetHandler(ibo, iboVal);
                        return;
                    }
                };
            }
            else if (propInfo.PropertyType == typeof(long))
            {
                return delegate(IBusinessObject ibo, object value)
                {
                    propertySetHandler(ibo, ((IConvertible)value).ToInt64(null));
                };
            }
            else if (propInfo.PropertyType == typeof(int))
            {
                return delegate(IBusinessObject ibo, object value)
                {
                    propertySetHandler(ibo, ((IConvertible)value).ToInt32(null));
                };
            }
            else if (propInfo.PropertyType == typeof(string))
            {
                return delegate(IBusinessObject ibo, object value)
                {
                    propertySetHandler(ibo, (value as string));
                };
            }
            else if (propInfo.PropertyType == typeof(DateTime))
            {
                return delegate(IBusinessObject ibo, object value)
                {
                    propertySetHandler(ibo, (DateTime)value);
                };
            }
            else if (propInfo.PropertyType == typeof(bool))
            {
                return delegate(IBusinessObject ibo, object value)
                {
                    propertySetHandler(ibo, Convert.ToBoolean(value));
                };
            }
            else if (propInfo.PropertyType == typeof(char))
            {
                return delegate(IBusinessObject ibo, object value)
                {  //
                    propertySetHandler(ibo, ((IConvertible)value).ToChar(null));
                };
            }
            else if (propInfo.PropertyType == typeof(float))
            {
                return delegate(IBusinessObject ibo, object value)
                {  //
                    propertySetHandler(ibo, ((IConvertible)value).ToSingle(null));
                };
            }
            else if (propInfo.PropertyType == typeof(double))
            {
                return delegate(IBusinessObject ibo, object value)
                {  //
                    propertySetHandler(ibo, ((IConvertible)value).ToDouble(null));
                };
            }
            else if (propInfo.PropertyType.IsEnum)
            {
                return delegate(IBusinessObject ibo, object value)
                {
                    // Oversætter til array indeholdende alle mulige værdier for den pågældende enum.
                    // Dernæst vælges værdien gemt index Long-feltet index den pågældende PropertyValue record.
                    System.Collections.IList vals = (System.Collections.IList)Enum.GetValues(propInfo.PropertyType);
                    propertySetHandler(ibo, vals[Convert.ToInt32(value)]);
                    //return Enum.GetValues (propInfo.PropertyType). ((int)ie.GetPropertyValue(prop).LongValue);
                };
            }
            else if (propInfo.PropertyType == typeof(short))
            {
                return delegate(IBusinessObject ibo, object value)
                {  //
                    propertySetHandler(ibo, ((IConvertible)value).ToInt16(null));
                };
            }
            else if (propInfo.PropertyType == typeof(uint))
            {
                return delegate(IBusinessObject ibo, object value)
                {  //
                    propertySetHandler(ibo, ((IConvertible)value).ToUInt32(null));
                };
            }
            else
            {
                throw new NotTranslatableException("Have not implemented PropertySetter for field type.", propInfo);
            }
        }

        #region IPropertyConverter Members

        public void CloneProperty(object source, object target)
        {
            PropertySetHandler(target, PropertyGetHandler(source));
        }

        #endregion

        #region IPropertyConverter Members


        public bool CompareProperties(object a, object b)
        {
            if (ReferenceCompare)
            {
                return Object.ReferenceEquals(PropertyGetHandler(a), PropertyGetHandler(b));
            }
            else 
            {
                try{
                    object vala = PropertyGetHandler(a);
                    object valb = PropertyGetHandler(b);
                    // String are compared using equals, so test if both are null.
                    if (vala == null && valb == null)
                    {
                        return true;
                    }
                    else
                    {
                        return vala.Equals(valb);
                    }
                }
                catch(NullReferenceException nr)
                {
                    Console.WriteLine(this.propertyInfo);
                    Console.WriteLine(a.GetType());
                    Console.WriteLine(b.GetType());
                    throw nr;
                }
            }
        }

        #endregion
    }
}
