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
        static Random rnd = new Random(0);
        DBIdentifier entityPOID;

        public DBIdentifier DBIdentity
        {
            get { return entityPOID; }
            set { entityPOID = value; }
        }

        long id = rnd.Next(1000);

        //[Column (Id = true, AutoGen = true), Volatile]
        //public long Id
        //{
        //    get { return id; }
        //    set { id = value; }
        //}

        bool boo = rnd.Next() % 2 == 0;
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

        DateTime dt = new DateTime(rnd.Next(1900, 2007), rnd.Next(1, 10), rnd.Next(1, 28));

        [Column]
        public DateTime Dt
        {
            get { return dt; }
            set { dt = value; }
        }

        float fl = float.MaxValue / rnd.Next();

        [Column(DBType = "float")]
        public float Fl
        {
            get { return fl; }
            set { fl = value; }
        }

        double dbl = rnd.NextDouble();

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

        long lng = rnd.Next(0, 1000);

        [Column]
        public long Lng
        {
            get { return lng; }
            set { lng = value; }
        }

        int integer = rnd.Next();

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
    