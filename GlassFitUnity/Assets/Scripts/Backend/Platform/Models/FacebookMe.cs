using System;
namespace RaceYourself
{
	public class FacebookMe
	{
		public string id;
		public string email;
		public string username; // TODO seems to be always null...
		public string first_name;
		public string last_name;
		public string name;
        public string gender;
		public string locale;
		public string updated_time;
		public bool? verified;

		public string Picture {
			get {
				return string.Format("http://graph.facebook.com/{0}/picture", id);
			}
		}

		public FacebookMe() {
			this.verified = false;
		}
	}
}

