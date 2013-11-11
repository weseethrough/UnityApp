using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

[Serializable]
public class HexPanel : Panel 
{
    const string defaultExit = "Default Exit";

    public HexPanel() : base() { }
    public HexPanel(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt) {  }

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
    }

    public override void RebuildConnections()
    {
        base.RebuildConnections();

        NewOutput(defaultExit, "Flow");
    }

    public override void EnterStart()
    {
        base.EnterStart();

        Debug.Log("Panel Enter");

        if (physicalWidgetRoot != null)
        {
            DynamicHexList list = (DynamicHexList)physicalWidgetRoot.GetComponentInChildren(typeof(DynamicHexList));            
            list.SetParent(this);
        }                
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
