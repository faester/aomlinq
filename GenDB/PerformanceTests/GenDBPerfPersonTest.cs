using System;
using System.Collections.Generic;
using System.Text;
using GenDB;
using System.Data.DLinq;
using System.Diagnostics;


namespace PerformanceTests
{
    class GenDBPerfPersonTest : ReadWriteClearTest
    {
        GenDB.DataContext dataContext = GenDB.DataContext.Instance;
        GenDB.Table<PerfPerson> table = GenDB.DataContext.Instance.GetTable<PerfPerson>();
        int lastInsert = 0;

        public GenDBPerfPersonTest(GenDB.DataContext dataContext) {
            this.dataContext = dataContext;
        }

        public void InitTests(int objectCount)
        {
            long l = PerformWriteTest(objectCount);
        }

        public long PerformAllTests(int objectCount) 
        {
            return PerformWriteTest(objectCount)+PerformReadTest(objectCount)+PerformClearTest();
        }

        public long PerformAllQueryTests(int objectCount)
        {
            return PerformSimpleSelectTest(objectCount) + 
                PerformCompositeSelectTest(objectCount) +
                PerformJoinedSelectTest(objectCount) +
                PerformSubSelectTest(objectCount);
        }

        public long PerformWriteTest(int objectCount) 
        {   
            lastInsert = objectCount;
            //PerfPerson[] tmp = new PerfPerson[3];
            //int c=0;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for(int i=0; i<objectCount; i++)
            {
                PerfPerson pp = new PerfPerson{Name = "name"+i};
                if(i%2==0)
                {
                    pp.Spouse = new PerfPerson{Name = "spouse"+1};
                }

                // DICT AND LIST TEST
                //for(int j=0;j<3;j++) 
                //{
                //    pp.Aliases.Add("alias"+j);
                //    if(tmp[j]!=null)
                //    {
                //        pp.Friends.Add(tmp[j].Id,tmp[j]);
                //        tmp[j]=null;
                //    }
                //}
                //tmp[c]=pp;
                //c++;
                //if(c==3)c=0;

                table.Add(pp);
            }
            dataContext.SubmitChanges();
            long ms = sw.ElapsedMilliseconds;
            return ms;
        }

        public long PerformReadTest(int objectCount) 
        {
            Console.WriteLine("******:"+table.Count);
            int count = 0;
            Stopwatch sw = new Stopwatch();
            sw.Start();

            foreach (PerfPerson p in table)
            {
                count++;
                if (count >= objectCount)
                {
                    break;
                }
            }

            long ms = sw.ElapsedMilliseconds;

            if (count < objectCount)
            {
                return -1;
            }
            else if (count > objectCount)
            {
                return -ms;
            }
            else
            {
                return ms;
            }
        }

        public long PerformClearTest() 
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            table.Clear();
            dataContext.SubmitChanges();
            long ms = sw.ElapsedMilliseconds;
            return ms;
        }

        public long PerformSimpleSelectTest(int objectCount)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            int c=0;
            var v = from t in table
                    where t.Name == "name9" || t.Name == "name10" && t.Spouse.Name != "spouse11"
                    select t;
            foreach(PerfPerson pp in v) {c++;}
            long ms = sw.ElapsedMilliseconds;
            return ms;
        }

        public long PerformCompositeSelectTest(int objectCount)
        {
            return 0;
        }

        public long PerformJoinedSelectTest(int objectCount)
        {
            return 0;
        }

        public long PerformSubSelectTest(int objectCount) 
        {
            return 0;
        }
    }
}
