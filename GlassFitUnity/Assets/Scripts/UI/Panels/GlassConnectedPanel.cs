using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

[Serializable]
public class GlassConnectedPanel : FlowState {

	public GlassConnectedPanel() {}
    public GlassConnectedPanel(SerializationInfo info, StreamingContext ctxt)
        : base(info, ctxt)
    {
	}

    protected override void Initialize()
    {
        base.Initialize();

        NewInput("Enter", "Flow");

        NewOutput("Connected", "Flow");
        NewOutput("NotConnected", "Flow");        
    }
	
	public override void EnterStart()
    {
        base.EnterStart();

        CheckConnection();
    }

    public void CheckConnection()
    {
        string exit = "";
        
        if (IsConnected())
        {
            exit = "Connected";
        }
        else
        {
            exit = "NotConnected";
        }

        if (Outputs.Count > 0 && parentMachine != null)
        {
            GConnector gConect = Outputs.Find(r => r.Name == exit);
            if (gConect != null)
            {
                ConnectionWithCall(gConect, null);
            }
        }
    }

    bool IsConnected()
    {
        foreach (string peer in Platform.Instance.BluetoothPeers())
        {
            UnityEngine.Debug.LogWarning("Platform: BT peer: " + peer);
            return true;
        }

        return false;
    }
}
