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
        internal void RegisterTranslator(Type t, long entityTypePOID)
        {
      
#if DEBUG
            //TODO: Should be checked in DEBUG mode only.
            if (clrtype2translator.ContainsKey(t)) 
            { 
                throw new Exception("Translator for type " + t + " already created.");
            };
            
            if (etPOID2translator.ContainsKey(entityTypePOID))
            { 
                throw new Exception("Translator for entityPOID " + entityTypePOID + " already created.");
            };
#endif
            IIBoToEntityTranslator translator = CreateTranslator(t);
            clrtype2translator[t] = translator;
            etPOID2translator[entityTypePOID]  = translator;
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
        private IIBoToEntityTranslator CreateTranslator(Type t)
        {
            if (t.IsGenericType)
            {
                if (t.GetGenericTypeDefinition() == bolistGeneric)
                {
                    return new BOListTranslator(t, dataContext);
                }
                else
                {
                    throw new NotTranslatableException("Can not translate generic types", t);
                }
            }
            else
            {
                IEntityType et = dataContext.TypeSystem.GetEntityType(t);
                return new IBOTranslator(t, et, dataContext);
            }
        }

        internal class UnknownTranslatorException : Exception 
        {
            public UnknownTranslatorException(string msg) : base(msg) { }
        }
    }
}
