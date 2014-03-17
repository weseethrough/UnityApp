using System;
using Newtonsoft.Json;

namespace RaceYourself.Models
{
	[JsonConverter(typeof(DefaultConverter))]
	public class DurationChallenge : Challenge
	{
		public int duration;
		public int distance;

		public DurationChallenge() 
		{
			this.type = "duration";
		}
		public DurationChallenge(int duration, int distance) : this()
		{
			this.duration = duration;
			this.distance = distance;
		}

	}
}

