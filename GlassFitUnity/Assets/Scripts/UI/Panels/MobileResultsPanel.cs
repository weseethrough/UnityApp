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

		Notification challengeNotification = (Notification)DataVault.Get("challenge_notification");

		chosenUser = (User)DataVault.Get("chosen_user");

		string button = "test";
		GameObject playerPicObj = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "PlayerPicture");
		if(playerPicObj != null) {
			User user = Platform.Instance.User();
			if(user != null)
			{
				Platform.Instance.RemoteTextureManager.LoadImage(user.image, button, (tex, empty) => {
					UITexture playerPic = playerPicObj.GetComponentInChildren<UITexture>();
					if(playerPic != null) 
					{
						playerPic.mainTexture = tex;
					}
				});
			}
			else 
			{ 
				UnityEngine.Debug.LogError("MobileResults: No user"); 
			}
		}

		GameObject obj = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "RivalPicture");
		if(obj != null && chosenUser != null) 
		{
			UITexture rivalPicture = obj.GetComponentInChildren<UITexture>();
			if(rivalPicture != null) 
			{

				Platform.Instance.RemoteTextureManager.LoadImage(chosenUser.image, button, (tex, empty) => {
					rivalPicture.mainTexture = tex;
				});
			}
			GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "RivalName", chosenUser.forename);
		}

		string playerDistance = (string)DataVault.Get("distance");
		string playerDistanceUnits = (string)DataVault.Get("distance_units");
		GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "DistanceTravelledText", playerDistance);
		GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "PlayerDistanceText", playerDistance);
		GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "DistanceTravelledUnitsText", playerDistanceUnits.ToUpper());
		GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "PlayerDistanceUnitsText", playerDistanceUnits);

		GameObject playerDistanceObj = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "PlayerDistance");
		UITexture playerCircle = playerDistanceObj.GetComponentInChildren<UITexture>();

		playerCircle.color = new Color(57/255f, 188/255f, 60/255f);

		GameObject rivalDistanceObj = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "RivalDistance");
		UITexture rivalCircle = rivalDistanceObj.GetComponentInChildren<UITexture>();

		string timeText = (string)DataVault.Get("finish_time_minutes");
		string durationText = (string)DataVault.Get("duration");
		GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "DurationText", timeText);
		if(!timeText.Equals(durationText)) 
		{
			GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "ChallengeResultText", "Challenge incomplete - try again!");
		} 
		else
		{
			Track track = (Track)DataVault.Get("current_track");
			
			if(track != null) 
			{
				string opponentDistance = (track.distance / 1000f).ToString("f2");
				string opponentDistanceUnits = "KM";
				GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "RivalDistanceText", opponentDistance);
				GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "RivalDistanceUnitsText", opponentDistanceUnits);
				GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "RivalQuestionText", "");
				
				bool isAhead = Convert.ToBoolean(DataVault.Get("player_is_ahead"));
				GameObject reward = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "Reward");
				if(reward != null)
				{
					UIAnchor rewardAnchor = reward.GetComponent<UIAnchor>();
					if(rewardAnchor != null) 
					{
						if(isAhead) 
						{
							rewardAnchor.relativeOffset = new Vector2(-0.15f, 0.35f);
							GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "ChallengeResultText", "You Won!");
							if(playerCircle != null) 
							{
								playerCircle.color = new Color(255/255f, 204/255f, 0);
							} 
							if(rivalCircle != null) 
							{
								rivalCircle.color = new Color(255/255f, 69/255f, 28/255f);
							}
						} else 
						{
							rewardAnchor.relativeOffset = new Vector2(0.15f, 0.35f);
							GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "ChallengeResultText", "You Lost!");
							if(playerCircle != null) 
							{
								playerCircle.color = new Color(255/255f, 69/255f, 28/255f);
							} 
							if(rivalCircle != null) 
							{
								rivalCircle.color = new Color(255/255f, 204/255f, 0);
							}
						}
					}
				}
			}
		}

		string playerAveragePace = (string)DataVault.Get ("player_average_pace");
		GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "AveragePaceText", playerAveragePace);

	}

	public override void Exited ()
	{
		base.Exited ();
		DataVault.Remove("chosen_user");
		DataVault.Remove("challenge_notification");
		DataVault.Remove("current_track");
	}
}
