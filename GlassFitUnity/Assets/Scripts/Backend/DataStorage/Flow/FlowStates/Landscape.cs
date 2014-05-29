using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

[Serializable]
public class LandscapeState : FlowState {

	/// <summary>
	/// default constructor
	/// </summary>
	public LandscapeState() : base() {}

	/// <summary>
	/// deserialziation constructor
	/// </summary>
	/// <param name="info">seirilization info conataining class data</param>
	/// <param name="ctxt">serialization context </param>
	/// <returns></returns>
	public LandscapeState(SerializationInfo info, StreamingContext ctxt)
		: base(info, ctxt)
	{
	}

	
	/// <summary>
	/// Gets display name of the node, helps with node identification in editor
	/// </summary>
	/// <returns>name of the node</returns>
	public override string GetDisplayName()
	{
		base.GetDisplayName();
		
		return "Landscape";
	}
	
	/// <summary>
	/// initializes node and creates name for it. Makes as well default input/output connection sockets
	/// </summary>
	/// <returns></returns>
	protected override void Initialize()
	{
		base.Initialize();
		
		Size = new Vector2(300, 300);                
		NewParameter("Name", GraphValueType.String, "Landscape");
	}

	public override void Entered ()
	{
		base.Entered ();

		//force to landscape if not there already
		Screen.orientation = ScreenOrientation.Landscape;

		UnityEngine.Debug.Log("Switching to landscape orientation");
	}

	public override void Exited ()
	{
		base.Exited ();

		Screen.orientation = ScreenOrientation.Portrait;

		UnityEngine.Debug.Log("Switching to portrait orientation");
	}
}
