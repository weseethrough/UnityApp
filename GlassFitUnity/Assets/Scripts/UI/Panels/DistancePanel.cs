using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

[Serializable]
public class DistancePanel : HexPanel {

	public DistancePanel() { }
    public DistancePanel(SerializationInfo info, StreamingContext ctxt)
        : base(info, ctxt)
    {
    }
	
	public override void EnterStart ()
	{
		GConnector gameExit;
		if((string)DataVault.Get("race_type") == "tutorial") {
			gameExit = Outputs.Find(r => r.Name == "TutorialExit");
		} else {
			gameExit = Outputs.Find(r => r.Name ==  "gameExit");
		}
		GraphComponent gComponent = GameObject.FindObjectOfType(typeof(GraphComponent)) as GraphComponent;
		
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
		
		if(gameExit.Link.Count > 0) {
			gComponent.Data.Connect(gc, gameExit.Link[0]);
		}
		
		hbd = GetButtonAt(0, -1);
		
		if(hbd == null)
		{
			hbd = new HexButtonData();
			buttonData.Add(hbd);
		}
		
		hbd.column = 0;
		hbd.row = -1;
		hbd.buttonName = "1km";
		hbd.textNormal = "1km";
		
		gc = NewOutput(hbd.buttonName, "Flow");
        gc.EventFunction = "SetFinish";
		
		if(gameExit.Link.Count > 0) {
			gComponent.Data.Connect(gc, gameExit.Link[0]);
		}
		
		hbd = GetButtonAt(1, 0);
		
		if(hbd == null)
		{
			hbd = new HexButtonData();
			buttonData.Add(hbd);
		}
		
		hbd.column = 1;
		hbd.row = 0;
		hbd.buttonName = "2km";
		hbd.textNormal = "2km";
		
		gc = NewOutput(hbd.buttonName, "Flow");
        gc.EventFunction = "SetFinish";
		
		if(gameExit.Link.Count > 0) {
			gComponent.Data.Connect(gc, gameExit.Link[0]);
		}
		
		hbd = GetButtonAt(0, 1);
		
		if(hbd == null)
		{
			hbd = new HexButtonData();
			buttonData.Add(hbd);
		}
		
		hbd.column = 0;
		hbd.row = 1;
		hbd.buttonName = "3km";
		hbd.textNormal = "3km";
		
		gc = NewOutput(hbd.buttonName, "Flow");
        gc.EventFunction = "SetFinish";
		
		if(gameExit.Link.Count > 0) {
			gComponent.Data.Connect(gc, gameExit.Link[0]);
		}
		
		hbd = GetButtonAt(-1, 1);
		
		if(hbd == null)
		{
			hbd = new HexButtonData();
			buttonData.Add(hbd);
		}
		
		hbd.column = -1;
		hbd.row = 1;
		hbd.buttonName = "4km";
		hbd.textNormal = "4km";
		
		gc = NewOutput(hbd.buttonName, "Flow");
        gc.EventFunction = "SetFinish";
		
		if(gameExit.Link.Count > 0) {
			gComponent.Data.Connect(gc, gameExit.Link[0]);
		}
		
		hbd = GetButtonAt(-1, 0);
		
		if(hbd == null)
		{
			hbd = new HexButtonData();
			buttonData.Add(hbd);
		}
		
		hbd.column = -1;
		hbd.row = 0;
		hbd.buttonName = "5km";
		hbd.textNormal = "5km";
		
		gc = NewOutput(hbd.buttonName, "Flow");
        gc.EventFunction = "SetFinish";
		
		if(gameExit.Link.Count > 0) {
			gComponent.Data.Connect(gc, gameExit.Link[0]);
		}
		
		hbd = GetButtonAt(-1, -1);
		
		if(hbd == null)
		{
			hbd = new HexButtonData();
			buttonData.Add(hbd);
		}
		
		hbd.column = -1;
		hbd.row = -1;
		hbd.buttonName = "10km";
		hbd.textNormal = "10km";
		
		gc = NewOutput(hbd.buttonName, "Flow");
        gc.EventFunction = "SetFinish";
		
		if(gameExit.Link.Count > 0) {
			gComponent.Data.Connect(gc, gameExit.Link[0]);
		}
		
		hbd = GetButtonAt(-2, 0);
		
		if(hbd == null)
		{
			hbd = new HexButtonData();
			buttonData.Add(hbd);
		}
		
		hbd.column = -2;
		hbd.row = 0;
		hbd.buttonName = "CustomHex";
		hbd.textNormal = "Custom";
		hbd.locked = true;
		
		base.EnterStart ();
	}
}
