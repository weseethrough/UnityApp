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
		GConnectorBase presetExit = Outputs.Find(r => r.Name == "presetExit");
		GConnectorBase trackExit = Outputs.Find(r => r.Name == "trackExit");
		
		GraphComponent gComponent = GameObject.FindObjectOfType(typeof(GraphComponent)) as GraphComponent;
		
		HexButtonData hbd = GetButtonAt(0, 0);
        //if we do not have button at provided coordinates we will create new button data for it
        if (hbd == null)
        {
            hbd = new HexButtonData();
			buttonData.Add(hbd);
        }
        else
        {
            //disconnect old connection which could possibly change. Also we don't want to double it if it doesn't change
            GConnectorBase oldConnection =  Outputs.Find(r => r.Name == hbd.buttonName);
            if (oldConnection != null)
            {
                gComponent.Data.Disconnect(oldConnection);
                Outputs.Remove(oldConnection);
            }
        }
		
		hbd.row = 0;
		hbd.column = 0;
		hbd.buttonName = "TrackHex";
		hbd.textNormal = "Race a previous track";
		hbd.displayInfoData = false;
		
		GConnectorBase gc = NewOutput(hbd.buttonName, "Flow");
		
		if(trackExit.Link.Count > 0) 
		{
			gc.EventFunction = "CheckTracks";
			gComponent.Data.Connect(gc, trackExit.Link[0]);
		}
		
		
		hbd = GetButtonAt(1, 0);
        //if we do not have button at provided coordinates we will create new button data for it
        if (hbd == null)
        {
            hbd = new HexButtonData();
			buttonData.Add(hbd);
        }
        else
        {
            //disconnect old connection which could possibly change. Also we don't want to double it if it doesn't change
            GConnectorBase oldConnection =  Outputs.Find(r => r.Name == hbd.buttonName);
            if (oldConnection != null)
            {
                gComponent.Data.Disconnect(oldConnection);
                Outputs.Remove(oldConnection);
            }
        }
		
		//hbd = new HexButtonData();
		hbd.row = 0;
		hbd.column = 1;
		hbd.buttonName = "PresetHex";
		hbd.textNormal = "Create a new track";
		hbd.displayInfoData = false;
		
		gc = NewOutput(hbd.buttonName, "Flow");
		
		if(presetExit.Link.Count > 0) 
		{
			UnityEngine.Debug.Log("RaceTypePanel: preset exit found");
			gc.EventFunction = "StartPresetSpeed";
			gComponent.Data.Connect(gc, presetExit.Link[0]);
		}
		
		//buttonData.Add(hbd);
		
		base.EnterStart ();
	}
}
