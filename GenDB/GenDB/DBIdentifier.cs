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

        internal DBIdentifier (long value, bool isPersistent)
        {
            this.value = isPersistent ? value | IS_SET_MASK : value;
        }

        public bool IsPersistent
        {
            get {
                long left = (value & IS_SET_MASK) ;
                return left == IS_SET_MASK; 
            }
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
            return "DBIdentifier { " + IsPersistent + ", " + Value + "}";
        }
    }
}
