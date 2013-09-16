using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;

public class Platform {
	private AndroidJavaClass java;
	private AndroidJavaObject gps;
	private AndroidJavaObject target;
	
	// Initialization may fail silently, assume failure unless properly initialized
	private Boolean error = true;	
	private string errorLog = "Not yet initialized";
	
	public void Start() {
		error = true;
		try {
			AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
    	    AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
			AndroidJavaObject app = activity.Call<AndroidJavaObject>("getApplicationContext");
  
			java = new AndroidJavaClass("com.glassfitgames.glassfitplatform.gpstracker.Helper");
        	activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
	        {
				try {
					gps = java.CallStatic<AndroidJavaObject>("getGPSTracker", app);
					Boolean hasGPS = gps.Call<Boolean>("canGetLocation");
					errorLog = "canget: " + hasGPS;
					if (!hasGPS) gps.Call("setIndoorMode", true);
					gps.Call("startTracking");
					target = java.CallStatic<AndroidJavaObject>("getTargetTracker", "pb");
					error = false;
				} catch (Exception e) {
					errorLog = e.Message;
				}
        	}));		
		} catch (Exception e) {
			errorLog = e.Message;
		}
	}
	
	public long DistanceBehindTarget() {
		if (error) return 0;
		try {
			long time = Time();
			return target.Call<long>("getCumulativeDistanceAtTime", time);
		} catch (Exception e) {
			errorLog = errorLog + "\n" + e.Message;
			return 0;
		}
	}
	
	public long Time() {
		if (error) return 0;
		try {
			return gps.Call<long>("getElapsedTime");
		} catch (Exception e) {
			errorLog = errorLog + "\n" + e.Message;
			return 0;
		}
	}
	
	public long Distance() {
		if (error) return 0;
		try {
			return gps.Call<long>("getElapsedDistance");
		} catch (Exception e) {
			errorLog = errorLog + "\n" + e.Message;
			return 0;
		}
	}
	
	public int Calories() {
		return 0;
	}
	
	public float Pace() {
		if (error) return 0;
		try {
			return gps.Call<float>("getCurrentPace");
		} catch (Exception e) {
			errorLog = errorLog + "\n" + e.Message;
			return 0;
		}
	}
	
	public string DebugLog() {
		return errorLog + ", \nOn device";
	}
}