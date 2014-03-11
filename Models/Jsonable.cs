using System;
using Newtonsoft.Json;

namespace RaceYourself.Models
{
	public interface Jsonable
	{
		void WriteJson(JsonWriter writer, JsonSerializer serializer);
		// static Jsonable ReadJson(JsonReader reader, JsonSerializer serializer); used through reflection
	}
}

