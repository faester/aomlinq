using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Query;
using System.Expressions;
using Business;
using Translation;
using Persistence;
using AOM;
using System.Reflection;

namespace DBLayer
{
    // IQueryable<T>,

    public class Table<T> : ICollection<T>
        where T : IBusinessObject, new()
    {
        BO2AOMTranslator<T> converter = new BO2AOMTranslator<T>();
        Func<T, bool> linqWhereCondition = null;
        ICondition genDBcondition = null;

        T[] content = new T[1];
        int capacity=1;
        int size=0;

       
        private void Resize(int newCapacity)
        {
            if (newCapacity == 0) { newCapacity = 1; }
            Array.Resize<T>(ref content, newCapacity);
            capacity = newCapacity;
        }

        #region ICollection members

        public void Add(T e)
        {
            Entity ent = converter.ToEntity(e);

            if (size == capacity)
            {
                Resize(capacity * 2);
            }
            content[size++] = e;

        }

        public void Clear()
        {
            throw new Exception("Not implemented");
        }

        public bool Contains(T e)
        {
            throw new Exception("Not implemented");
        }

        public void CopyTo(T[] arr, int i)
        {
            throw new Exception("Not implemented");
        }

        public bool Remove(T e)
        {
            if (e.DatabaseID != null)
            {
                DBTag tag = e.DatabaseID;
                BOCache.Delete(tag);
                e.DatabaseID = null;
                return true;
            }
            else if (BOCache.HasObject(e))
            {
                BOCache.RemoveObject(e);
                return true;
            }
            return false;
        }

        public int Count
        {
            get { return content.Count(); }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public System.Collections.Generic.IEnumerator<T> GetEnumerator()
        {    
            for (int idx = 0; idx < size; idx++)
            {
                yield return content[idx];
            }

            //throw new Exception("Not implemented");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            Console.WriteLine("SECOND");
            return GetEnumerator();			  
        }

        #endregion

        public Table() { /* empty */ }

        #region Query Expression 

        static void ParseExpression(Expression<Func<T, bool>> expr)
        {
            Type t = typeof(T);
            Console.WriteLine(expr.GetType());
            ParseExpression (expr.Body);

            object o = expr.Compile();
            Console.WriteLine(o);
        }

        static void ParseBinaryExpression(BinaryExpression be)
        {
            string t = be.GetType().ToString();
            Console.WriteLine(be.NodeType);
            Console.WriteLine(be.Left);
            Console.WriteLine(be.Right);

            ParseExpression(be.Left);
            ParseExpression(be.Right);
        }

        static void ParseMemberExpression(MemberExpression me)
        {
            Console.WriteLine("MemberExpression: {0}", me);
            MemberInfo m = me.Member; //MemberInfo fra Reflection
            Console.WriteLine(m.MemberType);
            if (m.MemberType == MemberTypes.Field)
            {
                Console.WriteLine(m);
            }
            else 
            {
                throw new Exception ("Don't know how to handle " + m);
            }
        }

        static void ParseExpression(Expression e)
        {
            string t = e.GetType().ToString();
            Console.WriteLine(e);
            if (e is BinaryExpression )
            {
                BinaryExpression be = (BinaryExpression)e;
                ParseBinaryExpression(be);
            }
            else if (e is MemberExpression)
            {
                ParseMemberExpression((MemberExpression)e);
            }
            else if (e is MethodCallExpression )
            {
                MethodCallExpression mce = (MethodCallExpression)e;
                foreach (Expression ex in mce.Parameters)
                {
                    Type theT = ex.GetType();
                    ExpressionType exprType = ex.NodeType;
                    if (exprType == System.Expressions.ExpressionType.MemberAccess)
                    {
                        Console.WriteLine ("Yahooo!");
                    }
                    Console.WriteLine(exprType.ToString());
                }
                MethodInfo methodInfo = mce.Method ;
                                
                Console.WriteLine(methodInfo);
            }
            else
            {
                Console.WriteLine("We have a: '{0}' in '{1}' ", e.GetType().ToString(), e);
            }
        }

        public ICollection<T> Where(Expression<Func<T, bool>> expr)
        {
            ParseExpression(expr);
            return this;
            //return this.Where<T> (expr.Compile()).ToQueryable();
        }

        #endregion
    }
}
