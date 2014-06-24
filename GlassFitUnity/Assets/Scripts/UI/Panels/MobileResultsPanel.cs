using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System.Threading;
using System;
using System.IO;
using System.Collections.Generic;

using RaceYourself.Models;
using Newtonsoft.Json;

[Serializable]
public class MobileResultsPanel : MobilePanel {

	User chosenUser;

	double playerDistance = 0;
	double rivalDistance = 0;

	GameObject shareContainer;
	GameObject rewardContainer;
	GameObject raceButtonContainer;
	GameObject expireContainer;

	double yourPaceValue;
	double yourSpeedValue;
	double yourDownValue;
	double yourUpValue;
	
	double rivalPaceValue;
	double rivalSpeedValue;
	double rivalDownValue;
	double rivalUpValue;

	public MobileResultsPanel() { }
	public MobileResultsPanel(SerializationInfo info, StreamingContext ctxt)
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
			return "MobileResultsPanel: " + gName.Value;
		}
		return "MobileResultsPanel: Uninitialized";
	}

	public override void EnterStart ()
	{
		base.EnterStart ();

		DataVault.Set("your_pace", "- -");
		DataVault.Set("your_speed", "- -");
		DataVault.Set("your_up", "- -");
		DataVault.Set("your_down", "- -");
		DataVault.Set("your_weather", "");
		DataVault.Set("your_distance", "");
		DataVault.Set("your_distance_sub", "");
		DataVault.Set("your_empty_run", "");
		DataVault.Set("rivals_pace", "- -");
		DataVault.Set("rivals_speed", "- -");
		DataVault.Set("rivals_up", "- -");
		DataVault.Set("rivals_down", "- -");
		DataVault.Set("rivals_weather", "- -");
		DataVault.Set("rivals_distance", "");
		DataVault.Set("rivals_distance_sub", "");
		DataVault.Set("rival_empty_run", "");

		DataVault.Set("title", "");
		DataVault.Set("description", "");

		DataVault.Set("challenge_title", "");
		DataVault.Set("share_title", "Share your run!");

		DataVault.Set("header_text", "");

		rewardContainer = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "RewardContainer");
		if(rewardContainer != null)
		{
			rewardContainer.SetActive(false);
		}
		else
		{
			UnityEngine.Debug.LogError("MobileChallengeInfoPanel: couldn't find reward container!");
		}
		
		shareContainer = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "ShareContainer");
		if(shareContainer != null)
		{
			shareContainer.SetActive(true);
		}
		else
		{
			UnityEngine.Debug.LogError("MobileChallengeInfoPanel: couldn't find share container!");
		}

		raceButtonContainer = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "RaceButtonContainer");
		if(raceButtonContainer != null)
		{
			raceButtonContainer.SetActive(false);
		}
		else
		{
			UnityEngine.Debug.LogError("MobileChallengeInfoPanel: race button container is null");
		}

		expireContainer = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "ExpireContainer");
		expireContainer.SetActive(false);
		
		DataVault.Set("your_time", "");
		DataVault.Set("rival", "?");
		DataVault.Set("rivals_time", "");
		
        chosenUser = (User)DataVault.Get("opponent_user");



		string button = "test";

		GameObject obj = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "PlayerPicture");
		if(obj != null && chosenUser != null) 
		{
			UITexture rivalPicture = obj.GetComponentInChildren<UITexture>();
			if(rivalPicture != null) 
			{
				Platform.Instance.RemoteTextureManager.LoadImage(chosenUser.image, button, (tex, empty) => {
					rivalPicture.mainTexture = tex;
				});
			}
			DataVault.Set("rival", chosenUser.DisplayName);
		}

		Track track = (Track)DataVault.Get("current_track");

		DataVault.Set("title", chosenUser.DisplayName);
		DataVault.Set("description", "Race results");

		DataVault.Set("header_text", "Results");

		string timeText = (string)DataVault.Get("finish_time_minutes");
		string durationText = (string)DataVault.Get("duration");
	
		Track playerTrack = (Track)DataVault.Get("player_track");

        double playerDistance = (double) DataVault.Get("rawdistance");
		DataVault.Set("your_distance", Math.Round(playerDistance/1000, 2).ToString());
		DataVault.Set("your_distance_sub", "KM");
		DataVault.Set("your_empty_run", "");
		DataVault.Set("your_time", playerTrack.date.ToString("ran h:mm tt dd.MM.yy").ToLower());
		
		float? init_alt = null;
		float min_alt = float.MaxValue;
		float max_alt = float.MinValue;
		float max_speed = 0;
		foreach (var position in playerTrack.positions) {
			if (position.alt.HasValue && !init_alt.HasValue) init_alt = position.alt;
			if (position.alt.HasValue && max_alt < position.alt) max_alt = position.alt.Value;
			if (position.alt.HasValue && min_alt > position.alt) min_alt = position.alt.Value;
			if (position.speed > max_speed) max_speed = position.speed;
		}
		yourPaceValue = Math.Round((playerTrack.distance*60*60/1000) / playerTrack.time, 1);
        DataVault.Set("your_pace", double.IsNaN(yourPaceValue) ? "- -" : yourPaceValue.ToString());
		yourSpeedValue = Math.Round((max_speed*60*60)/1000, 1);
		DataVault.Set("your_speed", yourSpeedValue.ToString());
		if (init_alt.HasValue) {
			yourUpValue = Math.Round(max_alt - init_alt.Value, 1);
			DataVault.Set("your_up", yourUpValue.ToString());
			yourDownValue = Math.Round(init_alt.Value - min_alt, 1);
			DataVault.Set("your_down", yourDownValue.ToString());
		}
		DataVault.Set("your_weather", "?");

		GameObject yourCircle = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "LeftCircle");
		GameObject rivalsCircle = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "RightCircle");
		
