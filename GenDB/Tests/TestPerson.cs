using System;
using System.Collections.Generic;
using System.Text;
using GenDB;

namespace TableTests
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

        TestPerson spouse;

        internal TestPerson Spouse
        {
            get { return spouse; }
            set { spouse = value; }
        }
    }
}
