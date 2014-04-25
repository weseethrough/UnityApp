using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System;

using RaceYourself.Models;

[Serializable]
public class ChallengeControllerPanel : Panel {

	List<ChallengeMaster> sampleChallengeList = null;

	GestureHelper.OnSwipeLeft leftHandler = null;
	GestureHelper.OnSwipeRight rightHandler = null;

	int currentChallenge = -1;

	/// <summary>
	/// default constructor
	/// </summary>
	/// <returns></returns>
	public ChallengeControllerPanel() : base() { }
	
	/// <summary>
	/// deserialziation constructor
	/// </summary>
	/// <param name="info">seirilization info conataining class data</param>
	/// <param name="ctxt">serialization context </param>
	/// <returns></returns>
	public ChallengeControllerPanel(SerializationInfo info, StreamingContext ctxt)
		: base(info, ctxt)
	{
	}

	/// <summary>
	/// Gets display name of the node, helps with node identification in editor
	/// </summary>
	/// <returns>name of the node</returns>
	public override string GetDisplayName()
	{
		base.GetDisplayName();
		
		GParameter gName = Parameters.Find(r => r.Key == "Name");
		if (gName != null)
		{
			return "ChallengeControllerPanel: " + gName.Value;
		}
		return "ChallengeControllerPanel: UnInitialzied";
	}

	public override void EnterStart ()
	{
		base.EnterStart ();

		sampleChallengeList = CreateSampleChallenges();

		DataVault.Set("chosen_challenges", " ");

		if(sampleChallengeList != null) {
			leftHandler = new GestureHelper.OnSwipeLeft(() => {
				DislikeChallenge();
			});
			GestureHelper.onSwipeLeft += leftHandler;

			rightHandler = new GestureHelper.OnSwipeRight(() => {
				LikeChallenge();
			});
			GestureHelper.onSwipeRight += rightHandler;

			SetNewChallenge();
		}
	}

	public override void StateUpdate ()
	{
		base.StateUpdate ();

	}

	public void DislikeChallenge() {
		// TODO: Add code to save when a user dislikes a challenge

		SetNewChallenge();
	}

	public void LikeChallenge() {
		// TODO: Add code to save when a user likes a challenge
		string challengeString = (string)DataVault.Get("chosen_challenges");
		if(challengeString == " ") {
			DataVault.Set("chosen_challenges", sampleChallengeList[currentChallenge].type);
		} else {
			DataVault.Set("chosen_challenges", challengeString + ", " + sampleChallengeList[currentChallenge].type);
		}

		SetNewChallenge();
	}

	public void SetNewChallenge() {
		currentChallenge++;		
		if(currentChallenge >= sampleChallengeList.Count) {
			FollowFlowLinkNamed("Exit");
		} else {
			DataVault.Set("challenge_mobile_name", sampleChallengeList[currentChallenge].name);
			DataVault.Set("challenge_mobile_description", sampleChallengeList[currentChallenge].description);
		}

	}

	public override void Exited ()
	{
		base.Exited ();

		GestureHelper.onSwipeLeft -= leftHandler;
		GestureHelper.onSwipeRight -= rightHandler;
	}

	public List<ChallengeMaster> CreateSampleChallenges() {
		List<ChallengeMaster> challenges = new List<ChallengeMaster>();

		ChallengeMaster challenge = new ChallengeMaster("Run 10km", "Run a total of 10km in a single run", "", "distance based");
		challenges.Add(challenge);

		challenge = new ChallengeMaster("Survive for 10 minutes", "Survive again the zombies for at least 10 minutes", "", "survival based");
		challenges.Add(challenge);

		challenge = new ChallengeMaster("Run 3 laps around Hyde Park", "Run 3 laps of the Hyde Park track", "", "location based");
		challenges.Add(challenge);

		challenge = new ChallengeMaster("Run 30km with your group", "Run with your group for a total of 30km", "", "group based");
		challenges.Add(challenge);

		challenge = new ChallengeMaster("Collect all pills", "Collect all the collectibles around the map", "", "points based");
		challenges.Add(challenge);

		return challenges;
	}
}































