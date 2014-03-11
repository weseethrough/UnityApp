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
			return objectType.GetMethod("ReadJson").Invoke(null, new object[] {reader, serializer});
		}

		public override bool CanConvert(Type objectType)
		{
			return (typeof(Jsonable).IsAssignableFrom(objectType));
		}
	}
}

