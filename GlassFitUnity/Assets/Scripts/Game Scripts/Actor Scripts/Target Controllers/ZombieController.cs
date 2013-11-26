using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System;

/// <summary>
/// Controls the position of the zombies
/// </summary> 
public class ZombieController : TargetController {
	
	/// <summary>
	/// Start this instance. Sets the attributes
	/// </summary>
	void Start () {
		// Initialise base and set the starting attributes.
		base.Start();
		SetAttribs(50, 135, -150f, 13.88668f);
	}
	
	/// <summary>
	/// Raises the enable event. Sets the attributes
	/// </summary>
	void OnEnable() {
		// Enable the base and set the attributes
		base.OnEnable();
		SetAttribs(50, 135, -150f, 13.88668f);
	}
	
	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update () {
		// Update the base.
		base.Update();
	}
}
