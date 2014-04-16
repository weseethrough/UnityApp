using System;
using System.Collections.Concurrent;
using System.Threading;
using Sqo;

namespace RaceYourself.Models
{
    public class Sequences
    {
        public static Sequences Instance {
            get {
                if (instance == null) {
                    lock (syncRoot) {
                        if (instance == null) {
                            instance = new Sequences();
                        }
                    }
                }
                return instance;
            }
        }
        private static volatile Sequences instance = null;
        private static object syncRoot = new Object();

        private ConcurrentDictionary<string, Sequence> sequences = new ConcurrentDictionary<string, Sequence>();

        private Sequences() {}

        public int Next(string id, Siaqodb db) {
            Sequence sequence = null;
            if (!sequences.TryGetValue (id, out sequence)) {
                lock (syncRoot) {
                    if (!sequences.TryGetValue (id, out sequence)) {
                        if (!sequences.TryAdd (id, Sequence.Get(id, db))) {
                            throw new Exception("Unhandled race condition for sequence " + id);
                        }
                        if (!sequences.TryGetValue (id, out sequence)) {
                            throw new Exception("Unhandled race condition for sequence " + id);
                        }
                    }
                }
            }

            return Interlocked.Increment(ref sequence.seq);
        }

        public void Flush(Siaqodb db) {
            lock (syncRoot) {
                foreach (var sequence in sequences.Values) {
                    sequence.Save(db);
                }
                sequences.Clear();
            }
        }
    }
}

