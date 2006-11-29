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

            Person p = new Person { name = "Mr. Tester" };
            Student s = new Student { name = "Student", avg = 8.0, major = "interesting" };

            //table.Add (p);
            //table.Add (s);
            //list.AddLast(p);
            //list.AddLast(s);

            var selection = from o in table 
                    where (o.name == "Mr. Tester" && o.birthYear == 1978)
                    select o;

            var selection2 = from o in list
                    where o.name == "Mr. Tester" 
                    select o;

            foreach(Person rp in selection)
            {
                Console.WriteLine ("RETE: " + rp.name );
            }
        }
    }
}
