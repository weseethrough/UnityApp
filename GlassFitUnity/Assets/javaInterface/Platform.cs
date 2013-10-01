using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System;

public class Platform {
	private double targetElapsedDistance = 0;
	private long time = 0;
	private double distance = 0.0;
	private int calories = 0;
	private float pace = 0;
	//private float timer = 3.0f;
	
	private bool countdown = false;
	private bool started = false;
	private bool error = false;
	
	private Boolean tracking = false;
	
	private Stopwatch timer = new Stopwatch();
	
	private AndroidJavaObject helper;
	private AndroidJavaObject gps;
	private AndroidJavaObject target;
	private AndroidJavaClass helper_class;
	
	
	public Platform() {
		
		UnityEngine.Debug.Log("Platform: constructor called");
		
		try {
			AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
    	    AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
			AndroidJavaObject app = activity.Call<AndroidJavaObject>("getApplicationContext");
  			//gps = new AndroidJavaClass("com.glassfitgames.glassfitplatform.gpstracker.GPSTracker");
			helper_class = new AndroidJavaClass("com.glassfitgames.glassfitplatform.gpstracker.Helper");
			UnityEngine.Debug.LogWarning("Platform: helper_class created OK");
			
			// call the following on the UI thread
			activity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
				
				// Get the singleton helper
				try {
					helper = helper_class.CallStatic<AndroidJavaObject>("getInstance");
        	  	    UnityEngine.Debug.LogWarning("Platform: unique helper instance returned OK");
				} catch (Exception e) {
					UnityEngine.Debug.LogWarning("Platform: Helper.getInstance() failed");
					UnityEngine.Debug.LogException(e);
				}
				// Try to get a Java GPSTracker object
				try {
					gps = helper.Call<AndroidJavaObject>("getGPSTracker", app);
					UnityEngine.Debug.LogWarning("Platform: unique GPS tracker obtained");
				} catch (Exception e) {
					UnityEngine.Debug.LogWarning("Platform: Helper.getGPSTracker() failed");
					UnityEngine.Debug.LogException(e);
				}
				
				// Try to get a Java TargetTracker object
				try {
					target = helper.Call<AndroidJavaObject>("getTargetTracker");
					UnityEngine.Debug.LogWarning("Platform: unique target tracker obtained");
				} catch (Exception e) {
					UnityEngine.Debug.LogWarning("Platform: Helper.getTargetTracker() failed" + e.Message);
					UnityEngine.Debug.LogException(e);
				}
        	}));
			
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Error in constructor" + e.Message);
			UnityEngine.Debug.LogException(e);
		} 
		
	}
	
	public void StartTrack(bool indoor) {
		try {
			gps.Call("setIndoorMode", indoor);
			UnityEngine.Debug.LogWarning("Platform: Indoor mode set to " + indoor.ToString());
			gps.Call("startTracking");
			tracking = true;
			UnityEngine.Debug.LogWarning("Platform: StartTrack succeeded");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: StartTrack failed " + e.Message);
			UnityEngine.Debug.LogException(e);
		}
	}
	
	public Boolean hasLock() {
		try {
			bool gpsLock = gps.Call<Boolean>("hasPosition");
			UnityEngine.Debug.Log("Platform: hasLock() returned " + gpsLock);
			return gpsLock;
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: hasLock() failed: " + e.Message);
			UnityEngine.Debug.LogException(e);
			return false;
		}
	}
	
	public void stopTrack() {
		try {
			gps.Call("stopTracking");
		} catch(Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Problem stopping tracking");
		}
	}
	
	public void reset() {
		try {
			gps.Call("reset");
			UnityEngine.Debug.LogWarning("Platform: GPS has been reset");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: reset() failed: " + e.Message);
			UnityEngine.Debug.LogException(e);
		}
	}
	
	public void setTargetSpeed(float speed)
	{
		try {
			target.Call("setSpeed", speed);
			UnityEngine.Debug.LogWarning("Platform: Speed has been set to " + speed.ToString ());
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: setTargetSpeed() failed: " + e.Message);
			UnityEngine.Debug.LogException(e);
		}
	}
	
	public void setTargetTrack(int trackID)
	{
		try {
			target.Call("setTrack", trackID);
			UnityEngine.Debug.LogWarning("Platform: Track has been set to " + trackID.ToString ());
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: setTargetTrack() failed: " + e.Message);
			UnityEngine.Debug.LogException(e);
		}
	}
	
	public void Poll() {
		
//		if (!hasLock ()) return;
		try {
			time = gps.Call<long>("getElapsedTime");			
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: getElapsedTime() failed: " + e.Message);
			UnityEngine.Debug.LogException(e);
		}
		try {
			targetElapsedDistance = target.Call<double>("getCumulativeDistanceAtTime", Time());
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: getCumulativeDistanceAtTime() failed: " + e.Message);
			UnityEngine.Debug.LogException(e);
		}
		try {
			distance = gps.Call<double>("getElapsedDistance");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: getElapsedDistance() failed: " + e.Message);
			UnityEngine.Debug.LogException(e);
		}
		try {
			pace = gps.Call<float>("getCurrentSpeed");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: getCurrentSpeed() failed: " + e.Message);            
			UnityEngine.Debug.LogException(e);
		}
	}
	/*
	public void SetTargetSpeed(float speed)
	{
		try{
			target.Call<long>("setTargetSpeed", speed);
		}
		catch(Exception e){
		}
		
	}
	*/
	public double DistanceBehindTarget() {
		double returnDistance = (targetElapsedDistance - distance);
		return returnDistance;
	}
	
	public long Time() {
		return time;
	}
	
	public double Distance() {
		return distance;
	}
	
	public int Calories() {
		double cal = 76.0 / 1000.0 * distance;
		return (int)cal;
	}
	
	public float Pace() {
		return pace;
	}
	
}