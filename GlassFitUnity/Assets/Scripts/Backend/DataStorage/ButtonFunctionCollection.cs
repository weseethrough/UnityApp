using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using RaceYourself;
using RaceYourself.Models;
using Newtonsoft.Json;

/// <summary>
/// Every function in collection have to accept FlowButton and panel variable and return boolean helping to decide if navigation should continue or stop
/// </summary>
public class ButtonFunctionCollection
{
    protected static Log log = new Log("ButtonFunctionCollection");  // for use by subclasses

    /// <summary>
    /// default testing function 
    /// </summary>
    /// <param name="fb"> button providng event </param>
    /// <param name="panel">parent panel of the event/button. You might have events started from panel itself without button involved</param>
    /// <returns> Is button in state to continue? If False is returned button will not navigate forward on its own connection!</returns>
    static public bool MyFunction1(FlowButton fb, FlowState panel)
    {
        Debug.Log("Testing linked function true");
      
        return true;
    }

    /// <summary>
    /// example function which redirects navigation to custom exit named "CustomExit"
    /// </summary>
    /// <param name="fb"> button providng event </param>
    /// <param name="panel">parent panel of the event/button. You might have events started from panel itself without button involved</param>
    /// <returns> Is button in state to continue? If False is returned button will not navigate forward on its own connection!</returns>
    static public bool GoToCustomExit(FlowButton fb, FlowState panel)
    {
        Debug.Log("Testing linked function true");       
        if (panel != null)
        {
            if (panel is HexPanel)
            {
                List<HexButtonData> data = (panel as HexPanel).buttonData;
                HexButtonData buttonData = data.Find(r => r.buttonName == fb.name);
                if (buttonData != null && buttonData.locked == true)
                {
                    GConnector gc = panel.Outputs.Find(r => r.Name == "CustomExit");
                    if (gc != null)
                    {
                        panel.parentMachine.FollowConnection(gc);
                        return false;
                    }
                }                
            }                        
        }
        return true;
    }

    /// <summary>
    /// Check if user is authenticated
    /// </summary>
    /// <param name="fb"> button providng event </param>
    /// <param name="panel">parent panel of the event/button. You might have events started from panel itself without button involved</param>
    /// <returns>always return false blocking further navigation</returns>
    static public bool isAuthent(FlowButton fb, FlowState panel) 
	{
		// Connection followed asynchronously
		return false; 
	}

    static public bool RegisterDevice(FlowButton fb, FlowState panel) 
	{
		
		return false;
	}

    static public bool SetTutorial(FlowButton fb, FlowState panel)
	{
		DataVault.Set("type", "Runner");
		DataVault.Set("race_type", "tutorial");
	
		return true;
	}
	
	static public bool SetQuickRace(FlowButton fb, FlowState panel)
	{
		DataVault.Set("type", "Runner");
		DataVault.Set("race_type", "tutorial");
		DataVault.Set("finish", 1000);
		
		return true;
	}

	static public bool ImportFacebook(FlowButton fb, FlowState panel) 
	{
		NetworkMessageListener.OnAuthenticated networkHandler = null;
		networkHandler = new NetworkMessageListener.OnAuthenticated((authenticated) => {
			if (authenticated && Platform.Instance.HasPermissions("facebook", "login")) {
				NetworkMessageListener.OnSync syncHandler = null;
				syncHandler = new NetworkMessageListener.OnSync((message) => {
					Platform.Instance.NetworkMessageListener.onSync -= syncHandler;
					DataVault.Set("facebook_message", "Facebook sync successful!");
					FlowState.FollowBackLink();
				});
				Platform.Instance.NetworkMessageListener.onSync += syncHandler;
				Platform.Instance.SyncToServer();

				DataVault.Set("facebook_message", "Facebook login successful! Syncing friends...");
			} else {
				DataVault.Set("facebook_message", "Error authorizing Facebook! Tap to try again");
				FlowState fs = FlowStateMachine.GetCurrentFlowState();
				GameObject FacebookButton = GameObjectUtils.SearchTreeByName(((Panel)fs).physicalWidgetRoot, "FacebookButton");
				FacebookButton.GetComponentInChildren<UIButton>().enabled = true;
			}
			Platform.Instance.NetworkMessageListener.onAuthenticated -= networkHandler;
		});
		DataVault.Set ("facebook_message", "Authorizing...");
		FlowState flowState = FlowStateMachine.GetCurrentFlowState();
		GameObject fbButton = GameObjectUtils.SearchTreeByName(((Panel)flowState).physicalWidgetRoot, "FacebookButton");
		fbButton.GetComponentInChildren<UIButton>().enabled = false;
		Platform.Instance.NetworkMessageListener.onAuthenticated += networkHandler;

		Platform.Instance.Authorize("facebook", "login");

		return false;
	}
	
