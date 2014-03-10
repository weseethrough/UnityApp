using System;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using Sqo.Attributes;

namespace RaceYourself.Models
{
	public class Event
	{
		private const int VERSION = 1;

		[JsonProperty("device_id")]
		public int deviceId;
		[JsonProperty("session_id")]
		public int sessionId;
		public long ts;
		public int version;
		[Text]
		[JsonConverter(typeof(JsonStringConverter))]
		public string data;

		public Event() {}
		public Event(string data, int session_id) {
			this.version = VERSION;
			this.ts = Date.UnixTime.Milliseconds;
			this.data = data;
			this.sessionId = session_id;
		}

		[OnSerializing]
		internal void OnSerializingMethod(StreamingContext context)
		{
			// TODO: Remove in production?
			if (deviceId <= 0) throw new Exception ("Set a device_id before serializing!");
		}
	}
}

