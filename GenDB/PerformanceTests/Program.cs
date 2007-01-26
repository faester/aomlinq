using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace PerformanceTests
{
    class Program
    {
        public static void Main(string[] args)
        {
            ITest theTest = null;
            ReadWriteClearTest dbtest = null;
            string dbSystem = "G";
            string testType = "R";
            int repetitions = 0;
            string fileName = null;
            LinkedList<int> objectCounts = new LinkedList<int>();

            if (args.Length < 4)
            {
                Console.WriteLine("Usage: PerformanceTests {D | G} {R | A} repetitions objectCount1, ..., objectCountN");
                return;
            }
            
            dbSystem = args[0].ToUpper();
            testType = args[1].ToUpper();
            repetitions = int.Parse(args[2]);
            
            if (testType == "A")
            {
                fileName = "alltests.xls";
            } 
            else
            {
                fileName = "readtest.xls";
            }

            for(int i = 3; i < args.Length; i++)
            {
                objectCounts.AddLast(int.Parse(args[i]));
            }

            TestOutput to = new TestOutput (fileName, dbSystem);

            if (dbSystem == "D")
            {
                dbtest = new DLinqTest(to);
            }
            else if (dbSystem  == "G")
            {
                GenDB.DataContext dc = GenDB.DataContext.Instance;
                dc.DatabaseName = "perftest";
                if (!dc.DatabaseExists())
                {
                    dc.CreateDatabase();
                }
                dc.Init();
                dbtest= new GenDBPerfTests<PerfTestAllPrimitiveTypes>(dc);
            }
            else 
            {
                Console.WriteLine ("DBSystem should be 'D' (DLinq) or 'G' (GenDB).");
            }

            if (testType == "R")
            {
                theTest = new JustRead (dbtest, objectCounts, repetitions, to);
            }
            else if (testType == "A")
            {
                theTest = new RunAllTests(dbtest, objectCounts, repetitions, to);
            }

            theTest.PerformTest();
        }
    }
}
