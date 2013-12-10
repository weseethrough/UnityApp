using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System;

using SimpleJSON;

/// <summary>
/// Gesture helper listens for a message from the Java side
/// </summary>
public class GestureHelper : MonoBehaviour {
	
	public delegate void OnTap();
	public static OnTap onTap = null;
	
	public delegate void OnSwipeLeft();
	public static OnSwipeLeft swipeLeft = null;
	
	public delegate void TwoFingerTap();
	public static TwoFingerTap onTwoTap = null;
	
	public delegate void OnSwipeRight();
	public static OnSwipeRight swipeRight = null;
	
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
}
