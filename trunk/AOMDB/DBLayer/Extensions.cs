//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Query;
//using System.Xml.XLinq;
//using System.Data.DLinq;
//using System.Expressions ;

//namespace DBLayer
//{
//    public static class Extensions
//    {
//        //public static IEnumerable<T> Where<T>(this IEnumerable<T> col, Func<T, bool> func)
//        //{
//        //    Console.WriteLine("extension WHERE got filter: {0}", func);
//        //    List<T> where = new List<T>();
//        //    foreach (T t in col)
//        //    {
//        //        if (func.Invoke(t))
//        //        {
//        //            where.Add(t);
//        //        }
//        //    }
//        //    return where;
//        //}

//        public static IEnumerable<T> Where<T>(this IEnumerable<T> col, Func<T, int, bool> func)
//        {
//            Console.WriteLine("extension (indexed) got filter: {0}", func);
//            List<T> where = new List<T>();
//            foreach (T t in col)
//            {
//                if (func (t, 1))
//                {
//                    where.Add(t);
//                }
//            }
//            return where;
//        }

//        public static IEnumerable<S> Select<T, S>(this IEnumerable<T> source, Func<T, S> selector)
//        {
//            Console.WriteLine("extension SELECT got selector : {0}", selector);
//            List<S> res = new List<S>();
//            res.Add(default(S));
//            return res;
//        }

//        public static IEnumerable<S> Select<T, S>(this IEnumerable<T> source, Func<T, int, S> selector)
//        {
//            Console.WriteLine("extension SELECT got selector : {0}", selector);
//            List<S> res = new List<S>();
//            res.Add(default(S));
//            return res;
//        }

//        //public static IQueryable<S> Select<T, S>(this IQueryable<T> source, Expression<Func<T, S>> selector)
//        //{
//        //    return null;
//        //}

//        public static IEnumerable<S> SelectMany<T, S>(this IEnumerable<T> source, Func<T, IEnumerable<S>> sel)
//        {
//            Console.WriteLine("extension SelectMany got selector : {0}", sel);
//            return new List<S>();
//        }
//    }
//}
