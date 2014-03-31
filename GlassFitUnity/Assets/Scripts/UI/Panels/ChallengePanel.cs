using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System.Threading;
using System;
using System.Collections.Generic;

using RaceYourself.Models;

[Serializable]
public class ChallengePanel : HexPanel {
	
	private GConnector challengeExit;
	private GConnector previousExit;
	private GConnector sendExit;
		
	private GraphComponent gComponent;
	
	static List<ChallengeNotification> challengeNotifications;
	
	private bool threadComplete = false;
	
	private bool notSet = true;
	
	// Travel direction for hex creation
	private bool goingUp = true;
	
	// Current hex position
	private Vector2 currentPosition;
	
	// Top row of current level
	private int lowestRow;
	
	// Check for whether the next column should change
	private bool columnSame;
	
	// The end column position for travelling up
	private int columnTop;
	
	// Current level of hexes
	private int currentLevel;
	
	// End position for the hex level
	private Vector2 levelEndPosition;
	
	// Start position for the hex level
	private Vector2 levelStartPosition;
	
	// Friend list
	IList<Friend> friendList;
	
	public ChallengePanel() { }
    public ChallengePanel(SerializationInfo info, StreamingContext ctxt)
        : base(info, ctxt)
    {
    }
	
	// Use this for initialization
	public override void EnterStart ()
	{
		DataVault.Set("highlight", " ");
		DataVault.Set("incoming", " ");
		DataVault.Set("to_challenge", "  ");
		challengeExit = Outputs.Find(r => r.Name == "challengeExit");
		previousExit = Outputs.Find(r => r.Name == "previousExit");
		sendExit = Outputs.Find(r => r.Name == "sendExit");
		
		gComponent = GameObject.FindObjectOfType(typeof(GraphComponent)) as GraphComponent;
		
		buttonData = new List<HexButtonData>();
		
		challengeNotifications = new List<ChallengeNotification>();
		
		notSet = true;
		
		threadComplete = false;
		
		GetChallenges();		
		
		Platform.Instance.SyncToServer();
		
		DataVault.Set("tutorial_hint", "Syncing with the server");
		base.EnterStart ();
	}
	
	public void GetChallenges() {
		if (!Platform.Instance.HasPermissions("any", "login")) {			
		// Restart function once authenticated
            NetworkMessageListener.OnAuthenticated handler = null;
            handler = new NetworkMessageListener.OnAuthenticated((authenticated) => {
                Platform.Instance.NetworkMessageListener.onAuthenticated -= handler;
				if (authenticated) {
					GetChallenges();
				}
			});
            Platform.Instance.NetworkMessageListener.onAuthenticated += handler;	
				
			UnityEngine.Debug.Log("ChallengePanel: Need to authenticate");
			DataVault.Set("tutorial_hint", "Authenticating device");
			
			Platform.Instance.Authorize("any", "login");
			return;
		}
		
        NetworkMessageListener.OnSync shandler = null;
        shandler = new NetworkMessageListener.OnSync((message) => {
            Platform.Instance.NetworkMessageListener.onSync -= shandler;				
			UnityEngine.Debug.Log("ChallengePanel: about to lock datavault");
			DataVault.Set("tutorial_hint", "Getting challenges and friends");
			lock(DataVault.data) {
				if (DataVault.Get("loaderthread") != null) return;
				UnityEngine.Debug.Log("ChallengePanel: starting thread");
				Thread loaderThread = new Thread(() =>
                                                       {
#if UNITY_ANDROID
					AndroidJNI.AttachCurrentThread();
#endif
                                                           try {
						UnityEngine.Debug.Log("ChallengePanel: getting notifications");
						Notification[] notifications = Platform.Instance.Notifications();
						UnityEngine.Debug.Log("ChallengePanel: notifications obtained");
						foreach (Notification notification in notifications) {
							UnityEngine.Debug.Log("ChallengePanel: notification has been found");
							if (notification.read) {
								UnityEngine.Debug.Log("ChallengePanel: notification set to read");
								continue;
							}
							UnityEngine.Debug.Log("ChallengePanel: notification not read");
							if (string.Equals(notification.message.type, "challenge")) {
								int challengerId = notification.message.from;
								if (challengerId == null) continue;
								string challengeId = notification.message.challenge_id;
								if (challengeId == null || challengeId.Length == 0) continue;
								Challenge potential = Platform.Instance.FetchChallenge(challengeId);
								if(potential is DistanceChallenge) {
									User user = Platform.Instance.GetUser(challengerId);
									//			UnityEngine.Debug.Log("ChallengeNotification: getting first track");
									UnityEngine.Debug.Log("ChallengePanel: getting track");
									var attempt = potential.attempts.Find(a => a.user_id == user.id);
									UnityEngine.Debug.Log("ChallengePanel: fetching track using previous");
									if(attempt == null) continue;									
									Track realTrack = Platform.Instance.FetchTrack(attempt.device_id, attempt.track_id);
									
									UnityEngine.Debug.Log("ChallengePanel: creating challenge notification");
									ChallengeNotification challengeNot = new ChallengeNotification(notification, potential, user, realTrack);
									challengeNotifications.Add(challengeNot);
								}
							}
						}
					}		
					finally {
						UnityEngine.Debug.Log("ChallengePanel: removing loaderthread");
						DataVault.Remove("loaderthread");
						
						UnityEngine.Debug.Log("ChallengePanel: thread complete true");
						threadComplete = true;
#if UNITY_ANDROID
						UnityEngine.Debug.Log("ChallengePanel: detaching thread");
						AndroidJNI.DetachCurrentThread();
#endif
                        UnityEngine.Debug.Log("ChallengePanel: Adding hexes");
						
						DataVault.Set("challenge_notifications", challengeNotifications);
					}
				});
				DataVault.Set("loaderthread", loaderThread);
				loaderThread.Start();
			}
		});
        Platform.Instance.NetworkMessageListener.onSync += shandler;

	}
	
