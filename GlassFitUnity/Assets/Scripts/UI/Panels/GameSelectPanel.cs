using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

[Serializable]
public class GameSelectPanel : HexPanel 
{
	
//	PlatformDummy platform = new PlatformDummy();
	
	public GameSelectPanel() {}
    public GameSelectPanel(SerializationInfo info, StreamingContext ctxt)
        : base(info, ctxt)
    {
	}

    public override void EnterStart()
    {
        GConnector raceExit = Outputs.Find(r => r.Name == "raceExit");
        GConnector pursuitExit = Outputs.Find(r => r.Name == "pursuitExit");
		GConnector challengeExit = Outputs.Find (r => r.Name == "challengeExit");
		GConnector unlockExit = Outputs.Find (r => r.Name == "unlockExit");
		GConnector celebExit = Outputs.Find (r => r.Name == "celebExit");
		GConnector modeExit = Outputs.Find (r => r.Name == "modeExit");
		
		DataVault.Set("rp", (int)Platform.Instance.GetOpeningPointsBalance());
		DataVault.Set("metabolism", (int)Platform.Instance.GetCurrentGemBalance());
		
        GraphComponent gComponent = GameObject.FindObjectOfType(typeof(GraphComponent)) as GraphComponent;
		
#if !UNITY_EDITOR
		List<Game> games = Platform.Instance.GetGames();
#else
		List<Game> games = PlatformDummy.Instance.GetGames();
#endif
		UnityEngine.Debug.Log("Games: There are currently " + games.Count + " games");
        //generate some buttons
        for(int i=0; i<games.Count; i++)
        {
            
            HexButtonData hbd = GetButtonAt(games[i].column, games[i].row);
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
            
            hbd.buttonName = games[i].name;
			hbd.activityName = games[i].name;
			hbd.activityContent = games[i].description;
			hbd.activityPrice = (int)games[i].priceInPoints;
            hbd.column = games[i].column;
            hbd.row = games[i].row;
			UnityEngine.Debug.Log("Game: position of " + games[i].name + " is: " + games[i].column + ", " + games[i].row);
            hbd.imageName = games[i].iconName;
			
			if(games[i].state == "Locked") {
				//UnityEngine.Debug.Log("Game: " + games[i].name + " Set to locked");
            	hbd.locked = true;
			} else {
				//UnityEngine.Debug.Log("Game: " + games[i].name + " Set to unlocked");
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

        }

        base.EnterStart();   
    }
}
