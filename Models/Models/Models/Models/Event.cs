using System;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace RaceYourself.Models
{
	public class Event
	{
		private const int VERSION = 1;

		public int? device_id;
		public int session_id;
		public long ts;
		public int version;
		[JsonConverter(typeof(JsonStringConverter))]
		public string data;

		public Event() {}
		public Event(string data, int session_id) {
			this.version = VERSION;
			this.ts = Date.UnixTime.Milliseconds;
			this.data = data;
			this.session_id = session_id;
		}

		[OnSerializing]
		internal void OnSerializingMethod(StreamingContext context)
		{
			if (!device_id.HasValue) throw new Exception ("Set a device_id before serializing!");
		}
	}
}

