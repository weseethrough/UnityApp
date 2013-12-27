using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

[Serializable]
public class RaceTypePanel : HexPanel {
	
	public RaceTypePanel() {}
	public RaceTypePanel(SerializationInfo info, StreamingContext ctxt)
        : base(info, ctxt)
    {
	}
	
	public override void EnterStart ()
	{
		GConnector presetExit = Outputs.Find(r => r.Name == "presetExit");
		GConnector trackExit = Outputs.Find(r => r.Name == "trackExit");
		
		GraphComponent gComponent = GameObject.FindObjectOfType(typeof(GraphComponent)) as GraphComponent;
		
		HexButtonData hbd = new HexButtonData();
		hbd.row = 0;
		hbd.column = 0;
		hbd.buttonName = "TrackHex";
		hbd.onButtonCustomString = "Race a previous track";
		hbd.displayInfoData = false;
		
		GConnector gc = NewOutput(hbd.buttonName, "Flow");
		
		if(trackExit.Link.Count > 0) 
		{
			gComponent.Data.Connect(gc, trackExit);
		}
		
		buttonData.Add(hbd);
		
		hbd = new HexButtonData();
		hbd.row = 0;
		hbd.column = 1;
		hbd.buttonName = "PresetHex";
		hbd.onButtonCustomString = "Create a new track";
		hbd.displayInfoData = false;
		
		gc = NewOutput(hbd.buttonName, "Flow");
		
		if(presetExit.Link.Count > 0) 
		{
			gComponent.Data.Connect(gc, presetExit);
		}
		
		buttonData.Add(hbd);
		
		base.EnterStart ();
	}
}
