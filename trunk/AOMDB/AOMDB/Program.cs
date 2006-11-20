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
        public BORecursive Next;

        public int Id;

        public DateTime dt;

        public string Name;
    }

    class Program
    {
        static void Main(string[] args)
        {
            int[] is0 = new int[10];
            int[] is1 = new int[10];

            for (int i = 0; i < 10; i++)
            {
                is0[i] = i;
                is1[i] = -i;
            }

            is1[5] = 5;


            var ers = from j in is0
                      from k in is1
                      where j == k && j != 0
                    select j;

            foreach (var k in ers)
            {
                Console.WriteLine(k);
            }


            DBLayer.Table <BORecursive> table = new DBLayer.Table<BORecursive>();
            LinkedList<BORecursive> list = new LinkedList<BORecursive>();

            BORecursive b0 = new BORecursive();
            b0.Name = "Mr. Tester";
            BORecursive b1 = new BORecursive();
            b1.Name = "Someone Else";

            table.Add (b0);
            table.Add (b1);
            list.AddLast(b0);
            list.AddLast(b1);

            var selection = from o in table 
                    where (o.Id > 0 && o.Name == "Mr. Tester") || (o.Id == 1) || o.dt < DateTime.Now
                    select o;

            var selection2 = from o in list
                    where o.Name == "Mr. Tester"
                    select o;

            foreach(BORecursive b in selection)
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
