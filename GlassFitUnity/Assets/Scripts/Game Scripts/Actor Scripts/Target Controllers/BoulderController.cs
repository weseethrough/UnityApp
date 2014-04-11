using UnityEngine;
using System.Collections;

/// <summary>
/// Controls the Boulder
/// </summary>
public class BoulderController : TargetController {
	
	// Rotation speed
	private float rotationSpeed = -20;
	
	// Time passed for speed up
	private float currentTime = 0;
	
	// Threshold for speed increase
	private float updateTime = 10f;
	
	// Value to increase speed by
	private float speedIncrease = 0.5f;
	
	// Players current distance
	private double playerDistance = 0.0;
	
	// Player's starting distance
	private double playerStartDistance = 0.0;
	
	// Headstart time
	float headstartTime = 5.0f;
	
	// Boolean to check if headstart is over
	private bool headstartComplete = false;

	//position controller for the boulder
	private ConstantVelocityPositionController posController;

	private float gradient = 1;

	/// <summary>
	/// Sets the attributes
	/// </summary>
	public override void Start () {
		base.Start();
	}
	
	/// <summary>
	/// Sets the attributes
	/// </summary>
	public void OnEnable() 
	{
		gradient = Mathf.Tan( -transform.rotation.eulerAngles.x * Mathf.Deg2Rad );
		// Enable the object and set attributes.
		posController = gameObject.GetComponent<ConstantVelocityPositionController>();

		posController.enabled = true;

		//set velocity magnitude
		posController.setSpeed( 1.25f );

		Reset();
	}
	
	/// <summary>
	/// Reset the attributes.
	/// </summary>
	public void Reset()
	{
		// Set the starting speed
		posController.setSpeed(0);
		// Set the start time
		currentTime = 0.0f;
		// Set the player's initial distance
		playerDistance = Platform.Instance.LocalPlayerPosition.Distance;
		playerStartDistance = Platform.Instance.LocalPlayerPosition.Distance;
		// Set the boulder's starting distance

		headstartComplete = false;
		headstartTime = 5.0f;
	}

	public void Awake()
	{
		int i =0;
		i++;
		return;
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
				posController.setSpeed(1.25f);
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
				
				posController.setSpeed( posController.getSpeed() + speedIncrease );
			
			}
		}
	
		//rotate based on speed
		if(transform.childCount > 0) {
			transform.GetChild(0).Rotate(rotationSpeed * posController.getSpeed() * Time.deltaTime, 0, 0);
		}

		base.Update();

		//now move upwards to account for the slope
		Vector3 shiftedPosition = transform.position;
		shiftedPosition.y += (float)GetDistanceBehindTarget() * gradient;
		transform.position = shiftedPosition;

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
