using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System;

public class PBRunnerController : MonoBehaviour {
	
	#if UNITY_ANDROID && !UNITY_EDITOR 
	private Platform inputData = null;
#else
	private PlatformDummy inputData = null;
#endif
	
	public GameObject platform;
	private Transform CurrentLocation;
	private Transform targetLocation;
	private float myDistance;
	private float targetDistance;
	
	private float countTime = 3.99f;
	private bool countdown = false;
	private bool started = false;
	private Stopwatch timer = new Stopwatch();
	
	private float scaledPace;
	private float paceSlider;
	private Rect sliderBox;
	private float indoorDistance;
	private float timeChange;
	private double scaledDistance;
	private bool GoGoGo = false;
	
	// Use this for initialization
	void Start () {
		#if UNITY_ANDROID && !UNITY_EDITOR 
		inputData = new Platform();
		#else
		inputData = new PlatformDummy();
		#endif
	}

	
	void Update () {
		
//		if(!countdown)
//		{
//			if(inputData.hasLock())
//			{
//				countdown = true;
//			}
//		}
//		else
//		{
//			if(!started)
//			{
//				inputData.StartTrack(false);
//				started = true;
//			}
//			countTime -= Time.deltaTime;
//		}
		
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
