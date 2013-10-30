using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

[Serializable]
public class Exit : FlowState 
{
	public Exit() : base() {}
	
    public Exit(SerializationInfo info, StreamingContext ctxt)
        : base(info, ctxt)
	{
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext ctxt)
   	{
        base.GetObjectData(info, ctxt);
   	}

    public override string GetDisplayName()
    {
        base.GetDisplayName();
        
        return "Exit";
    }

    protected override void Initialize()
    {
        base.Initialize();

        Size = new Vector2(175, 80);        
        NewInput("Exit Point", "Flow");
        NewParameter("Name", GraphValueType.String, "Exit");
    }
}
