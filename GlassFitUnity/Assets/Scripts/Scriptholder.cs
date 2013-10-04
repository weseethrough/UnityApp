using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System;

using SimpleJSON;

public class Scriptholder : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	void NewGPSPosition(string message)
	{
		var node = JSON.Parse(message);
		
		//speed, bearing, elapsedDistance floats/double
		float speed;
		try {
			speed = node["currectSpeed"].AsFloat;
		} catch(Exception e) {
			speed = 0;
			UnityEngine.Debug.Log("JSON: Error with speed");
		}
		
		float bearing;
		try {
			bearing = node["currentBearing"].AsFloat;
		} catch(Exception e) {
			bearing = -999;
			UnityEngine.Debug.Log("JSON: Error with bearing");
		}
		
		float elapsedDistance;
		try {
			elapsedDistance = node["elapsedDistance"].AsFloat;
		} catch(Exception e) {
			elapsedDistance = 0;
			UnityEngine.Debug.Log("JSON: Error with elapsed distance");
		}
		
		bool hasPos;
		try {
			hasPos = node["hasPosition"].AsBool;
		} catch(Exception e) {
			hasPos = false;
			UnityEngine.Debug.Log("JSON: Error getting position");
		}
		
		bool hasBear;
		try {
			hasBear = node["hasBearing"].AsBool;
		} catch(Exception e) {
			hasBear = false;
			UnityEngine.Debug.Log("JSON: Error getting bearing");
		}
		
		bool isTrack;
		try {
			isTrack = node["isTracking"].AsBool;
		} catch(Exception e) {
			isTrack = false;
			UnityEngine.Debug.Log("JSON: Error checking tracking");
		}
		
		int time;
		try {
			time = node["elapsedTime"].AsInt;
		} catch(Exception e) {
			time = 0;
			UnityEngine.Debug.Log("JSON: Error getting elapsed time");
		}
		
		UnityEngine.Debug.Log("JSON: New speed "+speed);
		UnityEngine.Debug.Log("JSON: new bearing is "+bearing); 
		UnityEngine.Debug.Log ("JSON: new elapsedDistance is "+elapsedDistance);
		UnityEngine.Debug.Log("JSON: has bearing" + hasBear.ToString());
		UnityEngine.Debug.Log("JSON: has position" + hasPos.ToString());
		UnityEngine.Debug.Log("JSON: is tracking" + isTrack.ToString());
		UnityEngine.Debug.Log("JSON: elapsed time" + time);
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
