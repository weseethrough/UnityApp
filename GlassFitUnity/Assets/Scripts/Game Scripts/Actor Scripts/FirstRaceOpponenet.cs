using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using RaceYourself.Models;

public class FirstRaceOpponenet : ConstantVelocityPositionController {
	
	//protected float currentMovementSpeed = 0.0f;
	//private Animator anim; 
	
	public float headStartDistance = 0.0f;
	protected float playerDistance = 0.0f;	//cache this since we need it a lot, and retrieving it has some overhead.
	protected float playerSpeed = 0.0f;	//cache this since we need it a lot, and retrieving it has some overhead.
	
	protected float desiredLeadDistance = 20.0f;
	
	//track player's pace
	protected float timeRunStarted = 0.0f;			
	
	//protected float lead = 0.0f;
	
	protected float speedAdjustmentInterval = 1.0f;
	protected float leadAdjustmentInterval = 120.0f;
	
	protected float totalDistance = 1.0f;
	
	// Use this for initialization
	public override void Start () {

		base.Start();

		//start stationary
		velocity = Vector3.zero;
		UnityEngine.Debug.Log("FirstRace: started");

		timeRunStarted = Time.time;
		totalDistance = GameBase.getTargetDistance();

		worldObject.setRealWorldDist(headStartDistance);

	}

	// Update is called once per frame
	public override void Update () {

		//update player distance & speed cached value, and check if they've passed a 100m increment
		playerDistance = (float)Platform.Instance.LocalPlayerPosition.Distance;
		playerSpeed = Platform.Instance.LocalPlayerPosition.Pace;

		// update desired distance & adjust player speed to match
		UpdateSpeed();
		
		//call to base, which sets world position
		base.Update();
		
	}
	
	/// <summary>
	/// Updates the speed to guide towards current desired lead amount
	/// </summary>
	/// <returns>
	/// The speed.
	/// </returns>
	void UpdateSpeed()
	{

		float currentLead = worldObject.getRealWorldPos().z - playerDistance;
		float deltaLead = desiredLeadDistance - currentLead;
		float deltaSpeed = velocity.z - playerSpeed;
		float convergenceTime = deltaLead / deltaSpeed;

		if (velocity.z == 0.0f && currentLead > 10.0f)
		{
			// don't set off till the player is within 10m
			// makes it easy for the player to catch up and feel the movement
			return;
		}

		// accelerate / decelerate smoothly
		// aim to converge in 5s time
		float acceleration = 0.6f * Mathf.Sign (deltaLead) * Time.deltaTime; // metres per second squared
		if (convergenceTime < 0)
		{
			// moving the wrong way
			setSpeed( getSpeed() + acceleration );
		}
		else
		{
			// moving the right way, speed proportional to deltaLead up to a max cap (slightly slower than the player can run)
			if (getSpeed() < Mathf.Min(deltaLead, 3.0f))
			{
				setSpeed( getSpeed() + acceleration );
			}
			else
			{
				setSpeed( Mathf.Min(deltaLead, 3.0f) );
			}
		}

		setSpeed( Mathf.Clamp(getSpeed(), 0.0f, playerSpeed + 2.5f) );
		//UnityEngine.Debug.Log("FirstRace: Set opponent speed to " + currentMovementSpeed);			
	}
}
