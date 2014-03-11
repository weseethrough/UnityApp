using System;
using Newtonsoft.Json;

namespace RaceYourself.Models
{
	[JsonConverter(typeof(DefaultConverter))]
	public class DistanceChallenge : Challenge
	{
		public int distance;
		public int time;

		public DistanceChallenge() 
		{
			this.type = "distance";
		}
		public DistanceChallenge(int distance, int time) : this()
		{
			this.distance = distance;
			this.time = time;
		}
	}
}

