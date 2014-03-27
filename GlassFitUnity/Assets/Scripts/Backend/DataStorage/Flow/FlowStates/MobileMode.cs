using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

/// <summary>
/// 
/// </summary>
[Serializable]
public class MobileMode : FlowState 
{
    ScreenOrientation defaultOrientation;

    /// <summary>
    /// default constructor
    /// </summary>
	public MobileMode() : base() {}

    /// <summary>
    /// deserialziation constructor
    /// </summary>
    /// <param name="info">seirilization info conataining class data</param>
    /// <param name="ctxt">serialization context </param>
    /// <returns></returns>
    public MobileMode(SerializationInfo info, StreamingContext ctxt)
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

        return "MobileMode";
    }

    /// <summary>
    /// initializes node and creates name for it. Makes as well default input/output connection sockets
    /// </summary>
    /// <returns></returns>
    protected override void Initialize()
    {
        base.Initialize();

        Size = new Vector2(175, 80);                
        NewParameter("Name", GraphValueType.String, "MobileMode");
    }


    /// <summary>
    /// set orientation to mobile portrait mode
    /// </summary>
    /// <returns></returns>
    public override void Entered()
    {
        base.Entered();
        defaultOrientation = Screen.orientation;
        Screen.orientation = ScreenOrientation.Portrait;
    }

    /// <summary>
    /// restore orientation
    /// </summary>
    /// <returns></returns>
    public override void Exited()
    {
        base.Exited();
        Screen.orientation = defaultOrientation;
    }
}
