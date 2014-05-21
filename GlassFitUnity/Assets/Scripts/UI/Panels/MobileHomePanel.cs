using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System.Threading;
using System;
using System.Collections.Generic;

using RaceYourself.Models;
using Newtonsoft.Json;

[Serializable]
public class MobileHomePanel : MobilePanel {

	MobileList mobileList;

	UIButton challengeBtn;
	UIButton racersBtn;

	List<Friend> friendsData;
	List<Friend> betaFriends;

	List<Challenge> friendChallengeList;

	GameObject challengeNotification;

	bool initialized = false;

	public MobileHomePanel() { }
	public MobileHomePanel(SerializationInfo info, StreamingContext ctxt)
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
			return "MobileHomePanel: " + gName.Value;
		}
		return "MobileHomePanel: Uninitialized";
	}

	public override void EnterStart ()
	{
		base.EnterStart ();

		mobileList = physicalWidgetRoot.GetComponentInChildren<MobileList>();
		if (mobileList != null)
		{
			mobileList.SetTitle("");
			mobileList.SetParent(this);
		}

		GameObject bkg = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "iPhoneGreyBlackBackground");
		
		GameObjectUtils.SetTextOnLabelInChildren(bkg, "PlayerName", Platform.Instance.User().name);

		UITexture profilePicture = bkg.GetComponentInChildren<UITexture>();

		string empty = "";
		// TODO: add functionality for login information
		if(profilePicture != null) {
			Platform.Instance.RemoteTextureManager.LoadImage(Platform.Instance.User().image, empty, (tex, text) => {
				profilePicture.mainTexture = tex;
			});
		}

