using NUnit.Framework;
using System;
using System.IO;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using Sqo;

namespace RaceYourself.Models
{
	[TestFixture ()]
    public class SiaqoTests
	{
		[Test ()]
        public void TestSequence ()
		{
            const int ROUNDS = 1000;
            Random random = new Random();
            List<Thread> threads = new List<Thread>();
            var path = Path.Combine (Path.GetTempPath (), "siaqo_unit_tests");
            //if (Directory.Exists(path)) Directory.Delete(path, true);
            Directory.CreateDirectory(path);
            Siaqodb db = new Siaqodb(path);
            var oseq = Sequences.Instance.Next("_internal_unit_test", db);
            for (int i = 0; i < ROUNDS; i++) {
                Thread thread = new Thread (() => {
                    var n = i;
                    Sequences.Instance.Next("_internal_unit_test", db);
                    Thread.Sleep(random.Next()%100);
                    Sequences.Instance.Next("_internal_unit_test", db);
                    Thread.Sleep(random.Next()%100);
                    Sequences.Instance.Next("_internal_unit_test", db);
                });
                thread.Start();
                threads.Add(thread);
            }
            foreach (Thread thread in threads) {
                thread.Join();
            }
            var sequence = Sequence.Get("_internal_unit_test", db);
            int seq = Sequences.Instance.Next("_internal_unit_test", db);
            //Console.WriteLine(seq);
            //Console.WriteLine(seq-oseq);
            //Console.WriteLine(path);
            Assert.IsTrue (seq == sequence.seq+1);
            Assert.IsTrue ((seq-oseq) == (3*ROUNDS+1));
            db.Close();
		}

    }
}

