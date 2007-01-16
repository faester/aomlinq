using System;
using System.Collections.Generic;
using System.Text;
using GenDB;
using System.Data.DLinq;

namespace PerformanceTests
{
    /// <summary>
    /// Used for testing: Contains all types, that are considered primitive
    /// by the generic database.
    /// </summary>
    [Table(Name = "t_PerfTestAllPrimitiveTypes")]
    public class PerfTestAllPrimitiveTypes  : IBusinessObject  
    {
        DBIdentifier entityPOID;

        public DBIdentifier EntityPOID
        {
            get { return entityPOID; }
            set { entityPOID = value; }
        }

        long id;
        [Column(Id = true, AutoGen = true), Volatile]
        public long Id
        {
            get { return id; }
            set { id = value; }
        }

        bool boo;
        [Column]
        public bool Boo
        {
            get { return boo; }
            set { boo = value; }
        }

        string str = "";

        [Column]
        public string Str
        {
            get { return str; }
            set { str = value; }
        }

        DateTime dt = DateTime.Now;

        [Column]
        public DateTime Dt
        {
            get { return dt; }
            set { dt = value; }
        }

        float fl;

        [Column(DBType = "float")]
        public float Fl
        {
            get { return fl; }
            set { fl = value; }
        }

        double dbl;

        [Column]
        public double Dbl
        {
            get { return dbl; }
            set { dbl = value; }
        }

        char ch;

        [Column]
        public char Ch
        {
            get { return ch; }
            set { ch = value; }
        }

        long lng;

        [Column]
        public long Lng
        {
            get { return lng; }
            set { lng = value; }
        }

        int integer;

        [Column]
        public int Integer
        {
            get { return integer; }
            set { integer = value; }
        }

        short sh;

        [Column]
        public short Sh
        {
            get { return sh; }
            set { sh = value; }
        }

        uint ui;

        [Column(DBType = "int")]
        public uint Ui
        {
            get { return ui; }
            set { ui = value; }
        }
    }
}
    