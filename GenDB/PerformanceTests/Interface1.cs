using System;
using System.Collections.Generic;
using System.Text;

namespace PerformanceTests
{
    interface ITest
    {
        void PerformTest();
    }

    interface ReadWriteClearTest
    {
        void InitTests(int objectCount);
        long PerformAllTests(int objectCount);
        long PerformWriteTest(int objectCount);
        long PerformReadTest(int objectCount);
        long PerformSimpleSelectTest(int objectCount);
        long PerformCompositeSelectTest(int objectCount);
        long PerformJoinedSelectTest(int objectCount);
        long PerformSubSelectTest(int objectCount);
        long PerformClearTest();
    }
}
