using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using GenDB.DB;

namespace GenDB
{
    class FieldConverter
    {
        PropertyValueGetter pvg;
        PropertyValueSetter pvs;
        SetHandler sh;
        GetHandler gh;
        Type clrType;
        PropertyInfo fi;
        DataContext dataContext;

        public FieldConverter(Type t, PropertyInfo fi, IProperty property, DataContext dataContext)
        {
            this.dataContext = dataContext;
            this.fi = fi;
            clrType = t;
            sh = DynamicMethodCompiler.CreateSetHandler(t, fi);
            gh = DynamicMethodCompiler.CreateGetHandler(t, fi);
            pvg = CreateGetter(fi, property);
            pvs = CreateSetter(property);
        }

        public void SetObjectFieldValue(IBusinessObject ibo, IEntity entity)
        {
            object value = pvg(entity);
            sh(ibo, value);
        }

        public void SetEntityPropertyValue(IBusinessObject ibo, IEntity e)
        {
            pvs(e, gh(ibo));
        }

        /// <summary>
        /// TODO: Change the GetHandler etc to use primitives and objects.
        /// (Should be GetLongHandler etc..)
        /// </summary>
        /// <param name="clrProperty"></param>
        /// <param name="prop"></param>
        /// <returns></returns>
        private PropertyValueGetter CreateGetter(PropertyInfo fi, IProperty prop)
        {
            if ((fi.PropertyType.IsByRef || fi.PropertyType.IsClass) 
                && !(fi.PropertyType == typeof(string) || fi.PropertyType == typeof(DateTime))
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
            if (fi.PropertyType == typeof(long))
            {
                return delegate(IEntity ie)
                {
                    return ie.GetPropertyValue(prop).LongValue;
                };
            }
            else if (fi.PropertyType == typeof(int))
            {
                return delegate(IEntity ie) { return (int)ie.GetPropertyValue(prop).LongValue; };
            }
            else if (fi.PropertyType == typeof(string))
            {
                return delegate(IEntity ie)
                {
                    return ie.GetPropertyValue(prop).StringValue;
                };
            }
            else if (fi.PropertyType == typeof(DateTime))
            {
                return delegate(IEntity ie)
                {
                    return ie.GetPropertyValue(prop).DateTimeValue;
                };
            }
            else if (fi.PropertyType == typeof(bool))
            {
                return delegate(IEntity ie) { return ie.GetPropertyValue(prop).BoolValue; };
            }
            else if (fi.PropertyType == typeof(char))
            {
                return delegate(IEntity ie) {  //
                    return Convert.ToChar(ie.GetPropertyValue(prop).LongValue);
                };
            }
            else if (fi.PropertyType == typeof(float))
            {
                return delegate(IEntity ie)
                {  //
                    return Convert.ToSingle(ie.GetPropertyValue(prop).DoubleValue);
                };
            }
            else if (fi.PropertyType == typeof(double))
            {
                return delegate(IEntity ie)
                {  //
                    return ie.GetPropertyValue(prop).DoubleValue;
                };
            }
            else if (fi.PropertyType.IsEnum)
            {
                return delegate (IEntity ie)
                {
                    // Oversætter til array indeholdende alle mulige værdier for den pågældende enum.
                    // Dernæst vælges værdien gemt index Long-feltet index den pågældende PropertyValue record.
                    System.Collections.IList vals =  (System.Collections.IList) Enum.GetValues(fi.PropertyType);
                    return vals[(int)ie.GetPropertyValue(prop).LongValue];
                    //return Enum.GetValues (fi.PropertyType). ((int)ie.GetPropertyValue(prop).LongValue);
                };
            }
            else 
            {
                throw new NotTranslatableException("Have not implemented PropertyValueGetter for field type.", fi);
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
                        if (ibo.DBTag == null)
                        {
                            IEntity refered = dataContext.GenDB.NewEntity();
                            dataContext.IBOCache.Add(ibo, refered.EntityPOID);
                            IBOReference reference = new IBOReference (ibo.DBTag.EntityPOID);
                            e.GetPropertyValue(p).RefValue = reference;
                        }
                        else
                        {
                            IBOReference reference = new IBOReference(false, ibo.DBTag.EntityPOID);
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
    }
}
