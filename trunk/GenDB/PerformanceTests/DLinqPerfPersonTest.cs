using System;
using System.Collections.Generic;
using System.Text;
using GenDB;

namespace PerformanceTests
{
    class DLinqPerfPersonTest : ReadWriteClearTest
    {
        GenDB.DataContext dataContext = GenDB.DataContext.Instance;
        GenDB.Table<PerfPerson> table = GenDB.DataContext.Instance.GetTable<PerfPerson>();
        int lastInsert = 0;

        public DLinqPerfPersonTest(GenDB.DataContext dataContext)
        {
            this.dataContext = dataContext;
        }

        public void InitTests(int objectCount)
        {
            /*empty*/
        }

        public long PerformAllTests(int objectCount)
        {
            return 0;
        }

        public long PerformWriteTest(int objectCount)
        {
            return 0;
        }

        public long PerformReadTest(int objectCount)
        {
            return 0;
        }

        public long PerformClearTest()
        {
            return 0;
        }

        public long PerformSimpleSelectTest(int objectCount)
        {
            return 0;
        }

        public long PerformCompositeSelectTest(int objectCount)
        {
            return 0;
        }

        public long PerformJoinedSelectTest(int objectCount)
        {
            return 0;
        }

        public long PerformSubSelectTest(int objectCount) 
        {
            return 0;
        }
    }
}
