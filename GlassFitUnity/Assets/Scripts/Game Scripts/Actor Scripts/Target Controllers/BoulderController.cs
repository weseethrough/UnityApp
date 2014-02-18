using UnityEngine;
using System.Collections;

/// <summary>
/// Controls the Boulder
/// </summary>
public class BoulderController : TargetController {
	
	// Rotation in the x-axis.
	private float xRot;
	
	// Rotation speed
	private float rotationSpeed = 20;
	
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
	
	private float distanceFromStart = -50f;
	
	private double playerDistance = 0.0;
	
	private double playerStartDistance = 0.0;
	
	/// <summary>
	/// Sets the attributes
	/// </summary>
	void Start () {
		// Set attributes and initial rotation.
		SetAttribs(0.0f, 1.0f, transform.position.y, transform.position.x);
		xRot = 0;
	}
	
	/// <summary>
	/// Sets the attributes
	/// </summary>
	void OnEnable() 
	{
		// Enable the object and set attributes.
		base.OnEnable();
		Reset();
		SetAttribs(0.0f, 1.0f, transform.position.y, transform.position.x);
	}
	
	public void Reset()
	{
		currentSpeed = 1.25f;
		currentTime = 0.0f;
		playerDistance = Platform.Instance.Distance();
		playerStartDistance = Platform.Instance.Distance();
		distanceFromStart = (float)playerDistance - 50f;
	}
	
	/// <summary>
	/// Update this instance + updates rotation
	/// </summary>
	void Update () {
		playerDistance = Platform.Instance.Distance();
		
		currentTime += Time.deltaTime;
		
		if(currentTime > updateTime) 
		{
			currentTime -= updateTime;
			
			currentSpeed += speedIncrease;
			
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
		
		distanceFromStart += Time.deltaTime * currentSpeed;	
		
		// Update the base.
		base.Update();
		
		
	}
	
	/// <summary>
	/// Override base implementation, which queries target tracker.
	/// </summary>
	/// <returns>
	/// The distance behind this target.
	/// </returns>
	public override double GetDistanceBehindTarget ()
	{
		float relativeDist = distanceFromStart - (float)playerDistance;
		return relativeDist;
	}
	
	public double GetPlayerDistanceTravelled()
	{
		return playerDistance - playerStartDistance;
	}
}
