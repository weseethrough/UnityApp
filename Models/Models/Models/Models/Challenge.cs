using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Sqo.Attributes;

namespace RaceYourself.Models
{
	public class Challenge
	{
		[Index]
		[UniqueConstraint]
		[JsonConverter(typeof(ObjectIdConverter))]
		public string _id;

		public int? creator_id;
		public List<string> attempt_ids;
		public List<int> subscribers;
		public DateTime? start_time;
		public DateTime? stop_time;
		public List<double> location;
		public bool @public;

		// TODO: Polymorphism
		public int? duration;
		public int? distance;
		public int? time;
	}
}

