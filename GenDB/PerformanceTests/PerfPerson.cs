using System;
using System.Collections.Generic;
using System.Text;
using GenDB;
using System.Data.DLinq;

namespace PerformanceTests
{
    [Table(Name="Cars")]
    public class Car : IBusinessObject
    {

        long id = 0;
        [Column(Id = true), Volatile]
        public long Id 
        {
            get{return id;}
            set{id = value;}
        }

        [Column]
        private long PersonID;
        //[Volatile, Column]
        //public long PersonID
        //{
        //    get { return personID; }
        //    set { personID = value; }
        //}
        
        EntityRef<PerfPerson> _owner;

        [Volatile, Association(ThisKey = "PersonID")]
        public PerfPerson Owner
        {
            get { return this._owner.Entity; }
            set { this._owner.Entity = value; }
        }
        
        DBIdentifier entityPOID;
        public DBIdentifier DBIdentity 
        {
            get{return entityPOID;}
            set{entityPOID = value;}
        }
    }

    [Table(Name="Persons")]
    public class PerfPerson : IBusinessObject
    {


        public static long pid=1;
        public long Pid
        {
            get{return pid;}
            set{pid++;}
        }

        DBIdentifier entityPOID;
        public DBIdentifier DBIdentity 
        {
            get{return entityPOID;}
            set{entityPOID = value;}
        }
 
        [Column(Id = true)]
        public long PersonID = 0;

        private EntitySet<Car> _Cars = new EntitySet<Car>();

        [Volatile, Association(OtherKey = "PersonID")]
        public EntitySet<Car> Cars 
        {
            get { return _Cars; }
            set { _Cars.Assign(value); }
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

        LazyLoader<BOList<Car>> carListHolder = new LazyLoader<BOList<Car>>(new BOList<Car>());

        [LazyLoad("carListHolder")]
        public BOList<Car> GList
        {
            get { return carListHolder.Element; }
            set { carListHolder.Element = value; }
        }

        //LazyLoader<BODictionary<int, Car>> carDictHolder = new LazyLoader<BODictionary<int, Car>>(new BODictionary<int,Car>());

        //[LazyLoad("carDictHolder")]
        //public BODictionary<int, Car> GDict {
        //    get { return carDictHolder.Element; }
        //    set { carDictHolder.Element = value; }
        //}

    }
}
