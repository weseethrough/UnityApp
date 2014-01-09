using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

[Serializable]
public class TutorialPanel : HexPanel
{
	protected float elapsedTime = 0.0f;
	
	protected bool shouldAdd = true;
	
	private float maxTime = 3.0f;
	
	private GameObject highlightText = null;
	
	private GestureHelper.ThreeFingerTap threeHandler = null;
    
	private GestureHelper.TwoFingerTap twoHandler = null;
	
	private Camera camera;

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
		UnityEngine.Debug.Log("TutorialPanel: setting two finger tap");
		
		DataVault.Set("rp", Platform.Instance.GetOpeningPointsBalance());
		DataVault.Set("metabolism", Platform.Instance.GetCurrentMetabolism());
		
		DataVault.Set("race_type", "tutorial");
		
		if(Platform.Instance.GetTracks(10000, 0) != null) {
			if(Platform.Instance.GetTracks(10000, 0).Count > 0) {		
				StraightToMenu();
			}
		}	
        twoHandler = new GestureHelper.TwoFingerTap(() => {
			if(buttonData.Count == 0) {
				InitialButtons();
				elapsedTime = 0.0f;
			}
			GestureHelper.onTwoTap -= twoHandler;
		});
		
		GestureHelper.onTwoTap += twoHandler;
		
		UnityEngine.Debug.Log("TutorialPanel: Setting datavault stuff");
		DataVault.Set("tutorial_hint", " ");
		DataVault.Set("highlight", " ");
		
		UnityEngine.Debug.Log("TutorialPanel: Setting three finger tap");
		threeHandler = new GestureHelper.ThreeFingerTap(() => {
			StraightToMenu();
		});
		
		GestureHelper.onThreeTap += threeHandler;
		
		UnityEngine.Debug.Log("TutorialPanel: about to find camera");
		GameObject cam = GameObject.Find("Scene Camera");
		
		if(cam != null) {
			camera = cam.camera;
		}
		
