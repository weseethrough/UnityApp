using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

[Serializable]
public class TutorialPanel : HexPanel
{
	private float elapsedTime = 0.0f;
	
	private float maxTime = 5.0f;
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
		
		if(elapsedTime > maxTime)
		{
			AddButton();
			elapsedTime -= maxTime;
		}
	}
	
	public void AddButton() 
	{
		if (buttonData.Count == 1)
        {
            HexButtonData hbd = new HexButtonData();
            hbd.row = 1;
            hbd.column = 0;
            hbd.buttonName = "tutorialButton2";
            hbd.displayInfoData = false;
            hbd.onButtonCustomString = "Look here";

            buttonData.Add(hbd);
   
			hbd = new HexButtonData();
            hbd.row = 1;
            hbd.column = 1;
            hbd.buttonName = "tutorialButton3";
            hbd.displayInfoData = false;
            hbd.onButtonCustomString = "move your head to navigate in this menu";

            buttonData.Add(hbd);

        }

        if (buttonData.Count == 2)
        {
            HexButtonData hbd = new HexButtonData();
            hbd.row = -1;
            hbd.column = 0;
            hbd.buttonName = "tutorialButton4";
            hbd.displayInfoData = false;
            hbd.onButtonCustomString = "Some more important info";

            buttonData.Add(hbd);
        }

        if (buttonData.Count == 3)
        {
            HexButtonData hbd = new HexButtonData();
            hbd.row = 0;
            hbd.column = -1;
            hbd.buttonName = "tutorialButton5";
            hbd.displayInfoData = false;
            hbd.onButtonCustomString = "Some more important info";

            buttonData.Add(hbd);
        }

        if (buttonData.Count == 4)
        {
            HexButtonData hbd = new HexButtonData();
            hbd.row = 1;
            hbd.column = -1;
            hbd.buttonName = "tutorialButton6";
            hbd.displayInfoData = false;
            hbd.onButtonCustomString = "Some more important info";

            buttonData.Add(hbd);
        }

        if (buttonData.Count == 5)
        {
            HexButtonData hbd = new HexButtonData();
            hbd.row = 1;
            hbd.column = 0;
            hbd.buttonName = "tutorialButton7";
            hbd.displayInfoData = false;
            hbd.onButtonCustomString = "Some more important info";

            buttonData.Add(hbd);
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
            if (button.name == "tutorialButton1" && buttonData.Count == 1)
            {
                HexButtonData hbd = new HexButtonData();
                hbd.row = 1;
                hbd.column = 1;
                hbd.buttonName = "tutorialButton2";
                hbd.displayInfoData = false;
                hbd.onButtonCustomString = "Look here";

                buttonData.Add(hbd);

                hbd = new HexButtonData();
                hbd.row = 0;
                hbd.column = 1;
                hbd.buttonName = "tutorialButton3";
                hbd.displayInfoData = false;
                hbd.onButtonCustomString = "move your head to navigate in this menu";

                buttonData.Add(hbd);

            }

            if (button.name == "tutorialButton2" && buttonData.Count == 3)
            {
                HexButtonData hbd = new HexButtonData();
                hbd.row = -1;
                hbd.column = 0;
                hbd.buttonName = "tutorialButton4";
                hbd.displayInfoData = false;
                hbd.onButtonCustomString = "Some more important info";

                buttonData.Add(hbd);
            }

            if (button.name == "tutorialButton4" && buttonData.Count == 4)
            {
                HexButtonData hbd = new HexButtonData();
                hbd.row = 0;
                hbd.column = -1;
                hbd.buttonName = "tutorialButton5";
                hbd.displayInfoData = false;
                hbd.onButtonCustomString = "Some more important info";

                buttonData.Add(hbd);
            }

            if (button.name == "tutorialButton5" && buttonData.Count == 5)
            {
                HexButtonData hbd = new HexButtonData();
                hbd.row = 1;
                hbd.column = -1;
                hbd.buttonName = "tutorialButton6";
                hbd.displayInfoData = false;
                hbd.onButtonCustomString = "Some more important info";

                buttonData.Add(hbd);
            }

            if (button.name == "tutorialButton6" && buttonData.Count == 6)
            {
                HexButtonData hbd = new HexButtonData();
                hbd.row = 1;
                hbd.column = 0;
                hbd.buttonName = "tutorialButton7";
                hbd.displayInfoData = false;
                hbd.onButtonCustomString = "Some more important info";

                buttonData.Add(hbd);
            }            

            DynamicHexList list = (DynamicHexList)physicalWidgetRoot.GetComponentInChildren(typeof(DynamicHexList));
            list.UpdateButtonList();
        }
         
    }            
}
