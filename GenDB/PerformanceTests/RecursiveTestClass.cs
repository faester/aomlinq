using System;
using System.Collections.Generic;
using System.Text;
using GenDB;

namespace PerformanceTests
{
    class RecursiveTestClass : IBusinessObject
    {
        DBIdentifier dbIdentity;

        public DBIdentifier DBIdentity
        {
            get { return dbIdentity; }
            set { dbIdentity = value; }
        }

        RecursiveTestClass child;

        internal RecursiveTestClass Child
        {
            get { return child; }
            set { child = value; }
        }

        int inte;

        public int Inte
        {
            get { return inte; }
            set { inte = value; }
        }

        double doub;

        public double Doub
        {
            get { return doub; }
            set { doub = value; }
        }

        string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
    }

    class SubRecursiveTestClass : RecursiveTestClass
    {
        DateTime date;

        public DateTime Date
        {
            get { return date; }
            set { date = value; }
        }
    }
}
