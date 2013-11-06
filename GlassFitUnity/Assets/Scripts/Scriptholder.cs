using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System;

using SimpleJSON;

public class Scriptholder : Singleton<Scriptholder> {
	
	public bool isTapped = false;
	public bool left = false;
	public bool right = false;
	public bool up = false;
	public bool down = false;
	private float tapTimer = 0.0f;
	private float upTimer = 0.0f;
	private float downTimer = 0.0f;
	private float leftTimer = 0.0f;
	private float rightTimer = 0.0f;
	
	// Use this for initialization
	void Start () {
	
	}
	
//	void NewGPSPosition(string message)
//	{
//		var node = JSON.Parse(message);
//		
//		//speed, bearing, elapsedDistance floats/double
//		float speed;
//		try {
//			speed = node["currectSpeed"].AsFloat;
//		} catch(Exception e) {
//			speed = 0;
//			UnityEngine.Debug.Log("JSON: Error with speed");
//		}
//		
//		float bearing;
//		try {
//			bearing = node["currentBearing"].AsFloat;
//		} catch(Exception e) {
//			bearing = -999;
//			UnityEngine.Debug.Log("JSON: Error with bearing");
//		}
//		
//		float elapsedDistance;
//		try {
//			elapsedDistance = node["elapsedDistance"].AsFloat;
//		} catch(Exception e) {
//			elapsedDistance = 0;
//			UnityEngine.Debug.Log("JSON: Error with elapsed distance");
//		}
//		
//		bool hasPos;
//		try {
//			hasPos = node["hasPosition"].AsBool;
//		} catch(Exception e) {
//			hasPos = false;
//			UnityEngine.Debug.Log("JSON: Error getting position");
//		}
//		
//		bool hasBear;
//		try {
//			hasBear = node["hasBearing"].AsBool;
//		} catch(Exception e) {
//			hasBear = false;
//			UnityEngine.Debug.Log("JSON: Error getting bearing");
//		}
//		
//		bool isTrack;
//		try {
//			isTrack = node["isTracking"].AsBool;
//		} catch(Exception e) {
//			isTrack = false;
//			UnityEngine.Debug.Log("JSON: Error checking tracking");
//		}
//		
//		int time;
//		try {
//			time = node["elapsedTime"].AsInt;
//		} catch(Exception e) {
//			time = 0;
//			UnityEngine.Debug.Log("JSON: Error getting elapsed time");
//		}
//		
//		UnityEngine.Debug.Log("JSON: New speed "+speed);
//		UnityEngine.Debug.Log("JSON: new bearing is "+bearing); 
//		UnityEngine.Debug.Log ("JSON: new elapsedDistance is "+elapsedDistance);
//		UnityEngine.Debug.Log("JSON: has bearing" + hasBear.ToString());
//		UnityEngine.Debug.Log("JSON: has position" + hasPos.ToString());
//		UnityEngine.Debug.Log("JSON: is tracking" + isTrack.ToString());
//		UnityEngine.Debug.Log("JSON: elapsed time" + time);
//		
//	}
	
	void isTap(string message) {
		isTapped = true;
		tapTimer = 5.0f;
		UnityEngine.Debug.Log("Message Obtained: Tap");
	}
	
	void flingLeft(string message) {
		left = true;
		leftTimer = 5.0f;
		UnityEngine.Debug.Log("Message Obtained: Fling Left");
	}
	
	void flingRight(string message) {
		right = true;
		rightTimer = 5.0f;
		UnityEngine.Debug.Log("Message Obtained: Fling Right");
	}
	
	void flingUp(string message) {
		up = true;
		upTimer = 5.0f;
	}
	
	void flingDown(string message) {
		down = true;
		downTimer = 5.0f;
	}
	
	// Update is called once per frame
	void Update () {
		
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
