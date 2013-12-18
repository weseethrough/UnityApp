using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

/// <summary>
/// Every function in collection have to accept FlowButton and panel variable and return boolean helping to decide if navigation should continue or stop
/// </summary>
public class ButtonFunctionCollection  
{
   
    /// <summary>
    /// default testing function 
    /// </summary>
    /// <param name="fb"> button providng event </param>
    /// <param name="panel">parent panel of the event/button. You might have events started from panel itself without button involved</param>
    /// <returns> Is button in state to continue? If False is returned button will not navigate forward on its own connection!</returns>
    static public bool MyFunction1(FlowButton fb, Panel panel)
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
    static public bool GoToCustomExit(FlowButton fb, Panel panel)
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
    static public bool isAuthent(FlowButton fb, Panel panel) 
	{
		// Connection followed asynchronously
		return false; 
	}
	
	static public bool RegisterDevice(FlowButton fb, Panel panel) 
	{
		
		return false;
	}
	
	static public bool SetCeleb(FlowButton fb, Panel panel)
	{
		switch(fb.name)
		{
		case "activity_farah":
			DataVault.Set("type", "Mo");
			DataVault.Set("finish", 10000);
			AutoFade.LoadLevel("Race Mode", 0.1f, 1.0f, Color.black);
			break;
			
		case "activity_paula_radcliffe":
			DataVault.Set("type", "Paula");
			DataVault.Set("finish", 42195);
			AutoFade.LoadLevel("Race Mode", 0.1f, 1.0f, Color.black);
			break;
			
		case "activity_chris_hoy":
			DataVault.Set("type", "Chris");
			DataVault.Set("finish", 1000);
			AutoFade.LoadLevel("Race Mode", 0.1f, 1.0f, Color.black);
			break;
			
		case "activity_bradley_wiggins":
			DataVault.Set("type", "Bradley");
			DataVault.Set("finish", 4000);
			AutoFade.LoadLevel("Race Mode", 0.1f, 0.1f, Color.black);
			break;
			
		default:
			return false;
			break;
		}
		
		return true;
	}
	
	/// <summary>
	/// sets type f the challenge based on button which provided event
	/// </summary>
    /// <param name="fb"> button providng event </param>
    /// <param name="panel">parent panel of the event/button. You might have events started from panel itself without button involved</param>
	/// <returns>always return true no matter what type of activity is set</returns>
	static public bool SetType(FlowButton fb, Panel panel)
	{
		switch(fb.name) 
		{
		case "activity_run":
			DataVault.Set("type", "Runner");
			break;
			
		case "activity_bike":
			DataVault.Set("type", "Cyclist");
			break;
			
		case "activity_boulder":
			DataVault.Set("type", "Boulder");
			break;
			
		case "activity_eagle":
			DataVault.Set("type", "Eagle");
			break;
			
		case "activity_zombie":
			DataVault.Set("type", "Zombie");
			break;
			
		case "activity_train":
			DataVault.Set("type", "Train");
			break;
			
		case "activity_dinosaurs":
			DataVault.Set("type", "Dinosaur");
			break;
			
		case "activity_fire":
			DataVault.Set("type", "Fire");
			break;
		}
		
		return true;
	}
	
