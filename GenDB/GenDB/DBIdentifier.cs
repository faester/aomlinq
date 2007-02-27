using System;
using System.Collections.Generic;
using System.Text;

namespace GenDB
{
    public struct DBIdentifier 
    {
        public const int IS_SET_MASK = (1 << 31);
        public const int VALUE_BITS =  ~(1 << 31);
        int value;

        internal DBIdentifier (int value, bool isPersistent)
        {
            this.value = isPersistent 
                ? (value | IS_SET_MASK) 
                : value;
        }

        public bool IsPersistent
        {
            get {
                long left = (value & IS_SET_MASK) ;
                return left == IS_SET_MASK; 
            }
        }

        public static implicit operator int(DBIdentifier ident)
        {
            return ident.Value;
        }
        
        public int Value
        {
            get { return value & VALUE_BITS; }
        }

        public override string ToString()
        {
            return "DBIdentifier { " + IsPersistent + ", " + Value + "}";
        }
    }
}
