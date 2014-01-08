﻿using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

[Serializable]
public class EndTutorialPanel : TutorialPanel {
	
	private GestureHelper.OnTap tapHandler;
	
	private bool unlocked = false;
	
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
        GParameter gName = Parameters.Find(r => r.Key == "Name");
        if (gName != null)
        {
            return "End Tutorial Panel: " + gName.Value;
        }
        return "End Tutorial Panel: Uninitialized";
    }
	
	public void UnlockHex() 
    {
		HexInfoManager info = GameObject.FindObjectOfType(typeof(HexInfoManager)) as HexInfoManager;
        if (info != null)
        {
            if (info.IsInOpenStage())
            {
                info.AnimExit();

                for (int i = 0; i < buttonData.Count; i++)
                {
                    if (buttonData[i].buttonName == "VersusHex")
                    {
                        if (buttonData[i].locked)
                        {
                            buttonData[i].locked = false;

                            GConnector gameExit = Outputs.Find(r => r.Name == "GameExit");

                            GraphComponent gComponent = GameObject.FindObjectOfType(typeof(GraphComponent)) as GraphComponent;

                            GConnector gc = NewOutput(buttonData[i].buttonName, "Flow");

                            if (gameExit.Link.Count > 0)
                            {
                                gComponent.Data.Connect(gc, gameExit.Link[0]);
                            }
							
							unlocked = true;

                        }

                        DynamicHexList list = (DynamicHexList)physicalWidgetRoot.GetComponentInChildren(typeof(DynamicHexList));
                        list.UpdateButtonList();

                        GestureHelper.onTap -= tapHandler;
                        break;
                    }
                }
            }
        }
	}
	
	public override void EnterStart ()
	{
		parentMachine.ForbidBack();
		
		InitialButtons();
		
		UnityEngine.Debug.Log("EndTutorialPanel: initial buttons set");
		
		base.EnterStart();
		
		tapHandler = new GestureHelper.OnTap(() => {
			UnlockHex();
		});
		
		GestureHelper.onTap += tapHandler;
	}

	
	public override void StateUpdate ()
	{
		base.StateUpdate ();
		
//		if(Input.GetTouch(0).phase == TouchPhase.Began) {
//			UnlockHex();
//		}
		
		if(unlocked) 
		{
			DataVault.Set("highlight", "This is now unlocked! Enter it to go to the main menu");
		}
	}
	
	public override void InitialButtons() {
		HexButtonData hbd = new HexButtonData();
        hbd.row = 0;
        hbd.column = 0;
        hbd.buttonName = "PointsHex";
        hbd.displayInfoData = false;
        hbd.onButtonCustomString = "500RP";
		
        buttonData.Add(hbd);
		
		hbd = new HexButtonData();
		hbd.row = -1;
        hbd.column = 0;
		hbd.buttonName = "CongratsHex";
		hbd.displayInfoData = false;
		hbd.onButtonCustomString = "Congrats!";
		
		buttonData.Add(hbd);
	}
	
	public override void OnHover(FlowButton button, bool justStarted) 
	{
		if (justStarted == true && button != null)
        {                        
			if (button.name == "EarnHex" && buttonData.Count == 3)
	        {
	            HexButtonData hbd = new HexButtonData();
	            hbd.row = 1;
	            hbd.column = 0;
	            hbd.buttonName = "UseHex";
	            hbd.displayInfoData = false;
	            hbd.onButtonCustomString = "RP is used";
				hbd.displayInfoData = false;
	
				elapsedTime = 0f;
	            buttonData.Add(hbd);
	        } else if (button.name == "UseHex" && buttonData.Count == 4)
	        {
	            HexButtonData hbd = new HexButtonData();
	            hbd.row = 1;
	            hbd.column = -1;
	            hbd.buttonName = "ChallengeHex";
	            hbd.displayInfoData = false;
	            hbd.onButtonCustomString = "To unlock new challenges.";
				hbd.displayInfoData = false;
	
				elapsedTime = 0f;
	            buttonData.Add(hbd);
	        } else if (button.name == "ChallengeHex" && buttonData.Count == 5)
	        {
	            HexButtonData hbd = new HexButtonData();
	            hbd.row = 0;
	            hbd.column = -1;
	            hbd.buttonName = "TryHex";
	            hbd.displayInfoData = false;
	            hbd.onButtonCustomString = "Try this one ^";
				hbd.displayInfoData = false;
	
				elapsedTime = 0f;
	            buttonData.Add(hbd);
	        } else if(button.name == "TryHex" && buttonData.Count == 6) 
			{
				HexButtonData hbd = new HexButtonData();
	            hbd.row = -1;
	            hbd.column = -1;
				hbd.locked = true;
	            hbd.buttonName = "VersusHex";
				hbd.imageName = "activity_versus";
	            hbd.displayInfoData = true;
				hbd.activityPrice = 0;
				hbd.activityName = "Challenge a Friend";
				hbd.activityContent = "Unlock the ability to accept challenges from friends";
				
				elapsedTime = 0f;
				shouldAdd = false;
				buttonData.Add(hbd);
			}
			
			DynamicHexList list = (DynamicHexList)physicalWidgetRoot.GetComponentInChildren(typeof(DynamicHexList));
	        list.UpdateButtonList();
		}
	}
	
	public override void AddFinalString()
	{
		DataVault.Set("highlight", "Highlight the hex and tap to unlock");
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
			
            buttonData.Add(hbd);

        } else if (buttonData.Count == 3)
        {
            HexButtonData hbd = new HexButtonData();
            hbd.row = 1;
            hbd.column = 0;
            hbd.buttonName = "UseHex";
            hbd.displayInfoData = false;
            hbd.onButtonCustomString = "RP is used";
			hbd.displayInfoData = false;

            buttonData.Add(hbd);
        } else if (buttonData.Count == 4)
        {
            HexButtonData hbd = new HexButtonData();
            hbd.row = 1;
            hbd.column = -1;
            hbd.buttonName = "ChallengeHex";
            hbd.displayInfoData = false;
            hbd.onButtonCustomString = "To unlock new challenges.";
			hbd.displayInfoData = false;

            buttonData.Add(hbd);
        } else if (buttonData.Count == 5)
        {
            HexButtonData hbd = new HexButtonData();
            hbd.row = 0;
            hbd.column = -1;
            hbd.buttonName = "TryHex";
            hbd.displayInfoData = false;
            hbd.onButtonCustomString = "Try this one ^";
			hbd.displayInfoData = false;

            buttonData.Add(hbd);
        } else if(buttonData.Count == 6) 
		{
			HexButtonData hbd = new HexButtonData();
            hbd.row = -1;
            hbd.column = -1;
			hbd.locked = true;
            hbd.buttonName = "VersusHex";
			hbd.imageName = "activity_versus";
            hbd.displayInfoData = true;
			hbd.activityPrice = 0;
			hbd.activityName = "Challenge a Friend";
			hbd.activityContent = "Unlock the ability to accept challenges from friends";
				
			shouldAdd = false;
			buttonData.Add(hbd);

		}

        DynamicHexList list = (DynamicHexList)physicalWidgetRoot.GetComponentInChildren(typeof(DynamicHexList));
        list.UpdateButtonList();
	}
	
	public override void Exited ()
	{
		base.Exited ();
		
		DataVault.Set("first_menu", "This is your race grid. More tiles coming soon");
	}
}
