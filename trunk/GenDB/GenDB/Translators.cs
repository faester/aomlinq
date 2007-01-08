using System;
using System.Collections.Generic;
using System.Text;
using GenDB.DB;
using System.Query;

namespace GenDB
{
    static partial class Translators
    {
        static Type bolistGeneric = typeof(GenDB.BOList<>);

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


        /// <summary>
        /// Creates translator for the given type and performs translatability checking 
        /// for the types at the same time.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="et"></param>
        /// <returns></returns>
        private static IIBoToEntityTranslator CreateTranslator(Type t, IEntityType et)
        {
            if (t.IsGenericType)
            {
                if (t.GetGenericTypeDefinition() == bolistGeneric)
                {
                    return new BOListTranslator(t, et);
                }
                else
                {
                    throw new NotTranslatableException("Can not translate generic types", t);
                }
            }
            else
            {
                return new IBOTranslator(t, et);
            }
        }
    }
}
