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
	private Position position = null;
	private float bearing = 0;
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
	private AndroidJavaObject activity;
	private AndroidJavaObject context;
	
	
	public Platform() {
		
		UnityEngine.Debug.Log("Platform: constructor called");
		
		try {
			AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
    	    activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
			context = activity.Call<AndroidJavaObject>("getApplicationContext");
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
					gps = helper.Call<AndroidJavaObject>("getGPSTracker", context);
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
			gps.Call("startTracking");
			tracking = true;
			UnityEngine.Debug.LogWarning("Platform: StartTrack succeeded");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: StartTrack failed " + e.Message);
			UnityEngine.Debug.LogException(e);
		}
	}
	
	public void setIndoor(bool indoor) {
		try {
			AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
    	    AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
			
			activity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
				gps.Call("setIndoorMode", indoor);
				UnityEngine.Debug.LogWarning("Platform: Indoor mode set to " + indoor.ToString());
			}));
		} catch(Exception e) {
			UnityEngine.Debug.Log("Platform: Error setting indoor mode " + e.Message);
		}
	}
	
	public float getCurrentSpeed(long l) 
	{
		try {
			float ret = target.Call<float>("getCurrentSpeed", l);
			UnityEngine.Debug.Log("Platform: speed obtained, currently: " + ret.ToString());
			return ret;
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Error getting current speed: " + e.Message);
			return 0;
		}
	}
	
	public Boolean hasLock() {
		try {
			bool gpsLock = gps.Call<Boolean>("hasPosition");
//			UnityEngine.Debug.Log("Platform: hasLock() returned " + gpsLock);
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
	
	public void authenticate() {
		try {
			helper_class.CallStatic("authenticate", activity);
		} catch(Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Problem authenticating");
			UnityEngine.Debug.LogException(e);
		}
	}
	
	public void syncToServer() {
		try {
			helper_class.CallStatic("syncToServer", context);
		} catch(Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Problem syncing to server");
			UnityEngine.Debug.LogException(e);
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
	
	public byte[] LoadBlob(string id) {
		try {
			byte[] blob = helper_class.CallStatic<byte[]>("loadBlob", id);
			UnityEngine.Debug.LogWarning("Platform: Game blob " + id + " of size: " + blob.Length + " loaded");
			return blob;
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: LoadBlob() failed: " + e.Message);
			UnityEngine.Debug.LogException(e);			
		}
		return null;
	}
	
	public void StoreBlob(string id, byte[] blob) {
		try {
			helper_class.CallStatic("storeBlob", id, blob);
			UnityEngine.Debug.LogWarning("Platform: Game blob " + id + " of size: " + blob.Length + " stored");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: StoreBlob() failed: " + e.Message);
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
		try {
			if (hasLock()) {
				AndroidJavaObject ajo = gps.Call<AndroidJavaObject>("getCurrentPosition");
				position = new Position((float)ajo.Call<double>("getLatx"), (float)ajo.Call<double>("getLngx"));
				ajo = gps.Call<AndroidJavaObject>("getCurrentBearing");
				bearing = ajo.Call<float>("floatValue");
			}
		} catch (Exception e) {
//			errorLog = errorLog + "\ngetCurrentPosition|Bearing" + e.Message;
		}
		
	}

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
	
	public Position Position() {
		return position;
	}
	
	public float Bearing() {
		return bearing;
	}
	
}