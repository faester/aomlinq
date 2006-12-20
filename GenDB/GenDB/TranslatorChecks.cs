using System;
using System.Collections.Generic;
using System.Text;
using GenDB.DB;
using System.Reflection;

namespace GenDB
{
    static class TranslatorChecks
    {
        static string IBO_NAME = typeof(IBusinessObject).FullName;

        public static void CheckObjectTypeTranslateability(Type t)
        {
            Type hasIBO = t.GetInterface(IBO_NAME);
            if (hasIBO == null) { throw new NotTranslatableException("Reference types must implement IBusinessObject.", t); }
        }

        public static void CheckRefFieldTranslatability(PropertyInfo clrProperty)
        {
            if (clrProperty.PropertyType == typeof(string)) { /* ok */ }
            else if (clrProperty.PropertyType == typeof(DateTime)) { /* ok */ }
            else
            {
                Type hasIBO = clrProperty.PropertyType.GetInterface(IBO_NAME);
                if (hasIBO == null) { throw new NotTranslatableException("Reference type fields must implement IBusinessObject", clrProperty); }
            }
        }

        public static void CheckPropertyTranslatability(PropertyInfo[] clrProperties)
        {
            foreach (PropertyInfo clrProperty in clrProperties)
            {
                //if (clrProperty.IsStatic) { throw new NotTranslatableException("Can not translate static fields.", clrProperty); }
                MethodInfo setter = clrProperty.GetSetMethod();
                MethodInfo getter = clrProperty.GetGetMethod();
                if(setter == null || !setter.IsPublic) { throw new NotTranslatableException ("Public property has no setter or setter is non-public", clrProperty);}
                if(getter == null || !getter.IsPublic) { throw new NotTranslatableException ("Public property has no getter or getter is non-public", clrProperty);}
                if (clrProperty.PropertyType.IsArray) { throw new NotTranslatableException("Can not translate arrays.", clrProperty); }
                if (clrProperty.PropertyType.IsByRef) { CheckRefFieldTranslatability(clrProperty); }
            }
        }
    }
}