	static public bool SetChallenge(FlowButton fb, FlowState panel)
	{
		List<ChallengeNotification> challenges = (List<ChallengeNotification>)DataVault.Get("challenge_notifications");
		if(challenges != null)
		{
			for(int i=0; i<challenges.Count; i++)
			{
				if(fb.name == challenges[i].GetID())
				{
					UnityEngine.Debug.Log("ButtonFunc: getting track");
					Track track = challenges[i].GetTrack();
					UnityEngine.Debug.Log("ButtonFunc: setting track");
					DataVault.Set("current_track", track);
					DataVault.Set("race_type", "challenge");
					DataVault.Set("challenger", challenges[i].GetName());
					DataVault.Set("current_challenge_notification", challenges[i]);
					return true;
				}
			}
			UnityEngine.Debug.Log("ButtonFunc: challenge not found, returning");
			return false;
		} 
		else {
			UnityEngine.Debug.Log("ButtonFunc: challenges not found");
			return false;
		}
	}

    static public bool SetFriend(FlowButton fb, FlowState panel)
	{
		List<Friend> friendList = (List<Friend>)DataVault.Get("friend_list");
		
		if(friendList != null) {
			int finish = 10000;
			int lowerFinish = 100;
				
			List<Track> trackList = Platform.Instance.GetTracks(finish, lowerFinish);
			if(trackList != null && trackList.Count > 0) {
				for(int i=0; i<friendList.Count; i++)
				{
					if(fb.name == friendList[i].guid)
					{
						DataVault.Set("track_list", trackList);
						DataVault.Set ("chosen_friend", friendList[i]);
						DataVault.Set("finish", finish);
						DataVault.Set("lower_finish", lowerFinish);
						return true;
					}
				}
				UnityEngine.Debug.Log("ButtonFunc: Friend not found, returning");
				return false;
			} else {
				UnityEngine.Debug.Log("ButtonFunc: track list is null, returning");
				MessageWidget.AddMessage("Sorry", "You currently have no tracks", "!none");
				return false;
			}
		} else {
			UnityEngine.Debug.Log("ButtonFunc: friend list is null");
			return false;
		}
	}
	
	/// <summary>
	/// sets type f the challenge based on button which provided event
	/// </summary>
    /// <param name="fb"> button providng event </param>
    /// <param name="panel">parent panel of the event/button. You might have events started from panel itself without button involved</param>
	/// <returns>always return true no matter what type of activity is set</returns>
    static public bool SetType(FlowButton fb, FlowState panel)
	{
		DataVault.Set("current_game_id", fb.name);
		switch(fb.name) 
		{
		case "activity_race_yourself":
			UnityEngine.Debug.Log("ButtonFunc: type is set to race");
			DataVault.Set("type", "Runner");
			DataVault.Set("race_type", "race");
			break;
			
		case "activity_bike":
			DataVault.Set("type", "Cyclist");
			DataVault.Set("race_type", "bike");
			break;
			
		case "activity_boulder":
			DataVault.Set("type", "Boulder");
			DataVault.Set("race_type", "pursuit");
			break;
			
		case "activity_eagle":
			DataVault.Set("type", "Eagle");
			DataVault.Set("race_type", "pursuit");
			break;
			
		case "activity_zombie":
			DataVault.Set("type", "Zombie");
			DataVault.Set("race_type", "pursuit");
			break;
			
		case "activity_train":
			DataVault.Set("type", "Train");
			DataVault.Set("race_type", "trainRescue");
			break;
			
		case "activity_dinosaurs":
			DataVault.Set("type", "Dinosaur");
			DataVault.Set("race_type", "pursuit");
			break;
			
		case "activity_fire":
			DataVault.Set("type", "Fire");
			DataVault.Set("race_type", "pursuit");
			break;
			
		case "activity_food_burn":
			DataVault.Set("type", "Snack");
			DataVault.Set("race_type", "snack");
			break;
			
		}
		
		return true;
	}

    static public bool SetModeDesc(FlowButton fb, FlowState panel) 
	{
		List<Game> games = Platform.Instance.GetGames();
		for(int i=0; i < games.Count; i++)
		{
			if(games[i].iconName == fb.name)
			{
				DataVault.Set("actual_game", games[i]);
				DataVault.Set("game_desc", games[i].description);
				DataVault.Set("game_name", games[i].name);
				DataVault.Set("image_name", games[i].iconName);
				break;
			}
		}
		return true;
	}
	
