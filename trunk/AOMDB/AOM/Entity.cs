using System;
using System.Collections.Generic;
using System.Text;

namespace AOM
{
    public class Entity : AOMBaseObject
    {
        private Entity entityBase = null;

        private EntityType type;
        private Dictionary<string, string> propertyValues = new Dictionary<string, string>();

        public Entity callMe()
        {
            return this;
        }

        private Entity() { /* empty */ }


        /// <summary>
        /// The EntityType object of this Entity.
        /// </summary>
        /// <param name="type"></param>
        internal Entity(EntityType type)
        {
            this.type = type;
            if (type.SuperType != null)
            {
                entityBase = type.SuperType.New();
            }
        }

        /// <summary>
        /// True if EntityPOIDs are identical. 
        /// In case this has not yet been set, 
        /// only reference equality will return 
        /// true.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>

        public override bool Equals(object obj)
        {
            if (obj == null) {
                return false;
            }
            if (!(obj is Entity))
            {
                return false;
            }
            if (object.ReferenceEquals(obj, this))
            {
                return true;
            }
            Entity other = (Entity)obj;
            if (this.Id != AOMBaseObject.UNDEFINED_ID && this.Id == other.Id)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Just calls overridden method in base.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public Entity EntityBase
        {
            get { return entityBase; }
            set { entityBase = value; }
        }

        /// <summary>
        /// Used internally by EntityType to prepare the values 
        /// dictionary to storage a value for each property. 
        /// Properties are stored in the EntityType, and thus 
        /// referenced through this class.
        /// </summary>
        /// <param typename="pt"></param>
        internal void AddProperty(Property p)
        {
            propertyValues.Add(p.Name, p.DefaultValue);
        }

        public EntityType Type { get { return type; } internal set { type = value; } }

        public Entity SetProperty(string propertyName, string propertyValue)
        {
            if (propertyValues.ContainsKey(propertyName))
            {
                base.IsPersistent = false;
                propertyValues[propertyName] = propertyValue;
            }
            else
            {
                if (entityBase == null) { throw new UnknownPropertyException(this.type.Name, propertyName); }
                entityBase.SetProperty(propertyName, propertyValue);
            }
            return this;
        }

        public override long Id
        {
            get { return base.Id; }
            set
            {
                if (entityBase != null) { entityBase.Id = value; }
                base.Id = value;
            }
        }

        /// <summary>
        /// Returns value of the property for this object.
        /// No checking if p.EntityType == this.Type.
        /// Will return InvalidKeyException if this is violated.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public string GetPropertyValue(Property p)
        {
            return propertyValues[p.Name];
        }

        public string GetPropertyValue(string propertyName)
        {
            if (propertyValues.ContainsKey(propertyName))
            {
                return propertyValues[propertyName];
            }
            else
            {
                if (entityBase == null) { throw new UnknownPropertyException(this.type.Name, propertyName); }
                return entityBase.GetPropertyValue(propertyName);
            }
        }

        #region ToString
        private void ToString(StringBuilder sb)
        {
            sb.Append(type.Name);
            sb.Append("={");
            bool first = true;
            foreach (KeyValuePair<string, string> kvp in propertyValues)
            {
                if (first) { first = false; } else { sb.Append(", "); }
                sb.Append(kvp.Key);
                sb.Append('=');
                sb.Append(kvp.Value);
            }
            if (entityBase != null)
            {
                if (!first) { sb.Append(", "); }
                sb.Append('{');
                entityBase.ToString(sb);
                sb.Append('}');
            }
            sb.Append("}");
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            ToString(sb);
            return sb.ToString();
        }
        #endregion
    }
}
