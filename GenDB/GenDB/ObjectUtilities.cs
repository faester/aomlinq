using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;

namespace GenDB
{
    /// <summary>
    /// Simple helper class to create
    /// text representations of objects.
    /// </summary>
    public static class ObjectUtilities
    {
        static Type[] EMPTY_TYPE_ARRAY = new Type[0];
        static Type TYPEOF_STRING = typeof(string);
        static Type TYPEOF_DATETIME = typeof(DateTime);
        //static Type TYPEOF_DBTAG = typeof(DBTag);

        static int MAX_INDENT_LEVEL = 5;
        static string Indent(int level)
        {
            switch (level)
            {
                case 0: return "";
                case 1: return "\t";
                case 2: return "\t\t";

            }
            string res = "\t\t";
            for (int i = 2; i < level; i++)
            {
                res += "\t";
            }
            return res;
        }

        /// <summary>
        /// Print object information to 
        /// Console.Out
        /// </summary>
        /// <param name="o">Object to print</param>
        public static void PrintOut(object o)
        {
            PrintOut(o, Console.Out);
        }

        /// <summary>
        /// Print object information to 
        /// any TextWriter. TextWriter 
        /// must not be null and must be
        /// open when this method is called.
        /// </summary>
        /// <param name="o">Object to print</param>
        /// <param name="output">Where to send output</param>
        public static void PrintOut(object o, TextWriter output)
        {
            PrintObject(o, output, 0);
        }

        private static void PrintObject(object o, TextWriter output, int indentLevel)
        {
            if (indentLevel > MAX_INDENT_LEVEL) 
            {
                output.WriteLine(Indent(indentLevel) + "...");
                return;
            }
            char prefix = '+';
            if (o == null)
            {
                output.WriteLine("{0}{1}Type: {2}", Indent(indentLevel), prefix, null);
                return;
            }

            Type t = o.GetType();
            
            if (!o.GetType().IsPublic) { prefix = '-'; }

            IEnumerable<FieldInfo> staticFields = GetFieldsRecurse(t, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            IEnumerable<FieldInfo> localPubFields = GetFieldsRecurse(t, BindingFlags.Public | BindingFlags.Instance);
            IEnumerable<FieldInfo> localNonPubFields = GetFieldsRecurse(t, BindingFlags.NonPublic | BindingFlags.Instance);
            output.WriteLine("{0}{1}Type: {2}", Indent(indentLevel), prefix, t.FullName);
            PrintFields(o, staticFields, output, indentLevel + 1);
            PrintFields(o, localPubFields, output, indentLevel + 1);
            PrintFields(o, localNonPubFields, output, indentLevel + 1);
        }

        private static IEnumerable<FieldInfo> GetFieldsRecurse (Type t, BindingFlags bflags)
        {
            LinkedList<FieldInfo> fields = new LinkedList<FieldInfo>();

            foreach (FieldInfo fi in t.GetFields(bflags))
            {
                fields.AddLast(fi);
            }
            if (t.BaseType != null)
            {
                foreach (FieldInfo fi in GetFieldsRecurse(t.BaseType, bflags ))
                {
                    fields.AddLast(fi);
                }
            }
            return fields;
        }

        private static void PrintFields(object o, IEnumerable<FieldInfo> fields, TextWriter output, int indentLevel)
        {
            string indentString = Indent (indentLevel);
            foreach (FieldInfo fi in fields)
            {
                Type fieldType = fi.FieldType;
                string prefix;
                if (fi.IsStatic) { prefix = "S"; } else { prefix = "L"; }
                if (fi.IsPublic) { prefix += "+"; } else { prefix += "-"; }

                object value = fi.GetValue(o);
                output.Write (indentString + prefix + fi.FieldType.Name + " " + fi.Name);

                if (fi.FieldType.IsValueType  )
                {
                    output.WriteLine (" = " + value); 
                }
                else if (fi.FieldType == typeof(string))
                {
                    if (value == null)
                    {
                        output.WriteLine(" = null");
                    }
                    else
                    {
                        output.WriteLine(" = \"" + value + "\"");
                    }
                }
                else if (fi.FieldType.IsArray )
                {
                    output.WriteLine (" IS ARRAY!");
                }
                else 
                {
                    output.WriteLine ();
                    PrintObject (value, output, indentLevel + 1);
                }
            } // foreach
        } // method

        /// <summary>
        /// Clones object so all field values are identical.
        /// DBTags are silently ignored.
        /// This clone method is used by the IBOCache as 
        /// part of the change tracking mechanism.
        /// 
        /// If a field in o is attributed by [Volatile]
        /// it will not be considered during cloning.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static object MakeClone(object o)
        {
            if (o == null)
            {
                return null;
            }
            Type t = o.GetType();
            FieldInfo[] fields = t.GetFields (
                BindingFlags.Instance
            | BindingFlags.Public
            | BindingFlags.NonPublic
            );

            InstantiateObjectHandler inst = DynamicMethodCompiler.CreateInstantiateObjectHandler(t);
            //ConstructorInfo cinf = t.GetConstructor(EMPTY_TYPE_ARRAY);
            object clone = inst();

            foreach (FieldInfo f in fields)
            {
                Attribute v = Volatile.GetCustomAttribute(f, typeof(Volatile));

                if (f.GetType() != typeof(DBIdentifier) && v == null)
                {
                    object fv = f.GetValue(o);
                    f.SetValue(clone, fv);
                }
            }
            return clone;
        }

        /// <summary>
        /// Tests if fields of object a equals fields of object b.
        /// The test returns true if typeof(a) == typeof(b) and 
        /// all primitive fields have same cstProperty. Fields of reference
        /// type must by ReferenceEquals for the method to return
        /// true. Note that fields with attribute [Volatile] are 
        /// not tested, since the method is ment to be used to 
        /// determine if an object needs to be rewritten to the
        /// database. 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool TestFieldEquality(object a, object b)
        {
            if (a == null && b == null)
            {
                return true;
            }
            if (a == null || b == null)
            {
                return false;
            }

            Type t = a.GetType();
            if (b.GetType () != t)
            {
                return false;
            }

            FieldInfo[] fields = t.GetFields (
                        BindingFlags.Instance
                        | BindingFlags.Public
                        | BindingFlags.NonPublic
            );

            foreach(FieldInfo f in fields)
            {
                Type fieldType = f.FieldType;
                Attribute attr = Attribute.GetCustomAttribute(f, typeof(Volatile));
                if (attr == null)
                {
                    object va = f.GetValue(a);
                    object vb = f.GetValue(b);
                    if (va == null ^ vb == null) { return false; }
                    if (va == null && vb == null) { return true; }
                    if (fieldType.IsPrimitive)
                    {
                        if (!va.Equals(vb)) { return false; }
                    }
                    else
                    {
                        if (fieldType == TYPEOF_DATETIME)
                        {
                            if (!va.Equals(vb))
                            {
                                return false;
                            }
                        }
                        else if (fieldType == TYPEOF_STRING)
                        {
                            if (!va.Equals(vb))
                            {
                                return false;
                            }
                        }
                        else
                        {
                            if (!Object.ReferenceEquals(va, vb))
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }
    }
}
