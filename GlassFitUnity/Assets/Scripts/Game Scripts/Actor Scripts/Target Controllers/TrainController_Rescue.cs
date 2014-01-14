using UnityEngine;
using System.Collections;

public class TrainController_Rescue : TargetController {
	
	protected float distanceFromStart = 0.0f;
	protected float headStartDistance = 0.0f;
	protected float currentMovementSpeed = 7.0f;
	protected float timeRunStarted;
	
	// Use this for initialization
	void Start () {
		travelSpeed = 1.0f;	//somewhat arbitrary scale factor for positioning distance
		lane = 1;
		lanePitch = 1.0f;
		SetAttribs(0.0f, 1.0f, transform.position.y, transform.position.x);
		UnityEngine.Debug.Log("FirstRaceOpponent: started");
		
		timeRunStarted = Time.time;
			
	}
	
	// Update is called once per frame
	void Update () {

		//update our total distance moved based on current speed and time passed.
		distanceFromStart += Time.deltaTime * currentMovementSpeed;

		//base update to set position.
		base.Update();
		
		//Set position
		
		//Retrieve desired position from track
	}
	
}
