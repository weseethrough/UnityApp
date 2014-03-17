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
	
	// Headstart time
	float headstartTime = 5.0f;
	
	// Boolean to check if headstart is over
	private bool headstartComplete = false;
	
	/// <summary>
	/// Sets the attributes
	/// </summary>
	public override void Start () {
		// Set attributes and initial rotation.
		SetAttribs(0.0f, 1.0f, transform.position.y, transform.position.x);
		xRot = 0;
	}
	
	/// <summary>
	/// Sets the attributes
	/// </summary>
	public override void OnEnable() 
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
		currentSpeed = 0f;
		// Set the start time
		currentTime = 0.0f;
		// Set the player's initial distance
		playerDistance = Platform.Instance.LocalPlayerPosition.Distance;
		playerStartDistance = Platform.Instance.LocalPlayerPosition.Distance;
		// Set the boulder's starting distance
		distanceFromStart = (float)playerDistance - 10f;
		
		headstartComplete = false;
		headstartTime = 5.0f;
	}
	
	/// <summary>
	/// Update this instance + updates rotation
	/// </summary>
	public override void Update () {
		// Set the player distance 
		playerDistance = Platform.Instance.LocalPlayerPosition.Distance;	
		if(!headstartComplete) {
			headstartTime -= Time.deltaTime;
			
			if(headstartTime < 0.0f)
			{
				headstartComplete = true;
				currentSpeed = 1.25f;
			}
		}
		else
		{
			// Increase the time
			currentTime += Time.deltaTime;
			
			// Update the speed if enough time has passed
			if(currentTime > updateTime) 
			{
				currentTime -= updateTime;
				
				currentSpeed += speedIncrease;
			
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
		if(transform.childCount > 0) {
			transform.GetChild(0).rotation = rot;
		}
		else {
			transform.rotation = rot;
		}
		
		
		// Increase distance
		distanceFromStart += Time.deltaTime * currentSpeed;	
		//UnityEngine.Debug.Log("BoulderController: distance is " + distanceFromStart.ToString("f2"));
		
		scaledDistance = GetDistanceBehindTarget();
		
		//UnityEngine.Debug.Log("TargetController: distance behind is: " + scaledDistance.ToString());
		UnityEngine.Debug.Log("BoulderController: current z position is " + transform.position.z.ToString("f2"));
		//set position
		if(transform.childCount > 0) {
			Vector3 movement = new Vector3(xOffset, transform.GetChild(0).localPosition.y, (float)scaledDistance);
			transform.GetChild(0).localPosition = movement;
		}
		else
		{
			base.Update();
		}
		
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
