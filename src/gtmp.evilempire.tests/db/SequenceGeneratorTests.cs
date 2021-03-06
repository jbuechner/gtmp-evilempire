﻿using gtmp.evilempire.db;
using gtmp.evilempire.services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace gtmp.evilempire.tests.db
{
    [TestClass]
    public class SequenceGeneratorTests : DatabaseTestBase
    {
        [TestMethod]
        public void CreateNewSequenceAndReturnValue()
        {
            using (IDbService dbService = DbServiceFactory())
            {
                var currentValue = dbService.ValueFor("sequence1");
                Assert.AreEqual(currentValue, (int?)null);

                var nextValue = dbService.NextValueFor("sequence1");
                currentValue = dbService.ValueFor("sequence1");

                Assert.AreEqual(nextValue, 0);
                Assert.AreEqual(nextValue, currentValue);

                nextValue = dbService.NextValueFor("sequence1");
                Assert.AreEqual(nextValue, 1);
            }
        }

        [TestMethod]
        public void CreateNewSequenceAndIncrementMultipleTimes()
        {
            using (IDbService dbService = DbServiceFactory())
            {
                var sequence = "seq";
                var max = 1000;
                Stopwatch sw = new Stopwatch();
                sw.Start();
                for (var i = 0; i < max; i++)
                {
                    var nextValue = dbService.NextValueFor(sequence);
                    Assert.AreEqual(nextValue, i);
                }
                sw.Stop();
                TestContext.WriteLine($"Time for {max} values: {sw.ElapsedMilliseconds}ms");
            }
        }

        [TestCategory(TestConstants.LongRunningCategory)]
        [TestMethod]
        public void CreateNewSequenceAndIncrementMultipleTimesUsingHighConcurrency()
        {
            using (IDbService dbService = DbServiceFactory())
            {
                var sequence = "x";
                Stopwatch sw = new Stopwatch();
                sw.Start();
                var workerCount = 100;
                var max = 1000;

                List<Task> workers = new List<Task>();
                HashSet<int> hashSet = new HashSet<int>();
                for (var i = 0; i < workerCount; i++)
                {
                    workers.Add(new Task(() =>
                    {
                        for (var x = 0; x < max; x++)
                        {
                            var v = dbService.NextValueFor(sequence);
                            Assert.IsTrue(hashSet.Add(v));
                        }
                    }));
                }
                var r = Parallel.ForEach(workers, t =>
                {
                    t.Start();
                    t.Wait();
                });

                Assert.AreEqual(hashSet.Count, workerCount * max);
            }
        }

        [TestCategory(TestConstants.LongRunningCategory)]
        [TestMethod]
        public void CreateNewSequenceAndIncrementMultipleTimesUsingHighConcurrencyForInt64Values()
        {
            using (IDbService dbService = DbServiceFactory())
            {
                var sequence = "x22";
                Stopwatch sw = new Stopwatch();
                sw.Start();
                var workerCount = 100;
                var max = 1000;

                List<Task> workers = new List<Task>();
                HashSet<long> hashSet = new HashSet<long>();
                for (var i = 0; i < workerCount; i++)
                {
                    workers.Add(new Task(() =>
                    {
                        for (var x = 0; x < max; x++)
                        {
                            long v = dbService.NextInt64ValueFor(sequence);
                            Assert.IsTrue(hashSet.Add(v));
                        }
                    }));
                }
                var r = Parallel.ForEach(workers, t =>
                {
                    t.Start();
                    t.Wait();
                });

                Assert.AreEqual(hashSet.Count, workerCount * max);
            }
        }

        [TestMethod]
        public void CreateSequenceWithSeed()
        {
            using (var dbService = DbServiceFactory())
            {
                var seed = -15;
                var sequence = "y";
                var round = 100;
                for (var i = 0; i < round; i++)
                {
                    var next = dbService.NextValueFor(sequence, seed);
                    Assert.AreEqual(seed + i, next);
                }
            }
        }

        [TestMethod]
        public void CreateInt64SequenceWithSeed()
        {
            using (var dbService = DbServiceFactory())
            {
                var seed = long.MinValue + 1;
                var sequence = "y";
                var round = 400;
                for (var i = 0; i < round; i++)
                {
                    var next = dbService.NextInt64ValueFor(sequence, seed);
                    Assert.AreEqual(next, seed + i);
                }
            }
        }
    }
}
