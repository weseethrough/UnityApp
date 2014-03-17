using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sqo.Attributes;

namespace RaceYourself.Models
{
	[JsonConverter(typeof(CustomConverter))]
	public abstract class Challenge : Jsonable
	{
		[Index]
		[UniqueConstraint]
		[JsonConverter(typeof(ObjectIdConverter))]
		[JsonProperty("_id")]		
		public string id;

		public int? creator_id;
		public List<Attempt> attempts;
		public List<int> subscribers;
		public DateTime? start_time;
		public DateTime? stop_time;
		public List<double> location;
		public bool @public;

		public string type;
					
		///  Internal

		void Jsonable.WriteJson(JsonWriter writer, JsonSerializer serializer) {
			throw new NotImplementedException(); // abstract, handle in subclass or override with DefaultConverter
		}
		public static Challenge ReadJson(JsonReader reader, JsonSerializer serializer) {
			JObject jo = JObject.Load(reader);

			var type = jo.Value<string>("type");
			switch(type) {
			case "distance":
				return jo.ToObject<DistanceChallenge>();
			case "duration":
				return jo.ToObject<DurationChallenge>();
			default:
				throw new NotImplementedException("Unknown challenge type: " + jo["type"]);
			}
		}

		public class Attempt 
		{
			public int device_id;
			public int track_id;
			public int user_id;
		}
	}
}

