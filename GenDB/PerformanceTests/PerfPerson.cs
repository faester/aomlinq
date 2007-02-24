using System;
using System.Collections.Generic;
using System.Text;
using GenDB;
using System.Data.DLinq;

namespace PerformanceTests
{
    [Table(Name="t_PerfPerson")]
    class PerfPerson : IBusinessObject
    {
        static Random rand = new Random(0);

        DBIdentifier entityPOID;
        public DBIdentifier DBIdentity 
        {
            get{return entityPOID;}
            set{entityPOID = value;}
        }
        
        long id = rand.Next(10000);
        [Column(Id = true, AutoGen=true), Volatile]
        public long Id 
        {
            get{return id;}
            set{id = value;}
        }

        string name;
        [Column]
        public string Name 
        {
            get{return name;}
            set{name = value;}
        }

        PerfPerson spouse;
        [Column]
        public PerfPerson Spouse
        {
            get{return spouse;}
            set{spouse = value;}
        }
        
        public BOList<string> aliases = new BOList<string>();
        [Column]
        public BOList<string> Aliases {
            get{return aliases;}
            set{aliases = value;}
        }

        BODictionary<long, PerfPerson> friends = new BODictionary<long,PerfPerson>();
        [Column]
        public BODictionary<long, PerfPerson> Friends {
            get{return friends;}
            set{friends = value;}
        }

    }
}
