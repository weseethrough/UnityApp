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
	
	// Booleans for tapping, left, right, up and down fast swipes
	public bool isTapped = false;
	public bool left = false;
	public bool right = false;
	public bool up = false;
	public bool down = false;
	
	// Timers to expire the booleans
	private float tapTimer = 0.0f;
	private float upTimer = 0.0f;
	private float downTimer = 0.0f;
	private float leftTimer = 0.0f;
	private float rightTimer = 0.0f;
	
	// Get a tapping message and set the timer
	void isTap(string message) {
		isTapped = true;
		tapTimer = 5.0f;
		UnityEngine.Debug.Log("Message Obtained: Tap");
	}
	
	// Get a fling left message and set the timer
	void flingLeft(string message) {
		left = true;
		leftTimer = 5.0f;
		UnityEngine.Debug.Log("Message Obtained: Fling Left");
	}
	
	// Get a fling right message and set the timer
	void flingRight(string message) {
		right = true;
		rightTimer = 5.0f;
		UnityEngine.Debug.Log("Message Obtained: Fling Right");
	}
	
	// Get a fling up message and set the timer
	void flingUp(string message) {
		up = true;
		upTimer = 5.0f;
	}
	
	// Get a fling down message and set the timer
	void flingDown(string message) {
		down = true;
		downTimer = 5.0f;
	}
	
	void Update () {
		
		// Decrease the timers and reset the booleans when the timers run out
		// Time will only run out if boolean isn't handled, but should be.
		tapTimer -= Time.deltaTime;
		if(tapTimer <= 0.0f) {
			isTapped = false;
		}
		
		leftTimer -= Time.deltaTime;
		if(leftTimer <= 0.0f) {
			left = false;
		}
		
		rightTimer -= Time.deltaTime;
		if(rightTimer <= 0.0f) {
			right = false;
		}
		
		upTimer -= Time.deltaTime;
		if(upTimer <= 0.0f) {
			up = false;
		}
		
		downTimer -= Time.deltaTime;
		if(downTimer <= 0.0f) {
			down = false;
		}
	}
}
