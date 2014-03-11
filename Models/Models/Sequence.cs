using System;
using Sqo;
using Sqo.Attributes;

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

		public static int Next(string id, Siaqodb db)
		{
			var transaction = db.BeginTransaction ();
			try {
				var sequence = db.Cast<Sequence>().Where<Sequence>(s => s.id == id).FirstOrDefault();
				if (sequence == null) sequence = new Sequence(id);
				sequence.seq++;
				db.StoreObject(sequence);
				return sequence.seq;
			} catch (Exception ex) {
				transaction.Rollback ();
				throw ex;
				// TODO: Retry?
			}
		}
	}
}