	static public bool SetModeDesc(FlowButton fb, Panel panel) 
	{
		#if !UNITY_EDITOR
		List<Game> games = Platform.Instance.GetGames();
#else
		List<Game> games = PlatformDummy.Instance.GetGames();
		return false;
#endif
		for(int i=0; i < games.Count; i++)
		{
			if(games[i].name == fb.name)
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
	static public bool SetGameDesc(FlowButton fb, Panel panel)
	{
#if !UNITY_EDITOR
		List<Game> games = Platform.Instance.GetGames();
#else
		List<Game> games = PlatformDummy.Instance.GetGames();
#endif
		
		for(int i=0; i < games.Count; i++)
		{
			if(games[i].name == fb.name)
			{
				DataVault.Set("actual_game", games[i]);
				DataVault.Set("price_points", "Price in points: " + games[i].priceInPoints);
				DataVault.Set("price_gems", "Price in gems: " + games[i].priceInGems);
				DataVault.Set("game_desc", games[i].description);
				DataVault.Set("game_name", games[i].gameId);
				DataVault.Set("image_name", games[i].name);
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
    static public bool StartGame(FlowButton fb, Panel panel)
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
		}
		
		return true;
	}
	
	static public bool Purchase(FlowButton fb, Panel panel)
	{
		Game current = (Game)DataVault.Get("actual_game");
		UnityEngine.Debug.Log("Purchase: Game bought");
		current.Unlock();
		return true;
	}
	
	static public bool SetMode(FlowButton fb, Panel panel) 
	{
		string currentMode = (string)DataVault.Get("game_name");
		
		currentMode = currentMode.Replace(" ", "_");
		
		UnityEngine.Debug.Log("BFC: Name is: " + currentMode.ToLower());
		
		bool setting = (bool)DataVault.Get(currentMode.ToLower());
		
//		if(currentMode == "Rearview") 
//		{
//			bool rearview = (bool)DataVault.Get("rearview");
//			if(rearview) {
//				DataVault.Set("active_mode", "Press to turn on");
//				rearview = false;
//				DataVault.Set("rearview", rearview);
//			} else {
//				DataVault.Set("active_mode", "Press to turn off");
//				rearview = true;
//				DataVault.Set("rearview", rearview);
//			}
//		} else if(currentMode == "Settings") 
//		{
//			bool indoor = (bool)DataVault.Get("indoor");
//			if(indoor) {
//				UnityEngine.Debug.Log("Button: Indoor turning false now");
//				DataVault.Set("active_mode", "Tap to turn on");
//				indoor = false;
//				DataVault.Set("indoor", indoor);
//			} else 
//			{
//				UnityEngine.Debug.Log("Button: Indoor turning true now"); 
//				DataVault.Set("active_mode", "Tap to turn off");
//				indoor = true;
//				DataVault.Set("indoor", indoor);
//			}
//		}
		if(setting) {
			DataVault.Set("active_mode", "Tap to turn on");
			setting = false;
			DataVault.Set(currentMode.ToLower(), setting);
		} else {
			DataVault.Set("active_mode", "Tap to turn off");
			setting = true;
			DataVault.Set(currentMode.ToLower(), setting);
		}
		
		return true;
	}
	
    /// <summary>
    /// loads next game level and initializes game with some custom settings based on button pressed
    /// </summary>
    /// <param name="fb"> button providng event </param>
    /// <param name="panel">parent panel of the event/button. You might have events started from panel itself without button involved</param>
    /// <returns>always allow for further navigation</returns>
	static public bool StartPursuitGame(FlowButton fb, Panel panel) 
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
		}
		return true;
	}

    /// <summary>
    /// sets pursuit game mode to indoor mode
    /// </summary>
    /// <param name="fb"> button providng event </param>
    /// <param name="panel">parent panel of the event/button. You might have events started from panel itself without button involved</param>
    /// <returns>never allow further navigation</returns>
	static public bool SetIndoor(FlowButton fb, Panel panel)
	{
		PursuitGame ps = (PursuitGame) GameObject.FindObjectOfType(typeof(PursuitGame));
		ps.SetIndoor();
		return false;
	}

    /// <summary>
    /// sets pursuit game mode to eagle mode
    /// </summary>
    /// <param name="fb"> button providng event </param>
    /// <param name="panel">parent panel of the event/button. You might have events started from panel itself without button involved</param>
    /// <returns>never allow further navigation</returns>
	static public bool ChangeToEagle(FlowButton fb, Panel panel)
	{
		PursuitGame ps = (PursuitGame) GameObject.FindObjectOfType(typeof(PursuitGame));
		ps.SetActorType(PursuitGame.ActorType.Eagle);
		return false;
	}
	
	static public bool DemoLeaderBoard(FlowButton fb, Panel panel)
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
	
    /// <summary>
    /// sets pursuit game mode to zombie mode
    /// </summary>
    /// <param name="fb"> button providng event </param>
    /// <param name="panel">parent panel of the event/button. You might have events started from panel itself without button involved</param>
    /// <returns>never allow further navigation</returns>
	static public bool ChangeToZombie(FlowButton fb, Panel panel)
	{
		PursuitGame ps = (PursuitGame) GameObject.FindObjectOfType(typeof(PursuitGame));
		ps.SetActorType(PursuitGame.ActorType.Zombie);
		return false;
	}

    /// <summary>
    /// sets pursuit game mode to train mode
    /// </summary>
    /// <param name="fb"> button providng event </param>
    /// <param name="panel">parent panel of the event/button. You might have events started from panel itself without button involved</param>
    /// <returns>never allow further navigation</returns>
	static public bool ChangeToTrain(FlowButton fb, Panel panel)
	{
		PursuitGame ps = (PursuitGame) GameObject.FindObjectOfType(typeof(PursuitGame));
		ps.SetActorType(PursuitGame.ActorType.Train);
		return false;
	}

    /// <summary>
    /// sets pursuit game mode to boulder mode
    /// </summary>
    /// <param name="fb"> button providng event </param>
    /// <param name="panel">parent panel of the event/button. You might have events started from panel itself without button involved</param>
    /// <returns>never allow further navigation</returns>
	static public bool ChangeToBoulder(FlowButton fb, Panel panel)
	{
		PursuitGame ps = (PursuitGame) GameObject.FindObjectOfType(typeof(PursuitGame));
		ps.SetActorType(PursuitGame.ActorType.Boulder);
		return false;
	}

    /// <summary>
    /// resets pursuit game (todo: confirmation this is not clear)
    /// </summary>
    /// <param name="fb"> button providng event </param>
    /// <param name="panel">parent panel of the event/button. You might have events started from panel itself without button involved</param>
    /// <returns>always allow further navigation</returns>
	static public bool BackPursuit(FlowButton fb, Panel panel)
	{
		PursuitGame ps = (PursuitGame) GameObject.FindObjectOfType(typeof(PursuitGame));
		ps.Back();
		GameObject h = GameObject.Find("blackPlane");
		h.renderer.enabled = false;
		return true;
	}

    /// <summary>
    /// leaves game state and resets platform instance
    /// </summary>
    /// <param name="fb"> button providng event </param>
    /// <param name="panel">parent panel of the event/button. You might have events started from panel itself without button involved</param>
    /// <returns>always allow further navigation</returns>
	static public bool EndGame(FlowButton fb, Panel panel)
	{
		Debug.Log("EndGame called");
		Track track = Platform.Instance.StopTrack();			
		if (track != null) DataVault.Set("track", track);
		else DataVault.Remove("track");		
		
		DataVault.Set("total", Platform.Instance.GetCurrentPoints() + Platform.Instance.GetOpeningPointsBalance());
		DataVault.Set("bonus", 0);
		
		//Platform.Instance.Reset();
		Platform.Instance.ResetTargets();
		AutoFade.LoadLevel("Game End", 0f, 1.0f, Color.black);

		// TODO: Disable share and challenge hexes if (track == 0 || track.tracPositoons.Count == 0)

		// Log attempts
		List<Challenge> challenges = DataVault.Get("challenges") as List<Challenge>;		
		if (challenges != null && challenges.Count > 0) {
			double? distance = DataVault.Get("rawdistance") as double?;
			long? time = DataVault.Get("rawtime") as long?;
			Notification[] notifications = Platform.Instance.Notifications();
			
			if (track != null && track.trackPositions.Count > 0) {
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
						if (string.Equals(notification.node["type"], "challenge")) {
							string challengeId = notification.node["challenge_id"].ToString();
							if (challengeId == null || challengeId.Length == 0) continue;
//							if (challengeId.Contains("$oid")) challengeId = notification.node["challenge_id"]["$oid"].ToString();
//							challengeId = challengeId.Replace("\"", "");
//							Debug.Log(challengeId + " vs " + generic.id);
							// TODO: Standardize oids
							if (!challengeId.Equals(generic.id)) continue;
							
							notification.setRead(true);
							
							int challengerId = notification.node["from"].AsInt;
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
	static public bool ClearHistory(FlowButton fb, Panel panel)
	{
		panel.parentMachine.ForbidBack();
		return true;
	}
	

    /// <summary>
    /// sets friend name in database from button name, authorize friend provider and asynchronously follow button connection
    /// </summary>
    /// <param name="fb"> button providng event </param>
    /// <param name="panel">parent panel of the event/button. You might have events started from panel itself without button involved</param>
    /// <returns>allow further navigation if friend provider authorized</returns>
	static public bool SetFriendType(FlowButton fb, Panel panel) 
	{
		DataVault.Set("friend_type", fb.name);
			
		if (Platform.Instance.HasPermissions(fb.name.ToLower(), "login")) {
			// TODO: Trigger friends list refresh without authorization?
			return true;
		}
		
        GConnector gConect = panel.Outputs.Find(r => r.Name == fb.name);
		// Follow connection once authentication has returned asynchronously
		Platform.OnAuthenticated handler = null;
		handler = new Platform.OnAuthenticated((authenticated) => {
			if (authenticated) {
				Platform.Instance.SyncToServer();
				panel.parentMachine.FollowConnection(gConect);
			}
			Platform.Instance.onAuthenticated -= handler;
		});
		Platform.Instance.onAuthenticated += handler;	
		
		Platform.Instance.Authorize(fb.name.ToLower(), "login");
		
		return false;
	}
	
	static public bool AuthenticateUser(FlowButton fb, Panel panel) {
		GConnector gConect = panel.Outputs.Find(r => r.Name == fb.name);
		// Follow connection once authentication has returned asynchronously
		Platform.OnAuthenticated handler = null;
		handler = new Platform.OnAuthenticated((authenticated) => {
			if (authenticated) {
				//Platform.Instance.SyncToServer();
				panel.parentMachine.FollowConnection(gConect);
			}
			Platform.Instance.onAuthenticated -= handler;
		});
		Platform.Instance.onAuthenticated += handler;	
		
		Platform.Instance.Authorize("any", "login");
		
		return false;
	}
	
	static public bool ShareTrack(FlowButton button, Panel panel)
	{
		Platform.OnAuthenticated handler = null;
		handler = new Platform.OnAuthenticated((authenticated) => {
			if (authenticated) {
				Track track = DataVault.Get("track") as Track;
				Platform.Instance.QueueAction(string.Format(@"{{
					'action' : 'share',
					'provider' : 'facebook',
					'message' : 'Dummy message',
					'track' : [{0}, {1}]
				}}", track.deviceId, track.trackId).Replace("'", "\""));		
				Debug.Log ("Track: [" + track.deviceId + "," + track.trackId + "] shared to Facebook");
			}
			Platform.Instance.onAuthenticated -= handler;
		});
		Platform.Instance.onAuthenticated += handler;	
		Platform.Instance.Authorize("facebook", "share");
		return false;
	}
	
    /// <summary>
    /// sends asynchronous challenge to other registered user
    /// </summary>
    /// <param name="fb"> button providng event </param>
    /// <param name="panel">parent panel of the event/button. You might have events started from panel itself without button involved</param>
    /// <returns>always allow further navigation</returns>
    static public bool Challenge(FlowButton button, Panel panel)
    {
		Track track = DataVault.Get("track") as Track;
		if (track == null) {
			Platform.Instance.message = "Can't challenge with null track";			
			return false; // TODO: Allow solo rounds?
		}
		if (track.trackPositions.Count == 0) {
			Platform.Instance.message = "Can't challenge with empty track " + track.deviceId + "-" + track.trackId;			
			return false; // TODO: Remove track?		
		}
		Friend friend = DataVault.Get("current_friend") as Friend;
		if (friend == null ) {
			Platform.Instance.message = "Can't challenge with null friend";			
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
		// TODO: Real message
		Platform.Instance.message = "You challenged " + friendUid;
		Platform.Instance.SyncToServer();
		
		return true;
	}
	
	
    /// <summary>
    /// accept all distance challenges and start a race
    /// </summary>
    /// <param name="fb"> button providng event </param>
    /// <param name="panel">parent panel of the event/button. You might have events started from panel itself without button involved</param>
    /// <returns>never allow further navigation</returns>
	static public bool AcceptChallenges(FlowButton button, Panel panel) 
	{
		Debug.Log("AcceptChallenges: click");
		if (!Platform.Instance.HasPermissions("any", "login")) {			
			// Restart function once authenticated
			Platform.OnAuthenticated handler = null;
			handler = new Platform.OnAuthenticated((authenticated) => {
				Platform.Instance.onAuthenticated -= handler;
				if (authenticated) {
					AcceptChallenges(button, panel);
				}
			});
			Platform.Instance.onAuthenticated += handler;	
			
			Platform.Instance.Authorize("any", "login");
			return false;
		}
		
	    GConnector gConect = panel.Outputs.Find(r => r.Name == button.name);	
		Platform.OnSync shandler = null;
		shandler = new Platform.OnSync(() => {
			Platform.Instance.onSync -= shandler;
			
			lock(DataVault.data) {
				if (DataVault.Get("loaderthread") != null) return;
				Thread loaderThread = new Thread(() => {
#if !UNITY_EDITOR
					AndroidJNI.AttachCurrentThread();
#endif				
					try {
						// Reset world
						Platform.Instance.ResetTargets();
						DataVault.Remove("challenges");
						DataVault.Remove("finish");
						
						List<Challenge> relevant = new List<Challenge>();		
						int? finish = null;
						
						Notification[] notifications = Platform.Instance.Notifications();
						foreach (Notification notification in notifications) {
							if (notification.read) continue;
							if (string.Equals(notification.node["type"], "challenge")) {
								int challengerId = notification.node["from"].AsInt;
								if (challengerId == null) continue;
								string challengeId = notification.node["challenge_id"].ToString();
								if (challengeId == null || challengeId.Length == 0) continue;
								if (challengeId.Contains("$oid")) challengeId = notification.node["challenge_id"]["$oid"].ToString();
								challengeId = challengeId.Replace("\"", "");
								
								Debug.Log("AcceptChallenges: " + challengeId + " from " + challengerId);
								
								Platform.Instance.message = "Please wait while we fetch challenge"; 
								Challenge potential = Platform.Instance.FetchChallenge(challengeId);
								if (potential == null) continue;
								if (potential is DistanceChallenge) {
									DistanceChallenge challenge = potential as DistanceChallenge;					
									
									Platform.Instance.message = "Please wait while we fetch track"; 
									Track track = challenge.UserTrack(challengerId);
									if (track != null) {
										Platform.Instance.FetchTrack(track.deviceId, track.trackId); // Make sure we have the track in the local db
										TargetTracker tracker = Platform.Instance.CreateTargetTracker(track.deviceId, track.trackId);
										User challenger = Platform.Instance.GetUser(challengerId);
										tracker.name = challenger.username;
										if (tracker.name == null || tracker.name.Length == 0) tracker.name = challenger.name;
									} // else race leader/friends/creator?
				
									relevant.Add(challenge); 					
									if (!finish.HasValue || finish.Value < challenge.distance) finish = challenge.distance;					
								}
							}
						}		
						if (!finish.HasValue || relevant.Count == 0) {
							Platform.Instance.message = "No relevant challenges";
							return;
						}
						
						Debug.Log("AcceptChallenges: Accepted " + relevant.Count + " challenges");
						Platform.Instance.message = "Accepted " + relevant.Count + " challenges";
						DataVault.Set("challenges", relevant);
						DataVault.Set("finish", finish.Value);
						
						AutoFade.LoadLevel("Race Mode", 0.1f, 1.0f, Color.black);
						
						// Follow connection since the function may have been called asynchronously
						panel.parentMachine.FollowConnection(gConect);
					} finally {
						DataVault.Remove("loaderthread");
#if !UNITY_EDITOR
						AndroidJNI.DetachCurrentThread();
#endif					
					}
				});
				DataVault.Set("loaderthread", loaderThread);
				loaderThread.Start();
			}
		});
		Platform.Instance.onSync += shandler;
		
		Platform.Instance.message = "Please wait while we sync the database";
		Platform.Instance.SyncToServer();
		
		return false;
	}	
	
	/// <summary>
	/// Enables the flow to go straight to game if we are in the editor
	/// </summary>
	static public bool GoStraightToGameInEditor(FlowButton button, Panel panel)
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
}
