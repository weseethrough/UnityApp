using System;

namespace RaceYourself.Models
{
	public class Track
	{
		public string _id;

		public int device_id;
		public int track_id;

		public string track_name;
		public int track_type_id;
		public long ts;
		public bool @public;
		public float distance;
		public int time;

		public DateTime? deleted_at;
	}
}

