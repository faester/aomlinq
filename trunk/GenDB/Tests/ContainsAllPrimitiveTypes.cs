using System;
using System.Collections.Generic;
using System.Text;
using GenDB;

namespace Tests
{
    /// <summary>
    /// Used for testing: Contains all types, that are considered primitive
    /// by the generic database.
    /// </summary>
    class ContainsAllPrimitiveTypes : AbstractBusinessObject
    {
        bool boo;

        public bool Boo
        {
            get { return boo; }
            set { boo = value; }
        }

        string str;

        public string Str
        {
            get { return str; }
            set { str = value; }
        }

        DateTime dt;

        public DateTime Dt
        {
            get { return dt; }
            set { dt = value; }
        }

        float fl;

        public float Fl
        {
            get { return fl; }
            set { fl = value; }
        }

        double dbl;

        public double Dbl
        {
            get { return dbl; }
            set { dbl = value; }
        }

        char ch;

        public char Ch
        {
            get { return ch; }
            set { ch = value; }
        }

        long lng;

        public long Lng
        {
            get { return lng; }
            set { lng = value; }
        }

        int integer;

        public int Integer
        {
            get { return integer; }
            set { integer = value; }
        }
    }
}
