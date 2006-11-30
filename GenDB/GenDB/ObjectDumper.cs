using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;

namespace GenDB
{
    public static class ObjectDumper
    {
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

        public static void PrintOut(object o)
        {
            PrintOut(o, Console.Out);
        }

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
                    output.WriteLine(" = \"" + value + "\"");
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
            }
        }
    }
}
