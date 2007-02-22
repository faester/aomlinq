using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Data.SqlClient;
using GenDB;

namespace PerformanceTests
{
    class Program
    {
        public static void Main(string[] args)
        {
            args = new String[]{"G","R","20","30","400"};
            

            ITest theTest = null;
            ReadWriteClearTest dbtest = null;
            string dbSystem = "G";
            string testType = "R";
            int repetitions = 0;
            string fileName = null;
            LinkedList<int> objectCounts = new LinkedList<int>();

            //GenDB.DataContext.Instance.DatabaseName = "knold";
            //if (!GenDB.DataContext.Instance.DatabaseExists())
            //{
            //    GenDB.DataContext.Instance.CreateDatabase();
            //}
            //GenDB.DataContext.Instance.Init();
            
            //GenDB.Table<PerfTestAllPrimitiveTypes > table = GenDB.DataContext.Instance.GetTable<PerfTestAllPrimitiveTypes>();

            //var q = from p in table
            //        where p.Dbl * p.Integer == 0.2
            //        select p;

            //foreach(PerfTestAllPrimitiveTypes pt in q)
            //{
            //    Console.WriteLine (pt);
            //}
            //return;

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
                fileName = "read.xls";
            }

            for(int i = 3; i < args.Length; i++)
            {
                objectCounts.AddLast(int.Parse(args[i]));
            }

            Console.WriteLine("Output: " + fileName);
            TestOutput to = new TestOutput (fileName, dbSystem);

            string dbName = "perftest" + dbSystem + testType;


            if (dbSystem == "D")
            {
                dbtest = new DLinqTest(to, dbName);
            }
            else if (dbSystem  == "G")
            {
                GenDB.DataContext dc = GenDB.DataContext.Instance;
                dc.DatabaseName = dbName;
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
