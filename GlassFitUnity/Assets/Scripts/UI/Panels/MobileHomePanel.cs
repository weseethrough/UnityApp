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

	// List object that holds all buttons
	MobileList mobileList;

	// Buttons for challenges and racers
	UIButton challengeBtn;
	UIButton racersBtn;

	// List of friends for buttons - main friends, already invited friends and beta friends
	List<Friend> friendsData;
	List<Friend> invitedFriends;
	List<Friend> betaFriends;

	// List of challenges for buttons - new challenges, not-ran-yet and player-completed 
	List<Challenge> newChallenges;
	List<Challenge> incompleteChallenges;

	// List of invites
	List<Invite> invites;

	// List of notifications
	List<Notification> notifications;

    // List of challenges from friends
	List<Challenge> friendChallengeList;

	// Challenge notification popup
	GameObject challengeNotification;

	// Boolean to check if screen has been initialized
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

		// Find the mobile list, set the title and parent
		mobileList = physicalWidgetRoot.GetComponentInChildren<MobileList>();
		if (mobileList != null)
		{
			mobileList.SetTitle("");
			mobileList.SetParent(this);
		}

		// Get the iPhone background 
		GameObject bkg = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "iPhoneGreyBlackBackground");

		// Set the player's name
		GameObjectUtils.SetTextOnLabelInChildren(bkg, "PlayerName", Platform.Instance.User().name);

		// Get the texture for the profile picture
		UITexture profilePicture = bkg.GetComponentInChildren<UITexture>();
		
		// Load the profile picture and set it
		if(profilePicture != null) {
			Platform.Instance.RemoteTextureManager.LoadImage(Platform.Instance.User().image, "", (tex, text) => {
				profilePicture.mainTexture = tex;
			});
		}

		// Find the challenge button and get the UIButton component
		GameObject btnObj = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "ChallengeBtn");
		if(btnObj != null) {
			challengeBtn = btnObj.GetComponentInChildren<UIButton>();
		}

		// Find the racers button and get the UIButton component
		btnObj = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "RacersBtn");
		if(btnObj != null) {
			racersBtn = btnObj.GetComponentInChildren<UIButton>();
		}

		// Find the invite notification object and turn it off
		GameObject inviteObj = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "InviteNotification");
		if(inviteObj != null) {
			inviteObj.SetActive(false);
		}

		// If the challenge button exists, disable it and set the default colour
		if(challengeBtn != null) {
			challengeBtn.enabled = false;
			challengeBtn.defaultColor = new Color(202 / 255f, 202 / 255f, 202 / 255f);
		}

		// Find the ChallengeNotification game object
		challengeNotification = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "ChallengeNotification");

		// Change the list to the challenge list
		ChangeList("challenge");

		Platform.Instance.SyncToServer();		

		// Remove previous invite codes from the DataVault
		DataVault.Remove("invite_codes");

		// Start the coroutine that retrieves the invites
		Platform.Instance.partner.StartCoroutine(Platform.Instance.api.get("invites", body => {
			// Deserialize the JSON for the invites
			invites = JsonConvert.DeserializeObject<RaceYourself.API.ListResponse<RaceYourself.Models.Invite>>(body).response;	
			// Set the invite codes in the DataVault
			DataVault.Set("invite_codes", invites);
			// Initialize the number of unused invites
			int numUnused = 0;
			// Get the friends, needs the invites
			GetFriends();	
			// Find all the unused invites
			List<Invite> unusedInvites = invites.FindAll(r => r.used_at == null);
			// If there are any unused invites
			if(unusedInvites != null && unusedInvites.Count > 0) {
				// Set the number in the DataVault
				DataVault.Set("number_invites", unusedInvites.Count);
				// Make the InviteNotification object active
				if(inviteObj != null) {
					inviteObj.SetActive(true);
				}
			}
		})) ;

		// Set initialized to true
		initialized = true;
	}

	private IEnumerator LoadChallenges()
	{
		List<Notification> filteredNotifications = notifications.FindAll(r => r.message.type == "challenge");
		if(filteredNotifications != null) {
			filteredNotifications = filteredNotifications.FindAll(r => r.message.challenge_type == "duration");
		}

		if(filteredNotifications != null && filteredNotifications.Count > 0){
			List<Notification> newNotifications = filteredNotifications.FindAll(r => r.read == false);
			if(newNotifications != null && newNotifications.Count > 0) {
				DataVault.Set("new_challenges", newNotifications.Count);
			}

			filteredNotifications.RemoveAll(x => x.read == false);
			 
			foreach(Notification notification in newNotifications) {
				int challengerId = notification.message.from;
				int challengeId = notification.message.challenge_id;
				yield return Platform.Instance.partner.StartCoroutine(Platform.Instance.FetchChallengeFromNotification(challengeId, notification, (potential, note) => {
					User user = Platform.Instance.GetUser(note.message.from);
					newChallenges.Add(potential);
					Dictionary<string, string> dictionary = new Dictionary<string, string>();
					dictionary.Add("TitleText", user.name);
//					dictionary.Add("DurationText", (potential as DurationChallenge).duration.ToString());
					int duration = (potential as DurationChallenge).duration / 60;
					dictionary.Add("DurationText", duration.ToString());
					AddButtonData("NewChallenges" + potential.id, dictionary, "", user.image, ListButtonData.ButtonFormat.NewChallengeButton, GetConnection("ChallengeExit"));
					
					Platform.Instance.ReadNotification(notification.id);
				}));
			}

			foreach(Notification notification in filteredNotifications) {
				int challengerId = notification.message.from;
				int challengeId = notification.message.challenge_id;
				yield return Platform.Instance.partner.StartCoroutine(Platform.Instance.FetchChallengeFromNotification(challengeId, notification, (potential, note) => {
					User user = Platform.Instance.GetUser(note.message.from);
					incompleteChallenges.Add(potential);
					Dictionary<string, string> dictionary = new Dictionary<string, string>();
					dictionary.Add("TitleText", user.name);
					int duration = (potential as DurationChallenge).duration / 60;
					dictionary.Add("DurationText", duration.ToString());
					AddButtonData("IncompleteChallenges" + potential.id, dictionary, "", user.image, ListButtonData.ButtonFormat.ActiveChallengeButton, GetConnection("ChallengeExit"));
				}));
			}
		} else {
			AddButtonData("NoChallengeButton", null, "SetMobileHomeTab", ListButtonData.ButtonFormat.InvitePromptButton, GetConnection("RacersBtn"));
		}
	}



	/// <summary>
	/// Gets the challenges.
	/// </summary>
	public void GetChallenges() {
		notifications = Platform.Instance.Notifications();
		notifications.RemoveAll(x => x.message.from == Platform.Instance.User().id);
		newChallenges = new List<Challenge>();
		incompleteChallenges = new List<Challenge>();
		Platform.Instance.partner.StartCoroutine(LoadChallenges());
	}

	/// <summary>
	/// Gets the three lists of friends.
	/// </summary>
	public void GetFriends() {
		// First get the main list of friends
		friendsData = Platform.Instance.Friends();

		// Initialize the beta friends
		betaFriends = new List<Friend>();

		// If there are any friends
		if(friendsData != null) {
			for(int i=0; i<friendsData.Count; i++) {
				// If the friend has a userID they are part of the beta
				if(friendsData[i].userId != null) {
					// Add the friend to the betaFriend list
					betaFriends.Add(friendsData[i]);
					// Remove the friend from the main list
					friendsData.Remove(friendsData[i]);
					i--;
				}
			}
		

			if(invites != null) {
				invitedFriends = new List<Friend>();
//				List<Invite> usedInvites = invite
				for(int i=0; i<invites.Count; i++) {
					string uid = invites[i].identity_uid;
					Friend friend = friendsData.Find(x => x.uid == uid);
					if(friend != null) {
						invitedFriends.Add(friend);
						friendsData.Remove(friend);
					}
				}
			}
		}

		// Sort the betaFriends to A-Z by name
		if(betaFriends != null && betaFriends.Count > 1) {
			betaFriends.Sort((t1, t2) => t1.name.CompareTo(t2.name));
		}

		// Sort the invitedFriends to A-Z by name
		if(invitedFriends != null && invitedFriends.Count > 1) {
			invitedFriends.Sort((t1, t2) => t1.name.CompareTo(t2.name));
		}

		// Sort the main friends list to A-Z by name
		if(friendsData != null && friendsData.Count > 1) {
			friendsData.Sort((t1, t2) => t1.name.CompareTo(t2.name));
		}
	}

	/// <summary>
	/// Changes the list to either "challenge" or "friend".
	/// </summary>
	/// <param name="type">Type.</param>
	public void ChangeList(string type) {
		// Enable both buttons
		racersBtn.enabled = true;
		challengeBtn.enabled = true;

		// Re-initialize the list of buttons
		buttonData = new List<ListButtonData>();

		// Switch the type based on either "challenge" or "friend"
		switch(type) {
		case "challenge":
			// Disable the challenge notification object
			if(challengeNotification != null) {
				challengeNotification.SetActive(false);
			}

			// If the panel is initialized reset the mobile list and set the cell size to 300
			if(initialized) {
				mobileList.ResetList(350f);
			}

			// Get the challenges
			GetChallenges();

			// Disable the challenge button to make it go black
			challengeBtn.enabled = false;
			break;

		case "friend":
			// Reset the mobile list with cell size of 155
			mobileList.ResetList(155f);
			// Disable the racers button to make it go black
			racersBtn.enabled = false;
			// If there is a challenge notification and there are challenges
//			if(challengeNotification != null && newChallenges.Count > 0) {
//				// Make the challenge notification active
//				challengeNotification.SetActive(true);
//				// Set the number of challenges for the notification
//				DataVault.Set("new_challenges", friendChallengeList.Count);
//			}

			// If the user has facebook permissions and friends
			if(Platform.Instance.HasPermissions("facebook", "login") && friendsData != null && friendsData.Count > 0) {
				// If there are beta friends
				if(betaFriends != null && betaFriends.Count > 0) {
					// Loop through all beta friends
					for(int i=0; i<betaFriends.Count; i++) {
						// Set the button name with the number corresponding to the friend index
						string betaButtonName = "challenge" + i;
						// Create a new dictionary for the friend to set the name
						Dictionary<string, string> betaFriendDictionary = new Dictionary<string, string>();
						betaFriendDictionary.Add("Name", betaFriends[i].name);
						// Add the button to the list
						AddButtonData(betaButtonName, betaFriendDictionary, "", betaFriends[i].image, ListButtonData.ButtonFormat.ChallengeButton, GetConnection("ChallengeButton"));
					}
				}

				// If there are friends the player hasn't invited
				if(invitedFriends != null) 
				{
					// Loop through the list
					for(int i=0; i<invitedFriends.Count; i++) 
					{
						// Set the button name with the number corresponding to the friend index
						string invitedButtonName = "invited" + i;
						// Create a new Dictionary for the friend to set the name
						Dictionary<string, string> invitedDictionary = new Dictionary<string, string>();
						invitedDictionary.Add("Name", invitedFriends[i].name);
						// Add the button to the list
						AddButtonData(invitedButtonName, invitedDictionary, "", invitedFriends[i].image, ListButtonData.ButtonFormat.InvitedButton, GetConnection ("InvitedButton"));
					}
				}

				// If there are uninvited friends
				if (friendsData != null)
				{        
					// Loop through all friends
					for (int i = 0; i < friendsData.Count; i++)
					{
						// Set the button name with the number corresponding to the friend index
						string buttonName = "uninvited" + i;
						// Create a new Dictionary for the friend to set the name
						Dictionary<string, string> friendDictionary = new Dictionary<string, string>();
						friendDictionary.Add("Name", friendsData[i].name);
						// Add the button to the list
						AddButtonData(buttonName, friendDictionary, "", friendsData[i].image, ListButtonData.ButtonFormat.InviteButton, GetConnection("InviteButton"));
					}
				}

			} else {
				// If there are no friends, attempt to get the list of friends
				GetFriends();
				// Add an import friends button
				AddButtonData ("ImportButton", null, "", ListButtonData.ButtonFormat.ImportButton, GetConnection("ImportButton"));
			}

			break;

		default:
			UnityEngine.Debug.Log("MobileHomePanel: type not implemented");
			break;
		}


	}

	/// <summary>
	/// whenever button get clicked it would be handled here
	/// </summary>
	/// <param name="button">button which send this event</param>
	/// <returns></returns>
	public override void OnClick(FlowButton button)
	{
		// Check there actually is a button
		if (button != null)
		{
			// If the button is for an uninvited friend
			if(button.name.Contains("uninvited")) {
				// Get the list of invites
				List<Invite> invites = (List<Invite>)DataVault.Get("invite_codes");
				// Check if there are any unused invites
				Invite unusedInvite = invites.Find(x => x.used_at == null);
				// If there are none don't proceed through the exit
				if(unusedInvite == null) {
					return;
				}
				// Remove the prefix to find the friend index
				string prefix = "uninvited";
				string index = button.name.Substring(prefix.Length);
				int i = Convert.ToInt32(index);
				// Using this index, get the friend and set it in the DataVault
				DataVault.Set("chosen_friend", friendsData[i]);
				Debug.Log("chosen_friend set to " + friendsData[i].name);
			} else if(button.name.Contains("challenge")) {
				// For a challenge button, remove the "challenge" prefix and get the index
				string prefix = "challenge";
				string index = button.name.Substring(prefix.Length);
				int i = Convert.ToInt32(index);
				// Using this index get the friend and set it in the DataVault
				DataVault.Set("chosen_friend", betaFriends[i]);
				Debug.Log("beta chosen_friend set to " + betaFriends[i].name);
			} else if(button.name.Contains("NewChallenges")) {
				string prefix = "NewChallenges";
				string index = button.name.Substring(prefix.Length);
				int challengeId = Convert.ToInt32(index);

				Notification note = notifications.Find(x => x.message.challenge_id == challengeId);
				if(note != null) {
					UnityEngine.Debug.Log("MobileHomePanel: notification found");
					DataVault.Set("challenge_notification", note);
				} else {
					UnityEngine.Debug.LogError("MobileHomePanel: notification not found " + challengeId);
				}

			} else if(button.name.Contains("IncompleteChallenges")) {
				string prefix = "IncompleteChallenges";
				string index = button.name.Substring(prefix.Length);
				int incompleteChallengeId = Convert.ToInt32(index);
				
				Notification note = notifications.Find(x => x.message.challenge_id == incompleteChallengeId);
				if(note != null) {
					UnityEngine.Debug.Log("MobileHomePanel: notification found");
					DataVault.Set("challenge_notification", note);
				} else {
					UnityEngine.Debug.LogError("MobileHomePanel: notification not found");
				}
			}
		}
		else
		{
			return;
		}
		
		base.OnClick(button);
	}

	/// <summary>
	/// exit finalization and clearing process
	/// </summary>
	/// <returns></returns>
	public override void Exited ()
	{
		base.Exited ();
		// Set initialized to false so that it can be re-initialized when going back in the panel
		initialized = false;
	}

}
