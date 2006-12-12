using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Reflection;

namespace GenDB
{
    /*
     * Kan persisteres:
     *  - Objekterne skal implementere IBusinessObject og new() (Dette skal sikres af generisk deklarering på public Table<T> )
     *      - Primitive felter persisteres.
     *      - Felter af referencetype persisteres kun, hvis de implementerer IBusinessObject
     *      - Ønskes et felt, der opfylder ovenstående, ikke persisteret kan det annoteres med [Volatile]
     *      - Statiske felter persisteres ikke
     */
    public class GenTable
    {
        public GenTable()
        {
        }

        public void Add(IBusinessObject ibo)
        {
            if (!TypeSystem.IsTypeKnown(ibo.GetType()))
            {
               Assembly assembly = ibo.GetType().Assembly;
               Assembly.Load(assembly.FullName );
                
               TypeSystem.RegisterType(ibo.GetType());
            }
            DelegateTranslator trans = TypeSystem.GetTranslator(ibo.GetType());
            IEntity e = trans.Translate(ibo);
        }

        public void CommitChanges()
        {
            IBOCache.FlushToDB();
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

        ///// <summary>
        ///// Returns all instances of specific type.
        ///// entityTypePOID must exist.
        ///// Only considers in-database objects. (For now, submit before invoking)
        ///// </summary>
        ///// <param name="entityTypePOID"></param>
        ///// <returns></returns>
        //public IEnumerable<IBusinessObject> GetAll(IEntityType et) 
        //{
        //    throw new Exception("Not implemented");
        //}
    }
}
       