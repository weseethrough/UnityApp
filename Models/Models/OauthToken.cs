using System;

namespace RaceYourself.Models
{
	public class OauthToken 
	{
		public string access_token;
		public string token_type;			
		public int expires_in;
		public string refresh_token;
		// Non-JSON fields
		private DateTime created;
		public int userId;

		public OauthToken() {
			created = DateTime.Now;
		}
		
		public bool HasExpired {
			get {
				return (DateTime.Now - created).Seconds > expires_in;
			}
		}
	}		
}

