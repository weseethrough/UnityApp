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

	// List objects that holds all buttons
	MobileList challengeList;
	MobileList friendsList;
    
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

	GameObject syncIcon;

	NetworkMessageListener.OnSync syncHandler = null;

	// Boolean to check if screen has been initialized
	bool initialized = false;

	bool loadingChallengeIncomplete = false;
	bool loadingFriendsIncomplete = false;

	private string tab = "preinit";

	private string[] challengeSprites = new string[] {
		"homescreen_card_01_2-02",
		"homescreen_card_01_2-05", 
		"homescreen_card_01_2-07",
		"homescreen_card_01_2-08"
	};

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

		GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "LoadingTextLabel", "");

		// Find the mobile list, set the title and parent
		MobileList[] lists = physicalWidgetRoot.GetComponentsInChildren<MobileList>();
		foreach (MobileList list in lists) 
		{
			if (list.tag == "ChallengeList") {
				list.SetParent(this);
				list.SetList("challenges");
				challengeList = list;
			}
			else if (list.tag == "FriendsList") {
				list.SetParent(this);
				list.SetList("friends");
				friendsList = list;
            }
			else {
				UnityEngine.Debug.LogError("Unknown mobilelist tag: " + list.tag);
			}
        }
		if (challengeList == null) UnityEngine.Debug.LogError("MobileHomePanel: challenge list missing!");
		if (friendsList == null) UnityEngine.Debug.LogError("MobileHomePanel: friends list missing!");

		// Get the iPhone background 
		GameObject bkg = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "MainProfilePicture", true);

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
			if(challengeBtn != null) {
				challengeBtn.enabled = true;
			}			
		}

		// Find the racers button and get the UIButton component
		btnObj = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "RacersBtn");
		if(btnObj != null) {
			racersBtn = btnObj.GetComponentInChildren<UIButton>();
			if(racersBtn != null) 
			{
				racersBtn.enabled = true;
			}
		}

		// Find the invite notification object and turn it off
		GameObject inviteObj = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "InviteNotification");
		if(inviteObj != null) {
			inviteObj.SetActive(false);
		}

		// TODO: Get sprite names from atlas

		// Find the ChallengeNotification game object
		challengeNotification = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "ChallengeNotification");

		GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "LoadingTextLabel", "Syncing");

		syncIcon = GameObjectUtils.SearchTreeByName(physicalWidgetRoot, "SyncIcon"); // TODO: Obsolete. Remove
		syncIcon.SetActive(false);

        syncHandler = new NetworkMessageListener.OnSync((message) => {
			Platform.Instance.NetworkMessageListener.onSync -= syncHandler;
			GetChallenges();

			// Remove previous invite codes from the DataVault
			DataVault.Remove("invite_codes");
			
			// Start the coroutine that retrieves the invites
			Platform.Instance.partner.StartCoroutine(Platform.Instance.api.get("invites", body => {
				if (body == null) {
					UnityEngine.Debug.LogError("MobleHomePanel: Could not fetch invites");
					return;
				}
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
		});
		Platform.Instance.NetworkMessageListener.onSync += syncHandler;

		Platform.Instance.SyncToServer();		

		// If we're coming back to this panel, go to the old tab. Otherwise, default to 'challenge'.
		string lastTab = DataVault.Get("mobilehome_selectedtab") as String;
		ChangeList(lastTab != null ? lastTab : "challenge");
	}

	/// <summary>
	/// Coroutine that loads all challenges
	/// </summary>
	/// <returns>Yields while fetching individual challenges.</returns>
	private IEnumerator LoadChallenges(List<Notification> notes)
	{
		if (notes == null) {
			loadingChallengeIncomplete = false;
			UnityEngine.Debug.LogWarning("MobileHomePanel: null notifications");
			yield break;
        }
        UnityEngine.Debug.Log("MobileHomePanel::LoadChallenges()");
		loadingChallengeIncomplete = true;
		try {
			if (tab == "challenges" && GetButtonData("challenges").Count == 0) GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "LoadingTextLabel", "Loading challenges");
			// Find all challenge notifications
			List<Notification> filteredNotifications = notes.FindAll(r => r.message.type == "challenge");
			if(filteredNotifications != null) {
				// Remove all challenges that aren't duration based
				filteredNotifications = filteredNotifications.FindAll(r => r.message.challenge_type == "duration");
			} else {
				loadingChallengeIncomplete = false;
				UnityEngine.Debug.LogWarning("MobileHomePanel: no challenge notifications");
				yield break;
			}

			UnityEngine.Debug.Log("MobileHomePanel: Got " + filteredNotifications.Count + " challenges");

			// If we actually have any challenges
			if(filteredNotifications != null)
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

				newChallenges = new List<ChallengeNotification>();

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
							ChallengeNotification foundChallenge = newChallenges.Find(x => x.GetChallenge().id == potential.id);
							if(foundChallenge == null)
							{
								newChallenges.Add(challengeNote);
							}
						
						}

					}
				}

				playerChallenges = new List<ChallengeNotification>();

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
							// Mark own challenge notifications as read as we take no other action
							if (challengeNote.GetRead()) challengeNote.SetRead(); 
							// Add the challenge to the list
							ChallengeNotification foundChallenge = playerChallenges.Find(x => x.GetChallenge().id == potential.id);
							if(foundChallenge == null)
							{
								playerChallenges.Add(challengeNote);
							}

						}
					} else yield break;
				}

				incompleteChallenges = new List<ChallengeNotification>();

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
							ChallengeNotification foundChallenge = incompleteChallenges.Find(x => x.GetChallenge().id == potential.id);
							if(foundChallenge == null)
							{
								incompleteChallenges.Add(challengeNote);
							}
						}
					}
				}
								
				CreateChallengeButtons();
			}
			notifications = notes;
			UnityEngine.Debug.Log("Loaded challenges");
		} finally {
		loadingChallengeIncomplete = false;
		GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "LoadingTextLabel", "");
		}
	}

	private void CreateChallengeButtons() 
	{
		UnityEngine.Debug.Log("CreateChallengeButtons()");
		// Reset list (scroll to top) if uninitialized or new challenges 
		if (challengeList.GetItemHeight() != 280 || newChallenges.Count > 0) {
			challengeList.ResetList(280f);
		} else {
			challengeList.ClearList();
		}
		GetButtonData("challenges").Clear();
		User player = Platform.Instance.User();
		UnityEngine.Debug.Log("CreateChallengeButtons: " + newChallenges.Count + " new, " + playerChallenges.Count + " player, " + incompleteChallenges.Count + " incomplete challenges");
		foreach(ChallengeNotification challengeNote in newChallenges)
		{
			Challenge challenge = challengeNote.GetChallenge();
			User user = challengeNote.GetUser();

			TimeSpan? difference = challenge.stop_time - DateTime.Now;
			if(difference != null && difference.Value.TotalMinutes > 0)
			{
				// Create a button name
				string newButtonName = "NewChallenges" + challenge.id;

				// Create a new dictionary for the text fields
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				// Add the title and description
				dictionary.Add("TitleText", user.DisplayName);
				dictionary.Add("DescriptionText", "Has sent you a challenge!");
				// Change the duration to minutes and add to the dictionary
				int duration = challengeNote.GetDuration() / 60;
				dictionary.Add("DurationText", duration.ToString());
				
				dictionary.Add("TimeRemainingText", challenge.stop_time.Value.ToString("O"));
				
				// Create a new dictionary for the textures
				Dictionary<string, Dictionary<string, string>> newChallengeImageDictionary = new Dictionary<string, Dictionary<string, string>>();
				// Create an inner dictionary for the previous dictionary
				Dictionary<string, string> innerNewDictionary = new Dictionary<string, string>();
				// Add the name and texture again
				innerNewDictionary.Add("name", newButtonName);
				innerNewDictionary.Add("texture", "PlayerPicture");
				// Add the data to the main dictionary for the rival's image
				newChallengeImageDictionary.Add(user.image, innerNewDictionary);
				newChallengeImageDictionary.Add("background", new Dictionary<string, string>() {
					{"name", newButtonName},
					{"sprite", challengeSprites[user.id % challengeSprites.Length]}
				});
				// Finally add the button to the list
				AddButtonData("challenges", newButtonName, dictionary, "", newChallengeImageDictionary, ListButtonData.ButtonFormat.FriendChallengeButton, GetConnection("ChallengeExit"), false);
			}
		}

		foreach(ChallengeNotification challengeNote in playerChallenges)
		{
			Challenge challenge = challengeNote.GetChallenge();
			User user = challengeNote.GetUser();

			TimeSpan? difference = challenge.stop_time - DateTime.Now;
			if(difference != null && difference.Value.TotalMinutes > 0)
			{
				// Create a button name
				string newButtonName = "PlayerChallenges" + challenge.id;
				
				// Create a new dictionary for the text fields
				Dictionary<string, string> playerDictionary = new Dictionary<string, string>();
				// Add the title and description
				playerDictionary.Add("TitleText", "You challenged");
				playerDictionary.Add("DescriptionText", user.DisplayName);
				// Change the duration to minutes and add to the dictionary
				int duration = challengeNote.GetDuration() / 60;
				playerDictionary.Add("DurationText", duration.ToString());
				
				playerDictionary.Add("TimeRemainingText", challenge.stop_time.Value.ToString("O"));
				
				// Create a new dictionary for the textures
				Dictionary<string, Dictionary<string, string>> playerChallengeImageDictionary = new Dictionary<string, Dictionary<string, string>>();
				// Create an inner dictionary for the previous dictionary
				Dictionary<string, string> innerPlayerDictionary = new Dictionary<string, string>();
				// Add the name and texture again
				innerPlayerDictionary.Add("name", newButtonName);
				innerPlayerDictionary.Add("texture", "PlayerPicture");
				// Add the data to the main dictionary for the rival's image
				
				if(!string.IsNullOrEmpty(user.image) && !playerChallengeImageDictionary.ContainsKey(user.image))
				{
					playerChallengeImageDictionary.Add(user.image, innerPlayerDictionary);
				}
				playerChallengeImageDictionary.Add("background", new Dictionary<string, string>() {
					{"name", newButtonName},
					{"sprite", challengeSprites[user.id % challengeSprites.Length]}
				});
				// Finally add the button to the list
				AddButtonData("challenges", newButtonName, playerDictionary, "", playerChallengeImageDictionary, ListButtonData.ButtonFormat.FriendChallengeButton, GetConnection("ChallengeExit"));
			}
		}

		foreach(ChallengeNotification challengeNote in incompleteChallenges)
		{
			Challenge challenge = challengeNote.GetChallenge();
			User user = challengeNote.GetUser();

			TimeSpan? difference = challenge.stop_time - DateTime.Now;
			if(difference != null && difference.Value.TotalMinutes > 0)
			{	
				string activeButtonName = "IncompleteChallenges" + challenge.id;		
				// Initialize the dictionary
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				// Add the title and description for the challenge
				dictionary.Add("TitleText", user.DisplayName);
				dictionary.Add("DescriptionText", "Has sent you a challenge!");
				// Convert the duration to minutes and add it to the dictionary
				int duration = challengeNote.GetDuration() / 60;
				dictionary.Add("DurationText", duration.ToString());
				
				dictionary.Add("TimeRemainingText", challenge.stop_time.Value.ToString("O"));
				
				// Create the image dictionary
				Dictionary<string, Dictionary<string, string>> activeChallengeImageDictionary = new Dictionary<string, Dictionary<string, string>>();
				// Create the inner dictionary for the images
				Dictionary<string, string> innerActiveDictionary = new Dictionary<string, string>();
				// Add the texture and name for the player's picture
				innerActiveDictionary = new Dictionary<string, string>();
				// Add the attributes for the rival's button name and texture name
				innerActiveDictionary.Add("name", activeButtonName);
				innerActiveDictionary.Add("texture", "PlayerPicture");
				// Add the rival picture to the dictionary
				activeChallengeImageDictionary.Add(user.image, innerActiveDictionary);
				activeChallengeImageDictionary.Add("background", new Dictionary<string, string>() {
					{"name", activeButtonName},
					{"sprite", challengeSprites[user.id % challengeSprites.Length]}
				});
				// Add the button
				AddButtonData("challenges", activeButtonName, dictionary, "", activeChallengeImageDictionary, ListButtonData.ButtonFormat.FriendChallengeButton, GetConnection("ChallengeExit"));
			}
		}

		if(!loadingChallengeIncomplete && GetButtonData("challenges").Count == 0) {
			AddButtonData("challenges", "NoChallengeButton", null, "SetMobileHomeTab", ListButtonData.ButtonFormat.InvitePromptButton, GetConnection("RacersBtn"));
		}
	}

	/// <summary>
	/// Gets the challenges.
	/// </summary>
	public void GetChallenges() {
		UnityEngine.Debug.Log("GetChallenges()");
		var notes = Platform.Instance.Notifications();
		if (notifications == null || notes.Count != notifications.Count) {
			Platform.Instance.partner.StartCoroutine(LoadChallenges(notes));
		}
		if(newChallenges != null && incompleteChallenges != null && playerChallenges != null && !loadingChallengeIncomplete) 
		{
			CreateChallengeButtons();
		}
	}

	/// <summary>
	/// Gets the three lists of friends.
	/// </summary>
	public void GetFriends() {
		if (!loadingFriendsIncomplete) Platform.Instance.partner.StartCoroutine(LoadFriends());
	}

	private IEnumerator LoadFriends() {
		UnityEngine.Debug.Log("LoadFriends()");
		loadingFriendsIncomplete = true;
		if (tab == "friends" && GetButtonData("friends").Count == 0) GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "LoadingTextLabel", "Loading friends");
		// First get the main list of friends
		friendsData = Platform.Instance.Friends();

		// Initialize the beta friends
		betaFriends = new List<Friend>();

		// If there are any friends
		if(friendsData != null) {
			for(int i=0; i<friendsData.Count; i++) {
				var friend = friendsData[i];
				// If the friend has a userID they are part of the beta
				if(friend.userId != null) {
					// Add the friend to the betaFriend list
					betaFriends.Add(friend);
					yield return Platform.Instance.partner.StartCoroutine(Platform.Instance.GetUserCoroutine(friendsData[i].userId.Value, (user) => {
						friend.user = user;
					}));
					// Remove the friend from the main list
					friendsData.RemoveAt(i);
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
						Friend foundFriend = invitedFriends.Find(y => y.uid == friend.uid);
						if(foundFriend == null)
						{
							invitedFriends.Add(friend);
						}

						friendsData.Remove(friend);
					}
				}
			}
		}
		

		// Sort the betaFriends to A-Z by name
		if(betaFriends != null && betaFriends.Count > 1) {
			betaFriends.Sort((t1, t2) => t1.DisplayName.CompareTo(t2.DisplayName));
		}

		// Sort the invitedFriends to A-Z by name
		if(invitedFriends != null && invitedFriends.Count > 1) {
			invitedFriends.Sort((t1, t2) => t1.DisplayName.CompareTo(t2.DisplayName));
		}

		// Sort the main friends list to A-Z by name
		if(friendsData != null && friendsData.Count > 1) {
			friendsData.Sort((t1, t2) => t1.DisplayName.CompareTo(t2.DisplayName));
		}

		loadingFriendsIncomplete = false;
		GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "LoadingTextLabel", "");
		UnityEngine.Debug.Log("LoadFriends() completed");
		yield break;
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

		// Switch the type based on either "challenge" or "friend"
		switch(type) {
		case "challenge":
			// Disable the challenge notification object
			if(challengeNotification != null) {
				challengeNotification.SetActive(false);
			}

			// Toggle visibility
			NGUITools.SetActive(friendsList.gameObject, false);
			NGUITools.SetActive(challengeList.gameObject, true);
            
			// Clear seen new challenges
			if(newChallenges != null && newChallenges.Count > 0)
			{
				User player = Platform.Instance.User();
				foreach(var challenge in newChallenges)
				{
					Notification note = challenge.GetNotification();
					if(note.read)
					{
						if(note.message.from == player.id)
						{
							playerChallenges.Add(challenge);
						}
						else
						{
							incompleteChallenges.Add(challenge);
						}
					}
				}
				newChallenges.RemoveAll(cn => cn.GetRead() == true);
			}

			// Get the challenges
//			if(initialized)
			GetChallenges();

			if (loadingChallengeIncomplete && GetButtonData("challenges").Count == 0) GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "LoadingTextLabel", "Loading challenges");
			else GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "LoadingTextLabel", "");

			racersBtn.isEnabled = true;
			challengeBtn.isEnabled = false;

			break;

		case "friend":
			if (loadingFriendsIncomplete && GetButtonData("friends").Count == 0) GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "LoadingTextLabel", "Loading friends");
			else GameObjectUtils.SetTextOnLabelInChildren(physicalWidgetRoot, "LoadingTextLabel", "");
			
			// If the user has facebook permissions and friends
			bool hasFriends = getHasFriends();

			if(!hasFriends)
			{
				GetFriends();
				hasFriends = getHasFriends();
			}

			if(Platform.Instance.HasPermissions("facebook", "login") && hasFriends) {
				// Reset the mobile list if uninitialized
				if (friendsList.GetItemHeight() != 115) {
					friendsList.ResetList(115f);				
				} else {
					friendsList.ClearList();
				}
				GetButtonData("friends").Clear();
				// If there are friends
				if(betaFriends != null && betaFriends.Count > 0) {
					// Loop through all beta friends
					for(int i=0; i<betaFriends.Count; i++) {
						// Set the button name with the number corresponding to the friend index
						string betaButtonName = "challenge" + i;
						// Create a new dictionary for the friend to set the name
						Dictionary<string, string> betaFriendDictionary = new Dictionary<string, string>();
						betaFriendDictionary.Add("Name", betaFriends[i].DisplayName);
						Dictionary<string, Dictionary<string, string>> betaImageDictionary = new Dictionary<string, Dictionary<string, string>>();
						Dictionary<string, string> innerBetaDictionary = new Dictionary<string, string>();
						innerBetaDictionary.Add("texture", "ChallengePlayerPicture");
						innerBetaDictionary.Add("name", betaButtonName);
						betaImageDictionary.Add(betaFriends[i].image, innerBetaDictionary);
						AddButtonData("friends", betaButtonName, betaFriendDictionary, "", betaImageDictionary, ListButtonData.ButtonFormat.ChallengeButton, GetConnection("ChallengeButton"));
					}
				}

				// If there are friends the player has sent invites to that haven't yet accepted
				if(invitedFriends != null) 
				{
					// Loop through the list
					for(int i=0; i<invitedFriends.Count; i++) 
					{
						// Set the button name with the number corresponding to the friend index
						string invitedButtonName = "invited" + i;
						// Create a new Dictionary for the friend to set the name
						Dictionary<string, string> invitedDictionary = new Dictionary<string, string>();
						invitedDictionary.Add("Name", invitedFriends[i].DisplayName);
						// Add the button to the list
						Dictionary<string, Dictionary<string, string>> invitedImageDictionary = new Dictionary<string, Dictionary<string, string>>();
						Dictionary<string, string> innerInvitedDictionary = new Dictionary<string, string>();
						innerInvitedDictionary.Add("texture", "InvitedProfilePicture");
						innerInvitedDictionary.Add("name", invitedButtonName);
						invitedImageDictionary.Add(invitedFriends[i].image, innerInvitedDictionary);
						AddButtonData("friends", invitedButtonName, invitedDictionary, "", invitedImageDictionary, ListButtonData.ButtonFormat.InvitedButton, null);
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
						friendDictionary.Add("Name", friendsData[i].DisplayName);
						Dictionary<string, Dictionary<string, string>> friendImageDictionary = new Dictionary<string, Dictionary<string, string>>();
						Dictionary<string, string> innerDictionary = new Dictionary<string, string>();
						innerDictionary.Add("texture", "InviteProfilePicture");
						innerDictionary.Add("name", buttonName);
						friendImageDictionary.Add(friendsData[i].image, innerDictionary);
						AddButtonData("friends", buttonName, friendDictionary, "", friendImageDictionary, ListButtonData.ButtonFormat.InviteButton, GetConnection("InviteButton"));
					}
				}

			} else {
				// Add an import friends button
				GetButtonData("friends").Clear();
				friendsList.ResetList(115f);
				if (!loadingFriendsIncomplete) AddButtonData ("friends", "ImportButton", null, "", ListButtonData.ButtonFormat.ImportButton, GetConnection("ImportButton"));
            }
            
            // Toggle visibility
            NGUITools.SetActive(friendsList.gameObject, true);
            NGUITools.SetActive(challengeList.gameObject, false);

			racersBtn.isEnabled = false;
			challengeBtn.isEnabled = true;

			break;

		default:
			UnityEngine.Debug.Log("MobileHomePanel: type not implemented");
			break;
		}
		
	//	racersBtn.UpdateColor(racersBtn.isEnabled, true);
	//	challengeBtn.UpdateColor(challengeBtn.isEnabled, true);
		tab = type;
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
				Hashtable eventProperties = new Hashtable();
				eventProperties.Add("event_name", "invite");
				eventProperties.Add("invite_code", unusedInvite.code);
				eventProperties.Add("provider", "facebook");
				Platform.Instance.LogAnalyticEvent(JsonConvert.SerializeObject(eventProperties));
				string[] toFriend = new string[1];
				toFriend[0] = friendsData[i].uid;
				
				FB.AppRequest("Hello " + friendsData[i].name + ", I have invited you to the Race Yourself mobile app!", toFriend, "", null, null, unusedInvite.code, "", result => {
					if(result.Error != null) {
						UnityEngine.Debug.Log("MobileFriendInvite: FB error - " + result.Error);
					} else {
						var responseObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(result.Text);            
						object obj = 0;
						if(responseObject.TryGetValue("request", out obj))
						{
							int numInvites = invites.FindAll(x => x.used_at == null).Count;
							DataVault.Set ("notification_message", "Invite sent! " + numInvites.ToString() + " invites remaining");
							// Invite will only be linked to the identity after a sync
							unusedInvite.used_at = DateTime.Now;
							InviteAction action = new InviteAction("invite", friendsData[i].provider, friendsData[i].uid, unusedInvite.code);
							Platform.Instance.QueueAction(JsonConvert.SerializeObject(action));
							
							Platform.Instance.SyncToServer();
							GConnector gConnect = Outputs.Find(r => r.Name == "InviteButton");
							parentMachine.FollowConnection(gConnect);
							ChangeList("friend");
						}
						else
						{
							UnityEngine.Debug.LogError("MobileHomePanel - request cancelled");
						}

					}
				});
				return;
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

                    // analytics
                    Hashtable eventProperties = new Hashtable();
                    eventProperties.Add("event_name", "open_challenge");
                    eventProperties.Add("challenge_id", challengeId);
                    eventProperties.Add("challenge_sender", note.message.from);
                    eventProperties.Add("challenge_type", note.message.challenge_type);
                    Platform.Instance.LogAnalyticEvent(JsonConvert.SerializeObject(eventProperties));

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

		Platform.Instance.NetworkMessageListener.onSync -= syncHandler;		
				// Set initialized to false so that it can be re-initialized when going back in the panel
		initialized = false;
	}

}
