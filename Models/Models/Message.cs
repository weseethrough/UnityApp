using System;
using Newtonsoft.Json;

namespace RaceYourself.Models
{
	public class Message
	{
		public string type;

		// TODO: Polymorphism
		public int from;
        public int to;
        public int challenge_id;
        public string challenge_type;
		public string taunt;
	}
}

