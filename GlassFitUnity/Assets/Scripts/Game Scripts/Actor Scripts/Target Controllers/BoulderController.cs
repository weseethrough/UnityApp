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
	private float updateTime = 10f;
	
	// Value to increase speed by
	private float speedIncrease = 0.5f;
	
	// Current speed
	private float currentSpeed = 2.5f;
	
	// Starting distance
	private float distanceFromStart = -50f;
	
	// Players current distance
	private double playerDistance = 0.0;
	
	// Player's starting distance
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
	
	/// <summary>
	/// Reset the attributes.
	/// </summary>
	public void Reset()
	{
		// Set the starting speed
		currentSpeed = 1.25f;
		// Set the start time
		currentTime = 0.0f;
		// Set the player's initial distance
		playerDistance = Platform.Instance.LocalPlayerPosition.Distance;
		playerStartDistance = Platform.Instance.LocalPlayerPosition.Distance;
		// Set the boulder's starting distance
		distanceFromStart = (float)playerDistance - 50f;
	}
	
	/// <summary>
	/// Update this instance + updates rotation
	/// </summary>
	void Update () {
		// Set the player distance 
		playerDistance = Platform.Instance.LocalPlayerPosition.Distance;
		
		// Increase the time
		currentTime += Time.deltaTime;
		
		// Update the speed if enough time has passed
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
		
		// Increase distance
		distanceFromStart += Time.deltaTime * currentSpeed;	
		//UnityEngine.Debug.Log("BoulderController: distance is " + distanceFromStart.ToString("f2"));
		
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
	
	/// <summary>
	/// Gets the distance the player has travelled in the minigame.
	/// </summary>
	/// <returns>
	/// The player's distance.
	/// </returns>
	public double GetPlayerDistanceTravelled()
	{
		return playerDistance - playerStartDistance;
	}
}
