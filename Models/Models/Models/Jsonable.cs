using System;
using Newtonsoft.Json;

namespace RaceYourself.Models
{
	[JsonConverter(typeof(CustomConverter))]
	public interface Jsonable
	{
		void WriteJson(JsonWriter writer, JsonSerializer serializer);
		void ReadJson(JsonReader reader, JsonSerializer serializer);
	}
}