	/// <summary>
	/// Sets the game description for the locked game.
	/// </summary>
	/// <returns>
	/// Boolean to allow traversal.
	/// </returns>
	/// <param name='fb'>
	/// The button.
	/// </param>
	/// <param name='panel'>
	/// The panel.
	/// </param>
    static public bool SetGameDesc(FlowButton fb, FlowState panel)
	{
		List<Game> games = Platform.Instance.GetGames();
		
		for(int i=0; i < games.Count; i++)
		{
			if(games[i].iconName == fb.name)
			{
				DataVault.Set("actual_game", games[i]);
				DataVault.Set("price_points", "Price in points: " + games[i].priceInPoints);
				DataVault.Set("price_gems", "Price in gems: " + games[i].priceInGems);
				DataVault.Set("game_desc", games[i].description);
				DataVault.Set("game_name", games[i].name);
				DataVault.Set("image_name", games[i].iconName);
				break;
			}
		}
				
		return true;
	}
	
    /// <summary>
    /// loads next game level and initializes game with some custom settings based on button pressed
    /// </summary>
    /// <param name="fb"> button providng event </param>
    /// <param name="panel">parent panel of the event/button. You might have events started from panel itself without button involved</param>
    /// <returns>always allow for further navigation</returns>
    static public bool StartGame(FlowButton fb, FlowState panel)
	{
		AutoFade.LoadLevel("Race Mode", 0.1f, 1.0f, Color.black);
		
		switch(fb.name) 
		{
		case "1km":
			DataVault.Set("finish", 1000);
			break;
			
		case "2km":
			DataVault.Set("finish", 2000);
			break;
			
		case "3km":
			DataVault.Set("finish", 3000);
			break;
			
		case "4km":
			DataVault.Set("finish", 4000);
			break;
			
		case "5km":
			DataVault.Set("finish", 5000);
			break;
			
		case "10km":
			DataVault.Set("finish", 10000);
			break;
		}
		
		return true;
	}

    static public bool CheckTracks(FlowButton fb, FlowState panel) 
	{
		int finish = (int)DataVault.Get("finish") + 500;
		int lowerFinish = (int)DataVault.Get("lower_finish");
		List<Track> trackList = Platform.Instance.GetTracks((double)finish, (double)lowerFinish);
		if(trackList != null) {
			if(trackList.Count == 0) {
				MessageWidget.AddMessage("Sorry!", "You currently have no tracks for this distance", "activity_delete");
				return false;
			} else {
				DataVault.Set("track_list", trackList);
				return true;
			}
		} else {
			MessageWidget.AddMessage("Sorry!", "You currently have no tracks for this distance", "activity_delete");
			return false;
		}
	}

    static public bool StartPresetSpeed(FlowButton fb, FlowState panel)
    {
		//DataVault.Set("current_track", null);
		//AutoFade.LoadLevel("Race Mode", 0.1f, 1.0f, Color.black);
		
		return true;
	}

    static public bool SetMode(FlowButton fb, FlowState panel)
	{
		if(panel is HexPanel)
		{
			HexPanel hex = panel as HexPanel;
			for(int i=0; i<hex.buttonData.Count; i++)
			{
				if(hex.buttonData[i].buttonName == fb.name)
				{
					bool mode = Convert.ToBoolean(DataVault.Get(fb.name));
					UnityEngine.Debug.Log("ButtonFunc: mode value before is " + mode.ToString());
					if(mode)
					{
						hex.buttonData[i].textSmall = "Off";
						if(fb.name == "activity_indoor") {
							UnityEngine.Debug.Log("ButtonFunc: GPS is off (indoor)");
							Platform.Instance.LocalPlayerPosition.SetIndoor(true);
						}
						UnityEngine.Debug.Log("ButtonFunc: mode - " + fb.name + " set to false");
						DataVault.Set(fb.name, false);
					}
					else
					{
						hex.buttonData[i].textSmall = "On";
						if(fb.name == "activity_indoor") {
							UnityEngine.Debug.Log("ButtonFunc: GPS is on (not indoor)");
							Platform.Instance.LocalPlayerPosition.SetIndoor(false);
						}
						UnityEngine.Debug.Log("ButtonFunc: mode - " + fb.name + " set to true");
						DataVault.Set(fb.name, true);
					}
					hex.buttonData[i].markedForVisualRefresh = true;
					DataVault.SaveToBlob();
					DynamicHexList list = (DynamicHexList)hex.physicalWidgetRoot.GetComponentInChildren(typeof(DynamicHexList));
        			list.UpdateButtonList();
					UnityEngine.Debug.Log("ButtonFunc: updated hex list for indoor");
					break;
				}
			}
		}
		return true;
	}

