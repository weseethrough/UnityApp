using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System;

/// <summary>
/// Controls the position of the train
/// </summary>
public class TrainController : TargetController {
	private float whistleTime = 0;
	
	/// <summary>
	/// Start this instance. Sets the attributes
	/// </summary>
	public override void Start () {
		// Set the base and initial attributes.
		base.Start();
	}

	
	/// <summary>
	/// Update this instance.
	/// </summary>
	public override void Update () {
		// Update the base
		base.Update();
		
		whistleTime += Time.deltaTime;
		
		if(whistleTime >= 10.0f)
		{
			//trainWhistle.Play();
			whistleTime -= 10.0f;
		}
	}
}
