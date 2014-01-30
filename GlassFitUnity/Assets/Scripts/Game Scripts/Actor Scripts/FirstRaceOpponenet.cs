using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class FirstRaceOpponenet : TargetController {
	
	protected float currentMovementSpeed = 0.0f;
	private Animator anim; 
	
	protected float distanceFromStart = 0.0f;
	protected float headStartDistance = 0.0f;
	protected float playerDistance = 0.0f;	//cache this since we need it a lot, and retrieving it has some overhead.
	protected float playerSpeed = 0.0f;	//cache this since we need it a lot, and retrieving it has some overhead.
	
	protected float desiredLeadDistance = 10.0f;
	
	//track player's pace
	protected float headStartSpeed = 0.0f;
	protected float prevIntervalSpeed = 0.0f;
	protected float timeCurrentIntervalStarted = 0.0f;
	protected float timeRunStarted = 0.0f;			
	
	const float intervalDistance = 50.0f;
	protected float nextIntervalDistance = 50.0f;
	
	protected float prevLead = 0.0f;
	protected float lead = 0.0f;
	
	protected float speedAdjustmentInterval = 1.0f;
	protected float leadAdjustmentInterval = 120.0f;
	
	protected float totalDistance = 1.0f;
	
	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator>();
		travelSpeed = 1.0f;	//somewhat arbitrary scale factor for positioning distance
		lane = 1;
		lanePitch = 1.0f;
		SetAttribs(0.0f, 1.0f, transform.position.y, transform.position.x);
		currentMovementSpeed = 0.0f;
		UnityEngine.Debug.Log("FirstRace: started");
		
		timeRunStarted = Time.time;
		
		try {
			Track selectedTrack = (Track)DataVault.Get("current_track");
			if(selectedTrack != null) {
				totalDistance = (int)selectedTrack.distance;
			} else {
				totalDistance = (int)DataVault.Get("finish");
			}
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("FirstRace: Couldn't obtain goal distance for race");
		}
	}
	
	public void setHeadStart(float dist) {
		headStartDistance = dist;
		distanceFromStart = dist;	
	}
	
	public void onCountdownComplete()
	{
		//set timestamp for interval start
		timeCurrentIntervalStarted = Time.time;
		
		
	}
	
	// Update is called once per frame
	void Update () {
		//set the animation speed
		SetAnimSpeed(currentMovementSpeed);
		
		//update our total distance moved based on current speed and time passed.
		distanceFromStart += Time.deltaTime * currentMovementSpeed;
		//UnityEngine.Debug.Log("FirstRace: speed is now " + currentMovementSpeed.ToString());
		
		
		//update player distance & speed cached value, and check if they've passed a 100m increment
		playerDistance = Platform.Instance.GetDistance();
		playerSpeed = Platform.Instance.Pace();

		// update desired distance & adjust player speed to match
		UpdateDesiredLead();
		UpdateSpeed();
		
		//call to base, which sets world position
		base.Update();
		
	}
	

	
	/// <summary>
	/// Updates the speed to guide towards current desired lead amount
	/// Basically just smoothes out any changes in desired distance
	/// </summary>
	/// <returns>
	/// The speed.
	/// </returns>
	void UpdateSpeed()
	{

		float currentLead = distanceFromStart - playerDistance;
		float deltaLead = desiredLeadDistance - currentLead;
		float deltaSpeed = currentMovementSpeed - playerSpeed;
		float convergenceTime = deltaLead / deltaSpeed;

		// accelerate / decelerate smoothly
		// aim to converge in 5s time
		float acceleration = 1.0f * Mathf.Sign (deltaLead) * Time.deltaTime;; // metres per second squared
		if (convergenceTime < 0)
		{
			// moving the wrong way
			currentMovementSpeed += acceleration;
		}
		else
		{
			// moving the right way, speed inversely porportional to desiredLeadDistance
			if (currentMovementSpeed < Mathf.Min(deltaLead, 3.2f))
			{
				currentMovementSpeed += acceleration;
			}
			else
			{
				currentMovementSpeed = Mathf.Min(deltaLead, 3.2f);
			}
		}

		currentMovementSpeed = Mathf.Clamp(currentMovementSpeed, 0.0f, playerSpeed + 2.5f);
		UnityEngine.Debug.Log("FirstRace: Set opponent speed to " + currentMovementSpeed);
		
	}
	
	/// <summary>
	/// Updates the desired lead distance.
	/// Start off behind, then speed up and overtake, then allow player to win
	/// </summary>
	/// <returns>
	/// The desired lead.
	/// </returns>
	void UpdateDesiredLead()
	{
		// opponent startes 10m ahead, then updates desired lead based on player speed
		desiredLeadDistance = 20.0f;
//		if (playerDistance < 7.0f) {
//			// start, let player catch up
//			desiredLeadDistance = 10.0f - playerDistance;
//	    }
//		else if (playerSpeed < 1.5f)
//		{
//			// aim to overtake
//			desiredLeadDistance += UnityEngine.Random.Range(0.0f, 0.1f);
//		}
//		else if (playerSpeed > 2.77f)  // 6min/km
//		{
//			// aim to drop behind
//			desiredLeadDistance += UnityEngine.Random.Range(-0.2f, 0.0f);
//		} else
//		{
//			// random walk
//			desiredLeadDistance += UnityEngine.Random.Range(-0.05f, 0.05f);
//		}
//		desiredLeadDistance = Mathf.Clamp(desiredLeadDistance, -20.0f, 20.0f);
		
		UnityEngine.Debug.Log("FirstRace: Set desired lead to " + desiredLeadDistance);
		
	}
		
	
	/// <summary>
	/// Called when we detect the player has completed a 100m segment.
	/// </summary>
	protected void PlayerDidCompleteInterval()
	{
		//log pace forprevious 100m
		float timeInterval = Time.time - timeCurrentIntervalStarted;
		prevIntervalSpeed = 100.0f / timeInterval;
		
		//add random factor to this - weight towards slower
		float randomSpeedScale = UnityEngine.Random.Range(0.9f, 1.05f);
		prevIntervalSpeed *= randomSpeedScale;
		
		//store timestamp
		timeCurrentIntervalStarted = Time.time;
		
		//set next target
		nextIntervalDistance += intervalDistance;
		
		UnityEngine.Debug.Log("FirstRace: interval complete. Time: " + timeInterval + " Speed:" + prevIntervalSpeed);
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
	
	void SetAnimSpeed(float speed)
	{
		//pick appropriate anim speed based on our movement speed.
		UnityEngine.Debug.Log("FirstRace: speed is " + speed.ToString("f2"));
		anim.SetFloat("Speed", speed);
		if(speed > 2.2f && speed < 4.0f) {
			anim.speed = speed / 2.2f;
		} else if(speed > 4.0f) {
			anim.speed = Mathf.Clamp(speed / 4.0f, 1, 2);
		} else {
			anim.speed = speed / 1.25f;
		}
	}
	
}
