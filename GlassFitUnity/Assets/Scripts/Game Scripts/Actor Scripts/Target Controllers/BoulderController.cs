using UnityEngine;
using System.Collections;

/// <summary>
/// Controls the Boulder
/// </summary>
public class BoulderController : TargetController {
	
	// Rotation in the x-axis.
	private float xRot;
	
	/// <summary>
	/// Sets the attributes
	/// </summary>
	void Start () {
		// Set attributes and initial rotation.
		SetAttribs(50f, 135f, 420f, 0f);
		xRot = 0;
	}
	
	/// <summary>
	/// Sets the attributes
	/// </summary>
	void OnEnable() 
	{
		// Enable the object and set attributes.
		base.OnEnable();
		SetAttribs(50f, 135f, 420f, 0f);		
	}
	
	/// <summary>
	/// Update this instance + updates rotation
	/// </summary>
	void Update () {
		
		// Update the base.
		base.Update();
		
		// Rotate the object based on speed.
		xRot += (10 * (((float)DataVault.Get("slider_val")* 9.15f) + 1.25f)) * Time.deltaTime;
		
		// If greater than 360, reset.
		if(xRot > 360)
		{
			xRot -= 360;
		}
		
		// Make a new quaternion based on the rotation and apply
		Quaternion rot = Quaternion.Euler(new Vector3(xRot,0,0));
		transform.rotation = rot;
	}
}
