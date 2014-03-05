using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RaceYourself.Models
{
	public class Notification
	{
		[JsonConverter(typeof(ObjectIdConverter))]
		public string _id;

		public bool read;
		public Dictionary<string, object> message;
	}
}

