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

        public string ReverseName()
        {
            int nl = name.Length;
            StringBuilder s = new StringBuilder(nl); 
            for (int i = nl - 1; i >= 0; i--)
            {
                s.Append (name[i]);
            }
            return s.ToString();
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
            LinkedList<BORecursive> list = new LinkedList<BORecursive>();

            BORecursive b0 = new BORecursive();
            b0.Name = "Mr. Tester";
            BORecursive b1 = new BORecursive();
            b1.Name = "Someone Else";

            Console.Out.WriteLine (b0.ReverseName());

            table.Add (b0);
            table.Add (b1);
            list.AddLast(b0);
            list.AddLast(b1);

            var selection = from o in table 
                    where o.Name.ToUpper() == "retseT .rM"
                    select o;

            var selection2 = from o in list
                    where o.ReverseName() == "retseT .rM"
                    select o;

            foreach(BORecursive b in selection2)
            {
                Console.WriteLine ("RETE: " + b.Name );
            }

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
