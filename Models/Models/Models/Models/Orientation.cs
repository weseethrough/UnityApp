using System;
using Newtonsoft.Json;
using Sqo.Attributes;

namespace RaceYourself.Models
{
	public class Orientation
	{
		[Index]
		[UniqueConstraint]
		public long id;

		[JsonConverter(typeof(ObjectIdConverter))]
		public string _id; // Server-side guid. TODO: Ignore in the future?

		[JsonProperty("device_id")]
		public int deviceId;
		[JsonProperty("orientation_id")]
		public int orientationId;

		public int track_id;
		public long ts;

		public float roll;
		public float pitch;
		public float yaw;

		public float mag_x;
		public float mag_y;
		public float mag_z;
		public float acc_x;
		public float acc_y;
		public float acc_z;
		public float gyro_x;
		public float gyro_y;
		public float gyro_z;
		public float rot_a;
		public float rot_d;
		public float rot_c;
		public float rot_b;
		public float linacc_x;
		public float linacc_y;
		public float linacc_z;

		public DateTime? deleted_at;

		[JsonIgnore]
		public bool dirty = false;

		public long GenerateCompositeId() {
			uint high = (uint)deviceId;
			uint low = (uint)orientationId;

			ulong composite = (((ulong) high) << 32) | low;
			this.id = (long)composite;
			return this.id;
		}
	}
}

