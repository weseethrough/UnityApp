using System;
using Newtonsoft.Json;

namespace RaceYourself.Models
{
	public class Profile
	{
        public string username {get; set;}
        [JsonProperty("first_name")]
        public string firstName {get; set;}
        [JsonProperty("last_name")]
        public string surname {get; set;}
        public char gender {get; set;}
        public string image {get; set;}
        public int? timezone {get; set;}
        [JsonProperty("running_fitness")]
        public string runningFitness {get; set;}
        [JsonProperty("cycling_fitness")]
        public string cyclingFitness {get; set;}
        [JsonProperty("workout_fitness")]
        public string workoutFitness {get; set;}
        /** In what context did this user join? Internal (employee of Race Yourself), 1st intake of beta users, 2nd intake...? */
        public string cohort;

        public Profile(string username, string firstName, string surname, char gender, string image, int? timezone,
                       string runningFitness, string cyclingFitness, string workoutFitness, string cohort)
        {
            this.username = username;
            this.firstName = firstName;
            this.surname = surname;
            this.gender = gender;
            this.image = image;
            this.timezone = timezone;
            this.runningFitness = runningFitness;
            this.cyclingFitness = cyclingFitness;
            this.workoutFitness = workoutFitness;
            this.cohort = cohort;
        }

        public Profile()
        {

        }
	}
}
