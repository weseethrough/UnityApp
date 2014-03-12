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
			string rawAction = "{}";
			Action action = new Action(rawAction);
			string json = JsonConvert.SerializeObject(action);
			Assert.IsTrue(json == rawAction);
			Action deaction = JsonConvert.DeserializeObject<Action>(json);
			Assert.IsTrue(rawAction == deaction.json);
		}

		[Test ()]
		public void TestChallenge ()
		{
			Challenge challenge = new DistanceChallenge(1000, 60);
			string json = JsonConvert.SerializeObject(challenge);
			challenge = JsonConvert.DeserializeObject<Challenge>(json);
			Assert.IsTrue(challenge.GetType() == typeof(DistanceChallenge));
		}
	}
}

