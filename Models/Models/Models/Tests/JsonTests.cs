using NUnit.Framework;
using System;
using Newtonsoft.Json;

namespace RaceYourself.Models
{
	[TestFixture ()]
	public class JsonTests
	{
		[Test ()]
		public void TestAction ()
		{
			Console.WriteLine ("who?");
			string rawAction = "{}";
			Action action = new Action(rawAction);
			string json = JsonConvert.SerializeObject(action);
			Assert.True(json == rawAction);
			Action deaction = JsonConvert.DeserializeObject<Action>(json);
			Assert.True(rawAction == deaction.json);
		}
		[Test ()]
		public void TestChallenge ()
		{
			Challenge challenge = new DistanceChallenge(1000, 60);
			string json = JsonConvert.SerializeObject(challenge);
			Console.WriteLine(json);
			challenge = JsonConvert.DeserializeObject<Challenge>(json);
			Console.WriteLine (challenge._id);
			Assert.True(challenge.GetType() == typeof(DistanceChallenge));
		}
	}
}

