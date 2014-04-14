using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

/// <summary>
/// start state which is simply forwarder to first node in the flow
/// </summary>
public abstract class StartBase : FlowStateBase 
{
    /// <summary>
    /// default constructor
    /// </summary>
	public StartBase() : base() {}

    /// <summary>
    /// deserialziation constructor
    /// </summary>
    /// <param name="info">seirilization info conataining class data</param>
    /// <param name="ctxt">serialization context </param>
    /// <returns></returns>
    public StartBase(SerializationInfo info, StreamingContext ctxt)
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

}
