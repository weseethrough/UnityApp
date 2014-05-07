﻿using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System.Threading;
using System;
using System.Collections.Generic;

using RaceYourself.Models;
using Newtonsoft.Json;

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

		Platform.Instance.Authorize("facebook", "login");
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

		AddButtonData("ActiveButton" + current, null, "", ListButtonData.ButtonFormat.ActiveChallengeButton, GetBaseButtonConnection());

		if(activeBtn != null) {
			activeBtn.enabled = false;
			activeBtn.defaultColor = new Color(202 / 255f, 202 / 255f, 202 / 255f);
		}
	
		challengeList.RebuildList();
	}

	public void AddButtons(List<Challenge> challengeList, ListButtonData.ButtonFormat format) {
		for(int i=0; i<challengeList.Count; i++) {
			string buttonName = format.ToString() + i;
			Dictionary<string, string> challengeDictionary = new Dictionary<string, string>();
			challengeDictionary.Add("TitleText", "test name " + i);
			challengeDictionary.Add("DescriptionText", "test description " + i);
			challengeDictionary.Add("DeadlineText", "Challenge expires in " + "5 days");
			if(format != ListButtonData.ButtonFormat.FriendChallengeButton) {
				challengeDictionary.Add("PrizePotText", "Prize pot: " + (i * 1000));
				if(format == ListButtonData.ButtonFormat.CommunityChallengeButton) {
					challengeDictionary.Add("ExtraPrizeText", "Extra Prize: "  + "Secret!");
				}
			}
			AddButtonData(buttonName, challengeDictionary, "", format, GetBaseButtonConnection());

		}
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

		switch(format) {
		case ListButtonData.ButtonFormat.ActiveChallengeButton:
			IList<Challenge> activeChallengeIList = Platform.Instance.Challenges();
			if(activeChallengeIList != null) {
				List<Challenge> activeChallengeList = new List<Challenge>(activeChallengeIList.Count);
				for(int i=0; i<activeChallengeIList.Count; i++) {
					activeChallengeList[i] = activeChallengeIList[i];
				}
				activeChallengeList.RemoveAll(r => r.accepted == false);
				AddButtons(activeChallengeList, ListButtonData.ButtonFormat.ActiveChallengeButton);
			}
			break;
			
		case ListButtonData.ButtonFormat.CommunityChallengeButton:
			Platform.Instance.partner.StartCoroutine(Platform.Instance.api.get("challenges", body => {
				UnityEngine.Debug.LogError(body);
				List<Challenge> communityChallengeList = JsonConvert.DeserializeObject<RaceYourself.API.ListResponse<RaceYourself.Models.Challenge>>(body).response;	
				AddButtons(communityChallengeList, ListButtonData.ButtonFormat.CommunityChallengeButton);
				if (challengeList != null)
				{
					challengeList.RebuildList();
				}
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
				AddButtons(friendChallengeList, ListButtonData.ButtonFormat.FriendChallengeButton);
			}
			break;
		}

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
