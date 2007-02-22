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
        public DBIdentifier DBIdentity {
            get{return entityPOID;}
            set{entityPOID = value;}
        }
        
        int id = rand.Next(10000);
        [Column(Id = true, AutoGen=true), Volatile]
        public int Id {
            get{return id;}
            set{id = value;}
        }
        
        string name;
        [Column]
        public string Name {
            get{return name;}
            set{name = value;}
        }

        BOList<string> aliases;
        [Column]
        public BOList<string> Aliases {
            get{return aliases;}
            set{aliases = value;}
        }

        BODictionary<int, PerfPerson> friends;
        [Column]
        public BODictionary<int, PerfPerson> Friends {
            get{return friends;}
            set{friends = value;}
        }
    }
}
