using System;
using System.Collections.Generic;
using System.Text;
using GenDB;
using System.Diagnostics;

namespace PerformanceTests
{
    class JustRead : ITest
    {
        ReadWriteClearTest test = null;
        IEnumerable<int> objectCounts = null;
        int recurrences = 0;

        public JustRead(ReadWriteClearTest test, IEnumerable<int> objectCounts, int recurrences)
        {
            this.test = test;
            this.objectCounts = objectCounts;
            this.recurrences = recurrences;
        }

        public void PerformTest()
        {
            double obsSec = 0;
            long totalMS = 0;
            long totalObjects = 0;

            foreach (int objectCount in objectCounts)
            {
                test.InitTests(objectCount);
                for (int r = 0; r < recurrences; r++)
                {
                    long gms = 0;

                    if (gms < 0) { break; }

                    gms = test.PerformReadTest(objectCount);
                    Console.WriteLine("Testing with {0} objects. {1} ms. {2} objs/sec", objectCount, gms, gms > 0 ? (objectCount * 1000) / gms : -1);
                    obsSec += (objectCount * 1000) / gms;
                    totalObjects += objectCount;
                    totalMS += gms;
                }
            }

            Console.WriteLine();
            Console.WriteLine("Total time used: {0} ms, average {1} objs/sec", totalMS, (totalObjects * 1000.0) / totalMS );

            Console.ReadLine();
        }
    }
}

