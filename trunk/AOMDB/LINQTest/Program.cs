using System;
using System.Collections.Generic;
using System.Text;
using System.Query;
using System.Xml.XLinq;
using System.Data.DLinq;
using System.Expressions;
using DBLayer;

/*
 * 26.7.1.1 where clauses 
 * A where clause in a query expression:
 *              from c in customers
 *              where c.City == "London"
 *              select c
 * translates to an invocation of a Where method with a synthesized lambda 
 * expression created by combining the iteration variable identifier and the 
 * expression of the where clause: customers.Where(c => c.City == "London")
 * 
 * 26.7.1.2 select clauses
 * The example in the previous section demonstrates how a select clause that 
 * selects the innermost iteration variable is erased by the translation to 
 * method invocations. A select clause that selects something other than the 
 * innermost iteration variable:
 *      from c in customers
 *          where c.City == "London"
 *          select c.Name
 * translates to an invocation of a Select method with a synthesized lambda expression:
 *       customers.Where(c => c.City == "London").Select(c => c.Name)
 *       
 * 26.7.1.3 group clauses
 * A group clause:
 *          from c in customers
 *          group c.Name by c.Country
 * translates to an invocation of a GroupBy method:
 *      customers.GroupBy(c => c.Country, c => c.Name)
 */

namespace LINQTest
{
    class Program
    {
        static void Main(string[] args)
        {
            DumCollection<int> d = new DumCollection<int>();
            for (int i = 0; i < 10; i++) { d.Add(i); }

            var e = from i in d
                    where (i > 3 && i < 7) || (i % 2 == 0)
                    select new {I = i, ThreeDivisible = i % 3 == 0};

            DumCollection<string> sc = new DumCollection<string>();

            for (int i = 0; i < 10; i++) { sc.Add ("s" + i.ToString ()); }

            var se = from s in sc 
                     where s.Length == 2
                     select s[1].ToString() + s[0].ToString();
            //var f = (i => i % 2 = 0);

            //var e = d.Select <int>( i => i * 2);

            foreach (var item in se)
            {
                Console.WriteLine(item);
            }
            Console.ReadLine();
        }
    }
}
