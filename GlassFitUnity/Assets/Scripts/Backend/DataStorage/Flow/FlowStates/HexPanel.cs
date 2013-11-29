using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

/// <summary>
/// advanced panel which allows to display complex hex menu screens
/// </summary>
[Serializable]
public class HexPanel : Panel 
{
    public const string CAMERA_3D_LAYER = "GUI";
    const string defaultExit = "Default Exit";

    public List<HexButtonData> buttonData;        
    private bool camStartMouseAction;
    private bool camStartTouchAction;

    /// <summary>
    /// default constructor
    /// </summary>
    /// <returns></returns>
    public HexPanel()
        : base()
    {
        buttonData = new List<HexButtonData>();
    }

    /// <summary>
    /// deserialziation constructor
    /// </summary>
    /// <param name="info">seirilization info conataining class data</param>
    /// <param name="ctxt">serialization context </param>
    /// <returns></returns>
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

    /// <summary>
    /// serialization function called by serializer
    /// </summary>
    /// <param name="info">serialziation info where all data would be pushed to</param>
    /// <param name="ctxt">serialzation context</param>
    /// <returns></returns>
    public override void GetObjectData(SerializationInfo info, StreamingContext ctxt)
    {
        base.GetObjectData(info, ctxt);

        info.AddValue("buttonData", this.buttonData);
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
            return "HEX Panel: " + gName.Value;
        }
        return "HEX Panel: UnInitialzied";
    }

    /// <summary>
    /// initialzies node and creates name for it. Makes as well default iput/output connection sockets
    /// </summary>
    /// <returns></returns>
    protected override void Initialize()
    {
        base.Initialize();

        NewOutput(defaultExit, "Flow");
        NewParameter("HexListManager", GraphValueType.HexButtonManager, "true"); //fake variable just to trigger option visibility on graph editor

    }   

    /// <summary>
    /// refreshes connections lists
    /// </summary>
    /// <returns></returns>
    public override void RebuildConnections()
    {
        base.RebuildConnections();

        NewOutput(defaultExit, "Flow");
		GConnector connection;
    }

    /// <summary>
    /// function called when screen started enter process
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// function called when panels started exiting process
    /// </summary>
    /// <returns></returns>
    public override void ExitStart()
    {
        base.ExitStart();

        Debug.Log("Panel Exit");
    }

    /// <summary>
    /// exit finalization and clearing process
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// whenever button get clicked it would be handled here
    /// </summary>
    /// <param name="button">button which send this event</param>
    /// <returns></returns>
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

    /// <summary>
    /// checks if class have button data and at least one button. As well do the parent checks if screen type is set
    /// </summary>
    /// <returns></returns>
    public override bool IsValid()
    {
        //this panel is marked as invalid until some buttons are defined. It might be not the case later and condition changed.
        return base.IsValid() && buttonData != null && buttonData.Count > 0;
    }

    /// <summary>
    /// function used to ensure components of this panel are rendered by specific camera
    /// </summary>
    /// <returns></returns>
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