//		string playerDistance = (string)DataVault.Get("distance");
//		string playerDistanceUnits = (string)DataVault.Get("distance_units");
//		GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "DistanceTravelledText", playerDistance);
//		GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "PlayerDistanceText", playerDistance);
//		GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "DistanceTravelledUnitsText", playerDistanceUnits.ToUpper());
//		GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "PlayerDistanceUnitsText", playerDistanceUnits);
//
//		GameObject playerDistanceObj = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "PlayerDistance");
//		UITexture playerCircle = playerDistanceObj.GetComponentInChildren<UITexture>();
//
//		playerCircle.color = new Color(57/255f, 188/255f, 60/255f);
//
//		GameObject rivalDistanceObj = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "RivalDistance");
//		UITexture rivalCircle = rivalDistanceObj.GetComponentInChildren<UITexture>();
//
//		string timeText = (string)DataVault.Get("finish_time_minutes");
//		string durationText = (string)DataVault.Get("duration");
//		GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "DurationText", timeText);
//		DataVault.Set("social_description", "I just ran " + playerDistance + playerDistanceUnits + " in " + timeText + "mins!");
		if(!timeText.Equals(durationText)) 
		{
			DataVault.Set("challenge_title", "Challenge incomplete - try again!");
			DataVault.Set("social_result", "I didn't complete my challenge against " + chosenUser.forename);
		} 
		else
		{
			DataVault.Set("challenge_title", "Challenge to run the furthest in [39BC3C]" + durationText + "[-] min");

			GameObject playerPace = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "YoursPace");
			GameObject playerSpeed = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "YoursSpeed");
			GameObject playerDown = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "YoursTotalDown");
			GameObject playerUp = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "YoursTotalUp");
			
			GameObject rivalPace = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "RivalPace");
			GameObject rivalSpeed = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "RivalSpeed");
			GameObject rivalDown = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "RivalTotalDown");
			GameObject rivalUp = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "RivalTotalUp");

			if(track != null) 
			{
				rivalDistance = track.distance;
				DataVault.Set("rivals_distance", Math.Round(track.distance/1000, 2).ToString());
				DataVault.Set("rivals_distance_sub", "KM");						
				DataVault.Set("rivals_time", track.date.ToString("ran h:mm tt dd.MM.yy").ToLower());
				
				float? rival_init_alt = null;
				float rival_min_alt = float.MaxValue;
				float rival_max_alt = float.MinValue;
				float rival_max_speed = 0;
				foreach (var position in track.positions) {
					if (position.alt.HasValue && !rival_init_alt.HasValue) rival_init_alt = position.alt;
					if (position.alt.HasValue && rival_max_alt < position.alt) rival_max_alt = position.alt.Value;
					if (position.alt.HasValue && rival_min_alt > position.alt) rival_min_alt = position.alt.Value;
					if (position.speed > rival_max_speed) rival_max_speed = position.speed;
				}
				rivalPaceValue = Math.Round((track.distance*60*60/1000) / track.time, 1);
				DataVault.Set("rivals_pace", rivalPaceValue.ToString());
				rivalSpeedValue = Math.Round((max_speed*60*60)/1000, 1);
				DataVault.Set("rivals_speed", rivalSpeedValue.ToString());
				if (init_alt.HasValue) {
					rivalUpValue = Math.Round(max_alt - init_alt.Value, 1);
					DataVault.Set("rivals_up", rivalUpValue.ToString());
					rivalDownValue =  Math.Round(init_alt.Value - min_alt, 1);
					DataVault.Set("rivals_down", rivalDownValue.ToString());
				}
				DataVault.Set("rivals_weather", "?");

				if(yourPaceValue > rivalPaceValue)
				{
					playerPace.GetComponent<UILabel>().color = Palette.green;
					rivalPace.GetComponent<UILabel>().color = Palette.red;
				}
				else
				{
					playerPace.GetComponent<UILabel>().color = Palette.red;
					rivalPace.GetComponent<UILabel>().color = Palette.green;
				}
				
				if(yourSpeedValue > rivalSpeedValue)
				{
					playerSpeed.GetComponent<UILabel>().color = Palette.green;
					rivalSpeed.GetComponent<UILabel>().color = Palette.red;
				}
				else
				{
					playerSpeed.GetComponent<UILabel>().color = Palette.red;
					rivalSpeed.GetComponent<UILabel>().color = Palette.green;
				}
				
				if(yourUpValue > rivalUpValue)
				{
					playerUp.GetComponent<UILabel>().color = Palette.green;
					rivalUp.GetComponent<UILabel>().color = Palette.red;
				}
				else
				{
					playerUp.GetComponent<UILabel>().color = Palette.red;
					rivalUp.GetComponent<UILabel>().color = Palette.green;
				}
				
				if(yourDownValue > rivalDownValue)
				{
					playerDown.GetComponent<UILabel>().color = Palette.green;
					rivalDown.GetComponent<UILabel>().color = Palette.red;
				}
				else
				{
					playerDown.GetComponent<UILabel>().color = Palette.red;
					rivalDown.GetComponent<UILabel>().color = Palette.green;
				}
				
				GameObject challengeTitle = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "ChallengeTitle");
				expireContainer.SetActive(false);
				DataVault.Set("description", "Race Complete");
				DataVault.Set("header_text", "Race Complete");
				if(playerDistance > rivalDistance) 
				{
					DataVault.Set("share_title", "time to bask in the glory of success!");
					DataVault.Set("social_description", "I beat " + chosenUser.DisplayName + " by running " + (playerDistance / 1000).ToString("f2") + "km in " + durationText + "mins!");
					DataVault.Set("social_result", "I beat " + chosenUser.DisplayName + "!");
					yourCircle.GetComponent<UITexture>().color = Palette.green;
					rivalsCircle.GetComponent<UITexture>().color = Palette.red;
					rewardContainer.SetActive(true);
					challengeTitle.GetComponent<UILabel>().color = Palette.green;
					DataVault.Set("challenge_title", "You Won - " + (playerDistance / 1000).ToString("f2") + "km in " + durationText + " mins");
				} else 
				{
					DataVault.Set("share_title", "congratulate your fellow competitor!");
					DataVault.Set("social_description", chosenUser.DisplayName + " beat me by running " + (rivalDistance / 1000).ToString("f2") + "km in " + durationText + "mins!");
					DataVault.Set("social_result", chosenUser.DisplayName + " beat me");
					yourCircle.GetComponent<UITexture>().color = Palette.red;
					rivalsCircle.GetComponent<UITexture>().color = Palette.green;
					DataVault.Set("challenge_title", "You Lost - " + (rivalDistance / 1000).ToString("f2") + "km in " + durationText + " mins");
				}
				GameObject card = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "DropCard");
				var cardPosition = card.transform.localPosition;
				cardPosition.y = -65; // TODO: Remove magic variable
				card.transform.localPosition = cardPosition;
				shareContainer.SetActive(true);
			}
			else 
			{
				DataVault.Set("challenge_title", "Awaiting friend's challenge attempt");
				expireContainer.SetActive(true);
				DataVault.Set("share_title", "Share your run!");
				playerPace.GetComponent<UILabel>().color = Palette.green;
				playerSpeed.GetComponent<UILabel>().color = Palette.green;
				playerDown.GetComponent<UILabel>().color = Palette.green;
				playerUp.GetComponent<UILabel>().color = Palette.green;
				yourCircle.GetComponent<UITexture>().color = Palette.green;
			}
		}

	}

	public override void Exited ()
	{
		base.Exited ();
        DataVault.Remove("opponent_user");
		DataVault.Remove("challenge_notification");
		DataVault.Remove("current_track");
	}
}
