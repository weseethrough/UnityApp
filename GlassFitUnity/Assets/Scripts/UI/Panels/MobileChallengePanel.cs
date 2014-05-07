using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System.Threading;
using System;
using System.Collections.Generic;

[Serializable]
public class MobileChallengePanel : MobilePanel {

	MobileList challengeList;
	UIGrid challengesGrid;
	UIButton activeBtn;
	UIButton communityBtn;
	UIButton friendBtn;
	int current = 0;

	bool initialized = false;

	public MobileChallengePanel() { }
	public MobileChallengePanel(SerializationInfo info, StreamingContext ctxt)
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
			return "MobileChallengePanel: " + gName.Value;
		}
		return "MobileChallengePanel: Uninitialized";
	}

	public override void EnterStart ()
	{
		base.EnterStart ();
		challengeList = physicalWidgetRoot.GetComponentInChildren<MobileList>();
		challengesGrid = physicalWidgetRoot.GetComponentInChildren<UIGrid>();
		if (challengeList != null)
		{
			challengeList.SetTitle("");
			challengeList.SetParent(this);
		}

		GameObject bkg = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "iPhoneGreyBlackBackground");

		GameObjectUtils.SetTextOnLabelInChildren(bkg, "PlayerName", "Amerigo Moscaroli");

		UITexture profilePicture = bkg.GetComponentInChildren<UITexture>();

		// TODO: add functionality for login information
		
		

		GameObject btnObj = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "ActiveBtn");
		if(btnObj != null) {
			activeBtn = btnObj.GetComponentInChildren<UIButton>();
		}
		btnObj = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "CommunityBtn");
		if(btnObj != null) {
			communityBtn = btnObj.GetComponentInChildren<UIButton>();
		}
		btnObj = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "FriendBtn");
		if(btnObj != null) {
			friendBtn = btnObj.GetComponentInChildren<UIButton>();
		}

		challengesGrid.cellHeight = 350f;

		AddButtonData("ActiveButton" + current, "", "", ListButtonData.ButtonFormat.ActiveChallengeButton, GetBaseButtonConnection());

		if(activeBtn != null) {
			activeBtn.enabled = false;
			activeBtn.defaultColor = new Color(202 / 255f, 202 / 255f, 202 / 255f);
		}
	

		challengeList.RebuildList();
	}

	public void ChangeListType(ListButtonData.ButtonFormat format) 
	{

		if(challengesGrid != null) {
			if(format == ListButtonData.ButtonFormat.ActiveChallengeButton) {
				challengesGrid.cellHeight = 350f;
			} else {
				challengesGrid.cellHeight = 190f;
			}
			challengesGrid.RemoveButtons();
		}

		buttonData = new List<ListButtonData>();
		AddButtonData(format.ToString() + current, "", "", format, GetBaseButtonConnection());

		current++;

		if (challengeList != null)
		{
			challengeList.RebuildList();
		}

		if(activeBtn != null && friendBtn != null && communityBtn != null) {
			activeBtn.enabled = true;
			communityBtn.enabled = true;
			friendBtn.enabled = true;

			switch(format) {
			case ListButtonData.ButtonFormat.ActiveChallengeButton:
				activeBtn.enabled = false;
				break;
				
			case ListButtonData.ButtonFormat.CommunityChallengeButton:
				communityBtn.enabled = false;
				break;
				
			case ListButtonData.ButtonFormat.FriendChallengeButton:
				friendBtn.enabled = false;
				break;
			}
		}

	}
}
