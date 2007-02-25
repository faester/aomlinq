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

        public void PerformTest()
        {
            for(int i=0; i<repetitions;i++)
            {
                Console.WriteLine("----------------");
                Console.WriteLine("Performing select 1 of {0} objects", objectCount);
                long ms = test.PerformSelectOneTest(objectCount);
                to.RecieveSelectOneTestResult(objectCount,ms);

                Console.WriteLine("Performing select 1% of {0} objects", objectCount);
                ms = test.PerformSelectOnePctTest(objectCount);
                to.RecieveSelectOnePctTestResult(objectCount,ms);

                Console.WriteLine("Performing select 10% of {0} objects", objectCount);
                ms = test.PerformSelectTenPctTest(objectCount);
                to.RecieveSelectTenPctTestResult(objectCount,ms);

                Console.WriteLine("Performing select 100% of {0} objects", objectCount);
                ms = test.PerformSelectHundredPctTest(objectCount);
                to.RecieveSelectHundredPctTestResult(objectCount,ms);

                Console.WriteLine("Performing select 50% sub 50% of {0} objects", objectCount);
                ms = test.PerformSelectFiftySubFiftyPctTest(objectCount);
                to.RecieveSelectFiftySubFiftyPctTestResult(objectCount,ms);

                Console.WriteLine("Performing select 0 of {0} objects", objectCount);
                ms = test.PerformSelectNothingTest(objectCount);
                to.RecieveSelectNothingTestResult(objectCount,ms);

                Console.WriteLine("Performing select unconditional of {0} objects", objectCount);
                ms = test.PerformSelectUnconditionalTest(objectCount);
                to.RecieveSelectUnconditionalTestResult(objectCount,ms);
            }
        }
    }
}
