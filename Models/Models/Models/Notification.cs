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
		public string _id;

		public bool read;
		public Message message;
	}
}

