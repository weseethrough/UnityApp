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
	
	public delegate void TwoFingerTap();
	public static TwoFingerTap onTwoTap = null;
	
	public delegate void ThreeFingerTap();
	public static ThreeFingerTap onThreeTap = null;

	public delegate void OnSwipeLeft();
	public static OnSwipeLeft onSwipeLeft = null;

	public delegate void OnSwipeRight();
	public static OnSwipeRight onSwipeRight = null;
	
	public delegate void UpSwipe();
	public static UpSwipe onSwipeUp = null;

	public delegate void DownSwipe();
	public static DownSwipe onSwipeDown = null;
	
	
	/// <summary>
	/// Handles the tap message from Java
	/// </summary>
	/// <param name='message'>
	/// Message from Java side.
	/// </param>
	void IsTap(string message) {
		UnityEngine.Debug.Log("GestureHelper: Tap meassage received...");
		if(onTap != null) {
			UnityEngine.Debug.Log("GestureHelper: ... calling tap delegates");
			onTap();
		} else {
			UnityEngine.Debug.Log("GestureHelper: ... but no tap delegates are registered. Dropping message.");
		}
	}
	
	/// <summary>
	/// Retrieves the message from Java if someone taps with two fingers.
	/// </summary>
	/// <param name='message'>
	/// Message from Java side.
	/// </param>
	void TwoTap(string message) {
		UnityEngine.Debug.Log("GestureHelper: Two Tap message received...");
		if(onTwoTap != null) {
			UnityEngine.Debug.Log("GestureHelper: ... calling tap delegates");
			onTwoTap();
		} else {
			UnityEngine.Debug.Log("GestureHelper: ... but no two-tap delegates are registered. Dropping message.");
		}
	}
	
	// Get a fling left message and set the timer
	void FlingLeft(string message) {
		UnityEngine.Debug.Log("GestureHelper: Swipe left message received...");
		if(onSwipeLeft != null) {
			UnityEngine.Debug.Log("GestureHelper: ... calling swipe left delegates");
			onSwipeLeft();
		} else {
			UnityEngine.Debug.Log("GestureHelper: ... but no swipe-left delegates are registered. Dropping message.");
		}
	}
	
	// Get a fling right message and set the timer
	void FlingRight(string message) {
		UnityEngine.Debug.Log("GestureHelper: Swipe right message received...");
		if(onSwipeRight != null) {
			UnityEngine.Debug.Log("GestureHelper: ... calling swipe right delegates");
			onSwipeRight();
		} else {
			UnityEngine.Debug.Log("GestureHelper: ... but no swipe right delegates are registered. Dropping message.");
		}

	}
	
	// Get a fling up message and set the timer
	void FlingUp(string message) {
		UnityEngine.Debug.Log("GestureHelper: Swipe up message received...");
		if(onSwipeUp != null) {
			UnityEngine.Debug.Log("GestureHelper: ... calling swipe up delegates");
			onSwipeUp();
		} else {
			UnityEngine.Debug.Log("GestureHelper: ... but no swipe up delegates are registered. Dropping message.");
		}
	}
	
	// Get a fling down message and set the timer
	void FlingDown(string message) {
		UnityEngine.Debug.Log("GestureHelper: Swipe down message received...");
		if(onSwipeDown != null) {
			UnityEngine.Debug.Log("GestureHelper: ... calling swipe down delegates");
			onSwipeDown();
		} else {
			UnityEngine.Debug.Log("GestureHelper: ... but no swipe down delegates are registered. Dropping message.");
		}
	}
	
	void ThreeTap(string message) {
		UnityEngine.Debug.Log("GestureHelper: Three tap message received...");
		if(onThreeTap != null) {
			UnityEngine.Debug.Log("GestureHelper: ... calling three-tap delegates");
			onThreeTap();
		} else {
			UnityEngine.Debug.Log("GestureHelper: ... but no three-tap delegates are registered. Dropping message.");
		}
	}

	
	void Update()
	{
		// If the back key is pressed, call any swipe-down delegates
		// (on glass, swipe down means back)
		if(Input.GetKeyDown(KeyCode.Escape))
		{
			if(onSwipeDown != null) {
				onSwipeDown();
			}
		}
#if UNITY_EDITOR		
		if(Input.GetKeyDown(KeyCode.Return)) 
		{
			if (onTap != null) onTap();
		}
#endif

		//check for gestures on non-glass devices where unity can pick up the input
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
							UnityEngine.Debug.Log("Gesture Helper: Touchscreen swipe left");
							touches.Remove(touch.fingerId);
							if (onSwipeLeft != null) onSwipeLeft();
						}
						else if (ti.distanceTravelled.x >= SWIPE_MIN_DIST)
						{
							//	swiped right
							UnityEngine.Debug.Log("Gesture Helper: Touchscreen swipe right");
							touches.Remove(touch.fingerId);
							if (onSwipeRight != null) onSwipeRight();
						}
						else if(ti.distanceTravelled.y <= - SWIPE_MIN_DIST)
						{
							//swiped down
							touches.Remove(touch.fingerId);
							UnityEngine.Debug.Log("Gesture Helper: Touchscreen swipe down");
							if (onSwipeDown != null) onSwipeDown();
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
				UnityEngine.Debug.Log("Gesture Helper: Touchscreen 1-tap");
				if (onTap != null) onTap();
				break;
			}
			case 2:
			{
				//double tap
				UnityEngine.Debug.Log("Gesture Helper: Touchscreen 2-tap");
				if (onTwoTap != null) onTwoTap();
				break;
			}
			case 3:
			{
				//triple tap
				UnityEngine.Debug.Log("Gesture Helper: Touchscreen 3-tap");
				if (onThreeTap != null) onThreeTap();
				break;
			}
			default:
				break;
			}
		}
	}
}
