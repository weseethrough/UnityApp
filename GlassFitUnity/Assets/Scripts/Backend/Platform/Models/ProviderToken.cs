using System;
namespace RaceYourself
{
	public class ProviderToken
	{
		public string provider;
		public string access_token;
		public string uid;

		public ProviderToken(string provider, string accessToken, string uid) 
		{
			this.provider = provider;
			this.access_token = accessToken;
			this.uid = uid;
		}
	}
}

