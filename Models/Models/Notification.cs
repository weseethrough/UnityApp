using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Sqo.Attributes;

namespace RaceYourself.Models
{
	public class Notification
	{
		[Index]
		[UniqueConstraint]
		[JsonConverter(typeof(ObjectIdConverter))]
		[JsonProperty(PropertyName="_id")]
		public string id;

		public bool read;
		public Message message;

		[JsonIgnore]
		public bool dirty = false;
	}
}

