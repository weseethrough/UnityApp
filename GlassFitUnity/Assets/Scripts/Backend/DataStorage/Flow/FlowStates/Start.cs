using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

/// <summary>
/// start state which is simply forwarder to first node in the flow
/// </summary>
[Serializable]
public class Start : FlowState 
{
    /// <summary>
    /// default constructor
    /// </summary>
	public Start() : base() {}

    /// <summary>
    /// deserialziation constructor
    /// </summary>
    /// <param name="info">seirilization info conataining class data</param>
    /// <param name="ctxt">serialization context </param>
    /// <returns></returns>
    public Start(SerializationInfo info, StreamingContext ctxt)
        : base(info, ctxt)
	{
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
   	}

    /// <summary>
    /// Gets display name of the node, helps with node identification in editor
    /// </summary>
    /// <returns>name of the node</returns>
    public override string GetDisplayName()
    {
        base.GetDisplayName();
        
        return "Start";
    }

    /// <summary>
    /// initialzies node and creates name for it. Makes as well default iput/output connection sockets
    /// </summary>
    /// <returns></returns>
    protected override void Initialize()
    {
        base.Initialize();

        Size = new Vector2(175, 80);        
        NewOutput("Start Point", "Flow");
        NewParameter("Name", GraphValueType.String, "Start");
    }


    /// <summary>
    /// function called as soon as state enters to navigate to next state along default connection out
    /// </summary>
    /// <returns></returns>
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
