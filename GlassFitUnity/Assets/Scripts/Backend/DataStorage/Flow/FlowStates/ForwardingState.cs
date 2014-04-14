using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Reflection;

/// <summary>
/// start state which is simply forwarder to first node in the flow
/// </summary>
[Serializable]
public class ForwardingState : FlowStateBase 
{
    /// <summary>
    /// default constructor
    /// </summary>
	public ForwardingState() : base() {}

    /// <summary>
    /// deserialziation constructor
    /// </summary>
    /// <param name="info">seirilization info conataining class data</param>
    /// <param name="ctxt">serialization context </param>
    /// <returns></returns>
    public ForwardingState(SerializationInfo info, StreamingContext ctxt)
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

        return "Forwarding State";
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
        NewParameter("Name", GraphValueType.String, "Forwarding State");
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
        
        if (Outputs.Count > 0 && parentMachine != null)
        {
            FollowOutput("Exit");
        }
        else
        {
            Debug.LogError("Dead end start");
        }
    }

    public override bool CallStaticFunction(string functionName, FlowButton caller)
    {
        MemberInfo[] info = typeof(ButtonFunctionCollection).GetMember(functionName);

        if (info.Length == 1)
        {
            System.Object[] newParams = new System.Object[2];
            newParams[0] = caller;
            newParams[1] = this;
            bool ret = (bool)typeof(ButtonFunctionCollection).InvokeMember(functionName,
                                    BindingFlags.InvokeMethod |
                                    BindingFlags.Public |
                                    BindingFlags.Static,
                                    null, null, newParams);
            return ret;
        }

        return true;
    }
}
