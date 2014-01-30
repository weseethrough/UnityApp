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
	
	protected float desiredLeadDistance = 0.0f;
	
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
		
		
		//update player distance cached value, and check if they've passed a 100m increment
		playerDistance = Platform.Instance.GetDistance();

		if(playerDistance > nextIntervalDistance)
		{
			PlayerDidCompleteInterval();
		}
		
		//if the player just closed the headstart
		if(headStartSpeed == 0.0f && playerDistance >= headStartDistance)
		{
			//log the headstart speed
			float timeInterval = Time.time - timeCurrentIntervalStarted;
			headStartSpeed = headStartDistance / timeInterval;
			UnityEngine.Debug.Log("FirstRace: HeadStart closed. Dist:" + headStartDistance + " Time:" + timeInterval + " Speed:" + headStartSpeed);
			
			//start the coroutine to set the speed
			//UnityEngine.Debug.Log("FirstRace: Update is the culprit, player distance is " + playerDistance.ToString() + " and headstart distance is " + headStartDistance.ToString());
			currentMovementSpeed = headStartSpeed;
			StartCoroutine(UpdateSpeed());
			StartCoroutine(UpdateDesiredLead());
		}
		
		//call to base, which sets world position
		base.Update();
		
	}
	
	protected void setSpeedToReachDesiredLead() 
	{
		//update our speed based on player's recent speed
		//if player hasn't closed headstart, stay put
		//negative feedback. If we're ahead of the player, slow down, if we're behind the player, speed up.
		float speedAdjustmentRate = 0.1f;		//adjust by this many m/s each update
		
		//initially try a damped harmonic
		//acceleration = -A*distance_ahead - B*closing_speed
		
		prevLead = lead;
		lead = distanceFromStart - playerDistance;
		float closingSpeed = (lead - prevLead)/speedAdjustmentInterval;	
		float dampingFactor = 1.0f;
		float distanceError = (lead - desiredLeadDistance);
		
		float acceleration = -(speedAdjustmentRate * distanceError) - dampingFactor*closingSpeed;
		
		//clamp acceleration to 0.5m/s per interval
		acceleration = Mathf.Min( acceleration, 0.5f);
		
		currentMovementSpeed += acceleration;
		
		//don't let it go below walking pace
		//UnityEngine.Debug.Log("FirstRace: setSpeedToReachDesiredLead is the culprit");
		currentMovementSpeed = Mathf.Max(1.25f, currentMovementSpeed);
	}
	
	/// <summary>
	/// Periodically updates the speed, to guide towards current desired lead amount
	/// </summary>
	/// <returns>
	/// The speed.
	/// </returns>
	IEnumerator UpdateSpeed()
	{
		float distanceRemaining = totalDistance - playerDistance;

		while(distanceRemaining > 500)
		{
			setSpeedToReachDesiredLead();
			distanceRemaining = totalDistance - playerDistance;
			
			yield return new WaitForSeconds(speedAdjustmentInterval);
		}
		
		//for last 500m, run at a little faster than player's average pace up to now.
		float averageSpeed = playerDistance/ (Time.time - timeRunStarted);
		//UnityEngine.Debug.Log("FirstRace: UpdateSpeed is the culprit");
		currentMovementSpeed = averageSpeed;
		
	}
	
	/// <summary>
	/// Updates the desired lead distance.
	/// Start off behind, then speed up and overtake, then allow player to win
	/// </summary>
	/// <returns>
	/// The desired lead.
	/// </returns>
	IEnumerator UpdateDesiredLead()
	{
		// start neck and neck with player
		desiredLeadDistance = -10.0f;
		yield return new WaitForSeconds(30.0f);
		
		//then surge ahead
		desiredLeadDistance = 4.0f;
		yield return new WaitForSeconds(30.0f);
		desiredLeadDistance = 7.0f;
		yield return new WaitForSeconds(30.0f);
		
		float distanceRemaining = totalDistance - playerDistance;
		
		//random walk until there's half a km left
		while(distanceRemaining > 1000)
		{
			//random +- 5m increment
			float delta = UnityEngine.Random.Range(-2.0f, 1.0f);
			desiredLeadDistance += delta;
			//clamp to +- 100m
			desiredLeadDistance = Mathf.Min(20.0f, desiredLeadDistance);
			desiredLeadDistance = Mathf.Max(-30.0f, desiredLeadDistance);
			
			UnityEngine.Debug.Log("FirstRace: Set desired lead to " + desiredLeadDistance);
			distanceRemaining = totalDistance - playerDistance;
			
			yield return new WaitForSeconds(60.0f);
		}
		
		UnityEngine.Debug.Log("FirstRun: last kilometre!");
		
		//drift back to within 2m
		while(Math.Abs(desiredLeadDistance) > 2.0f)
		{
			if(desiredLeadDistance > 20.0f)
			{
				desiredLeadDistance -= 0.5f;
			}
			if(desiredLeadDistance < -20.0f)
			{
				desiredLeadDistance += 0.5f;
			}
			
			yield return new WaitForSeconds(20.0f);
		}
		
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
