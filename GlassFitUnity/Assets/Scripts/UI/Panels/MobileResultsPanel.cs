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

	Friend chosenFriend;

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

		chosenFriend = (Friend)DataVault.Get("chosen_friend");
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
		obj = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "PlayerPicture");
		if(obj != null) {
			Platform.Instance.RemoteTextureManager.LoadImage(Platform.Instance.User().image, button, (tex, empty) => {
				UITexture playerPic = obj.GetComponentInChildren<UITexture>();
				if(playerPic != null) {
					playerPic.mainTexture = tex;
				}
			});
		}

		obj = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "RivalPicture");
		if(obj != null && chosenFriend != null) {
			UITexture rivalPicture = obj.GetComponentInChildren<UITexture>();
			if(rivalPicture != null) {

				Platform.Instance.RemoteTextureManager.LoadImage(chosenFriend.image, button, (tex, empty) => {
					rivalPicture.mainTexture = tex;
				});
			}
			UIBasiclabel friendName = obj.GetComponent<UIBasiclabel>();
			if(friendName != null) {
				friendName.SetLabel(chosenFriend.forename);
			}
		}
		string playerDistance = (string)DataVault.Get("distance");
		string playerDistanceUnits = (string)DataVault.Get("distance_units");
		GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "DistanceTravelledText", playerDistance);
		GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "PlayerDistanceText", playerDistance);
		GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "DistanceTravelledUnitsText", playerDistanceUnits);
		GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "PlayerDistanceUnitsText", playerDistanceUnits);

		string opponentDistance = (string)DataVault.Get("opponent_distance");
		string opponentDistanceUnits = (string)DataVault.Get("opponent_distance_units");
		GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "RivalDistanceText", opponentDistance);
		GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "RivalDistanceUnitsText", opponentDistanceUnits);


		GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "TimeText", "10");

		GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "AveragePaceText", "1.8");

		string aheadDistance = (string)DataVault.Get("distance_position") ;
		string aheadDistanceUnits = (string)DataVault.Get("target_units");
		GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "DistanceAheadText", aheadDistance);
		GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "DistanceAheadTextUnits", aheadDistanceUnits);
	}
}
