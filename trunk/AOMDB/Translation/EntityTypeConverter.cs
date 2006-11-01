using System;
using System.Collections.Generic;
using System.Text;
using AOM;
using System.Reflection;

namespace Translation
{
    /// <summary>
    /// Converts from regular objects to Entity instances.
    /// </summary>
    public static class EntityTypeConverter 
    {
        static EntityTypeConverter()
        {
            /*
             * Load already defined EntityTypes and PropertyTypes
             * 
             * TODO:
             * This may be problematic, since it assumes EntityTypes
             * never change (contrary to the fundamental idea in the
             * project. But should do for now.
             * 
             * We will need to compare stored definitions with the 
             * definition of the actual class definitions later on.
             * This will involve augmenting EntityType with methods 
             * for changing and removing Properties.
             */
            //AOM.Persistence.EntityTypeLoader.Load();
        }


        public static EntityType Construct (Type ot) {
            string name = ot.FullName;

            //If type already exists, return the type.
            if (EntityType.EntityTypeExists(name)) {
                return EntityType.GetType(name);
            }

            Dictionary<int, string> d = new Dictionary <int, string>();

            LinkedList<Property> properties = new LinkedList<Property>();

            /**
             * get all valFields declared at this level in 
             * the class hierarchy.
             */
            FieldInfo[] fields = ot.GetFields(
                BindingFlags.Instance
                | BindingFlags.Public
                | BindingFlags.NonPublic
                | BindingFlags.DeclaredOnly
                );

            /**
             * Step through valFields
             */
            foreach (FieldInfo field in fields) {
                PropertyType pt = null;
                string ptName = field.FieldType.FullName;

                // Add new PropertyType if necessary, otherwise retrieve old definition.
                pt = PropertyType.Exists(ptName) 
                    ? PropertyType.Get(ptName) 
                    : new PropertyType(ptName);
                //TODO: Burde sætte defaultvalue i overensstemmelse med Property....
                Property p = new Property(field.Name, pt);
                
                // Add property to list.
                properties.AddLast (p);
            }
            
            EntityType res = null;
            
            //Test if object has a super type.
            if (ot.BaseType != null) {
                // Super type exists. Construct depth-first using recursion.
                // Create super type representation
                EntityType super = Construct(ot.BaseType);

                // Create EntityType for this level, setting super EntityType as super.
                res = EntityType.CreateType(name, super);
            }
            else
            {
                // We are at the root of the class hierarchy, create super-less EntityType
                res = EntityType.CreateType(name, null);
            }

            foreach(Property p in properties) {
                res.AddProperty (p);
            }

            // Return EntityType
            return res;
        }

        public static EntityType Construct(object o) {
            return Construct(o.GetType());
        }
    }

}
