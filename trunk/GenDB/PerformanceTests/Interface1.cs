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
        long PerformClearTest();
    }

    interface QueryTest
    {
        void InitTests(int objectCount);
        void CleanDB();
        long PerformSelectNothingTest(int objectCount);
        long PerformSelectOneTest(int objectCount);
        long PerformSelectOnePctTest(int objectCount);
        long PerformSelectTenPctTest(int objectCount);
        long PerformSelectHundredPctTest(int objectCount);
        long PerformSelectFiftySubFiftyPctTest(int objectCount);
        long PerformSelectUnconditionalTest(int objectCount);

        //long PerformCompositeSelectTest(int objectCount);
        //long PerformJoinedSelectTest(int objectCount);
        //long PerformSubSelectTest(int objectCount);
    }
}
