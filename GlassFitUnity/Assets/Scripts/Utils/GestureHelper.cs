using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System;

using SimpleJSON;


/// <summary>
/// Touch info. Store index, distance travelled, time for each touch event.
/// </summary>
public class TouchInfo {
	public int touchIndex = -1;
	public Vector2 distanceTravelled;
	public float time = 0.0f;
	
	public TouchInfo(Touch t)
	{
		touchIndex = t.fingerId;
		distanceTravelled = new Vector2(0,0);
	}
}

/// <summary>
/// Gesture helper listens for a message from the Java side
/// </summary>
public class GestureHelper : MonoBehaviour {
	
	private Dictionary<int,TouchInfo> touches = new Dictionary<int, TouchInfo>();
	const float SWIPE_MIN_DIST = 10.0f;
	const float TAP_MAX_DIST = 2.0f;
	
	
	public delegate void OnTap();
	public static OnTap onTap = null;
	
	public delegate void OnSwipeLeft();
	public static OnSwipeLeft swipeLeft = null;
	
	public delegate void TwoFingerTap();
	public static TwoFingerTap onTwoTap = null;
	
	public delegate void OnSwipeRight();
	public static OnSwipeRight swipeRight = null;
	
	public delegate void ThreeFingerTap();
	public static ThreeFingerTap onThreeTap = null;
	
	public delegate void DownSwipe();
	public static DownSwipe onSwipeDown = null;
	
	
	/// <summary>
	/// Handles the tap message from Java
	/// </summary>
	/// <param name='message'>
	/// Message from Java side.
	/// </param>
	void IsTap(string message) {
		UnityEngine.Debug.Log("GestureHelper: Tap received - processing");
		if(onTap != null) {
			UnityEngine.Debug.Log("GestureHelper: Adding to the delegate");
			onTap();
		}		
		UnityEngine.Debug.Log("GestureHelper: message obtained - Tap");
	}
	
	/// <summary>
	/// Retrieves the message from Java if someone taps with two fingers.
	/// </summary>
	/// <param name='message'>
	/// Message from Java side.
	/// </param>
	void TwoTap(string message) {
		UnityEngine.Debug.Log("GestureHelper: Two Tap message received - processing");
		if(onTwoTap != null) {
			UnityEngine.Debug.Log("GestureHelper: Adding to two tap delegate");
			onTwoTap();
		}
	}
	
	// Get a fling left message and set the timer
	void FlingLeft(string message) {
		if(swipeLeft != null) {
			UnityEngine.Debug.Log("GestureHelper: Swipe left setting");
			swipeLeft();
		}
		UnityEngine.Debug.Log("GestureHelper: message obtained - Fling Left");
	}
	
	// Get a fling right message and set the timer
	void FlingRight(string message) {
		if(swipeRight != null) {
			UnityEngine.Debug.Log("GestureHelper: Swipe right setting");
			swipeRight();
		}
		UnityEngine.Debug.Log("GestureHelper: message obtained - Fling Right");
	}
	
	// Get a fling up message and set the timer
	void FlingUp(string message) {
		
	}
	
	// Get a fling down message and set the timer
	void FlingDown(string message) {
//		if(swipeLeft != null) {
//			UnityEngine.Debug.Log("GestureHelper: Swipe down setting");
//			swipeLeft();
//		}
		
		UnityEngine.Debug.Log("GestureHelper: message obtained - Fling down");
	}
	
	void ThreeTap(string message) {
		if(onThreeTap != null) {
			UnityEngine.Debug.Log("GestureHelper: Three tap setting");
			onThreeTap();
		}
		UnityEngine.Debug.Log("GestureHelper: message obtained - three tap");
	}
	
	void TwoSwipeLeft(string message) {
		//Application.Quit();
	}
	
	void Update() 
	{
		// Update the rotation and set it
		if(Input.GetKeyDown(KeyCode.Escape)) 
		{
			if(onSwipeDown != null) {
				onSwipeDown();
			}
		}
		
		//check for gestures on non-glass Android devices
		if(!Platform.Instance.OnGlass())
		{
			int tapCount = 0;
			
			for(int i=0; i<Input.touchCount; i++)
			{
				Touch touch = Input.touches[i];
				//collect touches beginning
				if(touch.phase == TouchPhase.Began)
				{
					TouchInfo ti = new TouchInfo(touch);
					touches.Add(touch.fingerId, ti);
				}
				
				//track touches moving
				if(touch.phase == TouchPhase.Moved)
				{
					if(touches.ContainsKey(touch.fingerId))
					{
						TouchInfo ti = touches[touch.fingerId];
						ti.distanceTravelled += touch.deltaPosition;
						ti.time += touch.deltaTime;
						
						// if they move far enough, count as a swipe
						if(ti.distanceTravelled.x <= -SWIPE_MIN_DIST)
						{
							//	swiped left
							UnityEngine.Debug.Log("Gesture Helper:Android Swipe Left");
							touches.Remove(touch.fingerId);
							FlingLeft("from Unity");
						}
						else if (ti.distanceTravelled.x >= SWIPE_MIN_DIST)
						{
							//	swiped right
							UnityEngine.Debug.Log("Gesture Helper:Android Swipe Right");
							touches.Remove(touch.fingerId);
							FlingRight("from Unity");
						}
						else if(ti.distanceTravelled.y <= - SWIPE_MIN_DIST)
						{
							//swiped down
							touches.Remove(touch.fingerId);
							UnityEngine.Debug.Log("Gesture Helper:Android Swipe Down");
							FlingDown("from Unity");
						}
					}
				}

				//track touches ending
				if(touch.phase == TouchPhase.Ended)
				{
					if(touches.ContainsKey(touch.fingerId))
					{
						TouchInfo ti = touches[touch.fingerId];
						
						// trigger tap if appropriate
						if (ti.distanceTravelled.magnitude <= TAP_MAX_DIST)
						{
							UnityEngine.Debug.Log("Gesture Helper: Android tap detected");
							tapCount ++;
						}
						
						// remove from list
						touches.Remove(touch.fingerId);
					}
				}
				
				//track touches cancelled
				if(touch.phase == TouchPhase.Canceled)
				{					
					if(touches.ContainsKey(touch.fingerId))
					{
						touches.Remove(touch.fingerId);
					}
				}
			}	//for all touches
			
			switch(tapCount)
			{
			case 1:
			{
				//single tap
				UnityEngine.Debug.Log("Gesture Helper:Android 1-tap");
				IsTap("from Unity");
				break;
			}
			case 2:
			{
				//double tap
				UnityEngine.Debug.Log("Gesture Helper:Android 2-tap");
				TwoTap("from Unity");
				break;
			}
			case 3:
			{
				//triple tap
				UnityEngine.Debug.Log("Gesture Helper:Android 3-tap");
				ThreeTap("from Unity");
				break;
			}
			default:
				break;
			}
		}
	}
}