		UnityEngine.Debug.Log("TutorialPanel: camera found, about to enter base");
        base.EnterStart();
    }
	
	public void StraightToMenu() {
		GConnector gConnect = Outputs.Find(r => r.Name == "MenuExit");
		if(gConnect != null) 
		{
			GestureHelper.onThreeTap -= threeHandler;
			parentMachine.FollowConnection(gConnect);
		} else 
		{
			UnityEngine.Debug.Log("TutorialPanel: Error finding menu exit");
		}
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
			AddFinalString();
		}
		
		if(camera != null) 
		{
			if((camera.transform.rotation.eulerAngles.x > 30 && camera.transform.rotation.eulerAngles.x < 330) || (camera.transform.rotation.eulerAngles.y > 40 && camera.transform.rotation.eulerAngles.y < 320)) {
				DataVault.Set("tutorial_hint", "Tap with two fingers to center view");
				LoadingTextComponent.SetVisibility(true);
			}
			else
			{
				string tutHint = (string)DataVault.Get("tutorial_hint");
				if(tutHint == "Tap with two fingers to center view") {
					DataVault.Set("tutorial_hint", " ");
				}
				
			}
		}
		
		if(buttonData.Count == 0) 
		{
			DataVault.Set("tutorial_hint", "Tap with two fingers to center view");
		}
		
	}
	
	public virtual void AddFinalString()
	{
		DataVault.Set("highlight", "Highlight the hex and tap to start");
	}
	
	public virtual void AddButton() 
	{
		if (buttonData.Count == 1)
        {
            HexButtonData hbd = new HexButtonData();
            hbd.row = -1;
            hbd.column = 0;
            hbd.buttonName = "LookHex";
            hbd.displayInfoData = false;
            hbd.onButtonCustomString = "Look at this hex to select it";
            hbd.allowEarlyHover = true;
			
			shouldAdd = false;
			
            buttonData.Add(hbd);

        } else if (buttonData.Count == 3)
        {
            HexButtonData hbd = new HexButtonData();
            hbd.row = 1;
            hbd.column = 0;
            hbd.buttonName = "TheseHex";
            hbd.displayInfoData = false;
            hbd.onButtonCustomString = "These hexes";
            hbd.allowEarlyHover = true;

            buttonData.Add(hbd);
        } else if (buttonData.Count == 4)
        {
            HexButtonData hbd = new HexButtonData();
            hbd.row = 1;
            hbd.column = -1;
            hbd.buttonName = "ChallengeHex";
            hbd.displayInfoData = false;
            hbd.onButtonCustomString = "are challenges";
            hbd.allowEarlyHover = true;

            buttonData.Add(hbd);
        } else if (buttonData.Count == 5)
        {
            HexButtonData hbd = new HexButtonData();
            hbd.row = 0;
            hbd.column = -1;
            hbd.buttonName = "TryHex";
            hbd.displayInfoData = false;
            hbd.onButtonCustomString = "Try this one ^";
            hbd.allowEarlyHover = true;

            buttonData.Add(hbd);
        } else if(buttonData.Count == 6) 
		{
			HexButtonData hbd = new HexButtonData();
            hbd.row = -1;
            hbd.column = -1;
            hbd.buttonName = "TryHex";
			hbd.imageName = "activity_run";
            hbd.displayInfoData = true;
			hbd.activityName = "First Race";
			hbd.activityContent = "Tutorial for the game. Race against James, the virtual trainer and sample some of the other games available to unlock";
			
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
    public virtual void InitialButtons()
    {
        HexButtonData hbd = new HexButtonData();
        hbd.row = 0;
        hbd.column = 0;
        hbd.buttonName = "tutorialButton1";
        hbd.displayInfoData = false;
        hbd.onButtonCustomString = "Welcome";
        hbd.allowEarlyHover = true;
		
        buttonData.Add(hbd);
		
		DynamicHexList list = (DynamicHexList)physicalWidgetRoot.GetComponentInChildren(typeof(DynamicHexList));
        list.UpdateButtonList();
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
                hbd.allowEarlyHover = true;
				
				elapsedTime = 0f;
				shouldAdd = true;
				
	            buttonData.Add(hbd);
			} 
			else if (button.name == "NiceHex" && buttonData.Count == 3)
	        {
	            HexButtonData hbd = new HexButtonData();
	            hbd.row = 1;
	            hbd.column = 0;
	            hbd.buttonName = "TheseHex";
	            hbd.displayInfoData = false;
	            hbd.onButtonCustomString = "These hexes";
                hbd.allowEarlyHover = true;
				
				elapsedTime = 0f;
	            buttonData.Add(hbd);
	        } else if (button.name == "TheseHex" && buttonData.Count == 4)
	        {
	            HexButtonData hbd = new HexButtonData();
	            hbd.row = 1;
	            hbd.column = -1;
	            hbd.buttonName = "ChallengeHex";
	            hbd.displayInfoData = false;
	            hbd.onButtonCustomString = "are challenges";
                hbd.allowEarlyHover = true;
	
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
				hbd.allowEarlyHover = true;
	
				elapsedTime = 0f;
	            buttonData.Add(hbd);
	        } else if(button.name == "TryHex" && buttonData.Count == 6) 
			{
				HexButtonData hbd = new HexButtonData();
	            hbd.row = -1;
	            hbd.column = -1;
	            hbd.buttonName = "TryHex";
				hbd.imageName = "activity_run";
	            hbd.displayInfoData = true;
				hbd.activityName = "First Race";
				hbd.activityContent = "Tutorial for the game. Race against James, the virtual trainer and sample some of the other games available to unlock";
				
	            elapsedTime = 0f;
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
	
	public override void Exited ()
	{
		base.Exited ();
		GestureHelper.onThreeTap -= threeHandler;
		GestureHelper.onTwoTap -= twoHandler;
		DataVault.Set("highlight", " ");
	}
}
