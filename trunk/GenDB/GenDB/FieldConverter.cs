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
    class FieldConverter
    {
        PropertyValueGetter pvg;
        PropertyValueSetter pvs;
        PropertySetter propertySetter;

        internal PropertySetter PropertySetter
        {
            get { return propertySetter; }
        }

        SetHandler fieldSetHandler;
        GetHandler fieldGetHandler;
        Type clrType;
        PropertyInfo propertyInfo;
        DataContext dataContext;

        long propertyPOID;

        public long PropertyPOID
        {
            get { return propertyPOID; }
            set { propertyPOID = value; }
        }

        public FieldConverter(Type t, PropertyInfo propInfo, IProperty property, DataContext dataContext)
        {
            this.dataContext = dataContext;
            this.propertyInfo = propInfo;
            this.propertyPOID = property.PropertyPOID;
            clrType = t;
            fieldSetHandler = DynamicMethodCompiler.CreateSetHandler(t, propInfo);
            fieldGetHandler = DynamicMethodCompiler.CreateGetHandler(t, propInfo);
            propertySetter = CreatePropertySetter(propInfo);
            pvg = CreateGetter(propInfo, property);
            pvs = CreateSetter(property);
        }

        public void SetObjectFieldValue(IBusinessObject ibo, IEntity entity)
        {
            object value = pvg(entity);
            fieldSetHandler(ibo, value);
        }

        public void SetEntityPropertyValue(IBusinessObject ibo, IEntity e)
        {
            pvs(e, fieldGetHandler(ibo));
        }

        private PropertyValueGetter CreateGetter(PropertyInfo propInfo, IProperty prop)
        {
            if ((propInfo.PropertyType.IsByRef || propInfo.PropertyType.IsClass) 
                && !(propInfo.PropertyType == typeof(string) || propInfo.PropertyType == typeof(DateTime))
                )
            { // Handles references other than string and DateTime
                return delegate(IEntity ie)
                {
                    IBOReference entityRef = (IBOReference)(ie.GetPropertyValue(prop).RefValue);
                    if (!entityRef.IsNullReference)
                    {
                        IBusinessObject res = dataContext.IBOCache.Get(entityRef.EntityPOID);
                        if (res == null)
                        {
                            IEntity e = dataContext.GenDB.GetEntity(entityRef.EntityPOID);
                            //IIBoToEntityTranslator trans = TypeSystem.RegisterTranslator(clrType);
                            IIBoToEntityTranslator trans = dataContext.Translators.GetTranslator(e.EntityType.EntityTypePOID);
                            res = trans.Translate(e);
                        }
                        return res;
                    }
                    else
                    {
                        return null;
                    }
                };
            }
            if (propInfo.PropertyType == typeof(long))
            {
                return delegate(IEntity ie)
                {
                    return ie.GetPropertyValue(prop).LongValue;
                };
            }
            else if (propInfo.PropertyType == typeof(int))
            {
                return delegate(IEntity ie) { return (int)ie.GetPropertyValue(prop).LongValue; };
            }
            else if (propInfo.PropertyType == typeof(string))
            {
                return delegate(IEntity ie)
                {
                    return ie.GetPropertyValue(prop).StringValue;
                };
            }
            else if (propInfo.PropertyType == typeof(DateTime))
            {
                return delegate(IEntity ie)
                {
                    return ie.GetPropertyValue(prop).DateTimeValue;
                };
            }
            else if (propInfo.PropertyType == typeof(bool))
            {
                return delegate(IEntity ie) { return ie.GetPropertyValue(prop).BoolValue; };
            }
            else if (propInfo.PropertyType == typeof(char))
            {
                return delegate(IEntity ie) {  //
                    return Convert.ToChar(ie.GetPropertyValue(prop).LongValue);
                };
            }
            else if (propInfo.PropertyType == typeof(float))
            {
                return delegate(IEntity ie)
                {  //
                    return Convert.ToSingle(ie.GetPropertyValue(prop).DoubleValue);
                };
            }
            else if (propInfo.PropertyType == typeof(double))
            {
                return delegate(IEntity ie)
                {  //
                    return ie.GetPropertyValue(prop).DoubleValue;
                };
            }
            else if (propInfo.PropertyType.IsEnum)
            {
                return delegate (IEntity ie)
                {
                    // Overs�tter til array indeholdende alle mulige v�rdier for den p�g�ldende enum.
                    // Dern�st v�lges v�rdien gemt index Long-feltet index den p�g�ldende PropertyValue record.
                    System.Collections.IList vals =  (System.Collections.IList) Enum.GetValues(propInfo.PropertyType);
                    return vals[(int)ie.GetPropertyValue(prop).LongValue];
                    //return Enum.GetValues (propInfo.PropertyType). ((int)ie.GetPropertyValue(prop).LongValue);
                };
            }
            else if (propInfo.PropertyType == typeof(short))
            {
                return delegate(IEntity ie)
                {  //
                    return Convert.ToInt16(ie.GetPropertyValue(prop).LongValue);
                };
            }
            else if (propInfo.PropertyType == typeof(uint))
            {
                return delegate(IEntity ie)
                {  //
                    return Convert.ToUInt32(ie.GetPropertyValue(prop).LongValue);
                };
            }
            else 
            {
                throw new NotTranslatableException("Have not implemented PropertyValueGetter for field type.", propInfo);
            }
        }

        PropertyValueSetter CreateSetter(IProperty p)
        {
            switch (p.MappingType)
            {
                case MappingType.BOOL:
                    return delegate(IEntity e, object value) {
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
                            IEntity refered = dataContext.GenDB.NewEntity();
                            dataContext.IBOCache.Add(ibo, refered.EntityPOID);
                            IBOReference reference = new IBOReference (ibo.DBIdentity);
                            e.GetPropertyValue(p).RefValue = reference;
                        }
                        else
                        {
                            IBOReference reference = new IBOReference(false, ibo.DBIdentity);
                            e.GetPropertyValue(p).RefValue = reference;
                        }
                    };
                case MappingType.STRING:
                    return delegate(IEntity e, object value)
                    {
                        e.GetPropertyValue(p).StringValue = Convert.ToString(value);
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
                        fieldSetHandler(ibo, null);
                        return;
                    }

                    long refEntityPOID = Convert.ToInt64(value);

                    IBusinessObject iboVal = IBOCache.Instance.Get(refEntityPOID);

                    if (iboVal == null) 
                    {
                        IExpression where = new OP_Equals(new CstLong(refEntityPOID), CstThis.Instance);
                        // TODO: Kunne nok g�res hurtigere...
                        iboVal = dataContext.GenDB.GetByEntityPOID(refEntityPOID);
                        fieldSetHandler(ibo, iboVal);
                        return;
                    }
                    else
                    {
                        fieldSetHandler(ibo, iboVal);
                    }
                };
            }
            else if (propInfo.PropertyType == typeof(long))
            {
                return delegate(IBusinessObject ibo, object value)
                {
                    fieldSetHandler(ibo, Convert.ToInt64(value));
                };
            }
            else if (propInfo.PropertyType == typeof(int))
            {
                return delegate(IBusinessObject ibo, object value) 
                { 
                    fieldSetHandler(ibo, Convert.ToInt32(value));
                };
            }
            else if (propInfo.PropertyType == typeof(string))
            {
                return delegate(IBusinessObject ibo, object value)
                {
                    fieldSetHandler(ibo, value.ToString());
                };
            }
            else if (propInfo.PropertyType == typeof(DateTime))
            {
                return delegate(IBusinessObject ibo, object value)
                {
                    fieldSetHandler(ibo, Convert.ToDateTime(value));
                };
            }
            else if (propInfo.PropertyType == typeof(bool))
            {
                return delegate(IBusinessObject ibo, object value) 
                {
                    fieldSetHandler(ibo, Convert.ToBoolean(value));
                };
            }
            else if (propInfo.PropertyType == typeof(char))
            {
                return delegate(IBusinessObject ibo, object value) {  //
                    fieldSetHandler(ibo, Convert.ToChar(value));
                };
            }
            else if (propInfo.PropertyType == typeof(float))
            {
                return delegate(IBusinessObject ibo, object value)
                {  //
                    fieldSetHandler(ibo, Convert.ToSingle(value));
                };
            }
            else if (propInfo.PropertyType == typeof(double))
            {
                return delegate(IBusinessObject ibo, object value)
                {  //
                    fieldSetHandler(ibo, Convert.ToDouble(value));
                };
            }
            else if (propInfo.PropertyType.IsEnum)
            {
                return delegate (IBusinessObject ibo, object value)
                {
                    // Overs�tter til array indeholdende alle mulige v�rdier for den p�g�ldende enum.
                    // Dern�st v�lges v�rdien gemt index Long-feltet index den p�g�ldende PropertyValue record.
                    System.Collections.IList vals =  (System.Collections.IList) Enum.GetValues(propInfo.PropertyType);
                    fieldSetHandler(ibo,  vals[Convert.ToInt32(value)]);
                    //return Enum.GetValues (propInfo.PropertyType). ((int)ie.GetPropertyValue(prop).LongValue);
                };
            }
            else if (propInfo.PropertyType == typeof(short))
            {
                return delegate(IBusinessObject ibo, object value)
                {  //
                    fieldSetHandler(ibo, Convert.ToInt16(value));
                };
            }
            else if (propInfo.PropertyType == typeof(uint))
            {
                return delegate(IBusinessObject ibo, object value)
                {  //
                    fieldSetHandler(ibo, Convert.ToUInt32(value));
                };
            }
            else 
            {
                throw new NotTranslatableException("Have not implemented PropertyValueGetter for field type.", propInfo);
            }
        }
    }
}
