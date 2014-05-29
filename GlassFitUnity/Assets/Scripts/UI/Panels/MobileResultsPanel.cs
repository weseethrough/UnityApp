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

		bool isAhead = Convert.ToBoolean(DataVault.Get("player_is_ahead"));
		GameObject obj;
		if(!isAhead) {
			GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "PlayerResultText", "You Lost!");
			obj = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "PlayerReward");
			if(obj != null){
				obj.SetActive(false);
			}
		} else {
			GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "PlayerResultText", "You Won!");
			obj = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "RivalReward");
			if(obj != null) {
				obj.SetActive(false);
			}
		}

		string button = "test";
		GameObject playerPicObj = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "PlayerPicture");
		if(playerPicObj != null) {
			User user = Platform.Instance.User();
			if(user != null)
			{
				Platform.Instance.RemoteTextureManager.LoadImage(user.image, button, (tex, empty) => {
					UITexture playerPic = playerPicObj.GetComponentInChildren<UITexture>();
					if(playerPic != null) {
						playerPic.mainTexture = tex;
					}
				});
			}
			else { UnityEngine.Debug.LogError("MobileResults: No user"); }
		}

		obj = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "RivalPicture");
		if(obj != null && chosenUser != null) {
			UITexture rivalPicture = obj.GetComponentInChildren<UITexture>();
			if(rivalPicture != null) {

				Platform.Instance.RemoteTextureManager.LoadImage(chosenUser.image, button, (tex, empty) => {
					rivalPicture.mainTexture = tex;
				});
			}
			UIBasiclabel friendName = obj.GetComponent<UIBasiclabel>();
			if(friendName != null) {
				friendName.SetLabel(chosenUser.forename);
			}
		}
		string playerDistance = (string)DataVault.Get("distance");
		string playerDistanceUnits = (string)DataVault.Get("distance_units");
		GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "DistanceTravelledText", playerDistance);
		GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "PlayerDistanceText", playerDistance);
		GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "DistanceTravelledUnitsText", playerDistanceUnits.ToUpper());
		GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "PlayerDistanceUnitsText", playerDistanceUnits);

		string opponentDistance = (string)DataVault.Get("opponent_distance");
		string opponentDistanceUnits = (string)DataVault.Get("opponent_distance_units");
		GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "RivalDistanceText", opponentDistance);
		GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "RivalDistanceUnitsText", opponentDistanceUnits);

		string timeText = (string)DataVault.Get("finish_time_minutes");
		GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "TimeText", timeText);

		string playerAveragePace = (string)DataVault.Get ("player_average_pace");
		GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "AveragePaceText", playerAveragePace);

		string aheadDistance = (string)DataVault.Get("ahead_box") ;
		string aheadDistanceUnits = (string)DataVault.Get("target_units");
		GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "DistanceAheadText", aheadDistance);
		if(isAhead) {
			aheadDistanceUnits = aheadDistanceUnits.ToUpper() + " Ahead";
		} else {
			aheadDistanceUnits = aheadDistanceUnits.ToUpper() + " Behind";
		}
		
		GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "DistanceAheadUnitsText", aheadDistanceUnits);
	}

	public override void Exited ()
	{
		base.Exited ();
		DataVault.Remove("chosen_user");
		DataVault.Remove("challenge_notification");
	}
}
