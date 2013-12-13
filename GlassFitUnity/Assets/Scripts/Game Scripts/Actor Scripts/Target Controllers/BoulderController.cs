using UnityEngine;
using System.Collections;

/// <summary>
/// Controls the Boulder
/// </summary>
public class BoulderController : TargetController {
	
	// Rotation in the x-axis.
	private float xRot;
	
	// Rotation speed
	private float rotationSpeed = 10;
	
	// Rotation limit
	private static float rotationLimit = 360f;
	
	// Time passed for speed up
	private float currentTime = 0;
	
	// Threshold for speed increase
	private float updateTime = 30f;
	
	// Value to increase speed by
	private float speedIncrease = 0.5f;
	
	// Current speed
	private float currentSpeed = 1.25f;
	
	/// <summary>
	/// Sets the attributes
	/// </summary>
	void Start () {
		// Set attributes and initial rotation.
		SetAttribs(50f, 135f, 420f, 0f);
		xRot = 0;
		if(target is FauxTargetTracker)
		{
			((FauxTargetTracker) target).SetTargetSpeed(currentSpeed);
		}
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
		
		currentTime += Time.deltaTime;
		
		if(currentTime > updateTime) 
		{
			currentTime -= updateTime;
			
			currentSpeed += speedIncrease;
			
			if(target is FauxTargetTracker)
			{
				((FauxTargetTracker) target).SetTargetSpeed(currentSpeed);
			}
		}
		
		// Rotate the object based on speed.
		xRot += (rotationSpeed * currentSpeed) * Time.deltaTime;
		
		// If greater than 360, reset.
		if(xRot > rotationLimit)
		{
			xRot -= rotationLimit;
		}
		
		// Make a new quaternion based on the rotation and apply
		Quaternion rot = Quaternion.Euler(new Vector3(xRot,0,0));
		transform.rotation = rot;
	}
}
