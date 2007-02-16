using System;
using System.Collections.Generic;
using System.Text;
using GenDB;

namespace CommonTestObjects
{
    public class TestPerson : AbstractBusinessObject
    {
        string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        int age;

        public int Age
        {
            get { return age; }
            set { age = value; }
        }

        bool goodLooking;

        public bool GoodLooking
        {
            get{return goodLooking;}
            set{goodLooking=value;}
        }

        TestPerson spouse;

        public TestPerson Spouse
        {
            get { return spouse; }
            set { spouse = value; }
        }

        BODictionary<int, TestPerson> friends;

        public BODictionary<int, TestPerson> Friends
        {
            get {return friends;}
            set {friends = value;}
        }
    }
}
