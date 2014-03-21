using UnityEngine;
using System.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;

using RaceYourself.Models;

[Serializable]
public class GameSelectPanel : HexPanel 
{
	GestureHelper.OnBack backHandler = null;
	
	// Handlers for cheats
	GestureHelper.OnTap tapHandler = null;
	GestureHelper.TwoFingerTap twoHandler = null;
	GestureHelper.ThreeFingerTap threeHandler = null;
	
	public enum Gestures {
		Tap,
		TwoTap,
		ThreeTap,
		None
	};
	
	private List<Gestures> lastGestures = new List<Gestures>();
	private List<Gestures> pointsCheat = new List<Gestures> {Gestures.ThreeTap, Gestures.TwoTap, Gestures.Tap};

	private List<Gestures> btServerCheat = new List<Gestures> {Gestures.ThreeTap, Gestures.ThreeTap, Gestures.Tap};
	private List<Gestures> btClientCheat = new List<Gestures> {Gestures.ThreeTap, Gestures.ThreeTap, Gestures.TwoTap};
	
	private float cheatMaxTime = 1.0f;
	private DateTime cheatDt = DateTime.Now;
	
	
	public GameSelectPanel() {}
    public GameSelectPanel(SerializationInfo info, StreamingContext ctxt)
        : base(info, ctxt)
    {
	}

	public void QuitApp() {
		if(!IsPopupDisplayed()) {
			GestureHelper.onBack -= backHandler;
			Application.Quit();
		}
	}
	
	public bool IsPopupDisplayed() {
		HexInfoManager info = GameObject.FindObjectOfType(typeof(HexInfoManager)) as HexInfoManager;
		if(info != null) {
			if(info.IsInOpenStage()) {
				info.AnimExit();
				return true;
			}
		}
		return false;
	}
	
	public void CheckCheat(Gestures current) {
		if ((DateTime.Now - cheatDt).TotalMilliseconds/1000.0f >= cheatMaxTime) lastGestures.Clear();
		
		lastGestures.Add(current);
		UnityEngine.Debug.Log ("GameSelectPanel: gestures: " + string.Join(", ", lastGestures.Select(i => i.ToString()).ToArray()));
		if (lastGestures.SequenceEqual(pointsCheat)) {
				UnityEngine.Debug.Log("GameSelectPanel: Final tap - points awarded");
				Platform.Instance.PlayerPoints.AwardPoints("Dev Cheat", "Dev Cheat", 10000);
				UnityEngine.Debug.Log("GameSelectPanel: points awarded in platform");
				MessageWidget.AddMessage("Points Cheat", "You got 10,000 points for nothing!", "trophy copy");
				lastGestures.Clear();
		}
		if (lastGestures.SequenceEqual(btServerCheat)) {
				Platform.Instance.BluetoothServer();
				MessageWidget.AddMessage("Debug", "Enabling Bluetooth server", "settings");
				lastGestures.Clear();
		}
		if (lastGestures.SequenceEqual(btClientCheat)) {
				Platform.Instance.BluetoothClient();
				MessageWidget.AddMessage("Debug", "Enabling Bluetooth client", "settings");
				lastGestures.Clear();
		}
		cheatDt = DateTime.Now;
	}
	
	public override void StateUpdate ()
	{
		base.StateUpdate ();
				
		if(!LoadingTextComponent.IsVisible()) {
			LoadingTextComponent.SetVisibility(true);
		}
	}
	
