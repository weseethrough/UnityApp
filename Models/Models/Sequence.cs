using System;
using Sqo;
using Sqo.Attributes;
using UnityEngine;

namespace RaceYourself.Models
{
	public class Sequence
	{
		[Index]
		[UniqueConstraint]
		public string id;
		public int seq;

		public Sequence() {}
		private Sequence(string id)
		{
			this.id = id;
			this.seq = 0;
		}

        public static Sequence Get(string id, Siaqodb db) {
            Exception fault = null;
            for (int retries = 3; retries > 0; retries--) {
                var transaction = db.BeginTransaction ();
                try {
                    var sequence = db.Query<Sequence> ().Where<Sequence> (s => s.id == id).FirstOrDefault ();
                    if (sequence == null) {
                        Console.WriteLine ("Sequence: creating sequence " + id);
                        sequence = new Sequence (id);
                        db.StoreObject (sequence, transaction);
                        transaction.Commit ();
                    }
                    return sequence;    
                } catch (Exception ex) {
                    if (retries > 0) {
                        Console.WriteLine ("Sequence: Failed to get sequence " + id + ", retrying " + retries + " times");
                        //                        UnityEngine.Debug.LogException (ex);
                    }
                    transaction.Rollback ();
                    fault = ex;
                }
            }
            throw fault;
        }

        public void Save(Siaqodb db) {
            if (!db.UpdateObjectBy ("id", this)) {
                db.StoreObject (this);
            }
        }

	}
}

