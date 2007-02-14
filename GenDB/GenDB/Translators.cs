using System;
using System.Collections.Generic;
using System.Text;
using GenDB.DB;

namespace GenDB
{
    class TranslatorSet
    {
        Dictionary<Type, IIBoToEntityTranslator> clrtype2translator = new Dictionary<Type, IIBoToEntityTranslator>();
        Dictionary<int, IIBoToEntityTranslator> etPOID2translator = new Dictionary<int, IIBoToEntityTranslator>();
        DataContext dataContext = null;

        internal TranslatorSet(DataContext dataContext)
        {
            if (dataContext == null) { throw new NullReferenceException("typeSystem"); }
            this.dataContext = dataContext;
        }

        /// <summary>
        /// Returns a translator appropriate for type T.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        internal void RegisterTranslator(Type t, IEntityType iet)
        {
            IIBoToEntityTranslator translator = CreateTranslator(t, iet);
            clrtype2translator[t] = translator;
            etPOID2translator[iet.EntityTypePOID]  = translator;
        }

        internal IIBoToEntityTranslator GetTranslator(Type t)
        {
            try {
                return clrtype2translator[t];
            }
            catch(KeyNotFoundException)
            {
                throw new UnknownTranslatorException("Can not find translator for type " + t);
            }
        }

        internal IIBoToEntityTranslator GetTranslator(int entityTypePOID)
        {
            try {
                return etPOID2translator[entityTypePOID];
            }
            catch(KeyNotFoundException) 
            {
                throw new UnknownTranslatorException("Can not find translator for entitytypePOID " + entityTypePOID);
            }
        }

        /// <summary>
        /// Creates translator for the given type and performs translatability checking 
        /// for the types at the same time.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="et"></param>
        /// <returns></returns>
        private IIBoToEntityTranslator CreateTranslator(Type t, IEntityType iet)
        {
            if (t.IsGenericType)
            {
                Type genericType = t.GetGenericTypeDefinition();
                if (genericType == BOListTranslator.TypeOfBOList)
                {
                    /*
                     * Ved depersistering skal der ses p� om IsList er sat. 
                     * I s� fald skal elementtypen hentes fra property. (Problem: Den burde vel v�re "statisk".)
                     * 
                     */
                    return new BOListTranslator(t, dataContext, iet);
                }
                // dirty stuff, could'n get GetGenericTypeDefinition to work with dictionaries...help me!
                else if(genericType == BODictionaryTranslator.TypeOfBODictionary)
                {
                    return new BODictionaryTranslator(t, dataContext, iet);
                }
                else
                {
                    throw new NotTranslatableException("Can not translate generic types", t.GetGenericTypeDefinition());
                }
            }
            else
            {
                return new IBOTranslator(t, iet, dataContext);
            }
        }

        internal class UnknownTranslatorException : Exception 
        {
            public UnknownTranslatorException(string msg) : base(msg) { }
        }
    }
}
