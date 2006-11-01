using System;
using System.Collections.Generic;
using System.Text;

namespace AOM
{

    /// <summary>
    /// Class describing a property of an object. 
    /// </summary>
    public class PropertyType : AOMBaseObject
    {
        /// <summary>
        /// All instances of PropertyType stored here for name lookup.
        /// </summary>
        private static Dictionary<string, PropertyType> propertyTypes = new Dictionary<string, PropertyType>();

        /// <summary>
        /// Validator associated with this PropertyType
        /// (Could be used as a means to check if a string
        /// was translatable into a numeric etc.)
        /// <para>
        /// The intention with the validator is to mimic
        /// some kind of type safety, rather than implementing
        /// conrete business rules. These should be placed 
        /// at the individual properties.
        /// </para>
        /// </summary>
        IValidator validator = new ValidatorAcceptAll();

        /// <summary>
        /// Get or set the validator for this PropertyType.
        /// </summary>
        public IValidator Validator
        {
            get { return validator; }
            set { validator = value; }
        }

        /// <summary>
        /// Initialize a property and associate it with its
        /// name in the underlying <pre>PropertyType</pre> 
        /// dictionary. The PropertyTypePOID (as it is specified 
        /// in the databse) can be specified. 
        /// <para>This constructor should
        /// only be used when retrieving data from the database, since
        /// manually setting the id will almost certainly cause data 
        /// havoc.
        /// </para>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="propertyTypePOID"></param>
        public PropertyType(string name, long propertyTypePOID)
        {
            Name = name;
            Id = propertyTypePOID;
            IsPersistent = true;
            propertyTypes[name] = this;
        }

        /// <summary>
        /// Construct a new <pre>PropertyType</pre> with the specified name.
        /// </summary>
        /// <param name="name"></param>
        public PropertyType(string name)
        {
            Name = name;
            propertyTypes[name] = this;
        }


        public StringBuilder ToString(StringBuilder sb)
        {
            if (sb == null)
            {
                sb = new StringBuilder();
            }
            sb.Append("PropertyType (");
            sb.Append(Name);
            sb.Append(")");
            return sb;
        }

        public override string ToString()
        {
            return ToString(null).ToString();
        }

        #region static part

        /// <summary>
        /// Check if a property with a given name exists.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static bool Exists(string propertyName)
        {
            return propertyTypes.ContainsKey(propertyName);
        }

        /// <summary>
        /// Return property with given name. Will throw an 
        /// exception if the property name is unknown. To avoid
        /// this use <see cref="Exists"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static PropertyType Get(string name)
        {
            return propertyTypes[name];
        }
        #endregion
    }
}
