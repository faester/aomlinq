using System;
using System.Collections.Generic;
using System.Text;

namespace GenDB.DB
{
    /// <summary>
    ///  Contains the generic db representation of a 
    ///  collection element.
    /// </summary>
    class GenCollectionElement : IGenCollectionElement
    {
        int elementIndex;
        string strVal;
        long lv;
        double dv;
        DateTime dtv;
        char ch;
        bool bv;
        IBOReference iboRef;

        /// <summary>
        /// The index of this element. (In unordered collections this 
        /// should just represent the iterator order. The translator 
        /// is responsible for implementing the correct behaviour.)
        /// </summary>
        public int ElementIndex
        {
            get { return elementIndex; }
            set { elementIndex = value; }
        }

        /// <summary>
        /// Stringvalue of this element. (Should be null, on other mapping types.)
        /// </summary>
        public string StringValue
        {
            get { return strVal; }
            set { strVal = value; }
        }


        /// <summary>
        /// Long value of this element. (Unspecified if another mapping type.)
        /// </summary>
        public long LongValue
        {
            get { return lv; }
            set { lv = value; }
        }

        /// <summary>
        /// Double value of this element. (Unspecified if another mapping type.)
        /// </summary>
        public double DoubleValue
        {
            get { return dv; }
            set { dv = value; }
        }

        /// <summary>
        /// DateTime value of this element. (Unspecified if another mapping type.)
        /// </summary>
        public DateTime DateTimeValue
        {
            get { return dtv; }
            set { dtv = value; }
        }

        /// <summary>
        /// Char value of this element. (Unspecified if another mapping type.)
        /// </summary>
       public char CharValue
        {
            get { return ch; }
            set { ch = value; }
        }

        /// <summary>
        /// Bool value of this element. (Unspecified if another mapping type.)
        /// </summary>
        public bool BoolValue
        {
            get { return bv; }
            set { bv = value; }
        }

        /// <summary>
        /// Used if the collection is of value type.
        /// </summary>
        public IBOReference RefValue
        {
            get { return iboRef; }
            set { iboRef = value; }
        }
    }
}
