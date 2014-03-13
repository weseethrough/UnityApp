using UnityEngine;
using System.Collections;

/// <summary>
/// Controls the cyclist
/// </summary>
public class CyclistController : TargetController {

	/// <summary>
	/// Sets the attributes
	/// </summary>
	public override void Start () {
		// Start the base and set the attributes.
		base.Start();
		SetAttribs(0, 135f, -220f, 100f);
	}
	
	/// <summary>
	/// Raises the enable event. Sets the attributes
	/// </summary>
	public override void OnEnable() {
		// Enable the base and set the attributes.
		base.OnEnable();
		SetAttribs(0, 135f, -220f, 100f);
	}
	
	/// <summary>
	/// Update this instance.
	/// </summary>
	public override void Update () {
		// Update the base.
		base.Update();
		
		
	}
}
