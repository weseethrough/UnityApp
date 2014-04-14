using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Reflection;

/// <summary>
/// exit state which is simply blank end. It doesn't have any navigation out connections.
/// </summary>
[Serializable]
public class Exit : FlowStateBase 
{
	/// <summary>
	/// default constructor
	/// </summary>	
	public Exit() : base() {}

    /// <summary>
    /// deserialziation constructor
    /// </summary>
    /// <param name="info">seirilization info conataining class data</param>
    /// <param name="ctxt">serialization context </param>
    /// <returns></returns>
    public Exit(SerializationInfo info, StreamingContext ctxt)
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
        
        return "Exit";
    }

    /// <summary>
    /// initialzies node and creates name for it. Makes as well default iput/output connection sockets
    /// </summary>
    /// <returns></returns>
    protected override void Initialize()
    {
        base.Initialize();

        Size = new Vector2(175, 80);        
        NewInput("Exit Point", "Flow");
        NewParameter("Name", GraphValueType.String, "Exit");
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
