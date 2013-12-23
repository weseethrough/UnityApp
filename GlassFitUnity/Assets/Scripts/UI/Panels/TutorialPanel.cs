using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

[Serializable]
public class TutorialPanel : HexPanel
{
	private float elapsedTime = 0.0f;
	
	private bool shouldAdd = true;
	
	private float maxTime = 2.0f;
    //	PlatformDummy platform = new PlatformDummy();

    public TutorialPanel() { }
    public TutorialPanel(SerializationInfo info, StreamingContext ctxt)
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
            return "Tutorial Panel: " + gName.Value;
        }
        return "Tutorial Panel: UnInitialzied";
    }

    /// <summary>
    /// Primary button preparation for panel entering state
    /// </summary>
    /// <returns></returns>
    public override void EnterStart()
    {
        InitialButtons();

        base.EnterStart();
    }
	
	public override void StateUpdate ()
	{
		base.StateUpdate ();
		
		elapsedTime += Time.deltaTime;
		
		if(elapsedTime > maxTime && shouldAdd)
		{
			AddButton();
			elapsedTime -= maxTime;
		}
		
		if(elapsedTime > 10.0f && buttonData.Count == 7) 
		{
			AddFinalButton();
		}
	}
	
	public void AddFinalButton()
	{
		HexButtonData hbd = new HexButtonData();
        hbd.row = 0;
        hbd.column = -2;
        hbd.buttonName = "FinalHex";
		hbd.displayInfoData = false;
		hbd.onButtonCustomString = "Highlight the hex and tap to start";
		
		buttonData.Add(hbd);
		
		DynamicHexList list = (DynamicHexList)physicalWidgetRoot.GetComponentInChildren(typeof(DynamicHexList));
        list.UpdateButtonList();
	}
	
	public void AddButton() 
	{
		if (buttonData.Count == 1)
        {
            HexButtonData hbd = new HexButtonData();
            hbd.row = -1;
            hbd.column = 0;
            hbd.buttonName = "LookHex";
            hbd.displayInfoData = false;
            hbd.onButtonCustomString = "Look at this hex to select it";
			
			shouldAdd = false;
			
            buttonData.Add(hbd);

        } else if (buttonData.Count == 3)
        {
            HexButtonData hbd = new HexButtonData();
            hbd.row = 1;
            hbd.column = 1;
            hbd.buttonName = "TheseHex";
            hbd.displayInfoData = false;
            hbd.onButtonCustomString = "These hexes";

            buttonData.Add(hbd);
        } else if (buttonData.Count == 4)
        {
            HexButtonData hbd = new HexButtonData();
            hbd.row = 1;
            hbd.column = 0;
            hbd.buttonName = "ChallengeHex";
            hbd.displayInfoData = false;
            hbd.onButtonCustomString = "are challenges";

            buttonData.Add(hbd);
        } else if (buttonData.Count == 5)
        {
            HexButtonData hbd = new HexButtonData();
            hbd.row = 1;
            hbd.column = -1;
            hbd.buttonName = "TryHex";
            hbd.displayInfoData = false;
            hbd.onButtonCustomString = "Try this one ^";

            buttonData.Add(hbd);
        } else if(buttonData.Count == 6) 
		{
			HexButtonData hbd = new HexButtonData();
            hbd.row = 0;
            hbd.column = -1;
            hbd.buttonName = "TryHex";
			hbd.imageName = "activity_run";
            hbd.displayInfoData = false;
			
            //hbd.onButtonCustomString = "Try this one ^";
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

    /// <summary>
    /// Initial button definitions
    /// </summary>
    /// <returns></returns>
    public void InitialButtons()
    {
        HexButtonData hbd = new HexButtonData();
        hbd.row = 0;
        hbd.column = 0;
        hbd.buttonName = "tutorialButton1";
        hbd.displayInfoData = false;
        hbd.onButtonCustomString = "Welcome";

        buttonData.Add(hbd);
    }

    /// <summary>
    /// On hover activity opening further buttons. Example shows opening two buttons first and then one after another based on rollover action
    /// </summary>
    /// <param name="button">button which triggered action</param>
    /// <param name="justStarted">is ti hover in(true) or hover out(false)</param>
    /// <returns></returns>
    public override void OnHover(FlowButton button, bool justStarted)
    {
 	     base.OnHover(button, justStarted);

		if (justStarted == true && button != null)
        {                        
			if(button.name == "LookHex" && buttonData.Count == 2)
			{
				HexButtonData hbd = new HexButtonData();
	            hbd.row = 0;
	            hbd.column = 1;
	            hbd.buttonName = "NiceHex";
	            hbd.displayInfoData = false;
	            hbd.onButtonCustomString = "Nice!";
				
				elapsedTime = 0f;
				shouldAdd = true;
				
	            buttonData.Add(hbd);
			}
	        
			DynamicHexList list = (DynamicHexList)physicalWidgetRoot.GetComponentInChildren(typeof(DynamicHexList));
	        list.UpdateButtonList();
		}
         
    }            
}
