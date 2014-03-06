using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RaceYourself.Models
{
	public class CustomConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			Jsonable jo = value as Jsonable;
			jo.WriteJson(writer, serializer);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			Jsonable jo = existingValue as Jsonable;
			if (jo == null) jo = Activator.CreateInstance(objectType) as Jsonable;
			jo.ReadJson(reader, serializer);
			return jo;
		}

		public override bool CanConvert(Type objectType)
		{
			return (typeof(Jsonable).IsAssignableFrom(objectType));
		}
	}
}