	public void AddFriendHexes()
	{
		friendList = Platform.Instance.Friends();
		
		if(friendList != null && friendList.Count > 0)
		{
			UnityEngine.Debug.Log("ChallengePanel: there are " + friendList.Count + " friends");
			
			currentPosition = new Vector2(-1, -1);
			lowestRow = -1;
			columnSame = true;
			columnTop = 0;
			currentLevel = 1;
			levelEndPosition = new Vector2(0, -1);
			levelStartPosition = new Vector2(-1, -1);
			
			HexButtonData hbd = GetButtonAt(-1, 1);
			
			if(hbd == null)
			{
				hbd = new HexButtonData();
				buttonData.Add(hbd);
			}
				
			hbd.column = (int)currentPosition.x;
			hbd.row = (int)-currentPosition.y;
			hbd.buttonName = friendList[0].guid;
			hbd.textNormal = friendList[0].name;
			
			GConnector gc = NewOutput(hbd.buttonName, "Flow");
		    gc.EventFunction = "SetFriend";
				
			if(sendExit.Link.Count > 0) {
				gComponent.Data.Connect(gc, sendExit.Link[0]);
			}
			
			for(int i=1; i<friendList.Count; i++) 
			{
				CalculatePosition();
				
				hbd = GetButtonAt((int)currentPosition.x, (int)-currentPosition.y);
				
				if(hbd == null)
				{
					hbd = new HexButtonData();
					buttonData.Add(hbd);
				}
				
				//UnityEngine.Debug.Log("ChallengePanel: HBD obtained");
					
				hbd.column      = (int)currentPosition.x;
				hbd.row         = (int)-currentPosition.y;
				hbd.buttonName  = friendList[i].guid;
				hbd.textNormal  = friendList[i].name;
					
				gc = NewOutput(hbd.buttonName, "Flow");
			    gc.EventFunction = "SetFriend";
					
				if(sendExit.Link.Count > 0) {
					gComponent.Data.Connect(gc, sendExit.Link[0]);
				}
					
				
			}
			DynamicHexList list = (DynamicHexList)physicalWidgetRoot.GetComponentInChildren(typeof(DynamicHexList));
        	list.UpdateButtonList();
			DataVault.Set("friend_list", friendList);
		} else 
		{
			UnityEngine.Debug.Log("ChallengePanel: friend list is null!");
		}
	}
	
	public override void StateUpdate ()
	{
		base.StateUpdate ();
		
		if(threadComplete && notSet) {
			AddChallengeHexes();
			AddFriendHexes();
			notSet = false;
		}
	}
	