    static public bool SetFinish(FlowButton fb, FlowState panel)
	{
		if((string)DataVault.Get("race_type") == "trainRescue") {
			DataVault.Set("finish", 350);
			DataVault.Set("lower_finish", 300);
			//AutoFade.LoadLevel("TrainRescue", 0f, 1.0f, Color.black);
		}
		DataVault.Remove("current_track");
		switch(fb.name) 
		{
		case "1km":
			DataVault.Set("finish", 1000);
			DataVault.Set("lower_finish", 10);
			break;
			
		case "2km":
			DataVault.Set("finish", 2000);
			DataVault.Set("lower_finish", 1500);
			if((string)DataVault.Get("race_type") == "pursuit") {
				AutoFade.LoadLevel("Pursuit Mode", 0f, 1.0f, Color.black);
			}
			break;
			
		case "3km":
			DataVault.Set("finish", 3000);
			DataVault.Set("lower_finish", 2500);
			if((string)DataVault.Get("race_type") == "pursuit") {
				AutoFade.LoadLevel("Pursuit Mode", 0f, 1.0f, Color.black);
			}
			break;
			
		case "4km":
			DataVault.Set("finish", 4000);
			DataVault.Set("lower_finish", 3500);
			if((string)DataVault.Get("race_type") == "pursuit") {
				AutoFade.LoadLevel("Pursuit Mode", 0f, 1.0f, Color.black);
			}
			break;
			
		case "5km":
			DataVault.Set("finish", 5000);
			DataVault.Set("lower_finish", 4500);
			if((string)DataVault.Get("race_type") == "pursuit") {
				AutoFade.LoadLevel("Pursuit Mode", 0f, 1.0f, Color.black);
			}
			break;
			
		case "10km":
			DataVault.Set("finish", 10000);
			DataVault.Set("lower_finish", 5500);
			if((string)DataVault.Get("race_type") == "pursuit") {
				AutoFade.LoadLevel("Pursuit Mode", 0f, 1.0f, Color.black);
			}
			break;
		}
//		string type = (string)DataVault.Get("race_type");
//		if(type == "pursuit") {
//			AutoFade.LoadLevel("Pursuit Mode", 0f, 1.0f, Color.black);
//		} else if(type == "tutorial") {
//			AutoFade.LoadLevel("FirstRun", 0.1f, 1.0f, Color.black);
//		}
		return true;
	}
	
	static public bool Purchase(FlowButton fb, Panel panel)
	{
		Game current = (Game)DataVault.Get("actual_game");
		UnityEngine.Debug.Log("Purchase: Game bought");
		current.Unlock();
		return true;
	}

//	static public bool SetMode(FlowButton fb, FlowState panel) 
//	{
//		string currentMode = (string)DataVault.Get("game_name");
//		
//		currentMode = currentMode.Replace(" ", "_");
//		
//		UnityEngine.Debug.Log("BFC: Name is: " + currentMode.ToLower());
//		
//		bool setting = (bool)DataVault.Get(currentMode.ToLower());
//		
//		if(setting) {
//			DataVault.Set("active_mode", "Tap to turn on");
//			setting = false;
//			DataVault.Set(currentMode.ToLower(), setting);
//		} else {
//			DataVault.Set("active_mode", "Tap to turn off");
//			setting = true;
//			DataVault.Set(currentMode.ToLower(), setting);
//		}
//		
//		return true;
//	}
	
    /// <summary>
    /// loads next game level and initializes game with some custom settings based on button pressed
    /// </summary>
    /// <param name="fb"> button providng event </param>
    /// <param name="panel">parent panel of the event/button. You might have events started from panel itself without button involved</param>
    /// <returns>always allow for further navigation</returns>
    static public bool StartPursuitGame(FlowButton fb, FlowState panel) 
	{
		AutoFade.LoadLevel("Pursuit Mode", 0f, 1.0f, Color.black);
		
		switch(fb.name) {
		case "1km":
			DataVault.Set("finish", 1000);
			break;
			
		case "2km":
			DataVault.Set("finish", 2000);
			break;
			
		case "3km":
			DataVault.Set("finish", 3000);
			break;
			
		case "4km":
			DataVault.Set("finish", 4000);
			break;
			
		case "5km":
			DataVault.Set("finish", 5000);
			break;
			
		case "10km":
			DataVault.Set("finish", 10000);
			break;
		}
		return true;
	}

    /// <summary>
    /// sets pursuit game mode to indoor mode
    /// </summary>
    /// <param name="fb"> button providng event </param>
    /// <param name="panel">parent panel of the event/button. You might have events started from panel itself without button involved</param>
    /// <returns>never allow further navigation</returns>
//	static public bool SetIndoor(FlowButton fb, FlowState panel)
//	{
////		PursuitGame ps = (PursuitGame) GameObject.FindObjectOfType(typeof(PursuitGame));
////		ps.SetIndoor();
//		bool indoor = Platform.Instance.LocalPlayerPosition.IsIndoor();
//		Platform.Instance.LocalPlayerPosition.SetIndoor(!indoor);
//		return false;
//	}

