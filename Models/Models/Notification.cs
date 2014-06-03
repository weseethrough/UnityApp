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
        public int id;

		public bool read;
		public Message message;

        public DateTime? created_at;
        public DateTime? updated_at;
        public DateTime? deleted_at;

		[JsonIgnore]
		public bool dirty = false;
	}
}

