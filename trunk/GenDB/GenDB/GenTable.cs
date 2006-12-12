using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace GenDB
{
    /*
     * Kan persisteres:
     *  - Objekterne skal implementere IBusinessObject og new() (Dette skal sikres af generisk deklarering på public Table<T> )
     *      - Kun felter med public getter og setter persisteres
     *          - Primitive felter persisteres.
     *          - Felter af referencetype persisteres kun, hvis de implementerer IBusinessObject
     *      - Ønskes et felt, der opfylder ovenstående, ikke persisteret kan det annoteres med [Volatile]
     *      - Statiske felter persisteres ikke
     */
    internal class GenTable
    {
        public GenTable()
        {
        }

        public void Add(IBusinessObject ibo)
        {
            DelegateTranslator trans = TypeSystem.Instance.GetTranslator(ibo.GetType());
            IEntity e = trans.Translate(ibo);
        }


        /// <summary>
        /// Returns all objects in database.
        /// If the cache contains an object with the same EntityPOID, the cached version will be used instead.
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IBusinessObject> GetAll()
        {
            throw new Exception("Not implemented");
        }

        /// <summary>
        /// Returns all instances of specific type.
        /// entityTypePOID must exist.
        /// Only considers in-database objects. (For now, submit before invoking)
        /// </summary>
        /// <param name="entityTypePOID"></param>
        /// <returns></returns>
        public IEnumerable<IBusinessObject> GetAll(IEntityType et) 
        {
            throw new Exception("Not implemented");
        }
    }
}
       