    static public bool DemoLeaderBoard(FlowButton fb, FlowState panel)
	{
		// Reset world
		Platform.Instance.ResetTargets();
		DataVault.Remove("challenges");
		
		DataVault.Set("finish", 1000);
		Platform.Instance.CreateTargetTracker(2.01f);
		Platform.Instance.CreateTargetTracker(1.9f);
		Platform.Instance.CreateTargetTracker(2.2f);
		
		AutoFade.LoadLevel("Race Mode", 0.1f, 1.0f, Color.black);
		
		return true;
	}
	
 

//    /// <summary>
//    /// resets pursuit game (todo: confirmation this is not clear)
//    /// </summary>
//    /// <param name="fb"> button providng event </param>
//    /// <param name="panel">parent panel of the event/button. You might have events started from panel itself without button involved</param>
//    /// <returns>always allow further navigation</returns>
//	static public bool BackPursuit(FlowButton fb, FlowState panel)
//	{
//		PursuitGame ps = (PursuitGame) GameObject.FindObjectOfType(typeof(PursuitGame));
//		ps.Back();
//		GameObject h = GameObject.Find("blackPlane");
//		if(h != null) {
//			h.renderer.enabled = false;
//		}
//		return true;
//	}

    /// <summary>
    /// leaves game state and resets platform instance
    /// </summary>
    /// <param name="fb"> button providng event </param>
    /// <param name="panel">parent panel of the event/button. You might have events started from panel itself without button involved</param>
    /// <returns>always allow further navigation</returns>
    static public bool EndGame(FlowButton fb, FlowState panel)
	{
		Debug.Log("EndGame called");
		Track track = Platform.Instance.LocalPlayerPosition.StopTrack();			
		if (track != null) DataVault.Set("track", track);
		else DataVault.Remove("track");		
		
		DataVault.Set("total", Platform.Instance.PlayerPoints.CurrentActivityPoints + Platform.Instance.PlayerPoints.OpeningPointsBalance);
		DataVault.Set("bonus", 0);
		
		//Platform.Instance.LocalPlayerPosition.Reset();
		Platform.Instance.ResetTargets();
		AutoFade.LoadLevel("Game End", 0f, 1.0f, Color.black);

		// TODO: Disable share and challenge hexes if (track == 0 || track.tracPositoons.Count == 0)

		// Log attempts
		List<Challenge> challenges = DataVault.Get("challenges") as List<Challenge>;		
		if (challenges != null && challenges.Count > 0) {
			double? distance = DataVault.Get("rawdistance") as double?;
			long? time = DataVault.Get("rawtime") as long?;
			Notification[] notifications = Platform.Instance.Notifications();
			
			if (track != null && track.positions.Count > 0) {
				User me = Platform.Instance.User();
				foreach (Challenge generic in challenges) {
					if (generic is DistanceChallenge) {
						DistanceChallenge challenge = generic as DistanceChallenge;
						
						Platform.Instance.QueueAction(string.Format(@"{{
							'action' : 'challenge_attempt',
							'challenge_id' : {0},
							'track_id' : [ {1}, {2} ]
						}}", challenge.id, track.deviceId, track.trackId).Replace("'", "\""));
						Debug.Log ("Challenge: attempt " + track.deviceId + "-" + track.trackId + " logged for " + challenge.id);
					}
					// Mark challenge notification as handled and challenge back
					foreach (Notification notification in notifications) {
						if (notification.read) continue;
						if (string.Equals(notification.message.type, "challenge")) {
							string challengeId = notification.message.challenge_id;
							if (challengeId == null || challengeId.Length == 0) continue;
//							if (challengeId.Contains("$oid")) challengeId = notification.node["challenge_id"]["$oid"].ToString();
//							challengeId = challengeId.Replace("\"", "");
//							Debug.Log(challengeId + " vs " + generic.id);
							// TODO: Standardize oids
							if (!challengeId.Equals(generic.id)) continue;
							
							Platform.Instance.ReadNotification(notification.id);
							
							int challengerId = notification.message.from;
							if (challengerId == 0) continue;
							if (me != null && me.id == challengerId) continue;
							// Challenge challenger back
							Platform.Instance.QueueAction(string.Format(@"{{
								'action' : 'challenge',
								'target' : {0},
								'taunt' : 'Defend thy honour, scoundrel!',
								'challenge' : {{
                                        '_id' : {1}
								}}
							}}", challengerId, challengeId).Replace("'", "\""));
						}
					}
				}
				Platform.Instance.SyncToServer();					
			}
		}		
		
		return true;
	}

    /// <summary>
    /// clears back functionality
    /// </summary>
    /// <param name="fb"> button providng event </param>
    /// <param name="panel">parent panel of the event/button. You might have events started from panel itself without button involved</param>
    /// <returns>always allow further navigation</returns>
    static public bool ClearHistory(FlowButton fb, FlowState panel)
	{
		panel.parentMachine.ForbidBack();
		return true;
	}


