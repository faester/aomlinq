using System;
using System.Collections.Generic;
using System.Text;
using GenDB;
using System.Data.DLinq;

namespace PerformanceTests
{
    class PerfPersonTest : ReadWriteClearTest
    {
        GenDB.DataContext dataContext = GenDB.DataContext.Instance;
        GenDB.Table<PerfPerson> table = GenDB.DataContext.Instance.GetTable<PerfPerson>();

        public PerfPersonTest(GenDB.DataContext dataContext) {
            this.dataContext = dataContext;
        }

        public void InitTests(int objectCount) {
            GenDB.Table<PerfPerson> table = dataContext.GetTable<PerfPerson>();

            for(int i=0;i<objectCount;i++) {

            }
        }

        public long PerformAllTests(int objectCount) {
            return -1;
        }

        public long PerformWriteTest(int objectCount) {
            return -1;
        }

        public long PerformReadTest(int objectCount) {
            return -1;
        }

        public long PerformClearTest() {
            return -1;
        }
    }
}
