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
    public class Person : AbsBusinessObj
    {
        public string name;
        public int birthYear;
    }

    public class Student : Person
    {
        public double avg;
        public string major;
    }

    public class Teacher : Person
    {
        public int yearHired;
    }

    class Program
    {
        static void Main(string[] args)
        {
            DBLayer.Table <Person> table = new DBLayer.Table<Person>();
            LinkedList<Person> list = new LinkedList<Person>();

            //DBLayer.CollectionTable<Person> tab = new CollectionTable<Person>();

            Person p = new Person { name = "Mr. Tester" };
            Student s = new Student { name = "Student", avg = 8.0, major = "interesting" };

            table.Add (p);
            table.Add (s);
            list.AddLast(p);
            list.AddLast(s);

            //var tmp = from o in table
            //          where (o.birthYear == table.Max(oo => oo.birthYear))
            //          select o;

                var selection = from o in table 
                        where (o.name != "Nuller")
                        select new Person {o.name} into x
                        where (x.birthYear > -1)
                        orderby x.name, x.birthYear descending
                        select x;
            
            //var selection2 = from o in list
            //        where o.name != "Morten" 
            //        orderby o.birthYear
            //        select o;

            Console.WriteLine(selection.Count());

            foreach(Person rp in selection)
            {
                Console.WriteLine ("Name: {0}", rp.name);
            }

            Console.WriteLine("");

            //foreach(Person rp in selection2) 
            //{
            //    Console.WriteLine("List-Name: "+rp.name);
            //}
        }
    }
}
