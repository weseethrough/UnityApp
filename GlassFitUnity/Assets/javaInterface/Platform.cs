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
	
	private AndroidJavaClass helper;
	private AndroidJavaObject gps;
	private AndroidJavaObject target;
	
	// Initialization may fail silently, assume failure unless properly initialized
	//private Boolean error = true;	
	private string errorLog = "Not yet initialized";
	
	public Platform() {
		error = true;
		UnityEngine.Debug.Log("Constructor is called");
		errorLog = errorLog + "GlassfitUnity \n Platform constructor called \n";
		try {
			AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
    	    AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
			AndroidJavaObject app = activity.Call<AndroidJavaObject>("getApplicationContext");
  			//gps = new AndroidJavaClass("com.glassfitgames.glassfitplatform.gpstracker.GPSTracker");
			helper = new AndroidJavaClass("com.glassfitgames.glassfitplatform.gpstracker.Helper");
        	activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
	        {
				try {
					gps = helper.CallStatic<AndroidJavaObject>("getGPSTracker", app);
					if(gps==null)
						errorLog = errorLog + "\n GlassfitUnity \n gps is null!";
					UnityEngine.Debug.Log("\n Glassfit Unity \n gps has been obtained \n");
					target = helper.CallStatic<AndroidJavaObject>("getTargetTracker");
					errorLog = "";
					error = false;
				} catch (Exception e) {
					UnityEngine.Debug.Log("\n Glassfit Unity \n error getting gps\target \n" + e.Message);
				}
        	}));		
		} catch (Exception e) {
			UnityEngine.Debug.Log("\n Glassfit Unity \n error getting class/object \n" + e.Message);;
					UnityEngine.Debug.Log("GlassfitUnity gps is null!");
				} 
	}
	
	public void StartTrack(bool indoor) {
		try {
			gps.Call("setIndoorMode", indoor);
			gps.Call("startTracking");
			tracking = true;
			errorLog = errorLog + "GlassfitUnity start function called\n";
		} catch (Exception e) {
			UnityEngine.Debug.Log("\n Glassfit Unity \n error calling start \n" + e.Message);
			error = true;
		}
	}
	
	public Boolean hasLock() {
		try {
			UnityEngine.Debug.Log(errorLog + "Checking for position" + "\n" + "GlassfitUnity");
			return gps.Call<Boolean>("hasPosition");
		} catch (Exception e) {
			UnityEngine.Debug.Log("\nGlassfitUnity\nProblem getting lock\n" + e.Message);
			return false;
		}
	}
	
	public void reset() {
		try {
			gps.Call("reset");
		} catch (Exception e) {
			UnityEngine.Debug.Log("Error resetting GPS " + e.Message);
		}
	}
	
	public void setTargetSpeed(float speed)
	{
		try {
			target.Call("setSpeed", speed);
		} catch (Exception e) {
			errorLog = errorLog + "\n" + e.Message;
		}
	}
	
	public void setTargetTrack(int trackID)
	{
		try {
			target.Call("setTrack", trackID);
		} catch (Exception e) {
			errorLog = errorLog + "\n" + e.Message;
		}
	}
	
	public void Poll() {
		if (error) return;
//		if (!hasLock ()) return;
		try {
			time = gps.Call<long>("getElapsedTime");			
		} catch (Exception e) {
//			errorLog = errorLog + "\ngetElapsedTime: " + e.Message;
		}
		try {
			targetElapsedDistance = target.Call<double>("getCumulativeDistanceAtTime", Time());
		} catch (Exception e) {
//			errorLog = errorLog + "\ngetCumulativeDistanceAtTime" + e.Message;
		}
		try {
			distance = gps.Call<double>("getElapsedDistance");
		} catch (Exception e) {
///			errorLog = errorLog + "\ngetElapsedDistance" + e.Message;
		}
		try {
			pace = gps.Call<float>("getCurrentSpeed");
		} catch (Exception e) {
//			errorLog = errorLog + "\ngetCurrentPace" + e.Message;
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
	
	public string DebugLog() {
		return errorLog + ", \nOn device";
	}
}