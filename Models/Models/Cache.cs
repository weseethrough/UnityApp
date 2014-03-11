using System;
using System.IO;

namespace RaceYourself.Models
{
	public class Cache
	{
		public string id;
		public DateTime expiration;

		public Cache() {}
		public Cache(string id, long maxAge)
		{
			this.id = id;
			this.expiration = DateTime.Now.AddSeconds(maxAge);
		}

		public bool Expired {
			get {
				return (DateTime.Now > expiration);
			}
		}
	}
}