    public override void EnterStart()
    {		
		DataVault.Set("first_menu", " ");
		
        GConnector raceExit = Outputs.Find(r => r.Name == "raceExit");
        GConnector pursuitExit = Outputs.Find(r => r.Name == "pursuitExit");
		GConnector challengeExit = Outputs.Find (r => r.Name == "challengeExit");
		GConnector unlockExit = Outputs.Find (r => r.Name == "unlockExit");
		GConnector celebExit = Outputs.Find (r => r.Name == "celebExit");
		GConnector modeExit = Outputs.Find (r => r.Name == "modeExit");
		GConnector deleteExit = Outputs.Find (r => r.Name == "deleteExit");
		GConnector trainExit = Outputs.Find (r => r.Name == "trainExit");
			
		
		DataVault.Set("rp", (int)Platform.Instance.PlayerPoints.OpeningPointsBalance);
		DataVault.Set("metabolism", (int)Platform.Instance.PlayerPoints.CurrentMetabolism);
		
        GraphComponent gComponent = GameObject.FindObjectOfType(typeof(GraphComponent)) as GraphComponent;

		//OnBack = back on mobile. All other gestures should be Glass only.
		backHandler = new GestureHelper.OnBack(() => {
			QuitApp();
		});
        GestureHelper.onBack += backHandler;
		
		tapHandler = new GestureHelper.OnTap(() => {
			CheckCheat(Gestures.Tap);
		});
		
		GestureHelper.onTap += tapHandler;
		
		twoHandler = new GestureHelper.TwoFingerTap(() => {
			CheckCheat(Gestures.TwoTap);
		});
		
		GestureHelper.onTwoTap += twoHandler;
		
		threeHandler = new GestureHelper.ThreeFingerTap(() => {
			CheckCheat(Gestures.ThreeTap);
		});
		
		GestureHelper.onThreeTap += threeHandler;
		
#if !UNITY_EDITOR
		List<Game> games = Platform.Instance.GetGames();
#else
        List<Game> games = Platform.Instance.GetGames();
#endif
		UnityEngine.Debug.Log("Games: There are currently " + games.Count + " games");
		
		HexButtonData hbd = GetButtonAt(0, 0);
		
		if(hbd == null)
		{
			hbd = new HexButtonData();
			buttonData.Add(hbd);
		}
		
		hbd.row = 0;
		hbd.column = 0;
		hbd.buttonName = "current_balance";
		hbd.displayInfoData = false;
		hbd.backgroundTileColor = 0x00A30EFF;
		long currentPoints = Platform.Instance.PlayerPoints.OpeningPointsBalance;
		hbd.textBold = string.Format("{0:#,###0}", currentPoints) + "RP";
		hbd.textSmall = Platform.Instance.PlayerPoints.CurrentMetabolism.ToString("f0");
				
        //generate some buttons
        for(int i=0; i<games.Count; i++)
        {
            
            hbd = GetButtonAt(games[i].column, games[i].row);
            //if we do not have button at provided coordinates we will create new button data for it
            if (hbd == null)
            {
                hbd = new HexButtonData();
				buttonData.Add(hbd);
            }
            else
            {
                //disconnect old connection which could possibly change. Also we don't want to double it if it doesn't change
                GConnector oldConnection =  Outputs.Find(r => r.Name == hbd.buttonName);
                if (oldConnection != null)
                {
                    gComponent.Data.Disconnect(oldConnection);
                    Outputs.Remove(oldConnection);
                }
            }
            
            hbd.buttonName = games[i].gameId;
			hbd.activityName = games[i].name;
			hbd.activityContent = games[i].description;
			hbd.activityPrice = (int)games[i].priceInPoints;
            hbd.column = games[i].column;
            hbd.row = games[i].row;
            hbd.imageName = games[i].iconName;
			
			
			if(games[i].type == "N/A")
			{
				hbd.displayInfoData = false;
				hbd.textOverlay = "Coming Soon";
			}
			
			hbd.displayInfoData = true;
			
			if(games[i].state == "Locked") {
	           	hbd.locked = true;
			} else {
				hbd.locked = false;
				hbd.textOverlay = string.Empty;
			}
			
			if(games[i].type == "Mode")
			{
				bool mode = Convert.ToBoolean(DataVault.Get(games[i].gameId));
				hbd.textNormal = games[i].name;
				if(mode)
				{
					hbd.textSmall = "On";
					if(games[i].name == "GPS Mode") {
						Platform.Instance.LocalPlayerPosition.SetIndoor(false);
					}
					UnityEngine.Debug.Log("GameSelectPanel: indoor set to true");
				}
				else
				{
					hbd.textSmall = "Off";
					if(games[i].name == "GPS Mode") {
						Platform.Instance.LocalPlayerPosition.SetIndoor(true);
					}
					UnityEngine.Debug.Log("GameSelectPanel: indoor set to false");
				}
			}
			
			GConnector gc = NewOutput(hbd.buttonName, "Flow");
            gc.EventFunction = "SetType";
			
			/*gComponent.Data.Disconnect(gc, unlockExit.Link[0]);
			
			if(games[i].state == "Locked" && games[i].type != "N/A")
			{
				gc.EventFunction = "SetGameDesc";
				if(unlockExit.Link.Count > 0)
				{
					gComponent.Data.Connect(gc, unlockExit.Link[0]);
				}
			}
			else */
			if(games[i].type == "Race") 
			{
				gc.EventFunction = "SetType";
				if(raceExit.Link.Count > 0) 
				{
					gComponent.Data.Connect(gc, raceExit.Link[0]);
				}
			} 
//			else if(games[i].type == "Pursuit") 
//			{
//				if(pursuitExit.Link.Count > 0) 
//				{
//					gComponent.Data.Connect(gc, pursuitExit.Link[0]);
//				}
//			} 
			else if(games[i].type == "Challenge") 
			{
				gc.EventFunction = "AuthenticateUser";
				if(challengeExit.Link.Count > 0) 
				{
					gComponent.Data.Connect(gc, challengeExit.Link[0]);
				}
			}
			else if(games[i].type == "Celeb")
			{
				gc.EventFunction = "SetCeleb";
				if(celebExit.Link.Count > 0)
				{
					gComponent.Data.Connect(gc, celebExit.Link[0]);
				}
			}
			else if(games[i].type == "TrainRescue")
			{
				if(trainExit.Link.Count > 0)
				{
					gComponent.Data.Connect(gc, trainExit.Link[0]);
				}
			}
			else if(games[i].type == "Mode")
			{
				gc.EventFunction = "SetMode";
			}

        }
				
        base.EnterStart();   
    }
	
	public override void Exited ()
	{
		base.Exited ();
		GestureHelper.onBack -= backHandler;
		GestureHelper.onTap -= tapHandler;
		GestureHelper.onTwoTap -= twoHandler;
		GestureHelper.onThreeTap -= threeHandler;
		DataVault.Set("first_menu", " ");
	}
	
    public override void OnClick(FlowButton button)
	{
		//store the name of the button in the DataVault. This is the gameID.
		DataVault.Set("current_game_id", button.name);
		
		base.OnClick(button);	
	}

}
