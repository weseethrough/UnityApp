using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System.Threading;
using System;
using System.Collections.Generic;

using RaceYourself.Models;
using Newtonsoft.Json;

[Serializable]
public class MobileChallengePanel : MobilePanel {

	MobileList mobileList;
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
		mobileList = physicalWidgetRoot.GetComponentInChildren<MobileList>();
		challengesGrid = physicalWidgetRoot.GetComponentInChildren<UIGrid>();

		Platform.Instance.Authorize("facebook", "login");
		if (mobileList != null)
		{
			mobileList.SetTitle("");
			mobileList.SetParent(this);
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

		AddButtonData("ActiveButton" + current, null, "", ListButtonData.ButtonFormat.ActiveChallengeButton, GetBaseButtonConnection());

		if(activeBtn != null) {
			activeBtn.enabled = false;
			activeBtn.defaultColor = new Color(202 / 255f, 202 / 255f, 202 / 255f);
		}
	}

	public void ChangeListType(ListButtonData.ButtonFormat format) 
	{
		if(challengesGrid != null) {
			if(format == ListButtonData.ButtonFormat.ActiveChallengeButton) {
				mobileList.ResetList(350f);
			} else {
				mobileList.ResetList(190f);
			}

		}

		GetButtonData().Clear();

		switch(format) {
		case ListButtonData.ButtonFormat.ActiveChallengeButton:
			IList<Challenge> activeChallengeIList = Platform.Instance.Challenges();
			if(activeChallengeIList != null) {
				List<Challenge> activeChallengeList = new List<Challenge>(activeChallengeIList.Count);
				for(int i=0; i<activeChallengeIList.Count; i++) {
					activeChallengeList[i] = activeChallengeIList[i];
				}
				activeChallengeList.RemoveAll(r => r.accepted == false);
				AddChallengeButtons(activeChallengeList, ListButtonData.ButtonFormat.ActiveChallengeButton);
			}
			break;
			
		case ListButtonData.ButtonFormat.CommunityChallengeButton:
			Platform.Instance.partner.StartCoroutine(Platform.Instance.api.get("challenges", body => {
				List<Challenge> communityChallengeList = JsonConvert.DeserializeObject<RaceYourself.API.ListResponse<RaceYourself.Models.Challenge>>(body).response;	
				AddChallengeButtons(communityChallengeList, ListButtonData.ButtonFormat.CommunityChallengeButton);
			})) ;
			break;
			
		case ListButtonData.ButtonFormat.FriendChallengeButton:
			IList<Challenge> challengeIList = Platform.Instance.Challenges();
			if(challengeIList != null) {
				List<Challenge> friendChallengeList = new List<Challenge>(challengeIList.Count);
				for(int i=0; i<challengeIList.Count; i++) {
					friendChallengeList[i] = challengeIList[i];
				}
				friendChallengeList.RemoveAll(r => r.accepted == true);
				AddChallengeButtons(friendChallengeList, ListButtonData.ButtonFormat.FriendChallengeButton);
			}
			break;
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
