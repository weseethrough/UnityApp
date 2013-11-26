using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System;

public class TrainController : TargetController {
	
	private float whistleTime = 0.0f;
//	private double scaledDistance;
	private AudioSource trainMove;
	private AudioSource trainWhistle;
//	private TargetTracker target;
	
	// Use this for initialization
	void Start () {
		base.Start();
		setAttribs(50, 135, -300, 103.8f);
		//target = Platform.Instance.getTargetTracker();
		
		var aSources = GetComponents<AudioSource>();
		trainMove = aSources[0];
		trainWhistle = aSources[1];
		//trainMove.Play();
	}
	
	void OnEnable() {
		//transform.position = new Vector3(103.8f, -300, -50);
		base.OnEnable();
		setAttribs(50, 135, -300, 103.8f);
		UnityEngine.Debug.Log("Train: Enable function called");
	}
	
	// Update is called once per frame
	void Update () {
		base.Update();
		whistleTime += Time.deltaTime;
		
		if(whistleTime >= 10.0f)
		{
			//trainWhistle.Play();
			whistleTime -= 10.0f;
		}
		
		//Platform.Instance.Poll();
	
		//scaledDistance = (target.getTargetDistance() - 50) * 135;

		//Vector3 movement = new Vector3(103.8f,-300,(float)scaledDistance);
		//transform.position = movement;
	}
}
