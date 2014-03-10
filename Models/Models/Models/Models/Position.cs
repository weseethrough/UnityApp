using System;
using Newtonsoft.Json;
using Sqo.Attributes;

namespace RaceYourself.Models
{
	public class Position
	{
		[Index]
		[UniqueConstraint]
		public long id;

		[JsonConverter(typeof(ObjectIdConverter))]
		public string _id; // Server-side guid. TODO: Ignore in the future?

		public int device_id;
		public int position_id;

		public int track_id;
		public int state_id;
		public long gps_ts;
		public long device_ts;
		[JsonProperty("lat")]
		public float latitude;
		[JsonProperty("lng")]
		public float longitude;
		public float alt;
		public float bearing;
		public float corrected_bearing;
		public float corrected_bearing_R;
		public float corrected_bearing_significance;
		public float speed;
		public float epe;
		public string nmea;

		public DateTime? deleted_at;

		[JsonIgnore]
		public bool dirty = false;

		public Position() {}
		public Position(float lat, float lng)
		{
			this.latitude = lat;
			this.longitude = lng;
		}

		public long GenerateCompositeId() {
			uint high = (uint)device_id;
			uint low = (uint)position_id;

			ulong composite = (((ulong) high) << 32) | low;
			this.id = (long)composite;
			return this.id;
		}
	}
}

