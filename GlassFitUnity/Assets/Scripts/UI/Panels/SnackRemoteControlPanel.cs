using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System;

using SimpleJSON;
using RaceYourself.Models;

[Serializable]
public class SnackRemoteControlPanel : HexPanel {
	
	HexButtonData currentActiveSnackHex = null;
	
	public SnackRemoteControlPanel() { }
	
    public SnackRemoteControlPanel(SerializationInfo info, StreamingContext ctxt)
        : base(info, ctxt)
    {
    }
	
	public override void EnterStart ()
	{	
		
		//Create hexes
		
		List<Game> games = SnackController.getSnackGames();
		
		//central hex says 'launch snack'
		Vector2i coords = getHexCoordsForSpiralIndex(0);
		HexButtonData hbd = GetButtonAt(coords.x, coords.y);
		if(hbd == null)
		{
			hbd = new HexButtonData();
			buttonData.Add(hbd);
		}
		hbd.column = 0;
		hbd.row = 0;
		hbd.buttonName = "ChooseHex";
		hbd.textNormal = "Select a snack";
		
		for(int i=0; i<games.Count; i++)
		{
			coords = getHexCoordsForSpiralIndex(i+1);
			HexButtonData hdata = GetButtonAt(coords.x, coords.y);
			//create and add the hbd if necessary
			if(hdata == null)
			{
				hdata = new HexButtonData();
				buttonData.Add(hdata);
			}
			
			//fill out fields
			hdata.column = coords.x;
			hdata.row = coords.y;
			
			hdata.buttonName = games[i].gameId;
			hdata.activityName = games[i].name;
			hdata.activityContent = games[i].description;
			hdata.activityPrice = (int)games[i].priceInPoints;
            hdata.imageName = games[i].iconName;
		}
		
		base.EnterStart();
	}
	
	public override void OnClick (FlowButton button)
	{
		//clear currently highlighted hex
		ClearCurrentSnackHex();
		
		//send a bluetooth message to start the game
		string gameID = button.name;
		
        JSONClass json = new JSONClass();
		json.Add("action", "InitiateSnack");
		json.Add("snack_gameID", button.name);
		Platform.Instance.BluetoothBroadcast(json.ToString());
		MessageWidget.AddMessage("Bluetooth", "Launching Snack on Glass", "settings");
		
		UnityEngine.Debug.Log("SnackRemote: Launching snack " + button.name);
		
		//turn hex green
		HexButtonData hbd = buttonData.Find( r => r.buttonName == button.name );
		hbd.backgroundTileColor = 0x00A30EFF;
		//refresh display
		hbd.markedForVisualRefresh = true;
		currentActiveSnackHex = hbd;
		DynamicHexList list = (DynamicHexList)physicalWidgetRoot.GetComponentInChildren(typeof(DynamicHexList));
		list.UpdateButtonList();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public override void OnBack ()
	{
		//base goes back on the flow state. Instead, we just want to reset the flow to the start
		FlowStateMachine.Restart("Start Point");
	}
	
	public void ClearCurrentSnackHex() {
		UnityEngine.Debug.Log("SnackRemoteController: Clearing current snack on Remote Control panel");
		if(currentActiveSnackHex != null)
		{
			currentActiveSnackHex.backgroundTileColor = 0x00000000;
			//refresh
			currentActiveSnackHex.markedForVisualRefresh = true;
			DynamicHexList list = (DynamicHexList)physicalWidgetRoot.GetComponentInChildren(typeof(DynamicHexList));
			list.UpdateButtonList();
			currentActiveSnackHex = null;
		}
	}
}
