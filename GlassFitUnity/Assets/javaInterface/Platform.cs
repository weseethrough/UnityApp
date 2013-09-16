using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;

public class Platform {
	private AndroidJavaClass jc;
	private AndroidJavaClass java;
	private AndroidJavaObject gps;
	private AndroidJavaObject target;
	
	private string errorLog = "";
	
	public void Start() {
		try {
			AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
    	    AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
  
			java = new AndroidJavaClass("com.glassfitgames.glassfitplatform.gpstracker.Helper");
        	activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
	        {
				gps = java.CallStatic<AndroidJavaObject>("getGPSTracker");
				Boolean hasGPS = gps.Call<Boolean>("canGetLocation");
				errorLog = "canget: " + hasGPS;
				if (!hasGPS) gps.Call("setIndoorMode", true);
				gps.Call("startTracking");
				target = java.CallStatic<AndroidJavaObject>("getTargetTracker", "pb");
        	}));		
			jc = new AndroidJavaClass("com.glassfitgames.glassfitplatform.gpstracker.HelperDummy");
		} catch (Exception e) {
			errorLog = e.Message;
		}
	}
	
	public long DistanceBehindTarget() {
		try {
			long time = Time();
			return target.Call<long>("getCumulativeDistanceAtTime", time);
		} catch (Exception e) {
			errorLog = errorLog + "\n" + e.Message;
			return 0;
		}
	}
	
	public long Time() {
		try {
			return gps.Call<long>("getElapsedTime");
		} catch (Exception e) {
			errorLog = errorLog + "\n" + e.Message;
			return 0;
		}
	}
	
	public long Distance() {
		try {
			return gps.Call<long>("getElapsedDistance");
		} catch (Exception e) {
			errorLog = errorLog + "\n" + e.Message;
			return 0;
		}
	}
	
	public int Calories() {
		return jc.CallStatic<int>("Calories");
	}
	
	public int Pace() {
		return jc.CallStatic<int>("Pace");
	}
	
	public string DebugLog() {
		return errorLog + ", \n" + jc.CallStatic<string>("DebugLog");
	}
}