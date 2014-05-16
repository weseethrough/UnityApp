using System;
using Newtonsoft.Json;

namespace RaceYourself
{
	public class FacebookMe
	{
		public string id;
		public string email;
		public string username;
		public string first_name;
		public string last_name;
		public string name;
        public string gender;
		public string locale;
		public string updated_time;
		public bool? verified;
        public int timezone;
        public FbData picture;

		public FacebookMe() {
			this.verified = false;
		}

        public string Picture
        {
            get
            {
                return picture.data.url;
            }
        }
	}

    public class FbData
    {
        public FbImage data;
    }

    public class FbImage
    {
        public string url;
    }
}