    /// <summary>
    /// Function which allows to attach generic "go back" functionality to buttons
    /// </summary>
    /// <param name="fb"> button providng event </param>
    /// <param name="panel">parent panel of the event/button. You might have events started from panel itself without button involved</param>
    /// <returns>Never allows to use connection navigation. Instead it uses state machine back navigation</returns>
    static public bool FollowBack(FlowButton fb, FlowState panel)
    {        

        bool backSucceed = panel.parentMachine.FollowBack();
        Debug.Log("Back succeed: " + backSucceed);
        return false;
    }

    /// <summary>
    /// sets friend name in database from button name, authorize friend provider and asynchronously follow button connection
    /// </summary>
    /// <param name="fb"> button providng event </param>
    /// <param name="panel">parent panel of the event/button. You might have events started from panel itself without button involved</param>
    /// <returns>allow further navigation if friend provider authorized</returns>
    static public bool SetFriendType(FlowButton fb, FlowState panel) 
	{
		DataVault.Set("friend_type", fb.name);
			
		if (Platform.Instance.HasPermissions(fb.name.ToLower(), "login")) {
			// TODO: Trigger friends list refresh without authorization?
			return true;
		}
		
        GConnector gConect = panel.Outputs.Find(r => r.Name == fb.name);
		// Follow connection once authentication has returned asynchronously
        NetworkMessageListener.OnAuthenticated handler = null;
        handler = new NetworkMessageListener.OnAuthenticated((authenticated) => {
			if (authenticated) {
				Platform.Instance.SyncToServer();
				panel.parentMachine.FollowConnection(gConect);
			}
            Platform.Instance.NetworkMessageListener.onAuthenticated -= handler;
		});
        Platform.Instance.NetworkMessageListener.onAuthenticated += handler;	
		
		Platform.Instance.Authorize(fb.name.ToLower(), "login");
		
		return false;
	}

    static public bool AuthenticateUser(FlowButton fb, FlowState panel)
    {
		GConnector gConect = panel.Outputs.Find(r => r.Name == fb.name);
		// Follow connection once authentication has returned asynchronously
        NetworkMessageListener.OnAuthenticated handler = null;
        handler = new NetworkMessageListener.OnAuthenticated((authenticated) => {
			if (authenticated) {
				//Platform.Instance.SyncToServer();
				panel.parentMachine.FollowConnection(gConect);
			}
            Platform.Instance.NetworkMessageListener.onAuthenticated -= handler;
		});
        Platform.Instance.NetworkMessageListener.onAuthenticated += handler;	
		
		Platform.Instance.Authorize("any", "login");
		
		return false;
	}

    static public bool ShareTrack(FlowButton button, FlowState panel)
	{
        NetworkMessageListener.OnAuthenticated handler = null;
        handler = new NetworkMessageListener.OnAuthenticated((authenticated) => {
			if (authenticated) {
				Track track = DataVault.Get("track") as Track;
				Platform.Instance.QueueAction(string.Format(@"{{
					'action' : 'share',
					'provider' : 'facebook',
					'message' : 'Dummy message',
					'track' : [{0}, {1}]
				}}", track.deviceId, track.trackId).Replace("'", "\""));		
				Debug.Log ("Track: [" + track.deviceId + "," + track.trackId + "] shared to Facebook");
				MessageWidget.AddMessage("Share", "Track shared to Facebook", "settings");
			}
            Platform.Instance.NetworkMessageListener.onAuthenticated -= handler;
		});
        Platform.Instance.NetworkMessageListener.onAuthenticated += handler;	
		Platform.Instance.Authorize("facebook", "share");
		return false;
	}

    static public bool ChallengeFriend(FlowButton button, FlowState panel)
	{
		Track track = DataVault.Get("current_track") as Track;
		if(track == null) {
			MessageWidget.AddMessage("Error", "Problem with track", "!none");
			UnityEngine.Debug.Log("ButtonFunc: error getting track");
			return false;
		}
		Friend friend = DataVault.Get("chosen_friend") as Friend;
		if(friend == null)
		{
			MessageWidget.AddMessage("Error", "Friend is null", "!none");
			UnityEngine.Debug.Log("ButtonFunc: error getting friend");
			return false;
		}
		
		string friendUid = friend.uid;
		if (friend.userId.HasValue) friendUid = friend.userId.Value.ToString();

		Platform.Instance.QueueAction(string.Format(@"{{
			'action' : 'challenge',
			'target' : {0},
			'taunt' : 'Try beating my track!',
			'challenge' : {{
					'distance': {1},
					'time': {2},
					'location': null,
					'public': true,
					'start_time': null,
					'stop_time': null,
					'type': 'distance',
					'attempts' : [
                        {{  'device_id': {3}, 'track_id': {4} }}
                    ]
			}}
		}}", friendUid, track.distance, track.time, track.deviceId, track.trackId).Replace("'", "\""));
		Debug.Log ("ButtonFunc: " + friendUid + " challenged");
		MessageWidget.AddMessage("Challenge", "You challenged " + friend.name, "settings");
		Platform.Instance.SyncToServer();
		if (Platform.Instance.connected && friend.userId.HasValue) Platform.Instance.MessageUser(friend.userId.Value, "Respond to my challenge, scoundrel!");
			
		return true;
	}
	
