using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using GenDB;
using System.Query;

namespace Tests
{
    /// <summary>
    /// Attempt to make more exhaustive tests of translatability
    /// of various logical expressions.
    /// </summary>
    [TestFixture]
    public class LogicalExpressionsTests
    {
        class BlibBlob : AbstractBusinessObject
        {
            bool blib;
            bool blob;

            int a, b;

            public int A
            {
                get { return a; }
                set { a = value; }
            }

            public int B
            {
                get { return b; }
                set { b = value; }
            }

            public bool Blib
            {
                get { return blib; }
                set { blib = value; }
            }

            public bool Blob
            {
                get { return blob; }
                set { blob = value; }
            }
        }

        Table<BlibBlob> bbs;
        DataContext context;

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            context = DataContext.Instance;

            if (!context.IsInitialized)
            {
                context.DatabaseName = "generic";
                if (!context.DatabaseExists())
                {
                    context.CreateDatabase();
                }
                context.Init();
            }

            bbs = context.CreateTable<BlibBlob>();

            for (int i = 0; i < 20; i++)
            {
                BlibBlob bb = new BlibBlob();
                bb.Blib = i % 2 == 0;
                bb.Blob = i % 3 == 0;
                bb.A = i;
                bb.B = i / 3;
                bbs.Add(bb);
            }

            context.SubmitChanges();
        }

        private void TryFunc(Func<BlibBlob, bool> fn)
        {
            var k = from b in bbs 
                    where fn(b)
                    select b;

            foreach(BlibBlob bb in k)
            {
                Assert.IsTrue (fn(bb), "Error in returned result.");
            }
        }

        [Test]
        public void XOrTranslatability()
        {
            TryFunc(b => b.Blib ^ b.Blob);
        }

        [Test]
        public void AndTranslatability()
        {
            TryFunc(b => b.Blib && b.Blob);
        }
        
        [Test]
        public void ConditionalTranslatability()
        {
            TryFunc(b => (b.Blib ? 1 : 0) + (b.Blob ? 1 : 0) == 1);
        }

        [Test]
        public void CastTranslatability()
        {
            TryFunc(b => ((short)b.A) * ((long)b.B) > 0);
        }
    }
}
