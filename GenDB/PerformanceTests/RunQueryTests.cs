using System;
using System.Collections.Generic;
using System.Text;

namespace PerformanceTests
{
    class RunQueryTests : ITest
    {
        QueryTest test;
        int objectCount;
        int repetitions;
        TestOutput to;

        public RunQueryTests(QueryTest test, int objectCount, int repetitions, TestOutput to)
        {
            this.test = test;
            this.objectCount = objectCount;
            this.to = to;
            this.repetitions = repetitions;
        }

        private long runningTime = 0;

        public void AddAndPrintRunningTime(long ms)
        {
            runningTime+=ms;
            Console.WriteLine("Running time so far is {0} ms", runningTime);
        }

        public void PerformTest()
        {
            Console.WriteLine("Running test of type: " + test.GetType());
            runningTime= 0;
            for(int i=0; i<repetitions;i++)
            {
                Console.WriteLine("----------------");
                Console.WriteLine("Performing select 1 of {0} objects", objectCount);
                long ms = test.PerformSelectOneTest(objectCount);
                to.RecieveSelectOneTestResult(objectCount,ms);
                AddAndPrintRunningTime(ms);

                Console.WriteLine("Performing select 1% of {0} objects", objectCount);
                ms = test.PerformSelectOnePctTest(objectCount);
                to.RecieveSelectOnePctTestResult(objectCount,ms);
                AddAndPrintRunningTime(ms);

                Console.WriteLine("Performing select 10% of {0} objects", objectCount);
                ms = test.PerformSelectTenPctTest(objectCount);
                to.RecieveSelectTenPctTestResult(objectCount,ms);
                AddAndPrintRunningTime(ms);

                Console.WriteLine("Performing select 100% of {0} objects", objectCount);
                ms = test.PerformSelectHundredPctTest(objectCount);
                to.RecieveSelectHundredPctTestResult(objectCount,ms);
                AddAndPrintRunningTime(ms);

                Console.WriteLine("Performing select 50% sub 50% of {0} objects", objectCount);
                ms = test.PerformSelectFiftySubFiftyPctTest(objectCount);
                to.RecieveSelectFiftySubFiftyPctTestResult(objectCount,ms);
                AddAndPrintRunningTime(ms);

                Console.WriteLine("Performing select 0 of {0} objects", objectCount);
                ms = test.PerformSelectNothingTest(objectCount);
                to.RecieveSelectNothingTestResult(objectCount,ms);
                AddAndPrintRunningTime(ms);

                Console.WriteLine("Performing select unconditional of {0} objects", objectCount);
                ms = test.PerformSelectUnconditionalTest(objectCount);
                to.RecieveSelectUnconditionalTestResult(objectCount,ms);
                AddAndPrintRunningTime(ms);
            }
        }
    }
}
