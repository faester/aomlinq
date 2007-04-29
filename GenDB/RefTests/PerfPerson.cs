using System;
using System.Collections.Generic;
using System.Text;
using GenDB;
using System.Data.DLinq;

namespace RefTests
{

    [Table(Name = "Cars")]
    public class Car : IBusinessObject
    {

        long id = 0;
        [Column(Id = true), Volatile]
        public long Id
        {
            get { return id; }
            set { id = value; }
        }

        DBIdentifier entityPOID;
        public DBIdentifier DBIdentity
        {
            get { return entityPOID; }
            set { entityPOID = value; }
        }

        string name;

        [Column]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        int gears = 0;

        [Column]
        public int Gears
        {
            get { return gears; }
            set { gears = value; }
        }
    }

    [Table(Name = "Persons")]
    public class PerfPerson : IBusinessObject
    {
        DBIdentifier dbIdentifier;
        public DBIdentifier DBIdentity
        {
            get { return dbIdentifier; }
            set { dbIdentifier = value; }
        }

        [Column(Id = true)]
        public long PersonID = 0;

        [Column]
        public long carID;

        private EntityRef<Car> _Cars = new EntityRef<Car>();

        [Volatile, Association(Storage = "_Cars", ThisKey = "carID", OtherKey = "Id")]
        public Car Car
        {
            get { return _Cars.Entity; }
            set { _Cars.Entity = value; }
        }

        [LazyLoad(Storage = "_car")]
        public Car GenDBCar
        {
            get { return _car.Element; }
            set { _car.Element = value; }
        }

        LazyLoader<Car> _car = new LazyLoader<Car>();

        string name;
        [Column]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        int age;

        [Column]
        public int Age
        {
            get { return age; }
            set { age = value; }
        }
    }
}  

