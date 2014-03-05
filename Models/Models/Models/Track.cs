using System;
using Sqo.Attributes;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace RaceYourself.Models
{
	public class Track
	{
		[Index]
		[UniqueConstraint]
		public long id;

		public string _id; // Server-side guid. TODO: Ignore in the future?

		public int device_id;
		public int track_id;

		public string track_name;
		public int track_type_id;
		public long ts;
		public bool @public;
		public float distance;
		public int time;

		public DateTime? deleted_at;

		[JsonIgnore]
		public bool dirty = false;

		public long GenerateCompositeId() {
			uint high = (uint)device_id;
			uint low = (uint)track_id;

			ulong composite = (((ulong) high) << 32) | low;
			this.id = (long)composite;
			return this.id;
		}
	}
}

