using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FirstRaceOpponenet : TargetController {
	
	protected float currentMovementSpeed = 0.0f;
	private Animator anim; 
	
	protected float distanceFromStart = 0.0f;
	protected float headStartDistance = 0.0f;
	protected float playerDistance = 0.0f;	//cache this since we need it a lot, and retrieving it has some overhead.
	
	//track player's pace
	protected float headStartSpeed = 0.0f;
	protected float prevIntervalSpeed = 0.0f;
	protected float timeCurrentIntervalStarted = 0.0f;
	
	const float intervalDistance = 50.0f;
	float nextIntervalDistance = 50.0f;
	
	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator>();
		travelSpeed = 1.0f;	//somewhat arbitrary scale factor for positioning distance
		lane = 1;
		lanePitch = 1.0f;
		SetAttribs(0.0f, travelSpeed, transform.position.y, transform.position.x);
		UnityEngine.Debug.Log("FirstRaceOpponent: started");
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
		
		//update player distance cached value, and check if they've passed a 100m increment
		playerDistance = Platform.Instance.GetDistance();

		if(playerDistance >nextIntervalDistance)
		{
			PlayerDidCompleteInterval();
		}
		
		
		
		//if the player just closed the headstart
		if(headStartSpeed == 0.0f && playerDistance >= headStartDistance)
		{
			//log the headstart speed
			float timeInterval = Time.time - timeCurrentIntervalStarted;
			headStartSpeed = headStartDistance / timeInterval;
			UnityEngine.Debug.Log("FirstRun: HeadStart closed. Dist:" + headStartDistance + " Time:" + timeInterval + " Speed:" + headStartSpeed);
		}
		
		//set our speed
		currentMovementSpeed = getDesiredSpeed();
		
		//call to base, which sets world position
		base.Update();
	}
	
	protected float getDesiredSpeed() 
	{
		//update our speed based on player's recent speed
		//if player hasn't closed headstart, stay put
		if(playerDistance < headStartDistance)
		{
			return 0.0f;
		}
		//else if player has covered at least 100m use speed for previous 100m
		else if(playerDistance > intervalDistance)
		{
			return Mathf.Max(prevIntervalSpeed, 1.25f);
		}
		//else use player's speed while closing headstart dist
		else
		{
			return Mathf.Max(headStartSpeed, 1.25f);
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
		float randomSpeedScale = Random.Range(0.9f, 1.05f);
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
			anim.speed = speed / 1.0f;
		}
	}
	
}
