using System;
using System.Collections.Generic;
using System.Text;
using GenDB;
using System.Data.DLinq;

namespace PerformanceTests
{
    [Table]
    public class Car : IBusinessObject
    {
        public Car(){/*empty*/}

        int id =0;
        [Column(Id = true, AutoGen=true), Volatile]
        public int Id 
        {
            get{return id;}
            set{id = value;}
        }

        DBIdentifier entityPOID;
        public DBIdentifier DBIdentity 
        {
            get{return entityPOID;}
            set{entityPOID = value;}
        }

        private EntityRef<PerfPerson> _entries = new EntityRef<PerfPerson>();
        [Volatile]
        public PerfPerson Entries
        {
            get{return this._entries.Entity;}
            set{this._entries.Entity=value;}
        }

    }

    [Table(Name="t_PerfPerson")]
    public class PerfPerson : IBusinessObject
    {
        static Random rand = new Random(0);

        DBIdentifier entityPOID;
        public DBIdentifier DBIdentity 
        {
            get{return entityPOID;}
            set{entityPOID = value;}
        }
 
        private Car car;

        public long id = rand.Next(10000);
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

        int age;
        [Column]
        public int Age
        {
            get{return age;}
            set{age=value;}
        }

        private EntitySet<Car> _entries = new EntitySet<Car>();
        [Volatile]
        public EntitySet<Car> Entries
        {
            get{return _entries;}
            set{_entries.Assign(value);}
        }

        private BOList<Car> gList = new BOList<Car>();
        public BOList<Car> GList
        {
            get{return gList;}
            set{gList=value;}
        }

        BODictionary<int, Car> gDict = new BODictionary<int,Car>();
        public BODictionary<int, Car> GDict {
            get{return gDict;}
            set{gDict = value;}
        }

    }
}
