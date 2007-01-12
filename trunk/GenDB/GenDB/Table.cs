using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Query;
using System.Expressions;
using GenDB.DB;

namespace GenDB
{
    // IQueryable<T>,

    public class Table<T> :  ICollection<T>, IEnumerable<T>
        where T : IBusinessObject
    {
        IExpression expression = new ExprInstanceOf(typeof(T));
        IGenericDatabase db = null;
        TranslatorSet translators = null;
        TypeSystem typeSystem;
        IBOCache iboCache;

        #region Constructors
        private Table() { /* empty */ }

        internal Table(IGenericDatabase db, TranslatorSet translators, TypeSystem typeSystem, IBOCache iboCache)
        {
            if (db == null) { throw new NullReferenceException ("db"); }
            if (translators == null) { throw new NullReferenceException("translators"); }
            if (typeSystem == null) { throw new NullReferenceException("typeSystem"); }
            if (iboCache == null) { throw new NullReferenceException("iboCache"); }

            this.db = db;
            this.translators = translators;
            this.typeSystem = typeSystem;
            this.iboCache = iboCache;

            if (!typeSystem.IsTypeKnown(typeof(T)))
            {
                typeSystem.RegisterType(typeof(T));
            }
        }
        #endregion

        internal IWhereable Expression
        {
            get { return expression; }
        }

        #region ICollection members

        public void Add(T ibo)
        {
            if (ibo == null) { throw new NullReferenceException("Value can not be null."); }
            Type t = ibo.GetType();
            if (!typeSystem.IsTypeKnown(t))
            {
               typeSystem.RegisterType(t);
            }
            IIBoToEntityTranslator trans = translators.GetTranslator(t);
            trans.SaveToDB(db, ibo);
        }

        /// <summary>
        /// Will clear all elements from database, that matches this tables current
        /// where expression. (For regularly instantiated objects this means all 
        /// entities of type T or a subclass of this type.)
        /// <br>
        /// Elements are also removed from the cache to ensure that they will not 
        /// be accidentally added later on. This slows down the operation, since 
        /// it is neccessary run the where-query twice. 
        /// </br>  
        /// </summary>
        public void Clear()
        {
            foreach (IEntity ie in db.Where (expression))
            {
                iboCache.Remove(ie.EntityPOID);
            }
            db.ClearWhere(expression);
        }


        /// <summary>
        /// Returns true, if the element given exists in the database.
        /// Equality is resolved solely on the EntityPOID of the objects.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public bool Contains(T e)
        {
            if (e == null) { return false;}
            if (e.DBTag == null) { return false; }
            IExpression where = new OP_Equals(new VarReference(e), new CstThis());

            foreach (IEntity ibo in db.Where(where))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Copies all persisted elements contained 
        /// in the table to the array given starting 
        /// from at position index in the array.
        /// </summary>
        /// <param name="arr">Target array for elements</param>
        /// <param name="index">Starting position in array</param>
        public void CopyTo(T[] arr, int index)
        {
            foreach(T t in this)
            {
                arr[index++] = t;
            }
        }

        /// <summary>
        /// Removes element 'e' from database if it exists. 
        /// Returns true, if element exists, false otherwise. 
        /// </summary>
        /// <param name="e">Element to remove.</param>
        /// <returns></returns>
        public bool Remove(T e)
        {
            if (e == null) { return false;}
            if (e.DBTag == null) { return false; }
            IWhereable where = new OP_Equals(new VarReference(e), new CstThis());
            return db.ClearWhere(where);
        }

        public int Count
        {
            get { return db.Count(expression); }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public System.Collections.Generic.IEnumerator<T> GetEnumerator()
        {
            IEntityType lastType = null;
            IIBoToEntityTranslator translator = null;
            foreach (IEntity e in db.Where(expression))
            {
                if (lastType != e.EntityType)
                {
                    translator = translators.GetTranslator (e.EntityType.EntityTypePOID);
                }
                IBusinessObject ibo = translator.Translate (e);
                yield return (T)ibo;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();			  
        }

        #endregion

        #region Query Expression 

        public Table<T> Where(Expression<Func<T, bool>> expr)
        {
            Table<T> res = new Table<T>();
            
            res.translators = this.translators ;
            res.typeSystem = this.typeSystem ;
            res.db = this.db;
            res.iboCache = this.iboCache;
            
            SqlExprTranslator exprTranslator = new SqlExprTranslator(typeSystem);
            res.expression = new ExprAnd( exprTranslator.Convert (expr), this.expression);
            return res;
        }

        #endregion

        public override string ToString()
        {
            return "Table<" + typeof(T).ToString() + "> with condition " + expression;
        }
    }
}
