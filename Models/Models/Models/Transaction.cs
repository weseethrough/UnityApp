using System;
using Sqo.Attributes;

namespace RaceYourself.Models
{
	public class Transaction
	{
		[Index]
		[UniqueConstraint]
		public string _id;

		public int device_id;
		public int transaction_id;
		public int user_id;

		public int points_delta;
		public int points_balance;
		public int gems_delta;
		public int gems_balance;
		public float metabolism_delta;
		public float metabolism_balance;
		public string transaction_type;
		public string transaction_calc;
		public string source_id;
		public long ts;

		public DateTime updated_at;
		public DateTime? deleted_at;
	}
}

