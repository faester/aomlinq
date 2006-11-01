using System;
using System.Collections.Generic;
using System.Text;

/**
 * Alle typer caches til hukommelsen. Objektinstanser 
 * hentes (og gc'es) efter behov.
 */

namespace AOM
{
    /// <summary>
    /// <pre>EntityType</pre> is the root node of objects. Here the 
    /// fields and inheritance of class is defined. 
    /// <para>
    /// <pre>EntityType</pre> also contains a static cache of all 
    /// s_types available to the client. 
    /// </para>
    /// </summary>
    public class EntityType : AOMBaseObject
    {
        protected EntityType superType;
        private Dictionary<string, Property> properties = new Dictionary<string, Property>();

        public EntityType SuperType
        {
            get { return superType; }
            protected internal set { superType = value; }
        }

        private EntityType() { /* empty */ }

        private EntityType(string name)
        {
            Name = name;
            s_types[name] = this;
        }

        private EntityType(string name, EntityType superType)
        {
            this.SuperType = superType;
            Name = name;
            s_types[name] = this;
        }

        public void SetSuperType(EntityType super)
        {
            superType = super;
        }

        public void RemoveProperty(Property p)
        {
            object.ReferenceEquals(p, p);

        }

        public EntityType AddProperty(Property p)
        {
            p.Owner = this;
            properties.Add(p.Name, p);
            return this;
        }

        public Entity New()
        {
            Entity resEntity = new Entity(this);
            foreach (Property p in properties.Values)
            {
                resEntity.AddProperty(p);
            }
            return resEntity;
        }

        public ICollection<Property> Properties
        {
            get
            {
                return properties.Values;
            }
        }

        public Property GetProperty(string name)
        {
            return properties[name];
        }

        public StringBuilder ToString(StringBuilder sb)
        {
            if (sb == null) { sb = new StringBuilder(); }
            sb.Append("EntityType:");
            sb.Append(Name);
            sb.Append(':');
            bool appendSeparator = false;
            foreach (KeyValuePair<string, Property> kvp in properties)
            {
                kvp.Value.ToString(sb);
                if (appendSeparator) { sb.Append("; "); }
                appendSeparator = true;
            }
            sb.Append("}");
            if (superType != null)
            {
                superType.ToString(sb);
            }
            return sb;
        }

        public override string ToString()
        {
            return ToString(null).ToString();
        }

        #region STATIC PART
        private static Dictionary<string, EntityType> s_types = new Dictionary<string, EntityType>();


        public static void PrintEntityTypeNames()
        {
            foreach (KeyValuePair<string, EntityType> kvp in s_types)
            {
                Console.WriteLine(kvp.Value.Name);
            }
        }

        /// <summary>
        /// Adds a type to the <pre>s_types</pre> cache. 
        /// Ment to be used when retrieving s_types from
        /// persistent storage.
        /// <para/>
        /// A type with typename <pre>typename</pre> must not exist 
        /// before this method is invoked.
        /// </summary>
        /// <param typename="id">The database ID of the EntityType object to create</param>
        /// <param typename="typename">The typename to give to this type.</param>
        public static EntityType AddType(long id, string typename)
        {
            EntityType e = new EntityType();
            e.Name = typename;
            e.Id = id;
            e.IsPersistent = true;
            e.superType = null;
            s_types[e.Name] = e;
            return e;
        }


        /// <summary>
        /// Adds a new EntityType to the cache. Will ensure 
        /// inheritance between the type to create and the 
        /// super type given. 
        /// <para/>
        /// A type with typename <pre>typename</pre> must not exist 
        /// before this method is invoked.
        /// </summary>
        /// <param typename="id"></param>
        /// <param typename="typename"></param>
        /// <param typename="super"></param>
        public static void AddType(int id, string typename, EntityType super)
        {
            AddType(id, typename);
        }

        /// <summary>
        /// Tests if a type with the typename <pre>typename</pre>
        /// exists.
        /// </summary>
        /// <param typename="typename"></param>
        /// <returns></returns>
        public static bool EntityTypeExists(string typename)
        {
            return s_types.ContainsKey(typename);
        }

        /// <summary>
        /// Returns an instance of the type with typename <pre>typename</pre>
        /// </summary>
        /// <param typename="typename"></param>
        /// <returns></returns>
        public static EntityType GetType(string typename)
        {
            return s_types[typename];
        }

        /// <summary>
        /// Creates a type with name <pre>typename</pre> and
        /// the given typer type. To make an object without 
        /// any superclass, pass null to <pre>superEntityType</pre>
        /// </summary>
        /// <param typename="typename">The name for the new type.</param>
        /// <param typename="superEntityType">
        /// Specifies the super class of this entity. 
        /// Null means that the created object will be a root node in the object tree.
        /// </param>
        /// <returns></returns>
        public static EntityType CreateType(string typename, EntityType superEntityType)
        {
            EntityType newOne = new EntityType(typename, superEntityType);
            return newOne;
        }
        #endregion
    }
}
