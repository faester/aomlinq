using System;
using System.Collections.Generic;
using System.Text;
using AOM;
using Business;
using System.Reflection;

namespace Translation
{
    public class BO2AOMTranslator<T> : IAOMConverter<T>
        where T : IBusinessObject, new()
    {
        AOMConverter cnv; //Does the real conversion
        EntityType et;

        public BO2AOMTranslator()
        {
            Init();
        }

        private void Init()
        {
            et = EntityTypeConverter.Construct(new T());
            cnv = new AOMConverter(typeof(T), et);
        }

        public Entity ToEntity(T obj)
        {
            if (obj == null) { return null; }
            Entity e = et.New();
            if (ObjectCache.HasObject (obj)) 
            {
                e.Id = ObjectCache.GetIDByObject(obj);
            }
            e = cnv.ToEntity(e, obj);
            Persistence.Database.Instance.Store(e);
            //e.Id should have been set in the AOMConverter
            if (e.Id == Entity.UNDEFINED_ID) { throw new Exception("Entity ID not set correctly! (ToEntity)"); }
            ObjectCache.Store (obj, e.Id);
            return e;
        }

        public T FromEntity(Entity e)
        {
            if (e == null) { return default(T); }
            T obj = default(T);
            if (e.Id != Entity.UNDEFINED_ID && ObjectCache.HasId(e.Id))
            {
                obj = (T)ObjectCache.GetObjectByID(e.Id);
                if (obj == null) { obj = new T(); }
            }
            else
            {
                obj = new T();
            }
            obj = (T)cnv.FromEntity(e, obj);
            //e.Id should have been set in the AOMConverter
            if (e.Id == Entity.UNDEFINED_ID) { throw new Exception("Entity ID not set correctly! (FromEntity)"); }
            ObjectCache.Store (obj, e.Id);
            return obj;
        }
    }
}
