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

    public class Table<T> : ICollection<T>, IEnumerable<T>
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
        /// </summary>
        public void Clear()
        {
            Configuration.GenDB.WhereClear(expression);
        }

        public bool Contains(T e)
        {
            if (e == null) { throw new NullReferenceException("Value can not be null.");}
            if (e.DBTag == null) { return false; }
            IWhereable where = new VarReference(e);

            IBusinessObject tst = e;

            foreach (IBusinessObject ibo in Configuration.GenDB.Where(where))
            {
                if (ibo == tst) { return true; }
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
            throw new Exception("Not implemented");
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

        public Table() { /* empty */ }

        #region Query Expression 

        public Table<T> Where(Expression<Func<T, bool>> expr)
        {
            Console.WriteLine("Table expression is " + expression);
            Table<T> res = new Table<T>();
            SqlExprTranslator exprTranslator = new SqlExprTranslator();
            res.expression = exprTranslator.Convert (expr);
            Console.WriteLine("Table expression now " + expression);
            Console.WriteLine ("Result table expression: " + res.expression);
            return res;
        }

        #endregion

        public override string ToString()
        {
            return "Table<" + typeof(T).ToString() + "> with condition " + expression;
        }
    }
}
