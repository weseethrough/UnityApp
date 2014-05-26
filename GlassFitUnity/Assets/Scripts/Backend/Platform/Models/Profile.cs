using System;
namespace RaceYourself
{
	public class Profile
	{
        public string username {get; set;}
        public string firstName {get; set;}
        public string surname {get; set;}
        public char gender {get; set;}
        public string image {get; set;}
        public int? timezone {get; set;}

        public Profile(string username, string firstName, string surname, char gender, string image, int? timezone)
        {
            this.username = username;
            this.firstName = firstName;
            this.surname = surname;
            this.gender = gender;
            this.image = image;
            this.timezone = timezone;
        }
	}
}
