using System;
using System.Collections.Generic;
using System.Text;
using GenDB.DB;

namespace GenDB
{
    

    internal class CollectionElementConverter
    {
        internal delegate object ValConv(object o);
        MappingType mt;
        DataContext dataContext;
        ValConv converter;
        Type t; 

        public CollectionElementConverter(MappingType mt, DataContext dataContext, Type t) 
        {
            this.dataContext = dataContext;
            this.mt = mt;
            this.t = t;
            InitConverter();
        }

        private void InitConverter()
        {
            if (TranslatorChecks.ImplementsIBusinessObject(t))
            {
                converter = delegate(object o) { return o; };
            }
            else if (t == typeof(int))
            {
                converter = delegate(object o) { return Convert.ToInt32(o); };
            }
            else if (t == typeof(string))
            {
                converter = delegate(object o) { return o.ToString(); };
            }
            else if (t == typeof(DateTime) || t == typeof(char) || t == typeof(bool) || t == typeof(long))
            {
                converter = delegate(object o) {
                    return o;
                };
            }
            else 
            {
                throw new NotTranslatableException("Error in bolist generic type parameter. Don't know how to handle element type.", t);
            }
        }

        public object PickCorrectElement(IGenCollectionElement ce)
        {
            object o = null;
            switch (mt)
            {
                case MappingType.BOOL:  o = ce.BoolValue; break;
                case MappingType.DATETIME: o = ce.DateTimeValue; break;
                case MappingType.DOUBLE: o = ce.DoubleValue; break;
                case MappingType.REFERENCE: o = GetObject(ce.RefValue); break;
                case MappingType.STRING: o = ce.StringValue; break;
                case MappingType.LONG: o = ce.LongValue; break;
                default:
                    throw new Exception("MappingType not implemented in " + GetType().Name + " (" + mt + ")");
            }
            return DoConvert(o);
        }

        public object DoConvert(object o)
        {
            return converter(o);
        }

        private IBusinessObject GetObject(IBOReference reference)
        {
            if (reference.IsNullReference) { return null; }

            IBusinessObject ibo = dataContext.IBOCache.Get(reference.EntityPOID);
            if (ibo != null) { return ibo; }

            IEntity e = dataContext.GenDB.GetEntity(reference.EntityPOID);

            IIBoToEntityTranslator trans = dataContext.Translators.GetTranslator(e.EntityType.EntityTypePOID);
            return trans.Translate(e);
        }

        public IGenCollectionElement Translate(object o)
        {
            IGenCollectionElement res = new GenCollectionElement();
            switch (mt)
            {
                case MappingType.BOOL:
                    res.BoolValue = Convert.ToBoolean(o);
                    break;
                case MappingType.DATETIME:
                    res.DateTimeValue = Convert.ToDateTime(o);
                    break;
                case MappingType.DOUBLE:
                    res.DoubleValue = Convert.ToDouble(o);
                    break;
                case MappingType.LONG:
                    res.LongValue = Convert.ToInt64(o);
                    break;
                case MappingType.REFERENCE:
                    res.RefValue = GetReference(o);
                    break;
                case MappingType.STRING:
                    res.StringValue = Convert.ToString(o);
                    break;
                default:
                    throw new Exception("MappingType not implemented in " + GetType().Name + " (" + mt + ")");
            }
            return res;
        }

        private IBOReference GetReference(object o)
        {
            if (o == null) { return new IBOReference(true); }
            IBusinessObject ibo = (IBusinessObject)o;
            if (ibo.DBTag == null)
            {
                IEntity e = dataContext.GenDB.NewEntity();
                dataContext.IBOCache.Add(ibo, e.EntityPOID);
            }
            return new IBOReference(ibo.DBTag.EntityPOID);
        }
    }
}
