using System;
using System.Collections.Generic;
using System.Text;
using System.Data.DLinq;

namespace RefTests
{
    class DLinqContext : DataContext
    {
        public DLinqContext(string cnnStr) : base(cnnStr) { }
        public Table<PerfPerson> Persons;
        public Table<Car> Cars;
    }
}
