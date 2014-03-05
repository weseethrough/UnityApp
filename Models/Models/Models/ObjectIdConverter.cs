using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RaceYourself.Models
{
	public class ObjectIdConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			JObject oid = new JObject(
				new JProperty("$oid", value.ToString())
			);
			oid.WriteTo(writer);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			JObject oid = JObject.Load(reader);
			return (string)oid["$oid"];
		}

		public override bool CanConvert(Type objectType)
		{
			return (objectType == typeof (string));
		}
	}
}

