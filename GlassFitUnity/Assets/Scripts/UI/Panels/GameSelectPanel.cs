using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

[Serializable]
public class GameSelectPanel : HexPanel 
{
	GestureHelper.DownSwipe downHandler = null;
	
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
	
	private Gestures lastGesture = Gestures.None;
	
	private float cheatElapsedTime = 1.0f;
	private float cheatMaxTime = 1.0f;
	
	
	public GameSelectPanel() {}
    public GameSelectPanel(SerializationInfo info, StreamingContext ctxt)
        : base(info, ctxt)
    {
	}

	public void QuitApp() {
		if(!IsPopupDisplayed()) {
			GestureHelper.onSwipeDown -= downHandler;
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
	
	public void CheckCheat(int current) {
		switch(current)
		{
		case 1:
			if(lastGesture == Gestures.TwoTap) {
				UnityEngine.Debug.Log("GameSelectPanel: Final tap - points awarded");
				Platform.Instance.AwardPoints("Dev Cheat", "Dev Cheat", 10000);
				MessageWidget.AddMessage("Points Cheat", "You got 10,000 points for nothing!", "trophy copy");
				lastGesture = Gestures.None;
			} else {
				lastGesture = Gestures.None;
				cheatElapsedTime = 1.0f;
			}
			break;
			
		case 2:
			if(lastGesture == Gestures.ThreeTap) {
				lastGesture = Gestures.TwoTap;
				UnityEngine.Debug.Log("GameSelectPanel: Two tap detected for cheats");
			} else {
				lastGesture = Gestures.None;
				cheatElapsedTime = 1.0f;
			}
			break;
			
		case 3:
			if(lastGesture == Gestures.None) {
				lastGesture = Gestures.ThreeTap;
				UnityEngine.Debug.Log("GameSelectPanel: Three tap detected for cheats");
				cheatElapsedTime = 0.0f;
			} else {
				lastGesture = Gestures.None;
				cheatElapsedTime = 1.0f;
			}
			break;
		}
	}
	
	public override void StateUpdate ()
	{
		base.StateUpdate ();
		
		if(cheatElapsedTime < 1.0f) {
			cheatElapsedTime += Time.deltaTime;
			if(cheatElapsedTime > 1.0f) {
				lastGesture = Gestures.None;
			}
		}
	}
	
    public override void EnterStart()
    {
        GConnector raceExit = Outputs.Find(r => r.Name == "raceExit");
        GConnector pursuitExit = Outputs.Find(r => r.Name == "pursuitExit");
		GConnector challengeExit = Outputs.Find (r => r.Name == "challengeExit");
		GConnector unlockExit = Outputs.Find (r => r.Name == "unlockExit");
		GConnector celebExit = Outputs.Find (r => r.Name == "celebExit");
		GConnector modeExit = Outputs.Find (r => r.Name == "modeExit");
		GConnector deleteExit = Outputs.Find (r => r.Name == "deleteExit");
		
		DataVault.Set("rp", (int)Platform.Instance.GetOpeningPointsBalance());
		DataVault.Set("metabolism", (int)Platform.Instance.GetCurrentGemBalance());
		
        GraphComponent gComponent = GameObject.FindObjectOfType(typeof(GraphComponent)) as GraphComponent;
		
		downHandler = new GestureHelper.DownSwipe(() => {
			QuitApp();
		});
		
		GestureHelper.onSwipeDown += downHandler;
		
		tapHandler = new GestureHelper.OnTap(() => {
			CheckCheat(1);
		});
		
		GestureHelper.onTap += tapHandler;
		
		twoHandler = new GestureHelper.TwoFingerTap(() => {
			CheckCheat(2);
		});
		
		GestureHelper.onTwoTap += twoHandler;
		
		threeHandler = new GestureHelper.ThreeFingerTap(() => {
			CheckCheat(3);
		});
		
		GestureHelper.onThreeTap += threeHandler;
		
#if !UNITY_EDITOR
		List<Game> games = Platform.Instance.GetGames();
#else
		List<Game> games = PlatformDummy.Instance.GetGames();
#endif
		UnityEngine.Debug.Log("Games: There are currently " + games.Count + " games");
		
		HexButtonData hbd = new HexButtonData();
		hbd.row = 0;
		hbd.column = 0;
		hbd.buttonName = "current_balance";
		hbd.onButtonCustomString = Platform.Instance.GetCurrentMetabolism().ToString("f0");
		//hbd.imageName = "";
		
		buttonData.Add(hbd);
		
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
            
            hbd.buttonName = games[i].iconName;
			hbd.activityName = games[i].name;
			hbd.activityContent = games[i].description;
			hbd.activityPrice = (int)games[i].priceInPoints;
            hbd.column = games[i].column;
            hbd.row = games[i].row;
			UnityEngine.Debug.Log("Game: position of " + games[i].name + " is: " + games[i].column + ", " + games[i].row);
            hbd.imageName = games[i].iconName;
			
			if(games[i].state == "Locked") {
	           	hbd.locked = true;
			} else {
				hbd.locked = false;
			}
			
			
			GConnector gc = NewOutput(hbd.buttonName, "Flow");
            gc.EventFunction = "SetType";
			
			gComponent.Data.Disconnect(gc, unlockExit.Link[0]);
			
			if(games[i].state == "Locked")
			{
				gc.EventFunction = "SetGameDesc";
				if(unlockExit.Link.Count > 0)
				{
					UnityEngine.Debug.Log("Game: Unlock exit set");
					gComponent.Data.Connect(gc, unlockExit.Link[0]);
				}
			}
			else if(games[i].type == "Race") 
			{
				if(raceExit.Link.Count > 0) 
				{
					gComponent.Data.Connect(gc, raceExit.Link[0]);
				}
			} 
			else if(games[i].type == "Pursuit") 
			{
				UnityEngine.Debug.Log("Game: Pursuit exit set");
				if(pursuitExit.Link.Count > 0) 
				{
					gComponent.Data.Connect(gc, pursuitExit.Link[0]);
				}
			} 
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
			else if(games[i].type == "Mode")
			{
				gc.EventFunction = "SetModeDesc";
				if(modeExit.Link.Count > 0)
				{
					gComponent.Data.Connect(gc, modeExit.Link[0]);
				}
			}
			else if(games[i].type == "Delete")
			{
				if(deleteExit.Link.Count > 0)
				{
					gComponent.Data.Connect(gc, deleteExit.Link[0]);
				}
			}

        }
		
		LoadingTextComponent.SetVisibility(false);
		
        base.EnterStart();   
    }
	
	public override void Exited ()
	{
		base.Exited ();
		GestureHelper.onSwipeDown -= downHandler;
		GestureHelper.onTap -= tapHandler;
		GestureHelper.onTwoTap -= twoHandler;
		GestureHelper.onThreeTap -= threeHandler;
	}
}
