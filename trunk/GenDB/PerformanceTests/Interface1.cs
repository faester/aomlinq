using System;
using System.Collections.Generic;
using System.Text;

namespace PerformanceTests
{
    interface ReadWriteClearTest
    {
        long PerformAllTests(int objectCount);
        long PerformWriteTest(int objectCount);
        long PerformReadTest(int objectCount);
        long PerformClearTest();
    }
}
