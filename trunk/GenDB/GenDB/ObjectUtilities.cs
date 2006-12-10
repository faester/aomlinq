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
        static Type TYPEOF_DBTAG = typeof(DBTag);

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

            FieldInfo[] staticFields = t.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            FieldInfo[] localPubFields = t.GetFields (BindingFlags.Public | BindingFlags.Instance);
            FieldInfo[] localNonPubFields = t.GetFields (BindingFlags.NonPublic | BindingFlags.Instance);
            output.WriteLine("{0}{1}Type: {2}", Indent(indentLevel), prefix, t.FullName);
            PrintFields(o, staticFields, output, indentLevel + 1);
            PrintFields(o, localPubFields, output, indentLevel + 1);
            PrintFields(o, localNonPubFields, output, indentLevel + 1);
        }

        private static void PrintFields (object o, FieldInfo[] fields, TextWriter output, int indentLevel)
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

            ConstructorInfo cinf = t.GetConstructor(EMPTY_TYPE_ARRAY);
            object clone = cinf.Invoke(null);

            foreach (FieldInfo f in fields)
            {
                if (f.FieldType != TYPEOF_DBTAG)
                {
                    object fv = f.GetValue(o);
                    f.SetValue(clone, fv);
                }
            }
            return clone;
        }

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

                object va = f.GetValue (a);
                object vb = f.GetValue (b);

                if (fieldType.IsPrimitive)
                {
                    if (!va.Equals(vb)) { return false; }
                }
                else
                {
                    if (fieldType == TYPEOF_DATETIME) { 
                        if (!va.Equals (vb))
                        {
                            return false;
                        }
                    }
                    else if (fieldType == TYPEOF_STRING) {
                        if (!va.Equals(vb))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!Object.ReferenceEquals (va, vb)) 
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }
}
