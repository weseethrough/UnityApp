using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

[Serializable]
public class EndTutorialPanel : TutorialPanel {
	
//	private float elapsedTime = 0.0f;
//	
//	private bool shouldAdd = true;
//	
//	private float maxTime = 3.0f;
	
	public EndTutorialPanel() { }
    public EndTutorialPanel(SerializationInfo info, StreamingContext ctxt)
        : base(info, ctxt)
    {
    }
	
	/// <summary>
    /// Gets display name of the node, helps with node identification in editor
    /// </summary>
    /// <returns>name of the node</returns>
    public override string GetDisplayName()
    {
        //base.GetDisplayName();

        GParameter gName = Parameters.Find(r => r.Key == "Name");
        if (gName != null)
        {
            return "End Tutorial Panel: " + gName.Value;
        }
        return "End Tutorial Panel: Uninitialized";
    }

	
	public override void InitialButtons() {
		HexButtonData hbd = new HexButtonData();
        hbd.row = 0;
        hbd.column = 0;
        hbd.buttonName = "PointsHex";
        hbd.displayInfoData = false;
        hbd.onButtonCustomString = "500RP";
		hbd.displayInfoData = false;
		
        buttonData.Add(hbd);
		
		hbd = new HexButtonData();
		hbd.row = -1;
        hbd.column = 0;
		hbd.buttonName = "CongratsHex";
		hbd.displayInfoData = false;
		hbd.onButtonCustomString = "Congrats!";
		hbd.displayInfoData = false;
		
		buttonData.Add(hbd);
	}
	
	public override void OnHover(FlowButton button, bool justStarted) 
	{
		
	}
	
	public override void AddFinalButton()
	{
		
		
	}
	
	public override void AddButton ()
	{
		if (buttonData.Count == 2)
        {
            HexButtonData hbd = new HexButtonData();
            hbd.row = 0;
            hbd.column = 1;
            hbd.buttonName = "EarnHex";
            hbd.displayInfoData = false;
            hbd.onButtonCustomString = "You earned 500RP!";
			hbd.displayInfoData = false;
			
			shouldAdd = false;
			
            buttonData.Add(hbd);

        } else if (buttonData.Count == 3)
        {
            HexButtonData hbd = new HexButtonData();
            hbd.row = 1;
            hbd.column = 1;
            hbd.buttonName = "UseHex";
            hbd.displayInfoData = false;
            hbd.onButtonCustomString = "RP is used";
			hbd.displayInfoData = false;

            buttonData.Add(hbd);
        } else if (buttonData.Count == 4)
        {
            HexButtonData hbd = new HexButtonData();
            hbd.row = 1;
            hbd.column = 0;
            hbd.buttonName = "ChallengeHex";
            hbd.displayInfoData = false;
            hbd.onButtonCustomString = "To unlock new challenges.";
			hbd.displayInfoData = false;

            buttonData.Add(hbd);
        } else if (buttonData.Count == 5)
        {
            HexButtonData hbd = new HexButtonData();
            hbd.row = 1;
            hbd.column = -1;
            hbd.buttonName = "TryHex";
            hbd.displayInfoData = false;
            hbd.onButtonCustomString = "Try this one ^";
			hbd.displayInfoData = false;

            buttonData.Add(hbd);
        } else if(buttonData.Count == 6) 
		{
			HexButtonData hbd = new HexButtonData();
            hbd.row = 0;
            hbd.column = -1;
			hbd.locked = true;
            hbd.buttonName = "VersusHex";
			hbd.imageName = "activity_versus";
            hbd.displayInfoData = true;
			hbd.activityName = "Challenge a Friend";
			hbd.activityContent = "Unlock the ability to accept challenges from friends";
			
			shouldAdd = false;
			buttonData.Add(hbd);
			
			GConnector gameExit = Outputs.Find(r => r.Name == "GameExit");
			
			GraphComponent gComponent = GameObject.FindObjectOfType(typeof(GraphComponent)) as GraphComponent;
			
			GConnector gc = NewOutput(hbd.buttonName, "Flow");
            gc.EventFunction = "SetTutorial";
			
			if(gameExit.Link.Count > 0) {
				gComponent.Data.Connect(gc, gameExit.Link[0]);
			}
		}

        DynamicHexList list = (DynamicHexList)physicalWidgetRoot.GetComponentInChildren(typeof(DynamicHexList));
        list.UpdateButtonList();
		
	}
}
