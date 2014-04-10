using UnityEngine;
using System.Collections;

public class FirstRaceIndoorOpponent : ConstantVelocityPositionController {
	
	private double playerDistance = 0f;
	private float playerSpeed = 0f;
	
	private float distanceInterval = 50f;
	private float intervalStartTime = 0.0f;
	
	private float distanceFromStart = 0;
	
	private bool notVisible = true;
	
	private Animator anim;
	
	private float currentSpeed = 0.0f;
	
	private double lastDistance = 0.0;
	
	// Use this for initialization
	public override void Start () {
	}
	
	// Update is called once per frame
	public override void Update () {
		//UnityEngine.Debug.Log("IndoorOpponent: we are in the update function");
		playerDistance = Platform.Instance.LocalPlayerPosition.Distance;
		
		if(playerDistance > distanceInterval)
		{
			SetRunnerSpeed();
			distanceInterval += 50f;
		}
		
		SetAnimSpeed(currentSpeed);
		
		distanceFromStart += Time.deltaTime * currentSpeed;
		
		base.Update();
	}
	
	public void SetRunnerSpeed()
	{
		double currentDistance = Platform.Instance.LocalPlayerPosition.Distance;
		float currentTime = Platform.Instance.LocalPlayerPosition.Time / 1000f;
		
		float intervalTotalTime = currentTime - intervalStartTime;
		
		double newDistance = currentDistance - lastDistance;
		
		float newSpeed = (float)newDistance/intervalTotalTime;
		
		if(newSpeed > currentSpeed)
		{
			currentSpeed = newSpeed;
		}
		
		lastDistance = currentDistance;
		intervalStartTime = currentTime;
		
		UnityEngine.Debug.Log("IndoorOpponent: speed is " + currentSpeed.ToString("f2"));
	}
	
	void SetAnimSpeed(float speed)
	{
		//pick appropriate anim speed based on our movement speed.
		//UnityEngine.Debug.Log("FirstRace: speed is " + speed.ToString("f2"));
		anim.SetFloat("Speed", speed);
		if(speed > 2.2f && speed < 4.0f) {
			anim.speed = speed / 2.2f;
		} else if(speed > 4.0f) {
			anim.speed = Mathf.Clamp(speed / 4.0f, 1, 2);
		} else if(speed > 0) {
			anim.speed = speed / 1.25f;
		} else {
			anim.speed = 1f;
		}
	}
}
