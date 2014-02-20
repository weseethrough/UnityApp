using UnityEngine;
using System.Collections;
using System.Runtime.Serialization;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using SimpleJSON;

[Serializable]
public class MultiplayerPanel : HexPanel {
	
	private GraphComponent gComponent;
	
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
	private List<Friend> friendList;
	private Dictionary<int, HexButtonData> friendButtons = new Dictionary<int, HexButtonData>();
	
	private Stopwatch timeout = new Stopwatch();
	
	public MultiplayerPanel() { }
    public MultiplayerPanel(SerializationInfo info, StreamingContext ctxt)
        : base(info, ctxt)
    {
    }
	
	// Use this for initialization
	public override void EnterStart ()
	{
		DataVault.Set("highlight", " ");
		DataVault.Set("incoming", " ");
		DataVault.Set("to_challenge", "  ");
		
		gComponent = GameObject.FindObjectOfType(typeof(GraphComponent)) as GraphComponent;
		
		buttonData = new List<HexButtonData>();
		friendButtons.Clear();
		
		notSet = true;
		
		DataVault.Set("tutorial_hint", "Connecting to server");
		if (!Platform.Instance.ConnectSocket()) {
			MessageWidget.AddMessage("Network", "Could not open a socket to server", "settings");
			FlowStateMachine.GetCurrentFlowState().parentMachine.FollowBack();
		}
		Platform.Instance.onUserMessage += OnUserMessage;
		DataVault.Set("tutorial_hint", "");
		base.EnterStart ();
	}	
	
	public void OnUserMessage(int userId, JSONNode message) {
		UnityEngine.Debug.Log("MultiplayerPanel: Pong from " + userId);
		HexButtonData hbd;
		if (friendButtons.TryGetValue(userId, out hbd)) {
			hbd.backgroundTileColor = 0x00A30EFF;	
			hbd.markedForVisualRefresh = true;
			DynamicHexList list = (DynamicHexList)physicalWidgetRoot.GetComponentInChildren(typeof(DynamicHexList));
        	list.UpdateButtonList();
		}
	}
	
	public void AddFriendHexes()
	{
		friendList = Platform.Instance.Friends();
		
		// Clear race variables
		DataVault.Remove("race_group");
		DataVault.Remove("racers");
		if(friendList != null && friendList.Count > 0)
		{
			UnityEngine.Debug.Log("MultiplayerPanel: there are " + friendList.Count + " friends");
			
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
				hbd.backgroundTileColor = 0x171717FF;	
				buttonData.Add(hbd);
			}
			if (friendList[0].userId.HasValue) {
				friendButtons.Add(friendList[0].userId.Value, hbd);
			}
				
			hbd.column = (int)currentPosition.x;
			hbd.row = (int)-currentPosition.y;
			hbd.buttonName = friendList[0].guid;
			hbd.textNormal = friendList[0].name;
			
			GConnector gc = NewOutput(hbd.buttonName, "Flow");
		    gc.EventFunction = "RaceFriend";
				
			for(int i=1; i<friendList.Count; i++) 
			{
				CalculatePosition();
				
				hbd = GetButtonAt((int)currentPosition.x, (int)-currentPosition.y);
				
				if(hbd == null)
				{
					hbd = new HexButtonData();
					hbd.backgroundTileColor = 0x171717FF;
					buttonData.Add(hbd);
				}
				if (friendList[i].userId.HasValue) {
					friendButtons.Add(friendList[i].userId.Value, hbd);
				}
				
				//UnityEngine.Debug.Log("MultiplayerPanel: HBD obtained");
					
				hbd.column = (int)currentPosition.x;
				hbd.row = (int)-currentPosition.y;
				hbd.buttonName = friendList[i].guid;
				hbd.textNormal = friendList[i].name;
					
				gc = NewOutput(hbd.buttonName, "Flow");
			    gc.EventFunction = "RaceFriend";
									
			}
			DynamicHexList list = (DynamicHexList)physicalWidgetRoot.GetComponentInChildren(typeof(DynamicHexList));
        	list.UpdateButtonList();
			DataVault.Set("friend_list", friendList);
			PingFriends();
		} else 
		{
			UnityEngine.Debug.Log("MultiplayerPanel: friend list is null!");
		}
	}
	
	private void PingFriends() {
		timeout.Start();
		foreach(int friendId in friendButtons.Keys) {
			Platform.Instance.MessageUser(friendId, "{\"type\" : \"online_query\"}");
			// TODO: OnDisconnect(friendId) => locked = true;?
		}
	}
	
	public override void StateUpdate ()
	{
		base.StateUpdate ();
		
		if(notSet) {
			timeout.Reset();
			AddFriendHexes();
			notSet = false;
		}
		if(timeout.ElapsedMilliseconds > 5000) {
			timeout.Reset();
			PingFriends();
		}
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
		Platform.Instance.onUserMessage -= OnUserMessage;
		base.Exited ();
		DataVault.Set("tutorial_hint", " ");
	}
	
}
