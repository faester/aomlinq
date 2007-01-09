using System;
using System.Collections.Generic;
using System.Text;
using GenDB.DB;

namespace GenDB
{
    internal class CollectionElementConverter
    {
        MappingType mt;
        public CollectionElementConverter(MappingType mt)
        {
            this.mt = mt;
        }

        public object Translate(IGenCollectionElement ce)
        {
            switch (mt)
            {
                case MappingType.BOOL: return ce.BoolValue;
                case MappingType.DATETIME: return ce.DateTimeValue;
                case MappingType.DOUBLE: return ce.DoubleValue;
                case MappingType.REFERENCE: return GetObject(ce.RefValue);
                case MappingType.STRING: return ce.StringValue;
                case MappingType.LONG: return ce.LongValue;
                default:
                    throw new Exception("MappingType not implemented in " + GetType().Name + " (" + mt + ")");
            }
        }

        private IBusinessObject GetObject(IBOReference reference)
        {
            if (reference.IsNullReference) { return null; }

            IBusinessObject ibo = IBOCache.Get(reference.EntityPOID);
            if (ibo != null) { return ibo; }

            IEntity e = Configuration.GenDB.GetEntity(reference.EntityPOID);

            IIBoToEntityTranslator trans = TypeSystem.GetTranslator(e.EntityType.EntityTypePOID);
            return trans.Translate(e);
        }

        public IGenCollectionElement Translate(object o)
        {
            if (o == null) { return null; }
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
                IEntity e = Configuration.GenDB.NewEntity();
                DBTag.AssignDBTagTo(ibo, e.EntityPOID);
            }
            return new IBOReference(ibo.DBTag.EntityPOID);
        }
    }
}
