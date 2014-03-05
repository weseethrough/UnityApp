using System;
using System.Collections.Generic;

namespace RaceYourself.Models
{
	public class Friendship
	{
		public string _id;

		public string identity_id;
		public Dictionary<string, string> friend;

		public DateTime updated_at;
		public DateTime? deleted_at;
	}
}

