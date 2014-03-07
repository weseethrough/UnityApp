using System;
using System.Collections.Generic;
using Sqo.Attributes;

namespace RaceYourself.Models
{
	public class Friendship
	{
		[Index]
		[UniqueConstraint]
		public string _id;

		public string identity_id;
		public Friend friend;

		public DateTime updated_at;
		public DateTime? deleted_at;
	}
}

