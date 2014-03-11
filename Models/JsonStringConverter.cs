using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RaceYourself.Models
{
	public class JsonStringConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			JObject json = JObject.Parse(value as string);
			json.WriteTo(writer);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			JObject json = JObject.Load(reader);
			return json.ToString();
		}

		public override bool CanConvert(Type objectType)
		{
			return (objectType == typeof (string));
		}
	}
}

