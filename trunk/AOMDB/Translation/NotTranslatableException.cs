using System;
using System.Collections.Generic;
using System.Text;

namespace Translation
{
    public class NotTranslatableException : Exception
    {
        public NotTranslatableException(string msg) : base(msg) { }
        public override string ToString()
        {
            return base.ToString() + "Cannot translate objects with reference type fields not implementing IBusinessObject (or string).";
        }
    }
}
