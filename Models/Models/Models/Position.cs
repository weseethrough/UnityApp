using System;
using Newtonsoft.Json;
using Sqo.Attributes;

namespace RaceYourself.Models
{
	public class Position
	{
		[Index]
		[UniqueConstraint]
		[JsonConverter(typeof(ObjectIdConverter))]
		public string _id;

		public int device_id;
		public int track_id;
		public int position_id;

		public int state_id;
		public long gps_ts;
		public long device_ts;
		public float lng;
		public float lat;
		public float alt;
		public float bearing;
		public float corrected_bearing;
		public float corrected_bearing_R;
		public float corrected_bearing_significance;
		public float speed;
		public float epe;
		public string nmea;

		public DateTime? deleted_at;
	}
}

