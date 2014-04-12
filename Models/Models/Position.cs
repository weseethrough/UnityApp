using System;
using Newtonsoft.Json;
using Sqo.Attributes;
using UnityEngine;
using Sqo;

namespace RaceYourself.Models
{
	public class Position
	{
		[Index]
		[UniqueConstraint]
        public long id = 0;

		[JsonProperty("device_id")]
		public int deviceId;
		[JsonProperty("position_id")]
        public int positionId;

		[JsonProperty("track_id")]
		public int trackId;
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

		private const float DEGREES_TO_METERS = 111111.11f;

		public Position() {}
		public Position(float lat, float lng)
		{
			this.latitude = lat;
			this.longitude = lng;
		}

        public void save(Siaqodb db) {
            if (this.positionId <= 0) {
                positionId = Sequence.Next("position", db);
            }

            if (this.id <= 0)
                GenerateCompositeId ();
                
            if (!db.UpdateObjectBy ("id", this)) {
                db.StoreObject (this);
            }
        }
        
        public long GenerateCompositeId ()
        {
            long high = deviceId;
            uint low = (uint) positionId;
            
            long composite = (high << 32) | low;
            this.id = composite;
            return this.id;
        }

		public Vector3 ToXYZ()
        {
			float x = latitude * DEGREES_TO_METERS;
			float y = 0f; //height
			float z = longitude * DEGREES_TO_METERS * (float)Math.Cos (latitude);
			return new Vector3(x,y,z);
		}
	}
}

