using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RaceYourself.Models
{
	[JsonConverter(typeof(CustomConverter))]
	public class Action : Jsonable
	{
		public string json;

		public Action() {}
		public Action(string json) {
			this.json = json;
		}

		void Jsonable.WriteJson(JsonWriter writer, JsonSerializer serializer) {
			JObject jo = JObject.Parse(json);
			jo.WriteTo(writer);
		}
		public static Action ReadJson(JsonReader reader, JsonSerializer serializer) {
			JObject jo = JObject.Load(reader);
			return new Action(jo.ToString());
		}
	}
}

