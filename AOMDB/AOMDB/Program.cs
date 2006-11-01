using System;
using System.Collections.Generic;
using System.Text;
using Persistence;
using Business;
using AOM;
using Translation;
using DBLayer;

namespace AOMDB
{
    class BORecursive : AbsBusinessObj
    {
        BORecursive next;

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
            BORecursive first = new BORecursive();
            BORecursive second = new BORecursive();
            first.Next = second;
            second.Next = first;
            BO2AOMTranslator<BORecursive> trans = new BO2AOMTranslator<BORecursive>();
            AOM.Entity e = trans.ToEntity (first);
        }
    }
}
