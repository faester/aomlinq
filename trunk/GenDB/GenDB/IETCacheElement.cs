using System;
using System.Collections.Generic;
using System.Text;
using GenDB.DB;

namespace GenDB
{
    internal class IETCacheElement
    {
        IEntityType entityType;
        Type clrType;
        IIBoToEntityTranslator translator;

        public IIBoToEntityTranslator Translator
        {
            get { return translator; }
        }

        public Type ClrType
        {
            get { return clrType; }
        }

        public IEntityType Target
        {
            get { return entityType; }
            set { entityType = value; }
        }

        ICollection<IEntityType> directSubTypes = new LinkedList<IEntityType>();

        public IEnumerable<IEntityType> DirectSubTypes
        {
            get { return directSubTypes; }
        }

        public void AddSubType(IEntityType iet)
        {
            directSubTypes.Add(iet);
        }

        public void InitTranslator()
        {
            translator = Translators.GetTranslator(clrType, entityType);
        }

        public IETCacheElement(IEntityType iet, Type t)
        {
            this.clrType = t;
            this.entityType = iet;
        }

        public override string ToString()
        {
            return GetType().FullName + "{ clr type = " + this.ClrType + ", IEntityType " + this.Target + ", Translator = " + this.Translator + " }";
        }
    }
}
