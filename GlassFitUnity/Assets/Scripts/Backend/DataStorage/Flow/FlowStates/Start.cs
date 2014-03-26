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
    /// If GraphComponent is notified about flow switch Start will go idle and never recover. It is GraphComponent
    /// responsibility to do whatever is needed to continue and call new start in another flow
    /// </summary>
    /// <returns></returns>
    public override void Entered()
    {
        base.Entered();
        GraphComponent gc = GameObject.FindObjectOfType(typeof(GraphComponent)) as GraphComponent;
        if (gc != null)
        {
            if (gc.GoToFlowStage2())
            {
                //flow is switching so we will not interfere with it and skip any build in progress
                return;
            }
        }

        if (Outputs.Count > 0 && parentMachine != null)
        {
			//in the editor, go straight to in-game HUD
#if UNITY_EDITOR
			//string playerExit = PlayerPrefs.GetString("playerStartExit");
			FollowOutput("Start Point");//playerExit);	
#else
            FollowOutput("Start Point");
#endif
        }
        else
        {
            Debug.LogError("Dead end start");
        }
    }
}