	public void AddChallengeHexes() 
	{
		UnityEngine.Debug.Log("ChallengePanel: Checking for challenges");
		if(challengeNotifications != null && challengeNotifications.Count > 0) {
			UnityEngine.Debug.Log("ChallengePanel: we have challenges");
			currentPosition = new Vector2(-1, -1);
			lowestRow = -1;
			columnSame = true;
			columnTop = 0;
			currentLevel = 1;
			levelEndPosition = new Vector2(0, -1);
			levelStartPosition = new Vector2(-1, -1);
			
			UnityEngine.Debug.Log("ChallengePanel: start attributes started");
			HexButtonData hbd = GetButtonAt(-1, -1);
			
			UnityEngine.Debug.Log("ChallengePanel: hbd obtained");
			
			if(hbd == null)
			{
				hbd = new HexButtonData();
				buttonData.Add(hbd);
			}
				
			hbd.column = (int)currentPosition.x;
			hbd.row = (int)currentPosition.y;
			hbd.buttonName = challengeNotifications[0].GetID();
			hbd.textNormal = challengeNotifications[0].GetName();
			hbd.textSmall = SiDistanceUnitless(challengeNotifications[0].GetTrack().distance);
			hbd.backgroundTileColor = 0x00A30EFF;	
			
			UnityEngine.Debug.Log("ChallengePanel: First button obtained, position is " + currentPosition.ToString());
			
			GConnector gc = NewOutput(hbd.buttonName, "Flow");
		    gc.EventFunction = "SetChallenge";
				
			if(challengeExit.Link.Count > 0) {
				gComponent.Data.Connect(gc, challengeExit.Link[0]);
			}
			
			//UnityEngine.Debug.Log("ChallengePanel: about to get more notifications, count is " + challengeNotifications.Count);
			for(int i=1; i < challengeNotifications.Count; i++) 
			{
				CalculatePosition();
				
				hbd = GetButtonAt((int)currentPosition.x, (int)currentPosition.y);
				
				if(hbd == null)
				{
					hbd = new HexButtonData();
					buttonData.Add(hbd);
				}
				
				//UnityEngine.Debug.Log("ChallengePanel: HBD obtained");
					
				hbd.column = (int)currentPosition.x;
				hbd.row = (int)currentPosition.y;
				hbd.buttonName = challengeNotifications[i].GetID();
				hbd.textNormal = challengeNotifications[i].GetName();
				hbd.textSmall = SiDistanceUnitless(challengeNotifications[i].GetTrack().distance);
					
				gc = NewOutput(hbd.buttonName, "Flow");
			    gc.EventFunction = "SetChallenge";
					
				if(challengeExit.Link.Count > 0) {
					gComponent.Data.Connect(gc, challengeExit.Link[0]);
				}
					
				UnityEngine.Debug.Log("ChallengePanel: position is " + currentPosition.ToString());
				
				
			}
			DynamicHexList list = (DynamicHexList)physicalWidgetRoot.GetComponentInChildren(typeof(DynamicHexList));
        	list.UpdateButtonList();
			UnityEngine.Debug.Log("ChallengePanel: removing tutorial hint");
		} else {
			UnityEngine.Debug.Log("ChallengePanel: No challenges, setting widget");
			MessageWidget.AddMessage("Sorry!", "You currently have no challenges", "activity_delete");
		}
		DataVault.Set("tutorial_hint", " ");
		DataVault.Set("incoming", "Incoming challenges");
		DataVault.Set("to_challenge", "Challenge a friend");
	}
	
	public void CalculatePosition() {
		if(goingUp)
		{
			if(currentPosition.y > lowestRow)
			{
				currentPosition.y--;
				if(columnSame)
				{
					currentPosition.x++;
					columnSame = false;
				} 
				else
				{
					columnSame = true;
				}
			} 
			else 
			{
				if(currentPosition.x < columnTop) 
				{
					currentPosition.x++;
					if(currentPosition.x == columnTop) 
					{
						goingUp = false;
						if(currentLevel % 2 != 0) 
						{
							columnSame = true;
						} 
						else
						{
							columnSame = false;
						}	
					}
				}
			}
		} 
		else
		{
			if(currentPosition == levelEndPosition)
			{
				levelEndPosition.x++;
				levelStartPosition.x--;
				goingUp = true;
				currentPosition = levelStartPosition;
				columnSame = true;
				lowestRow--;
				if(currentLevel % 2 != 0) 
				{
					columnTop++;
				}
				currentLevel++;
			}
			else
			{
				if(currentPosition.y < levelEndPosition.y)
				{
					currentPosition.y++;
				}
						
				if(columnSame)
				{
					currentPosition.x++;
					columnSame = false;
				} 
				else
				{
					columnSame = true;
				}
			}
		}
	}
	
	protected string SiDistanceUnitless(double meters) {
		string postfix = "m";
		string final;
		float value = (float)meters;
		if (value >= 1000) {
			value = value/1000;
			postfix = "km";
			if(value >= 10) {
				final = value.ToString("f1");
			} else {
				final = value.ToString("f2");
			}
		}
		else
		{
			final = value.ToString("f0");
		}
		//set the units string for the HUD
		
		return "\n" + final + postfix;
	}
	
	public override void Exited ()
	{
		base.Exited ();
		DataVault.Set("tutorial_hint", " ");
	}
}
