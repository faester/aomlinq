using System;
using System.Collections.Generic;
using System.Text;
using GenDB.DB;
using System.Query;

namespace GenDB
{
    static partial class Translators
    {
        static Dictionary<Type, IIBoToEntityTranslator> translators = new Dictionary<Type, IIBoToEntityTranslator>();

        /// <summary>
        /// Returns a translator appropriate for type T.
        /// Property descriptions must be stored in the given IEntityType element.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="et"></param>
        /// <returns></returns>
        public static IIBoToEntityTranslator GetTranslator(Type t, IEntityType et)
        {
            if (!translators.ContainsKey(t)) 
            { 
                translators[t] = CreateTranslator(t, et);
            };
            return translators[t];
        }

        private static IIBoToEntityTranslator CreateTranslator(Type t, IEntityType et)
        {
            if (t.IsGenericType)
            {
                throw new Exception("Problems with generic types. Don't know how to grab BOList<> instances");
                return new BOListTranslator();
                Console.WriteLine(t);
            }
            else
            {
                return new IBOTranslator(t, et);
            }
        }
    }
}
