﻿using System;
using Sqo;
using Sqo.Attributes;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace RaceYourself.Models
{
	public class Track
	{
		[Index]
		[UniqueConstraint]
        public long id = 0;

		public string _id; // Server-side guid. TODO: Ignore in the future?

		[JsonProperty("device_id")]		
		public int deviceId;
		[JsonProperty("track_id")]		
		public int trackId;

		[JsonProperty("track_name")]		
		public string trackName;
		public int track_type_id;
		public long ts;
		public bool @public;
		public double distance;
		public long time;

		public List<Position> positions; // Embedded positions for explicit track fetch

		public DateTime? deleted_at;

		[JsonIgnore]
		public bool dirty = false;

        public void save(Siaqodb db) {
            if (this.trackId <= 0) {
                trackId = Sequence.Next("track", db);
            }

            if (this.id <= 0)
                GenerateCompositeId ();

            if (!db.UpdateObjectBy ("id", this)) {
                db.StoreObject (this);
            }
        }

		public long GenerateCompositeId() {
			uint high = (uint)deviceId;
            uint low = (uint)trackId;

			ulong composite = (((ulong) high) << 32) | low;
			this.id = (long)composite;
			return this.id;
		}

		public DateTime date {
			get {
				return Date.FromUnixTime(ts);
			}
		}
	}
}

