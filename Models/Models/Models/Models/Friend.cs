using System;
using Sqo.Attributes;

namespace RaceYourself.Models
{
	public class Friend
	{
		[Index]
		[UniqueConstraint]
		public string _id;
		public bool has_glass;
		public string name;
		public string photo;
		public string uid;
		public string provider;
		public int? user_id;

		// TOOD: Polymophism
		public string screen_name;
	}
}

