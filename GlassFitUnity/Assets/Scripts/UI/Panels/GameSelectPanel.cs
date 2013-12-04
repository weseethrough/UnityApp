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
            
            hbd.buttonName = games[i].name;
            hbd.column = games[i].column;
            hbd.row = games[i].row;
            hbd.imageName = games[i].name;
			
			if(games[i].state == "locked") {
				UnityEngine.Debug.Log("Game: " + games[i].name + " Set to locked");
            	hbd.locked = true;
			} else {
				UnityEngine.Debug.Log("Game: " + games[i].name + " Set to unlocked");
				hbd.locked = false;
			}
			
			GConnector gc = NewOutput(hbd.buttonName, "Flow");
            gc.EventFunction = "SetType";
			
			if(games[i].state == "locked")
			{
				gc.EventFunction = "SetGameDesc";
				if(unlockExit.Link.Count > 0)
				{
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
				if(pursuitExit.Link.Count > 0) 
				{
					gComponent.Data.Connect(gc, pursuitExit.Link[0]);
				}
			} 
			else if(games[i].type == "Challenge") 
			{
				gc.EventFunction = "AcceptChallenge";
				if(challengeExit.Link.Count > 0) 
				{
					gComponent.Data.Connect(gc, challengeExit.Link[0]);
				}
			}
			
            

            
            /*if (UnityEngine.Random.Range(0.0f, 1.0f) > 0.5f ? true : false)
            {                
                if (activityExit.Link.Count > 0)
                {
                    gComponent.Data.Connect(gc, activityExit.Link[0]); 
                }
            }
            else if (UnityEngine.Random.Range(0.0f, 1.0f) > 0.5f ? true : false)
            {                
                if (unlockExit.Link.Count > 0)
                {
                    gComponent.Data.Connect(gc, unlockExit.Link[0]); 
                }
            }
            else if (gComponent != null)*/
//            {
//                foreach (GNode node in gComponent.Data.Nodes)
//                {
//                    GParameter gName = node.Parameters.Find(r => r.Key == "Name");
//                    if (gName.Value == "TargetScreen" && node.Inputs.Count > 0)
//                    {
//                        GConnector enter = node.Inputs[0];
//                        gComponent.Data.Connect(gc, enter);                        
//                    }
//                }
//                
//            }
            
          //  gc.Link.Add()

        }

        //addConnections

        

        base.EnterStart();   
    }
}