//		Platform.Instance.Authorize("facebook", "login");

		GameObject btnObj = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "ChallengeBtn");
		if(btnObj != null) {
			challengeBtn = btnObj.GetComponentInChildren<UIButton>();
		}
		btnObj = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "RacersBtn");
		if(btnObj != null) {
			racersBtn = btnObj.GetComponentInChildren<UIButton>();
		}

		if(challengeBtn != null) {
			challengeBtn.enabled = false;
			challengeBtn.defaultColor = new Color(68 / 255f, 68 / 255f, 68 / 255f);
		}

		IList<Challenge> challengeIList = Platform.Instance.Challenges();
		if(challengeIList != null && challengeIList.Count > 0) {
			friendChallengeList = new List<Challenge>();
			for(int i=0; i<challengeIList.Count; i++) {
				friendChallengeList.Add(challengeIList[i]);
			}
			friendChallengeList.RemoveAll(x => x.creator_id == Platform.Instance.User().id);
			if(friendChallengeList.Count > 0) {
				DataVault.Set("new_challenges", friendChallengeList.Count);
			}
		}

		challengeNotification = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "ChallengeNotification");
		
		ChangeList("challenge");

		GetFriends();

		initialized = true;

		DataVault.Remove("invite_codes");
		GameObject inviteObj = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "InviteNotification");
		if(inviteObj != null) {
			inviteObj.SetActive(false);
		}
		Platform.Instance.partner.StartCoroutine(Platform.Instance.api.get("invites", body => {
			
			List<Invite> invites = JsonConvert.DeserializeObject<RaceYourself.API.ListResponse<RaceYourself.Models.Invite>>(body).response;	
			DataVault.Set("invite_codes", invites);
			int numUnused = 0;

			List<Invite> unusedInvites = invites.FindAll(r => r.used_at == null);
			if(unusedInvites != null && unusedInvites.Count > 0) {
				DataVault.Set("number_invites", unusedInvites.Count);
				if(inviteObj != null) {
					inviteObj.SetActive(true);
				}
			}
		})) ;
		

	}

	public void GetFriends() {
		friendsData = Platform.Instance.Friends();
		
		betaFriends = new List<Friend>();
		
		if(friendsData != null) {
			for(int i=0; i<friendsData.Count; i++) {
				if(friendsData[i].userId != null) {
					betaFriends.Add(friendsData[i]);
					friendsData.Remove(friendsData[i]);
					i--;
				}
			}
		}
		
		if(betaFriends != null && betaFriends.Count > 1) {
			betaFriends.Sort((t1, t2) => t1.name.CompareTo(t2.name));
		}
		
		if(friendsData != null && friendsData.Count > 1) {
			friendsData.Sort((t1, t2) => t1.name.CompareTo(t2.name));
		}
	}

	public void ChangeList(string type) {
		racersBtn.enabled = true;
		challengeBtn.enabled = true;

		buttonData = new List<ListButtonData>();
		switch(type) {
		case "challenge":
			if(challengeNotification != null) {
				challengeNotification.SetActive(false);
			}
			if(initialized) {
				mobileList.ResetList(300f);
			}
			IList<Challenge> challengeIList = Platform.Instance.Challenges();
			if(challengeIList != null && challengeIList.Count > 0) {
				friendChallengeList = new List<Challenge>();
				for(int i=0; i<challengeIList.Count; i++) {
					friendChallengeList.Add(challengeIList[i]);
				}
				friendChallengeList.RemoveAll(x => x.creator_id == Platform.Instance.User().id);

				AddChallengeButtons(friendChallengeList, ListButtonData.ButtonFormat.FriendChallengeButton);
			} else {
				AddButtonData("NoChallengeButton", null, "SetMobileHomeTab", ListButtonData.ButtonFormat.InvitePromptButton, GetConnection("RacersBtn"));
			}
			challengeBtn.enabled = false;
			break;

		case "friend":
			mobileList.ResetList(155f);
			racersBtn.enabled = false;
			if(challengeNotification != null && friendChallengeList.Count > 0) {
				challengeNotification.SetActive(true);
				DataVault.Set("new_challenges", friendChallengeList.Count);
			}
			if(Platform.Instance.HasPermissions("facebook", "login") && friendsData != null && friendsData.Count > 0) {
				if(betaFriends != null && betaFriends.Count > 0) {
					for(int i=0; i<betaFriends.Count; i++) {
						string betaButtonName = "challenge" + i;
						Dictionary<string, string> betaFriendDictionary = new Dictionary<string, string>();
						betaFriendDictionary.Add("Name", betaFriends[i].name);
						Dictionary<string, Dictionary<string, string>> betaImageDictionary = new Dictionary<string, Dictionary<string, string>>();
						Dictionary<string, string> innerBetaDictionary = new Dictionary<string, string>();
						innerBetaDictionary.Add("texture", "ChallengePlayerPicture");
						innerBetaDictionary.Add("name", betaButtonName);
						betaImageDictionary.Add(betaFriends[i].image, innerBetaDictionary);
						AddButtonData(betaButtonName, betaFriendDictionary, "", betaImageDictionary, ListButtonData.ButtonFormat.ChallengeButton, GetConnection("ChallengeButton"));
					}
				}

				if (friendsData != null)
				{        
					for (int i = 0; i < friendsData.Count; i++)
					{
						string buttonName = "uninvited" + i;
						Dictionary<string, string> friendDictionary = new Dictionary<string, string>();
						friendDictionary.Add("Name", friendsData[i].name);
						Dictionary<string, Dictionary<string, string>> friendImageDictionary = new Dictionary<string, Dictionary<string, string>>();
						Dictionary<string, string> innerDictionary = new Dictionary<string, string>();
						innerDictionary.Add("texture", "InvitedProfilePicture");
						innerDictionary.Add("name", buttonName);
						friendImageDictionary.Add(friendsData[i].image, innerDictionary);
						AddButtonData(buttonName, friendDictionary, "", friendImageDictionary, ListButtonData.ButtonFormat.InviteButton, GetConnection("InviteButton"));
					}
				}

			} else {
				GetFriends();
				AddButtonData ("ImportButton", null, "", ListButtonData.ButtonFormat.ImportButton, GetConnection("ImportButton"));
			}

			break;

		default:
			UnityEngine.Debug.Log("MobileHomePanel: type not implemented");
			break;
		}


	}

	public override void OnClick(FlowButton button)
	{
		if (button != null)
		{
			if(button.name.Contains("uninvited")) {
				List<Invite> invites = (List<Invite>)DataVault.Get("invite_codes");
				Invite unusedInvite = invites.Find(x => x.used_at == null);
				if(unusedInvite == null) {
					return;
				}
				string prefix = "uninvited";
				string index = button.name.Substring(prefix.Length);
				int i = Convert.ToInt32(index);
				DataVault.Set("chosen_friend", friendsData[i]);
				Debug.Log("chosen_friend set to " + friendsData[i].name);
			} else if(button.name.Contains("challenge")) {
				string prefix = "challenge";
				string index = button.name.Substring(prefix.Length);
				int i = Convert.ToInt32(index);
				DataVault.Set("chosen_friend", betaFriends[i]);
				Debug.Log("beta chosen_friend set to " + betaFriends[i].name);
			}
		}
		else
		{
			return;
		}
		
		base.OnClick(button);
	}

	public override void Exited ()
	{
		base.Exited ();
		initialized = false;
	}

}
