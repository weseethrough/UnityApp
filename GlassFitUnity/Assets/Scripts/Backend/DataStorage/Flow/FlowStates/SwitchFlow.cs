using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

/// <summary>
/// exit state which is simply blank end. It doesn't have any navigation out connections.
/// </summary>
[Serializable]
public class SwitchFlow : FlowState 
{
    static public bool allowGameStateAutoSave = false;

	/// <summary>
	/// default constructor
	/// </summary>	
	public SwitchFlow() : base() {}

    /// <summary>
    /// deserialziation constructor
    /// </summary>
    /// <param name="info">seirilization info conataining class data</param>
    /// <param name="ctxt">serialization context </param>
    /// <returns></returns>
    public SwitchFlow(SerializationInfo info, StreamingContext ctxt)
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

        return "Switch Flow";
    }

    /// <summary>
    /// initialzies node and creates name for it. Makes as well default iput/output connection sockets
    /// </summary>
    /// <returns></returns>
    protected override void Initialize()
    {
        base.Initialize();

        Size = new Vector2(175, 80);

        NewInput("Enter", "Flow");
        NewOutput("Exit", "Flow");

        NewParameter("Flow Name", GraphValueType.String, "");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override void Entered()
    {
        base.Entered();


        GraphComponent gc = GameObject.FindObjectOfType(typeof(GraphComponent)) as GraphComponent;
        if (gc != null)
        {
            string name = Parameters.Find(r => r.Key == "Flow Name").Value;
            if (gc.GoToFlow(name))
            {
                //flow is switching so we will not interfere with it and skip any build in progress
                return;
            }
        }

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
