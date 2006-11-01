using System;
using System.Collections.Generic;
using System.Text;

namespace AOM
{
    public class Property : AOMBaseObject
    {
        private PropertyType type;

        private string defaultValue;

        private EntityType owner;

        public EntityType Owner
        {
            get { return owner; }
            internal set
            {
                IsPersistent = false;
                owner = value;
            }
        }

        public string DefaultValue
        {
            get { return defaultValue; }
            set
            {
                IsPersistent = false;
                defaultValue = value;
            }
        }

        /// <summary>
        /// Adds ToString description of this to the StringBuilder 
        /// given. If sb is null, a new StringBuilder instance will
        /// be created.
        /// </summary>
        /// <param name="sb"></param>
        /// <returns></returns>
        public StringBuilder ToString(StringBuilder sb)
        {
            if (sb == null) { sb = new StringBuilder(); }
            sb.Append(Name);
            sb.Append('(');
            sb.Append(this.Type);
            sb.Append(')');
            return sb;
        }

        public override string ToString()
        {
            return ToString(null).ToString();
        }

        private Property() { /* empty */ }

        public Property(string name, PropertyType type)
        {
            this.Name = name;
            this.type = type;
        }

        public Property(string name, PropertyType type, string defaultValue)
        {
            this.Name = name;
            this.type = type;
            this.DefaultValue = defaultValue;
        }

        public PropertyType Type { get { return type; } internal set { type = value; } }
    }
}
