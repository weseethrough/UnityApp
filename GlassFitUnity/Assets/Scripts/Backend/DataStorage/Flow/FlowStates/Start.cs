using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Start : FlowState 
{
    public override string GetDisplayName()
    {
        base.GetDisplayName();
        
        return "Start";
    }

    protected override void Initialize()
    {
        base.Initialize();

        Size = new Vector2(175, 80);        
        NewOutput("Start Point", "Flow");
        NewParameter("Name", GraphValueType.String, "Start");
    }

    public override void Entered()
    {
        base.Entered();
        if (Outputs.Count > 0 && parentMachine != null)
        {
            parentMachine.FollowConnection(Outputs[0]);
        }
        else
        {
            Debug.LogError("Dead end start");
        }
    }
}
