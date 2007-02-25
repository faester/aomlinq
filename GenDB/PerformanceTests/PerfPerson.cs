using System;
using System.Collections.Generic;
using System.Text;
using GenDB;
using System.Data.DLinq;

namespace PerformanceTests
{
    
    public class Car : IBusinessObject
    {
        public Car(){/*empty*/}

        DBIdentifier entityPOID;
        public DBIdentifier DBIdentity 
        {
            get{return entityPOID;}
            set{entityPOID = value;}
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

        //BODictionary<int, Car> gDict = new BODictionary<int,Car>();
        //public BODictionary<long, PerfPerson> Friends {
        //    get{return friends;}
        //    set{friends = value;}
        //}

    }
}
