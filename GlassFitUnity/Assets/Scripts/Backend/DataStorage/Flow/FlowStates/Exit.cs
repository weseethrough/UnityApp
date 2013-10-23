using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Exit : FlowState 
{
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
