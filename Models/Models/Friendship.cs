using System;
using System.Collections.Generic;
using Sqo.Attributes;

namespace RaceYourself.Models
{
	public class Friendship
	{
		[Index]
		[UniqueConstraint]
		public string id;
        public string identity_type;
        public string identity_uid;
		public Friend friend;

		public DateTime updated_at;
        public DateTime? deleted_at;
        
        public string GenerateCompositeId ()
        {
            id = identity_type + "_" + identity_uid;
            return id;
        }
	}
}

