using System;
using System.Collections.Generic;
using System.Text;

namespace GenDB.DB
{
    class GenCollectionElement : IGenCollectionElement
    {
        //IEntity entity;
        int elementIndex;
        string strVal;
        long lv;
        double dv;
        DateTime dtv;
        char ch;
        bool bv;
        IBOReference iboRef;

        //public IEntity Entity
        //{
        //    get { return entity; }
        //    set { entity = value; }
        //}

        public int ElementIndex
        {
            get { return elementIndex; }
            set { elementIndex = value; }
        }

        //public MappingType MappingType
        //{
        //    get { return mappingType; } 
        //    set { mappingType = value; }
        //}

        public string StringValue
        {
            get { return strVal; }
            set { strVal = value; }
        }

        public long LongValue
        {
            get { return lv; }
            set { lv = value; }
        }

        public double DoubleValue
        {
            get { return dv; }
            set { dv = value; }
        }

        public DateTime DateTimeValue
        {
            get { return dtv; }
            set { dtv = value; }
        }

        public char CharValue
        {
            get { return ch; }
            set { ch = value; }
        }

        public bool BoolValue
        {
            get { return bv; }
            set { bv = value; }
        }

        public IBOReference RefValue
        {

            get { return iboRef; }
            set { iboRef = value; }
        }
    }
}
