using System;
using Newtonsoft.Json;

namespace RaceYourself.Models
{
	public class Message
	{
		public string type;

		// TODO: Polymorphism
		public int from;
		[JsonConverter(typeof(ObjectIdConverter))]
		public string challenge_id;
		public string taunt;
	}
}

