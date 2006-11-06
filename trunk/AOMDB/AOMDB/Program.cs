using System;
using System.Collections.Generic;
using System.Text;
using Persistence;
using Business;
using AOM;
using Translation;
using DBLayer;
using System.Query;
using System.Expressions ;

namespace AOMDB
{
    class BORecursive : AbsBusinessObj
    {
        BORecursive next;

        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }


        internal BORecursive Next
        {
            get { return next; }
            set { next = value; }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Table<BORecursive> table = new Table<BORecursive>();

            var selection = from o in table 
                    where o.Name.ToUpper() == "Mr. Tester"
                    select o;

            BORecursive first = new BORecursive();
            BORecursive second = new BORecursive();
            first.Next = second;
            second.Next = first;
            BO2AOMTranslator<BORecursive> trans = new BO2AOMTranslator<BORecursive>();
            AOM.Entity e = trans.ToEntity (first);
            Console.ReadLine();
        }
    }
}
