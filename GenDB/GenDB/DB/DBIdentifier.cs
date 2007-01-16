using System;
using System.Collections.Generic;
using System.Text;

namespace GenDB
{
    public struct DBIdentifier 
    {
        public const long IS_SET_MASK = (1L << 63);
        public const long VALUE_BITS =  ~(1L << 63);
        long value;

        internal DBIdentifier (long value)
        {
            this.value = (value | IS_SET_MASK);
        }

        public bool IsInCache
        {
            get {return (value & IS_SET_MASK) == IS_SET_MASK; }
        }

        internal void Set()
        {
            value = value | IS_SET_MASK;
        }

        public static implicit operator long(DBIdentifier ident)
        {
            return ident.Value;
        }
        
        public long Value
        {
            get { return value & VALUE_BITS; }
        }

        public override string ToString()
        {
            return "DBIdentifier { " + IsInCache + ", " + Value + "}";
        }
    }
}
