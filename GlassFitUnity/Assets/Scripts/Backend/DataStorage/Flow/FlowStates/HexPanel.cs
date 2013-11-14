using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

[Serializable]
public class HexPanel : Panel 
{
    const string defaultExit = "Default Exit";

    public List<HexButtonData> buttonData;        
    private bool camStartMouseAction;
    private bool camStartTouchAction;

    public HexPanel()
        : base()
    {
        buttonData = new List<HexButtonData>();
    }
    public HexPanel(SerializationInfo info, StreamingContext ctxt)
        : base(info, ctxt)
    {
        try
        {
            this.buttonData = info.GetValue("buttonData", typeof(List<HexButtonData>)) as List<HexButtonData>;
        }
        catch (System.Exception ex)
        {
            this.buttonData = null;
        }
        
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext ctxt)
    {
        base.GetObjectData(info, ctxt);

        info.AddValue("buttonData", this.buttonData);
    }

    public override string GetDisplayName()
    {
        //base.GetDisplayName();

        GParameter gName = Parameters.Find(r => r.Key == "Name");
        if (gName != null)
        {
            return "HEX Panel: " + gName.Value;
        }
        return "HEX Panel: UnInitialzied";
    }

    protected override void Initialize()
    {
        base.Initialize();

        NewOutput(defaultExit, "Flow");
        NewParameter("HexListManager", GraphValueType.HexButtonManager, "true"); //fake variable just to trigger option visibility on graph editor

    }

    public override void RebuildConnections()
    {
        base.RebuildConnections();

        NewOutput(defaultExit, "Flow");
    }

    public override void EnterStart()
    {
        base.EnterStart();

        if (physicalWidgetRoot != null)
        {
            DynamicHexList list = (DynamicHexList)physicalWidgetRoot.GetComponentInChildren(typeof(DynamicHexList));            
            list.SetParent(this);
            list.SetButtonCount(7);
        }
        

#if !UNITY_EDITOR         
        UICamera uicam = Camera.FindObjectOfType(typeof(UICamera)) as UICamera;
        
        if (uicam != null)
        {            
            camStartMouseAction = uicam.useMouse;
            camStartTouchAction = uicam.useTouch;                        

            uicam.useMouse = false;
            uicam.useTouch = false;                
        }                   
#endif

    }

    public override void ExitStart()
    {
        base.ExitStart();

        Debug.Log("Panel Exit");
    }

    public override void Exited()
    {
        base.Exited();

        UICamera cam = GameObject.FindObjectOfType(typeof(UICamera)) as UICamera;
        if (cam != null)
        {
            cam.transform.rotation = Quaternion.identity;
        }

        DynamicHexList dh = physicalWidgetRoot.GetComponentInChildren<DynamicHexList>();
        if (dh != null)
        {
            dh.OnExit();
        }

#if !UNITY_EDITOR            
        UICamera uicam = Camera.FindObjectOfType(typeof(UICamera)) as UICamera;
        
        if (uicam != null)
        {            
            uicam.useMouse = camStartMouseAction;
            uicam.useTouch = camStartTouchAction;                                    
        }             
#endif
    }

    public override void OnClick(FlowButton button)
    {
        //base.OnClick(button);

        if (Outputs.Count > 0 && parentMachine != null)
        {
            GConnector gConect = Outputs.Find(r => r.Name == button.name);
            if (gConect != null)
            {
                parentMachine.FollowConnection(gConect);
            }
            else
            {
                gConect = Outputs.Find(r => r.Name == defaultExit);
                if (gConect != null)
                {
                    parentMachine.FollowConnection(gConect);
                }
            }
        }
        else
        {
            Debug.LogError("Dead end");
        }
    }
}
