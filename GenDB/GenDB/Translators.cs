using System;
using System.Collections.Generic;
using System.Text;
using GenDB.DB;
using System.Query;

namespace GenDB
{
    class TranslatorSet
    {
        static Type bolistGeneric = typeof(GenDB.BOList<>);

        Dictionary<Type, IIBoToEntityTranslator> clrtype2translator = new Dictionary<Type, IIBoToEntityTranslator>();
        Dictionary<long, IIBoToEntityTranslator> etPOID2translator = new Dictionary<long, IIBoToEntityTranslator>();
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

        internal IIBoToEntityTranslator GetTranslator(long entityTypePOID)
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
                if (t.GetGenericTypeDefinition() == bolistGeneric)
                {
                    /*
                     * Ved depersistering skal der ses på om IsList er sat. 
                     * I så fald skal elementtypen hentes fra property. (Problem: Den burde vel være "statisk".)
                     * 
                     */
                    return new BOListTranslator(t, dataContext, iet);
                }
                else
                {
                    throw new NotTranslatableException("Can not translate generic types", t);
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
