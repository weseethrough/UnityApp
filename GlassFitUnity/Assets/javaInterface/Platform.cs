﻿using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System;

public class Platform {
	private long distanceBehindTarget = 0;
	private long time = 0;
	private long distance = 0;
	private int calories = 0;
	private float pace = 0;
	private Position position = null;
	
	private Boolean tracking = false;
	
	private Stopwatch lerpTimer = new Stopwatch();
	
	private AndroidJavaClass helper;
	private AndroidJavaObject gps;
	private AndroidJavaObject target;
	
	// Initialization may fail silently, assume failure unless properly initialized
	private Boolean error = true;	
	private string errorLog = "Not yet initialized";
	
	public Platform() {
		error = true;
		try {
			AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
    	    AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
			AndroidJavaObject app = activity.Call<AndroidJavaObject>("getApplicationContext");
  
			helper = new AndroidJavaClass("com.glassfitgames.glassfitplatform.gpstracker.Helper");
        	activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
	        {
				try {
					gps = helper.CallStatic<AndroidJavaObject>("getGPSTracker", app);
					target = helper.CallStatic<AndroidJavaObject>("getTargetTracker", "pb");
					errorLog = "";
					error = false;
				} catch (Exception e) {
					errorLog = e.Message;
				}
        	}));		
		} catch (Exception e) {
			errorLog = e.Message;
		}
	}
	
	public void Start(Boolean indoor) {
		try {
			if (indoor) gps.Call("setIndoorMode", true);
			gps.Call("startTracking");
			tracking = true;
		} catch (Exception e) {
			errorLog = e.Message;
			error = true;
		}
	}
	
	public Boolean hasLock() {
		try {
			return gps.Call<Boolean>("canGetPosition");
		} catch (Exception e) {
			errorLog = errorLog + "\n" + e.Message;
			return false;
		}
	}
	
	public void Poll() {
		if (error) return;
//		if (!hasLock ()) return;
		try {
			long gpsTime = gps.Call<long>("getElapsedTime");
			if (gpsTime != time) {
				lerpTimer.Reset();
				lerpTimer.Start();
				time = gpsTime;
			}
		} catch (Exception e) {
//			errorLog = errorLog + "\ngetElapsedTime: " + e.Message;
		}
		try {
			distanceBehindTarget = target.Call<long>("getCumulativeDistanceAtTime", Time());
		} catch (Exception e) {
//			errorLog = errorLog + "\ngetCumulativeDistanceAtTime" + e.Message;
		}
		try {
			distance = gps.Call<long>("getElapsedDistance");
		} catch (Exception e) {
///			errorLog = errorLog + "\ngetElapsedDistance" + e.Message;
		}
		try {
			pace = gps.Call<float>("getCurrentPace");
		} catch (Exception e) {
//			errorLog = errorLog + "\ngetCurrentPace" + e.Message;
		}
		try {
			if (hasLock()) {
				AndroidJavaObject ajo = gps.Call<AndroidJavaObject>("getCurrentPosition");
				position = new Position((float)ajo.Call<double>("getLatx"), (float)ajo.Call<double>("getLngx"));
			}
		} catch (Exception e) {
			errorLog = errorLog + "\ngetCurrentPosition" + e.Message;
		}
		// TODO: getCurrentPosition()
	}
	
	public long DistanceBehindTarget() {
		return distanceBehindTarget;
	}
	
	public long Time() {
		return time+lerpTimer.ElapsedMilliseconds;
	}
	
	public long Distance() {
		return distance;
	}
	
	public int Calories() {
		return calories;
	}
	
	public float Pace() {
		return pace;
	}
	
	public Position Position() {
		return position;
	}
	
	public string DebugLog() {
		return errorLog + ", \nOn device";
	}
}