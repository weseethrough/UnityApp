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
	
	// Get a tapping message and set the timer
	void isTap(string message) {
		GameObject hex = GameObject.Find("UIScene");
		DynamicHexList list = hex.GetComponentInChildren<DynamicHexList>();
		if(list != null) {
			list.EnterGame();
		}
		
		BroadcastMessage("ResetGyro");

		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		
		GConnector gConect = fs.Outputs.Find(r => r.Name == "ContinueButton");
		if(gConect != null) {
			fs.parentMachine.FollowConnection(gConect);
		}
		
		UnityEngine.Debug.Log("Message Obtained: Tap");
	}
	
	// Get a fling left message and set the timer
	void flingLeft(string message) {
		
		UnityEngine.Debug.Log("Message Obtained: Fling Left");
	}
	
	// Get a fling right message and set the timer
	void flingRight(string message) {
		
		UnityEngine.Debug.Log("Message Obtained: Fling Right");
	}
	
	// Get a fling up message and set the timer
	void flingUp(string message) {
		
	}
	
	// Get a fling down message and set the timer
	void flingDown(string message) {
		GameObject scene = GameObject.Find("UIScene");
		DynamicHexList list = scene.GetComponentInChildren<DynamicHexList>();
		if(list != null) {
			list.GoBack();
		}
		
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		
		GConnector gConect = fs.Outputs.Find(r => r.Name == "MenuButton");
		if(gConect != null) {
			fs.parentMachine.FollowConnection(gConect);
		}
		
		gConect = fs.Outputs.Find(r => r.Name == "FinishButton");
		if(gConect != null) {
			fs.parentMachine.FollowConnection(gConect);
		}
		
		UnityEngine.Debug.Log("Message Obtained: Fling down");
	}
	
	void Update () {
		
		// Decrease the timers and reset the booleans when the timers run out
		// Time will only run out if boolean isn't handled, but should be.
//		tapTimer -= Time.deltaTime;
//		if(tapTimer <= 0.0f) {
//			isTapped = false;
//		}
//		
//		leftTimer -= Time.deltaTime;
//		if(leftTimer <= 0.0f) {
//			left = false;
//		}
//		
//		rightTimer -= Time.deltaTime;
//		if(rightTimer <= 0.0f) {
//			right = false;
//		}
//		
//		upTimer -= Time.deltaTime;
//		if(upTimer <= 0.0f) {
//			up = false;
//		}
//		
//		downTimer -= Time.deltaTime;
//		if(downTimer <= 0.0f) {
//			down = false;
//		}
	}
}
