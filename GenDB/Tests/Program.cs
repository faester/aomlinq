using System;
using System.Collections.Generic;
using System.Text;
using System.Query;
using System.Xml.XLinq;
using System.Data.DLinq;
using GenDB;

namespace Tests
{
    class Program
    {
        class Person : AbstractBusinessObject
        {
            string name;

            public string Name
            {
                get { return name; }
                set { name = value; }
            }

        }

        static void Main(string[] args)
        {
            GenTable gt = new GenTable();
            for (int i = 0; i < 1000; i++)
            {
                Person p = new Person();
                p.Name = "Navn " + i.ToString();
                gt.Add (p);
            }

            gt.CommitChanges();

            foreach (IBusinessObject ibo in gt.GetAll())
            {
                ObjectUtilities.PrintOut(ibo);
            }
        }
    }
}
