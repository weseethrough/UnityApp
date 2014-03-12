using System;
using Sqo.Attributes;
using Newtonsoft.Json;

namespace RaceYourself.Models
{
	public class Friend
	{
		[Index]
		[UniqueConstraint]
		[JsonProperty("_id")]
		public string guid;
		[JsonProperty("has_glass")]
		public bool hasGlass;
		public string name;
		[JsonProperty("photo")]
		public string image;
		public string uid;
		public string provider;
		[JsonProperty("user_id")]
		public int? userId;

		// TOOD: Polymophism
		[JsonProperty("screen_name")]
		public string username;
	}
}

