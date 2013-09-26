using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System;

public class PBRunnerController : MonoBehaviour {
	
	private Platform inputData = null;
	
	public GameObject platform;
	private Transform CurrentLocation;
	private Transform targetLocation;
	private float myDistance;
	private float targetDistance;
	
	private float countTime = 3.99f;
	private bool started = false;
	
	private float scaledPace;
	private float paceSlider;
	private Rect sliderBox;
	private float indoorDistance;
	private float timeChange;
	private double scaledDistance;
	private bool GoGoGo = false;
	
	// Use this for initialization
	void Start () {
		
	}
	
	void Update () {
		
		if(countTime == 3.99f && inputData.hasLock() && !started)
		{
			started = true;
		}
		
		if(started && countTime <= 0.0f)
		{
			inputData.StartTrack(false);
		}
		else if(started && countTime > 0.0f)
		{
			countTime -= Time.deltaTime;
		}
		
		inputData.Poll();
		
//		if(!started && Input.touchCount == 3)
//		{
//			started = true;
//			inputData.Start(false);
//		}

		scaledDistance = inputData.DistanceBehindTarget() * 6.666f;
		Vector3 movement = new Vector3(-10,-14,(float)scaledDistance);
		transform.position = movement;
	}
}
