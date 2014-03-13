using System;
using Sqo.Attributes;
using Newtonsoft.Json;
using Sqo;

namespace RaceYourself.Models
{
	public class Transaction
	{
		[Index]
		[UniqueConstraint]
		[JsonIgnore]
		public long id;

		public string _id;

		[JsonProperty("device_id")]		
		public int deviceId;
		[JsonProperty("transaction_id")]		
		public int transactionId;
		public int user_id;

		public long points_delta;
		public long points_balance;
		public int gems_delta;
		public int gems_balance;
		public float metabolism_delta;
		public float metabolism_balance;
		public string transaction_type;
		public string transaction_calc;
		public string source_id;
		[Index]
		public long ts;

		public DateTime updated_at;
		public DateTime? deleted_at;

		[JsonIgnore]
		public bool dirty = false;

		public long GenerateCompositeId() {
			uint high = (uint)deviceId;
			uint low = (uint)transactionId;

			ulong composite = (((ulong) high) << 32) | low;
			this.id = (long)composite;
			return this.id;
		}

		public void save(Siaqodb db) {
			if (this.id <= 0)
				GenerateCompositeId ();

			if (!db.UpdateObjectBy("id", this)) {
				db.StoreObject(this);
			}
		}
	}
}

