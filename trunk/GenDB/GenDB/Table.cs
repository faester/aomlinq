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
        IWhereable expression = new ExprInstanceOf(typeof(T));

        internal IWhereable Expression
        {
            get { return expression; }
        }

        #region ICollection members

        public void Add(T ibo)
        {
            if (ibo == null) { throw new NullReferenceException("Value can not be null."); }
            Type t = ibo.GetType();
            if (!TypeSystem.IsTypeKnown(t))
            {
               TypeSystem.RegisterType(t);
            }
            IIBoToEntityTranslator trans = TypeSystem.GetTranslator(t);
            IEntity e = trans.Translate(ibo);
            Configuration.GenDB.Save(e);
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
            foreach (IEntity ie in Configuration.GenDB.Where (expression))
            {
                IBOCache.Remove(ie.EntityPOID);
            }
            Configuration.GenDB.WhereClear(expression);
        }

        public bool Contains(T e)
        {
            if (e == null) { return false;}
            if (e.DBTag == null) { return false; }
            IWhereable where = new OP_Equals(new VarReference(e), new CstThis());

            foreach (IEntity ibo in Configuration.GenDB.Where(where))
            {
                return true;
            }
            return false;
        }

        public void CopyTo(T[] arr, int i)
        {
            int idx = i;

            foreach(T t in this)
            {
                arr[idx++] = t;
            }
        }

        public bool Remove(T e)
        {
            throw new Exception("Databasen tester ikke, om der bliver fjernet objekter.");
            if (e == null) { return false;}
            if (e.DBTag == null) { return false; }
            IWhereable where = new OP_Equals(new VarReference(e), new CstThis());
            Configuration.GenDB.WhereClear(where);
            return true;
        }

        public int Count
        {
            get { return Configuration.GenDB.Count(expression); }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public System.Collections.Generic.IEnumerator<T> GetEnumerator()
        {
            IEntityType lastType = null;
            IIBoToEntityTranslator translator = null;
            foreach (IEntity e in Configuration.GenDB.Where(expression))
            {
                if (lastType != e.EntityType)
                {
                    translator = TypeSystem.GetTranslator (e.EntityType.EntityTypePOID);
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

        public Table()
        {
            if (!TypeSystem.IsTypeKnown(typeof(T)))
            {
                TypeSystem.RegisterType(typeof(T));
            }
        }

        #region Query Expression 

        public Table<T> Where(Expression<Func<T, bool>> expr)
        {
            Table<T> res = new Table<T>();
            SqlExprTranslator exprTranslator = new SqlExprTranslator();
            res.expression = exprTranslator.Convert (expr);
            return res;
        }

        #endregion

        public override string ToString()
        {
            return "Table<" + typeof(T).ToString() + "> with condition " + expression;
        }
    }
}