    /// <summary>
    /// sends asynchronous challenge to other registered user
    /// </summary>
    /// <param name="fb"> button providng event </param>
    /// <param name="panel">parent panel of the event/button. You might have events started from panel itself without button involved</param>
    /// <returns>always allow further navigation</returns>
    static public bool Challenge(FlowButton button, FlowState panel)
    {
		Track track = DataVault.Get("track") as Track;
		if (track == null) {
			MessageWidget.AddMessage("ERROR", "Can't challenge with null track", "settings");
			return false; // TODO: Allow solo rounds?
		}
		if (track.positions.Count == 0) {
			MessageWidget.AddMessage("ERROR", "Can't challenge with empty track " + track.deviceId + "-" + track.trackId, "settings");
			return false; // TODO: Remove track?		
		}
		Friend friend = DataVault.Get("current_friend") as Friend;
		if (friend == null ) {
			MessageWidget.AddMessage("ERROR", "Can't challenge null friend", "settings");
			return false; // TODO: Challenge by third-party identity
		}
		string friendUid = friend.uid;
		if (friend.userId.HasValue) friendUid = friend.userId.Value.ToString();
		
		Platform.Instance.QueueAction(string.Format(@"{{
			'action' : 'challenge',
			'target' : {0},
			'taunt' : 'Your mother is a hamster!',
			'challenge' : {{
					'distance': 1000,
					'time': 42,
					'location': null,
					'public': true,
					'start_time': null,
					'stop_time': null,
					'type': 'distance',
					'attempts' : [
                        {{  'device_id': {1}, 'track_id': {2} }}
                    ]
			}}
		}}", friendUid, track.deviceId, track.trackId).Replace("'", "\""));
		Debug.Log ("Challenge: " + friendUid + " challenged");
		MessageWidget.AddMessage("Challenge", "You challenged " + friendUid, "settings");
		Platform.Instance.SyncToServer();
		if (Platform.Instance.connected && friend.userId.HasValue) Platform.Instance.MessageUser(friend.userId.Value, "I bite my thumb at thee, sir!");
		
		return true;
	}
	
	
    /// <summary>
    /// accept all distance challenges and start a race
    /// </summary>
    /// <param name="fb"> button providng event </param>
    /// <param name="panel">parent panel of the event/button. You might have events started from panel itself without button involved</param>
    /// <returns>never allow further navigation</returns>
    static public bool AcceptChallenges(FlowButton button, FlowState panel) 
	{
		MessageWidget.AddMessage("ERROR", "Deprecated. Stop using and remove once all links removed.", "settings");		
		return false;
	}	
	
	/// <summary>
	/// Enables the flow to go straight to game if we are in the editor
	/// </summary>
    static public bool GoStraightToGameInEditor(FlowButton button, FlowState panel)
	{
		if(Application.isEditor)
		{
			//go straight to the editor, by following the 'StraightToGame' exit
			if(panel != null)
			{
				GConnector gc = panel.Outputs.Find((r) => r.Name == "StraightToGame");
                if (gc != null)
                {
					Debug.Log("Found connection");
                    panel.parentMachine.FollowConnection(gc);
                    return false;
                }
			}
			return false;	
		}
		else
		{
			return true;
		}
	}

    /// <summary>
    /// Development function which is designed for test purposes only. Adding new buttons to currently calling panel
    /// </summary>
    /// <param name="button"></param>
    /// <param name="panel"></param>
    /// <returns></returns>
    static public bool AddButton(FlowButton button, FlowState panel)
    {
        if (panel != null)
        {
            HexPanel hp = panel as HexPanel;
            if (hp != null)
            {
                HexButtonData hbd = new HexButtonData();
                hbd.row = -1;
                hbd.column = 0;
                hbd.buttonName = "testButton1";
                hbd.displayInfoData = false;
                hbd.textNormal = "Go to point 1";

                hp.buttonData.Add(hbd);
            }
        }
        return false;
    }

    /// <summary>
    /// Example flow navigation method 1
    /// </summary>
    /// <param name="button"></param>
    /// <param name="panel"></param>
    /// <returns></returns>
    static public bool GoToFlow1(FlowButton button, FlowState panel)
    {
        GraphComponent gc = GameObject.FindObjectOfType(typeof(GraphComponent)) as GraphComponent;
        gc.GoToFlow("Flow1");
        return false;
    }

