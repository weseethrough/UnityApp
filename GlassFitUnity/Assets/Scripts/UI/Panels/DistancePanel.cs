﻿using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

[Serializable]
public class DistancePanel : HexPanel {
	
	GConnector gameExit;
	GConnector tutorialExit;
	string raceType;
	
	public DistancePanel() { }
    public DistancePanel(SerializationInfo info, StreamingContext ctxt)
        : base(info, ctxt)
    {
    }
	
	public override void EnterStart ()
	{
		ResetButtonData();
		
		//buttonData = new List<HexButtonData>();
		
		//some game types shouldn't allow picking of previous tracks.
		raceType = (string)DataVault.Get("race_type");
		
		tutorialExit = Outputs.Find(r => r.Name == "TutorialExit");
		gameExit = Outputs.Find(r => r.Name ==  "gameExit");
		
		GraphComponent gComponent = GameObject.FindObjectOfType(typeof(GraphComponent)) as GraphComponent;
				
		//Add hex buttons
		HexButtonData hbd = GetButtonAt(0, 0);
		
		if(hbd == null)
		{
			hbd = new HexButtonData();
			buttonData.Add(hbd);
		}
		
		hbd.column = 0;
		hbd.row = 0;
		hbd.buttonName = "ChooseHex";
		hbd.textNormal = "Choose distance";
		
		GConnector gc = NewOutput(hbd.buttonName, "Flow");
        gc.EventFunction = "SetFinish";

		if(raceType == "trainRescue")
		{
			//For the train game, just do 350m
			hbd = GetButtonAt(0, -1);
			
			if(hbd == null)
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
			
			hbd.column = 0;
			hbd.row = -1;
			hbd.buttonName = "350m";
			hbd.textNormal = "350m";
			
			gc = NewOutput(hbd.buttonName, "Flow");
	        gc.EventFunction = "SetFinish";			
			
			if(gameExit.Link.Count > 0) {
				gComponent.Data.Connect(gc, tutorialExit.Link[0]);
			}
			
			hbd = GetButtonAt(1, 0);
			
			if(hbd == null)
			{
				hbd = new HexButtonData();
				buttonData.Add(hbd);
			}
			
			//clear the others
			hbd = GetButtonAt(0,1);
			if(hbd == null) { 
				hbd = new HexButtonData();
				buttonData.Add(hbd);
			}
			
			hbd = GetButtonAt(-1,1);
			if(hbd == null) { 
				hbd = new HexButtonData();
				buttonData.Add(hbd);
			}
			hbd = GetButtonAt(-1,0);
			if(hbd == null) { 
				hbd = new HexButtonData();
				buttonData.Add(hbd);
			}
			hbd = GetButtonAt(-1,-1);
			if(hbd == null) { 
				hbd = new HexButtonData();
				buttonData.Add(hbd);
			}
			hbd = GetButtonAt(-2,0);
			if(hbd == null) { 
				hbd = new HexButtonData();
				buttonData.Add(hbd);
			}			
		}
		
		else
		{
			//regular distnace options
			hbd = GetButtonAt(0, -1);
			
			if(hbd == null)
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
			
			hbd.column = 0;
			hbd.row = -1;
			hbd.buttonName = "1km";
			hbd.textNormal = "1km";
			
			gc = NewOutput(hbd.buttonName, "Flow");
	        gc.EventFunction = "SetFinish";
			
			SetGConnector(gc, gComponent);
			
			hbd = GetButtonAt(1, 0);
			
			if(hbd == null)
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
			
			hbd.column = 1;
			hbd.row = 0;
			hbd.buttonName = "2km";
			hbd.textNormal = "2km";
			
			gc = NewOutput(hbd.buttonName, "Flow");
	        gc.EventFunction = "SetFinish";
			
			SetGConnector(gc, gComponent);
			
			hbd = GetButtonAt(0, 1);
			
			if(hbd == null)
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
			
			hbd.column = 0;
			hbd.row = 1;
			hbd.buttonName = "3km";
			hbd.textNormal = "3km";
			
			gc = NewOutput(hbd.buttonName, "Flow");
	        gc.EventFunction = "SetFinish";
			
			SetGConnector(gc, gComponent);
			
			hbd = GetButtonAt(-1, 1);
			
			if(hbd == null)
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
			
			hbd.column = -1;
			hbd.row = 1;
			hbd.buttonName = "4km";
			hbd.textNormal = "4km";
			
			gc = NewOutput(hbd.buttonName, "Flow");
	        gc.EventFunction = "SetFinish";
			
			SetGConnector(gc, gComponent);
			
			hbd = GetButtonAt(-1, 0);
			
			if(hbd == null)
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
			
			hbd.column = -1;
			hbd.row = 0;
			hbd.buttonName = "5km";
			hbd.textNormal = "5km";
			
			gc = NewOutput(hbd.buttonName, "Flow");
	        gc.EventFunction = "SetFinish";
			
			SetGConnector(gc, gComponent);
			
			hbd = GetButtonAt(-1, -1);
			
			if(hbd == null)
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
			
			hbd.column = -1;
			hbd.row = -1;
			hbd.buttonName = "10km";
			hbd.textNormal = "10km";
			
			gc = NewOutput(hbd.buttonName, "Flow");
	        gc.EventFunction = "SetFinish";
			
			SetGConnector(gc, gComponent);
			
			hbd = GetButtonAt(-2, 0);
			
			if(hbd == null)
			{
				hbd = new HexButtonData();
				buttonData.Add(hbd);
			}
		}
			
		hbd.column = -2;
		hbd.row = 0;
		hbd.buttonName = "CustomHex";
		hbd.textNormal = "Custom";
		hbd.locked = true;
		
		
		base.EnterStart ();
	}
	
	public void SetGConnector(GConnector gc, GraphComponent gComponent) {
		if(raceType == "tutorial" || raceType == "trainRescue") {
			if(tutorialExit.Link.Count > 0) {
				gComponent.Data.Connect(gc, tutorialExit.Link[0]);
			}
		} else {
			if(gameExit.Link.Count > 0) {
				gComponent.Data.Connect(gc, gameExit.Link[0]);
			}
		}
	}
}
