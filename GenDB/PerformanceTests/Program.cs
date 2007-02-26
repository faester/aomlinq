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
            if (args.Length == 0)
            {
                args = new String[] { "G", "Q", "1", "1000", "10000", "20000"};
                Console.Write("No parameters specified. Using '");
                bool first = true;
                foreach (string s in args)
                {
                    if (!first) { Console.Write(", "); }
                    else { first = false; }
                    Console.Write(s);
                }
                Console.WriteLine();
                Console.WriteLine("Use /? to get help");
                
            }
            
            ITest theTest = null;
            ReadWriteClearTest dbtest = null;
            QueryTest qtest = null;
            string dbSystem = "G";
            string testType = "R";
            int repetitions = 0;
            string fileName = null;
            LinkedList<int> objectCounts = new LinkedList<int>();

            if (args.Length < 4)
            {
                Console.WriteLine("Usage: PerformanceTests {D | G} {R | A | Q} repetitions objectCount1, ..., objectCountN");
                return;
            }
            
            dbSystem = args[0].ToUpper();
            testType = args[1].ToUpper();
            repetitions = int.Parse(args[2]);

            
            for(int i = 3; i < args.Length; i++)
            {
                objectCounts.AddLast(int.Parse(args[i]));
            }

            if(testType == "Q") 
            {
                fileName = "querytest2.xls";
                TestOutput to = new TestOutput (fileName, dbSystem,"Q");
                Console.WriteLine("Writing to {0}",fileName);

                //if(dbSystem=="G")
                //{
                //    qtest = new GenDBPerfPersonTest<PerfPerson>(GenDB.DataContext.Instance);
                //    qtest.CleanDB();
                //} 
                //else if(dbSystem=="D")
                //{
                //    qtest = new DLinqPerfPersonTest<PerfPerson>(to, "perftestDQ");
                //    qtest.CleanDB();
                //}
                

                foreach(int obj in objectCounts)
                {
                    Console.WriteLine("choosing db...");
                    if(dbSystem=="G")
                    {
                        GenDB.DataContext dc = GenDB.DataContext.Instance;
                        dc.DatabaseName = "perftestGQ";
                        //dc.DeleteDatabase();
                        if (dc.DatabaseExists() && !dc.IsInitialized)
                        {
                            dc.DeleteDatabase();
                        }
                        if (!dc.DatabaseExists())
                        {
                            dc.CreateDatabase();
                            //dc.Init();
                        }                        
                        qtest = new GenDBPerfPersonTest<PerfPerson>(GenDB.DataContext.Instance);
                    }
                    else if(dbSystem=="D")
                    {
                        qtest = new DLinqPerfPersonTest<PerfPerson>(to, "perftestDQ");
                    }
                    //Console.WriteLine("cleaning db...");
                    //qtest.CleanDB();

                    Console.WriteLine("initializing data...");
                    qtest.InitTests(obj);

                    Console.WriteLine("starting query tests...");
                    theTest = new RunQueryTests(qtest, obj, repetitions, to);
                    theTest.PerformTest();
                }
                Console.WriteLine("cleaning db...");
                qtest.CleanDB();
            } 
            else
            {
                 if (testType == "A")
                {
                    fileName = "alltests.xls";
                } 
                else
                {
                    fileName = "read.xls";
                }
                
                Console.WriteLine("Output: " + fileName);
                TestOutput to = new TestOutput (fileName, dbSystem, "X");

                string dbName = "perftest" + dbSystem + testType;


                if (dbSystem == "D")
                {
                    dbtest = new DLinqTest(to, dbName);
                }
                else if (dbSystem  == "G")
                {
                    GenDB.DataContext dc = GenDB.DataContext.Instance;
                    dc.DatabaseName = dbName;
                    if (testType != "R" && dc.DatabaseExists())
                    {
                        dc.DeleteDatabase();
                    }
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
}
