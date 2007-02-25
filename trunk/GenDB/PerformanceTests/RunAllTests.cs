using System;
using System.Collections.Generic;
using System.Text;

namespace PerformanceTests
{
    class RunAllTests : ITest
    {
        ReadWriteClearTest test = null;
        IEnumerable<int> objectSizes = null;
        int recurrences = 0;
        TestOutput to = null;

        public RunAllTests(ReadWriteClearTest test, IEnumerable<int> objectSizes, int recurrences, TestOutput to)
        {
            this.objectSizes = objectSizes;
            this.recurrences = recurrences;
            this.test = test;
            this.to = to;
        }

        public void PerformTest()
        {
            foreach (int objectCount in objectSizes)
            {

                for (int i = 0; i < recurrences; i++)
                {
                    Console.WriteLine("-------");
                    Console.Write("Performing write test with {0} objects ", objectCount);
                    long ms = test.PerformWriteTest(objectCount);
                    Console.WriteLine("{0} ms, {1} objs/sec", ms, (1000.0 * objectCount) / ms);
                    to.ReceiveWriteTestResult(objectCount, ms);

                    Console.Write("Performing read test with {0} objects ", objectCount);
                    ms = test.PerformReadTest(objectCount);
                    Console.WriteLine("{0} ms, {1} objs/sec", ms, (1000.0 * objectCount) / ms);
                    to.ReceiveReadTestResult(objectCount, ms);
                    
                    Console.Write("Performing clear test with {0} objects ", objectCount);
                    ms = test.PerformClearTest();
                    Console.WriteLine("{0} ms, {1} objs/sec", ms, (1000.0 * objectCount) / ms);
                    to.ReceiveClearTestResult(objectCount, ms);


                }
            }
        }
    }
}
