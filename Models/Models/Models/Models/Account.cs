using System;
using System.Collections.Generic;
using Sqo.Attributes;

namespace RaceYourself.Models
{
	public class Account
	{
		[Index]
		[UniqueConstraint]
		public int id;
		public string username;
		public string email;
		public string token;
		public string name;
		public DateTime created_at;
		public DateTime updated_at;
		public bool admin;
		public int sync_key;
		public DateTime? sync_timestamp;
		public string gender;
		public int points;
		public List<Authentication> authentications;
		
		public string DisplayName {
			get {
				if (!String.IsNullOrEmpty(username)) return username;
				if (!String.IsNullOrEmpty(name)) return name;
				return email;
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

