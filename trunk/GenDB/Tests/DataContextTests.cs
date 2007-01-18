using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using GenDB;
using CommonTestObjects;

namespace GenDBTests
{
    [TestFixture]
    public class DataContextTests
    {
        DataContext dc = DataContext.Instance;
        Table<TestPerson> t_tp;


        [SetUp]
        public void SetUp()
        {
            t_tp = dc.CreateTable<TestPerson>();
            t_tp.Clear();

            dc.SubmitChanges();
        }


        [Test]
        public void CanRollbackTransaction()
        {
            t_tp.Add (new TestPerson{Age = 0});
            t_tp.Add (new TestPerson{Age = 1});
            t_tp.Add (new TestPerson{Age = 2});
            t_tp.Add (new TestPerson{Age = 3});
            t_tp.Add (new TestPerson{Age = 4});
            dc.SubmitChanges();
            t_tp.Add (new TestPerson{Age = 5});
            t_tp.Add (new TestPerson{Age = 6});
            t_tp.Add (new TestPerson{Age = 7});
            t_tp.Add (new TestPerson{Age = 8});
            t_tp.Add (new TestPerson{Age = 9});
            dc.RollbackTransaction();

            bool[] exists = new bool[10];

            foreach(TestPerson p in t_tp)
            {
                exists[p.Age] = true;
            }


            for (int i = 0; i < 10; i++)
            {
                Assert.AreEqual(i < 5, exists[i], "RollBack did not work as intended");
            }
        }

    }
}
