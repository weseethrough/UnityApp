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
	List<ChallengeNotification> newChallenges;
	List<ChallengeNotification> incompleteChallenges;
	List<ChallengeNotification> playerChallenges;

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
//			mobileList.SetTitle("");
			mobileList.SetParent(this);
		}

		// Get the iPhone background 
		GameObject bkg = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "MainProfilePicture");

		// Set the player's name
//		GameObjectUtils.SetTextOnLabelInChildren(bkg, "PlayerName", Platform.Instance.User().name);

		// Get the texture for the profile picture
		UITexture profilePicture = bkg.GetComponent<UITexture>();

		string empty = "";
		// Load the profile picture and set it
		if(profilePicture != null) {
			Platform.Instance.RemoteTextureManager.LoadImage(Platform.Instance.User().image, empty, (tex, text) => {
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
			challengeBtn.defaultColor = new Color(68 / 255f, 68 / 255f, 68 / 255f);
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

//			if(Platform.Instance.HasPermissions("facebook", "login")) {
//				Platform.Instance.Authorize("facebook", "login");
//			}

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

	/// <summary>
	/// Coroutine that loads all challenges
	/// </summary>
	/// <returns>Yields while fetching individual challenges.</returns>
	private IEnumerator LoadChallenges()
	{
		// Find all challenge notifications
		List<Notification> filteredNotifications = notifications.FindAll(r => r.message.type == "challenge");
		if(filteredNotifications != null) {
			// Remove all challenges that aren't duration based
			filteredNotifications = filteredNotifications.FindAll(r => r.message.challenge_type == "duration");
		}

		UnityEngine.Debug.Log("MobileHomePanel: Got " + filteredNotifications.Count + " challenges");

		// If we actually have any challenges
		if(filteredNotifications != null && filteredNotifications.Count > 0)
		{
			// Get the player challenges
			List<Notification> playerNotifications = filteredNotifications.FindAll(r => r.message.from == Platform.Instance.User().id);

			// Remove all the player's sent challenges from the notifications
			filteredNotifications.RemoveAll(x => x.message.from == Platform.Instance.User().id);

			// Find all new notifications
			List<Notification> newNotifications = filteredNotifications.FindAll(r => r.read == false);
			// Set the number of new challenges in the DataVault
			if(newNotifications != null && newNotifications.Count > 0) {
				DataVault.Set("new_challenges", newNotifications.Count);
			}

			// Remove all unread notifications from the filtered notifications
			filteredNotifications.RemoveAll(x => x.read == false);

			// Loop through all new notifications
			foreach(Notification notification in newNotifications) {
				UnityEngine.Debug.Log("processing notification from:" + notification.message.from);

				// Get the challenger and challenge ID
				int challengerId = notification.message.from;
				int challengeId = notification.message.challenge_id;

				Challenge potential = null;

				// Start fetching the challenge
				yield return Platform.Instance.partner.StartCoroutine(Platform.Instance.FetchChallengeFromNotification(challengeId, notification, (c, note) => {
					potential = c;
				}));

				if(potential != null) {
					// Get the rival for the challenge
					User user = null;

					yield return Platform.Instance.partner.StartCoroutine(Platform.Instance.GetUserCoroutine(notification.message.from, (u) => {
						if(u == null) { UnityEngine.Debug.LogError("user is null"); }
						user = u;
						}));

					if(null == user)
					{
						UnityEngine.Debug.LogWarning("couldn't find user " + notification.message.from);
						continue;
					}

					if(newChallenges == null) { UnityEngine.Debug.LogError("newchallenges is null"); }

					TimeSpan? difference = potential.stop_time - DateTime.Now;
					if(difference != null && difference.Value.TotalMinutes > 0) {
						ChallengeNotification challengeNote = new ChallengeNotification(notification, potential, user);
						// Add the challenge to the list
						newChallenges.Add(challengeNote);
					}

				}
			}

			foreach(Notification notification in playerNotifications) 
			{
				// Get the challenger and challenge ID
				int challengerId = notification.message.from;
				int challengeId = notification.message.challenge_id;

				Challenge potential = null;

				// Start fetching the challenge
				yield return Platform.Instance.partner.StartCoroutine(Platform.Instance.FetchChallengeFromNotification(challengeId, notification, (c, note) => {
					potential = c;
				}));

				if(potential != null) {
					// Get the rival for the challenge
					//User user = Platform.Instance.GetUser(note.message.to);
					User user = null;
					//retrieve the user
					yield return Platform.Instance.partner.StartCoroutine(Platform.Instance.GetUserCoroutine(notification.message.to, (u) => {
						user = u;
					}));

					if(null == user)
					{
						UnityEngine.Debug.LogWarning("Couldn't find user " + notification.message.from);
						continue;
					}

					TimeSpan? difference;
					if(potential.stop_time.HasValue) {
						difference = potential.stop_time - DateTime.Now;
					} else {
						difference = TimeSpan.Zero;
					}

					if(difference != null && difference.Value.TotalMinutes > 0) {
						ChallengeNotification challengeNote = new ChallengeNotification(notification, potential, user);
						// Add the challenge to the list
						playerChallenges.Add(challengeNote);

					}
				}
			}

			// Loop through the normal notifications
			foreach(Notification notification in filteredNotifications) 
			{
				// Get the challenger and challenge ID
				int challengerId = notification.message.from;
				int challengeId = notification.message.challenge_id;

				Challenge potential = null;

				// Start the coroutine to fetch the challenge
				yield return Platform.Instance.partner.StartCoroutine(Platform.Instance.FetchChallengeFromNotification(challengeId, notification, (c, note) => {
					potential = c;
				}));

				if(potential != null) {
					// Get the rival user
					User user = null;
					yield return Platform.Instance.partner.StartCoroutine(
						Platform.Instance.GetUserCoroutine(notification.message.from, (u) => {
						if(u != null)
						{
							user = u;
						}
					}));

					if(null == user) { continue; }

					TimeSpan? difference = potential.stop_time - DateTime.Now;
					if(difference != null && difference.Value.TotalMinutes > 0) 
					{
						ChallengeNotification challengeNote = new ChallengeNotification(notification, potential, user);
						// Add the button to the list
						incompleteChallenges.Add(challengeNote);
					}

				}
			}
			CreateChallengeButtons();
		}
	}

	private void CreateChallengeButtons() 
	{
		User player = Platform.Instance.User();
		foreach(ChallengeNotification challengeNote in newChallenges)
		{
			Challenge challenge = challengeNote.GetChallenge();
			User user = challengeNote.GetUser();

			// Create a button name
			string newButtonName = "NewChallenges" + challenge.id;

			// Create a new dictionary for the text fields
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			// Add the title and description
			dictionary.Add("TitleText", user.name);
			dictionary.Add("DescriptionText", "challenges you");
			// Change the duration to minutes and add to the dictionary
			int duration = challengeNote.GetDuration() / 60;
			dictionary.Add("DurationText", duration.ToString());
			
			dictionary.Add("TimeRemainingText", challenge.stop_time.Value.ToString("O"));
			
			// Create a new dictionary for the textures
			Dictionary<string, Dictionary<string, string>> newChallengeImageDictionary = new Dictionary<string, Dictionary<string, string>>();
			// Create an inner dictionary for the previous dictionary
			Dictionary<string, string> innerNewDictionary = new Dictionary<string, string>();
			// Add the location of the texture object and the button name
			innerNewDictionary.Add("texture", "PlayerPicture");
			innerNewDictionary.Add("name", newButtonName);
			// Add the data to the main dictionary for the player's image
			newChallengeImageDictionary.Add(player.image, innerNewDictionary);
			
			// Re-initialize the dictionary
			innerNewDictionary = new Dictionary<string, string>();
			// Add the name and texture again
			innerNewDictionary.Add("name", newButtonName);
			innerNewDictionary.Add("texture", "RivalPicture");
			// Add the data to the main dictionary for the rival's image
			newChallengeImageDictionary.Add(user.image, innerNewDictionary);
			// Finally add the button to the list
			AddButtonData(newButtonName, dictionary, "", newChallengeImageDictionary, ListButtonData.ButtonFormat.FriendChallengeButton, GetConnection("ChallengeExit"));

			// Set the notification as read
			Platform.Instance.ReadNotification(challengeNote.GetID());
		}

		foreach(ChallengeNotification challengeNote in playerChallenges)
		{
			Challenge challenge = challengeNote.GetChallenge();
			User user = challengeNote.GetUser();

			// Create a button name
			string newButtonName = "PlayerChallenges" + challenge.id;
			
			// Create a new dictionary for the text fields
			Dictionary<string, string> playerDictionary = new Dictionary<string, string>();
			// Add the title and description
			playerDictionary.Add("TitleText", "You challenged");
			playerDictionary.Add("DescriptionText", user.name);
			// Change the duration to minutes and add to the dictionary
			int duration = challengeNote.GetDuration() / 60;
			playerDictionary.Add("DurationText", duration.ToString());
			
			playerDictionary.Add("TimeRemainingText", challenge.stop_time.Value.ToString("O"));
			
			// Create a new dictionary for the textures
			Dictionary<string, Dictionary<string, string>> playerChallengeImageDictionary = new Dictionary<string, Dictionary<string, string>>();
			// Create an inner dictionary for the previous dictionary
			Dictionary<string, string> innerPlayerDictionary = new Dictionary<string, string>();
			// Add the location of the texture object and the button name
			innerPlayerDictionary.Add("texture", "PlayerPicture");
			innerPlayerDictionary.Add("name", newButtonName);
			// Add the data to the main dictionary for the player's image
			playerChallengeImageDictionary.Add(player.image, innerPlayerDictionary);
			
			// Re-initialize the dictionary
			innerPlayerDictionary = new Dictionary<string, string>();
			// Add the name and texture again
			innerPlayerDictionary.Add("name", newButtonName);
			innerPlayerDictionary.Add("texture", "RivalPicture");
			// Add the data to the main dictionary for the rival's image
			
			if(!playerChallengeImageDictionary.ContainsKey(user.image))
			{
				playerChallengeImageDictionary.Add(user.image, innerPlayerDictionary);
			}
			// Finally add the button to the list
			AddButtonData(newButtonName, playerDictionary, "", playerChallengeImageDictionary, ListButtonData.ButtonFormat.FriendChallengeButton, GetConnection("ChallengeExit"));
		}

		foreach(ChallengeNotification challengeNote in incompleteChallenges)
		{
			Challenge challenge = challengeNote.GetChallenge();
			User user = challengeNote.GetUser();

			string activeButtonName = "IncompleteChallenges" + challenge.id;		
			// Initialize the dictionary
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			// Add the title and description for the challenge
			dictionary.Add("TitleText", user.name);
			dictionary.Add("DescriptionText", "challenged you");
			// Convert the duration to minutes and add it to the dictionary
			int duration = challengeNote.GetDuration() / 60;
			dictionary.Add("DurationText", duration.ToString());
			
			dictionary.Add("TimeRemainingText", challenge.stop_time.Value.ToString("O"));
			
			// Create the image dictionary
			Dictionary<string, Dictionary<string, string>> activeChallengeImageDictionary = new Dictionary<string, Dictionary<string, string>>();
			// Create the inner dictionary for the images
			Dictionary<string, string> innerActiveDictionary = new Dictionary<string, string>();
			// Add the texture and name for the player's picture
			innerActiveDictionary.Add("texture", "PlayerPicture");
			innerActiveDictionary.Add("name", activeButtonName);
			// Add the player's texture information to the dictionary
			activeChallengeImageDictionary.Add(player.image, innerActiveDictionary);
			// Re-initialize the inner dictionary for the rival
			innerActiveDictionary = new Dictionary<string, string>();
			// Add the attributes for the rival's button name and texture name
			innerActiveDictionary.Add("name", activeButtonName);
			innerActiveDictionary.Add("texture", "RivalPicture");
			// Add the rival picture to the dictionary
			activeChallengeImageDictionary.Add(user.image, innerActiveDictionary);
			// Add the button
			AddButtonData(activeButtonName, dictionary, "", activeChallengeImageDictionary, ListButtonData.ButtonFormat.FriendChallengeButton, GetConnection("ChallengeExit"));
		}
	
		if(buttonData.Count == 0) {
			AddButtonData("NoChallengeButton", null, "SetMobileHomeTab", ListButtonData.ButtonFormat.InvitePromptButton, GetConnection("RacersBtn"));
		}
	}

	/// <summary>
	/// Gets the challenges.
	/// </summary>
	public void GetChallenges() {
		notifications = Platform.Instance.Notifications();
		if(newChallenges != null && incompleteChallenges != null && playerChallenges != null) 
		{
			if(newChallenges.Count > 0)
			{
				User player = Platform.Instance.User();
				foreach(ChallengeNotification challengeNote in newChallenges)
				{
					Notification note = challengeNote.GetNotification();
					if(note.read)
					{
						if(note.message.from == player.id)
						{
							playerChallenges.Add(challengeNote);
						}
						else
						{
							incompleteChallenges.Add(challengeNote);
						}
						newChallenges.Remove(challengeNote);
					}
				}

			}
			CreateChallengeButtons();
		} 
		else
		{
			newChallenges = new List<ChallengeNotification>();
			incompleteChallenges = new List<ChallengeNotification>();
			playerChallenges = new List<ChallengeNotification>();
			Platform.Instance.partner.StartCoroutine(LoadChallenges());
		}

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

	bool getHasFriends()
	{
		//check they have at least one friend in one of the lists
		if( (friendsData != null) && (friendsData.Count > 0) ) { return true; }
		if( (betaFriends != null) && (betaFriends.Count > 0) ) { return true; }
		if( (invitedFriends != null) && (invitedFriends.Count > 0 ) ) { return true; }

		// no friends
		return false;
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
				mobileList.ResetList(250f);
			}

			// Get the challenges
			GetChallenges();

			// Disable the challenge button to make it go black
			challengeBtn.enabled = false;
			break;

		case "friend":
			// Reset the mobile list with cell size of 155
			mobileList.ResetList(155f);

			Platform.Instance.partner.StopCoroutine("LoadChallenges");

			// Disable the racers button to make it go black
			racersBtn.enabled = false;

			// If the user has facebook permissions and friends
			bool hasFriends = getHasFriends();

			if(!hasFriends)
			{
				GetFriends();
				hasFriends = getHasFriends();
			}

			if(Platform.Instance.HasPermissions("facebook", "login") && hasFriends) {
				// If there are beta friends
				if(betaFriends != null && betaFriends.Count > 0) {
					// Loop through all beta friends
					for(int i=0; i<betaFriends.Count; i++) {
						// Set the button name with the number corresponding to the friend index
						string betaButtonName = "challenge" + i;
						// Create a new dictionary for the friend to set the name
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
						Dictionary<string, Dictionary<string, string>> invitedImageDictionary = new Dictionary<string, Dictionary<string, string>>();
						Dictionary<string, string> innerInvitedDictionary = new Dictionary<string, string>();
						innerInvitedDictionary.Add("texture", "InvitedProfilePicture");
						innerInvitedDictionary.Add("name", invitedButtonName);
						invitedImageDictionary.Add(invitedFriends[i].image, innerInvitedDictionary);
						AddButtonData(invitedButtonName, invitedDictionary, "", invitedImageDictionary, ListButtonData.ButtonFormat.InvitedButton, GetConnection ("InvitedButton"));
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
						Dictionary<string, Dictionary<string, string>> friendImageDictionary = new Dictionary<string, Dictionary<string, string>>();
						Dictionary<string, string> innerDictionary = new Dictionary<string, string>();
						innerDictionary.Add("texture", "InviteProfilePicture");
						innerDictionary.Add("name", buttonName);
						friendImageDictionary.Add(friendsData[i].image, innerDictionary);
						AddButtonData(buttonName, friendDictionary, "", friendImageDictionary, ListButtonData.ButtonFormat.InviteButton, GetConnection("InviteButton"));
					}
				}

			} else {
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
			if(button.name.Contains("uninvited")) 
			{
				// Get the list of invites
				List<Invite> invites = (List<Invite>)DataVault.Get("invite_codes");
				// Check if there are any unused invites
				Invite unusedInvite = invites.Find(x => x.used_at == null);
				// If there are none don't proceed through the exit
				if(unusedInvite == null) 
				{
					return;
				}
				// Remove the prefix to find the friend index
				int i = GetIndex("uninvited", button.name);
				// Using this index, get the friend and set it in the DataVault
				DataVault.Set("chosen_friend", friendsData[i]);
			} else if(button.name.Contains("challenge")) 
			{
				// For a challenge button, remove the "challenge" prefix and get the index
				int i = GetIndex("challenge", button.name);
				// Using this index get the friend and set it in the DataVault
				DataVault.Set("chosen_friend", betaFriends[i]);
			} else if(button.name.Contains("NewChallenges")) 
			{
				int challengeId = GetIndex("NewChallenges", button.name);

				Notification note = notifications.Find(x => x.message.challenge_id == challengeId);
				if(note != null) 
				{
					DataVault.Set("challenge_notification", note);
				} else 
				{
					UnityEngine.Debug.LogError("MobileHomePanel: notification not found " + challengeId);
				}

			} else if(button.name.Contains("IncompleteChallenges")) 
			{
				int incompleteChallengeId = GetIndex("IncompleteChallenges", button.name);
				
				Notification note = notifications.Find(x => x.message.challenge_id == incompleteChallengeId);
				if(note != null) 
				{
					DataVault.Set("challenge_notification", note);
				} else 
				{
					UnityEngine.Debug.LogError("MobileHomePanel: notification not found");
				}
			} else if(button.name.Contains("PlayerChallenges")) 
			{
				int challengeId = GetIndex("PlayerChallenges", button.name);

				Notification note = notifications.Find(x => x.message.challenge_id == challengeId);
				if(note != null) 
				{
					DataVault.Set("challenge_notification", note);
				} else 
				{
					UnityEngine.Debug.LogError("MobileHomePanel: notification not found");
				}
			} else if(button.name.Contains("ImportButton")) 
			{
				DataVault.Set("facebook_message", "Connect to Facebook");
			}
		}
		else
		{
			return;
		}
		
		base.OnClick(button);
	}

	private int GetIndex(string prefix, string buttonName) {
		string index = buttonName.Substring(prefix.Length);
		return Convert.ToInt32(index);
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
