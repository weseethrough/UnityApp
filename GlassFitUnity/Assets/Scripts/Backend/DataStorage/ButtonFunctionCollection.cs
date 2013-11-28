using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

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
		case "Run":
			DataVault.Set("type", "Runner");
			break;
			
		case "Bike":
			DataVault.Set("type", "Cyclist");
			break;
			
		case "Boulder Level 1":
			DataVault.Set("type", "Boulder");
			break;
			
		case "Eagle Level 1":
			DataVault.Set("type", "Eagle");
			break;
			
		case "Zombie Level 1":
			DataVault.Set("type", "Zombie");
			break;
			
		case "Train Level 1":
			DataVault.Set("type", "Train");
			break;
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
		AutoFade.LoadLevel(1, 0.1f, 1.0f, Color.black);
		
		switch(fb.name) 
		{
		case "1km":
			DataVault.Set("finish", 1);
			break;
			
		case "2km":
			DataVault.Set("finish", 2);
			break;
			
		case "3km":
			DataVault.Set("finish", 3);
			break;
			
		case "4km":
			DataVault.Set("finish", 4);
			break;
			
		case "5km":
			DataVault.Set("finish", 5);
			break;
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
		AutoFade.LoadLevel(3, 0f, 1.0f, Color.black);
		
		switch(fb.name) {
		case "1km":
			DataVault.Set("finish", 1);
			break;
			
		case "2km":
			DataVault.Set("finish", 2);
			break;
			
		case "3km":
			DataVault.Set("finish", 3);
			break;
			
		case "4km":
			DataVault.Set("finish", 4);
			break;
			
		case "5km":
			DataVault.Set("finish", 5);
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
		Platform.Instance.Reset();
		
		AutoFade.LoadLevel(2, 0f, 1.0f, Color.black);
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
    /// sets friend name in database from button name
    /// </summary>
    /// <param name="fb"> button providng event </param>
    /// <param name="panel">parent panel of the event/button. You might have events started from panel itself without button involved</param>
    /// <returns>always allow further navigation</returns>
	static public bool SetFriendType(FlowButton fb, Panel panel) 
	{
		DataVault.Set("friend_type", fb.name);
		return true;
	}

    /// <summary>
    /// sends asynchronous challenge to other registered user
    /// </summary>
    /// <param name="fb"> button providng event </param>
    /// <param name="panel">parent panel of the event/button. You might have events started from panel itself without button involved</param>
    /// <returns>always allow further navigation</returns>
    static public bool Challenge(FlowButton button, Panel panel)
    {
		int friendId = (int)DataVault.Get("current_friend");
		if (friendId == 0) return false; // TODO: Challenge by third-party identity
		Platform.Instance.QueueAction(string.Format(@"{{
			'action' : 'challenge',
			'target' : {0},
			'taunt' : 'Your mother is a hamster!',
			'challenge' : {{
					'distance': 333,
					'duration': 42,
					'location': null,
					'public': false,
					'start_time': null,
					'stop_time': null,
					'type': 'duration'
			}}
		}}", friendId).Replace("'", "\""));
		Debug.Log ("Challenge: " + friendId + " challenged");
		Platform.Instance.SyncToServer();
		
		return true;
	}
	
	
    /// <summary>
    /// accept all distance challenges and start a race
    /// </summary>
    /// <param name="fb"> button providng event </param>
    /// <param name="panel">parent panel of the event/button. You might have events started from panel itself without button involved</param>
    /// <returns>allow further navigation if pending challenges</returns>
	static public bool AcceptChallenges(FlowButton button, Panel panel) 
	{
		Debug.Log("AcceptChallenges: click");
		// Reset world
		Platform.Instance.ResetTargets();
		DataVault.Remove("challenges");
		DataVault.Remove("finish");
		
		List<Challenge> relevant = new List<Challenge>();		
		int? finish = null;
		
		Notification[] notifications = Platform.Instance.Notifications();
		Debug.Log("AcceptChallenges: notes: " + notifications.Length);
		foreach (Notification notification in notifications) {
			if (string.Equals(notification.node["type"], "challenge")) {
				int challengerId = notification.node["from"].AsInt;
				if (challengerId == null) continue;
				string challengeId = notification.node["challenge_id"].ToString();
				if (challengeId == null || challengeId.Length == 0) continue;
				if (challengeId.Contains("$oid")) challengeId = notification.node["challenge_id"]["$oid"].ToString();
				challengeId = challengeId.Replace("\"", "");
				
				Debug.Log("AcceptChallenges: " + challengeId + " from " + challengerId);
				
				Challenge potential = Platform.Instance.FetchChallenge(challengeId);
				if (potential == null) continue;
				if (potential is DistanceChallenge) {
					DistanceChallenge challenge = potential as DistanceChallenge;					
					
					Track track = challenge.UserTrack(challengerId);
					if (track != null) {
						Platform.Instance.FetchTrack(track.deviceId, track.trackId); // Make sure we have the track in the local db
						Debug.Log("AcceptChallenges: t " + track.deviceId + " from " + track.trackId);
						TargetTracker tracker = Platform.Instance.CreateTargetTracker(track.deviceId, track.trackId);
						User challenger = Platform.Instance.GetUser(challengerId);
						tracker.name = challenger.username;
						if (tracker.name == null || tracker.name.Length == 0) tracker.name = challenger.name;
						Debug.Log("AcceptChallenges: tn " + tracker.name);
						// TODO: Link target tracker to challenge?
					} // else race leader/friends/creator?

					relevant.Add(challenge); 					
					if (!finish.HasValue || finish.Value < challenge.distance) finish = challenge.distance;					
				}
			}
		}		
		if (!finish.HasValue || relevant.Count == 0) return false;
		
		DataVault.Set("challenges", relevant);
		DataVault.Set("finish", finish.Value);
		
		AutoFade.LoadLevel(1, 0.1f, 1.0f, Color.black);

		return true;
	}	
}
