using System;
using System.Collections.Generic;
using Sqo.Attributes;

namespace RaceYourself.Models
{
	public class User
	{
		[Index]
		[UniqueConstraint]
		public int id;
		public string username;
		public string email;
		public string token;
		public string name;
//        public string forename;
//        public string surname;
		public DateTime created_at;
		public DateTime updated_at;
		public bool admin;
		public int sync_key;
		public DateTime? sync_timestamp;
		public char? gender;
		public int points;
		public List<Authentication> authentications;
        [MaxLength(255)]
        public string image;
        public Profile profile;

        public User() {}
		
		public string DisplayName {
			get {
				if (!String.IsNullOrEmpty(name)) return name;
                if (!String.IsNullOrEmpty(username)) return username;
				return email;
			}
		}

        public string forename {
            get {
                if(!string.IsNullOrEmpty(name)) return name.Split(' ')[0];
                return "Unknown";
            }
        }

        public string surname {
            get {
                if(!string.IsNullOrEmpty(name)) {
                    var split = name.Split(' ');
                    return split[split.Length - 1];
                }
                return "Unknown";
            }
        }
	}
	
	public class Authentication 
	{
		public string provider;
		public string uid;
		public string permissions;
	}
}