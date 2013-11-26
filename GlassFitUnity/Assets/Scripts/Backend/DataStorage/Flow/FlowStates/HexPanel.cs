using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

[Serializable]
public class HexPanel : Panel 
{
    public const string CAMERA_3D_LAYER = "GUI";
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
        foreach (SerializationEntry entry in info)
        {
            switch (entry.Name)
            {
                case "buttonData":
                    this.buttonData = entry.Value as List<HexButtonData>; 
                    break;                
            }
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

    public void UpdateSize()
    {
        int count = Mathf.Max(Inputs.Count, Outputs.Count);

        Size.y = Mathf.Max(count * 25, 80);
    }

    public override void RebuildConnections()
    {
        base.RebuildConnections();

        NewOutput(defaultExit, "Flow");
		GConnector connection;
    }

    public override void EnterStart()
    {
        base.EnterStart();

        if (physicalWidgetRoot != null)
        {
            DynamicHexList list = (DynamicHexList)physicalWidgetRoot.GetComponentInChildren(typeof(DynamicHexList));            
            list.SetParent(this);

            ForceLayer();
        }
        

#if !UNITY_EDITOR         
        Camera[] camList = (Camera[])Camera.FindObjectsOfType(typeof(Camera));
        UICamera uicam = null;
        foreach (Camera c in camList)
        {
            uicam = c.GetComponent<UICamera>();
            if (uicam != null && c.gameObject.layer == LayerMask.NameToLayer(CAMERA_3D_LAYER))
            {                
                break;
            }
        }

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
        Camera[] camList = (Camera[])Camera.FindObjectsOfType(typeof(Camera));
        UICamera uicam = null;
        foreach (Camera c in camList)
        {
            uicam = c.GetComponent<UICamera>();
            if (uicam != null && c.gameObject.layer == LayerMask.NameToLayer(CAMERA_3D_LAYER))
            {                
                break;
            }
        }

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
                ConnectionWithCall(gConect, button);
            }
            else
            {
                gConect = Outputs.Find(r => r.Name == defaultExit);
                if (gConect != null)
                {
                    ConnectionWithCall(gConect, button);
                }
            }
        }
        else
        {
            Debug.LogError("Dead end");
        }
    }

    public override bool IsValid()
    {
        //this panel is marked as invalid until some buttons are defined. It might be not the case later and condition changed.
        return base.IsValid() && buttonData != null && buttonData.Count > 0;
    }

    public void ForceLayer()
    {
        //this is 3d interface so it should switch to use this layer
        physicalWidgetRoot.layer = LayerMask.NameToLayer(CAMERA_3D_LAYER);
        UIWidget[] children = physicalWidgetRoot.GetComponentsInChildren<UIWidget>();
        foreach (UIWidget child in children)
        {
            child.CheckLayer();
        }
    }
}