    /// <summary>
    /// Example flow navigation method 2
    /// </summary>
    /// <param name="button"></param>
    /// <param name="panel"></param>
    /// <returns></returns>
    static public bool GoToFlow2(FlowButton button, FlowState panel)
    {
        GraphComponent gc = GameObject.FindObjectOfType(typeof(GraphComponent)) as GraphComponent;
        gc.GoToFlow("Flow2");
        return false;
    }

    static public bool SetChosenActivitySpriteName(FlowButton button, FlowState panel)
	{
		UnityEngine.Debug.Log("Game Intro: setting game id to: " + button.name);
		
		//assuming button name is always the same as the sprite name...
		DataVault.Set("current_game_id", button.name);
		
		//follow link as normal
		return true;
	}

    static public bool UseCustomRedirection(FlowButton button, FlowState panel)
    {
		UnityEngine.Debug.Log("BFC: about to get start string");
        string start = DataVault.Get("custom_redirection_point") as string;
        if (start != null && start.Length > 0)
        {
			UnityEngine.Debug.Log("BFC: the direction point says " + start);
            GConnector newExit = panel.Outputs.Find(r => r.Name == start);
            if (newExit != null)
            {
                //clear redirection target, next time passing this switch we will chose default behavior unless value get restored by external systems
                DataVault.Set("custom_redirection_point", "");

                //navigate using new connection.
                panel.parentMachine.FollowConnection(newExit);
                return false;
            }
        }
        return true;
    }
	
	static public bool GoToEndGameFlow(FlowButton button, FlowState panel)
	{
		GraphComponent gc = GameObject.FindObjectOfType(typeof(GraphComponent)) as GraphComponent;
		if(Convert.ToBoolean(DataVault.Get("is_testing"))) {
			DataVault.Set("custom_redirection_point", "ChallengePoint");
		} else {
			DataVault.Set("custom_redirection_point", "MenuPoint");
		}

		if(Platform.Instance.OnGlass())
		{
			gc.GoToFlow("MainFlow");
		}
		else
		{
			gc.GoToFlow("MobileUX");
		}
		return true;
	}
	
	static public bool SetHUD(FlowButton button, FlowState panel)
	{
		if(Platform.Instance.OnGlass())
		{
			UnityEngine.Debug.Log("BFC: exit being set to GlassExit");
			DataVault.Set("custom_redirection_point", "GlassExit");
		}
		else
		{
			UnityEngine.Debug.Log("BFC: exit being set to MobileExit");
			DataVault.Set("custom_redirection_point", "MobileExit");
		}
		return true;
    }

    static public bool FacebookLogin(FlowButton button, FlowState panel)
    {
        // TODO if FB != null && FB.IsLoggedIn, hide button
        try
        {
            FB.Login("", FacebookLoginCallback); // "" = zero permissions
            return true;
        }
        catch (Exception e) {
            // TODO show error to user
            DataVault.Set("facebook_error", e.Message);
            log.error("Facebook: error:\n" + e.Message);
            return false;
        }
    }
    
    private static void FacebookLoginCallback(FBResult result)
    {
        if (result.Error != null)
        {
            log.error("Facebook: Error Response:\n" + result.Error);
            DataVault.Set("facebook_error", result.Error);
        }
        else if (!FB.IsLoggedIn)
        {
            log.info("Facebook: Login cancelled by player");
        }
        else
        {
            log.info("Facebook: Login was successful! " + FB.UserId + " " + FB.AccessToken);
            try
            {
                FB.API("/me", Facebook.HttpMethod.GET, FacebookMeCallback);
            }
            catch (Exception e)
            {
                DataVault.Set("facebook_error", e.Message);
            }
        }
    }
    
    private static void FacebookMeCallback(FBResult result)
    {
        if (result.Error == null)
        {
            FacebookMe me = JsonConvert.DeserializeObject<FacebookMe>(result.Text);
            log.info("Facebook me: " + result.Text);

            Panel signupPanel = (Panel) FlowStateMachine.GetCurrentFlowState();
            GameObject widgetRoot = signupPanel.physicalWidgetRoot;

            UpdateLabel (widgetRoot, "ForenameInput", me.first_name);
            UpdateLabel (widgetRoot, "SurnameInput", me.last_name);
            UpdateLabel (widgetRoot, "EmailInput", me.email);
        }
    }

    private static void UpdateLabel(GameObject widgetRoot, string inputName, string value)
    {
        GameObject inputField = GameObjectUtils.SearchTreeByName(widgetRoot, inputName);
        if (inputField == null)
        {
            log.error("Unable to find " + inputName + " field in mobile signup panel!");
        }
        else
        {
            inputField.GetComponent<UIBasiclabel>().SetLabel(value == null ? "" : value);
        }
    }
}
