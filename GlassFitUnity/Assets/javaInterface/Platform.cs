using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using System;

public class Platform : MonoBehaviour {
	private double targetElapsedDistance = 0;
	private long time = 0;
	private double distance = 0.0;
	private int calories = 0;
	private float pace = 0;
	private Position position = null;
	private float bearing = 0;
	private bool started = false;
	private bool initialised = false;
	private long currentActivityPoints = 0;
	private long openingPointsBalance = 0;
	
	private List<Position> positions;
	
	private Boolean tracking = false;
	
	private AndroidJavaObject helper;
	private AndroidJavaObject points_helper;
	private AndroidJavaObject gps;
	private AndroidJavaClass helper_class;
	private AndroidJavaClass points_helper_class;
	private AndroidJavaObject activity;
	private AndroidJavaObject context;
	
	private List<TargetTracker> targetTrackers;
	
	
	private static Platform _instance;
	private static object _lock = new object();
	
	public static Platform Instance {
		get {
			if(applicationIsQuitting) {
				UnityEngine.Debug.Log("Singleton: already destroyed on application quit - won't create again");
				return null;
			}
			lock(_lock) {
				if(_instance == null) {
					_instance = (Platform) FindObjectOfType(typeof(Platform));
					if(FindObjectsOfType(typeof(Platform)).Length > 1) {
						UnityEngine.Debug.Log("Singleton: there is more than one singleton");
						return _instance;
					}
					if(_instance == null) {
						GameObject singleton = new GameObject();
						_instance = singleton.AddComponent<Platform>();
						singleton.name = "(singleton) " + typeof(Platform).ToString();
						
						DontDestroyOnLoad(singleton);
					} else {
						UnityEngine.Debug.Log("Singleton: already exists!!");
					}
				}
				while(!_instance.initialised) {
					continue;
				}
					return _instance;
			}
		}
	}
	
	private static bool applicationIsQuitting = false;
	
	public void OnDestroy() {
		applicationIsQuitting = true;
	}
	
	
	protected Platform() {
		
		targetTrackers = new List<TargetTracker>();
		UnityEngine.Debug.Log("Platform: constructor called");
		
		try {
			AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
    	    activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
			context = activity.Call<AndroidJavaObject>("getApplicationContext");
			helper_class = new AndroidJavaClass("com.glassfitgames.glassfitplatform.gpstracker.Helper");
			points_helper_class = new AndroidJavaClass("com.glassfitgames.glassfitplatform.points.PointsHelper");
			UnityEngine.Debug.LogWarning("Platform: helper_class created OK");
			
			// call the following on the UI thread
			activity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
				
				// Get the singleton helper objects
				try {
					helper = helper_class.CallStatic<AndroidJavaObject>("getInstance", context);
					points_helper = points_helper_class.CallStatic<AndroidJavaObject>("getInstance", context);
        	  	    UnityEngine.Debug.LogWarning("Platform: unique helper instance returned OK");
				} catch (Exception e) {
					UnityEngine.Debug.LogWarning("Platform: Helper.getInstance() failed");
					UnityEngine.Debug.LogException(e);
				}
				// Try to get a Java GPSTracker object
				try {
					gps = helper.Call<AndroidJavaObject>("getGPSTracker");
					UnityEngine.Debug.LogWarning("Platform: unique GPS tracker obtained");
				} catch (Exception e) {
					UnityEngine.Debug.LogWarning("Platform: Helper.getGPSTracker() failed");
					UnityEngine.Debug.LogException(e);
				}
				initialised = true;
        	}));
			
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Error in constructor" + e.Message);
			UnityEngine.Debug.LogException(e);
		} 
		
	}
	
	public AndroidJavaObject getHelper() {
		return helper;
	}
	
	public bool hasStarted() {
		return started;
	}
	
	public User User() {
		try {
			AndroidJavaObject ajo = helper_class.CallStatic<AndroidJavaObject>("getUser");
			return new User(ajo.Get<int>("guid"), ajo.Get<string>("username"), ajo.Get<string>("name"));
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: failed to fetch user " + e.Message);
			UnityEngine.Debug.LogException(e);
			return null;
		}
	}
	
	// Starts tracking
	public void StartTrack() {
		try {
			gps.Call("startTracking");
			tracking = true;
			started = true;
			UnityEngine.Debug.LogWarning("Platform: StartTrack succeeded");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: StartTrack failed " + e.Message);
			UnityEngine.Debug.LogException(e);
		}
	}
	
	// Set the indoor mode
	public void setIndoor(bool indoor) {
		try {
			AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
    	    AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
			
			activity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
				gps.Call("setIndoorMode", indoor);
				if (indoor) {
				    gps.Call("setIndoorSpeed", 2.0f);
				    UnityEngine.Debug.LogWarning("Platform: Indoor mode set to true, indoor speed = 2.0m/s");
				} else {
					UnityEngine.Debug.LogWarning("Platform: Indoor mode set to false, will use true GPS speed");
				}
			}));
		} catch(Exception e) {
			UnityEngine.Debug.Log("Platform: Error setting indoor mode " + e.Message);
		}
	}
	
	public void resetTargets() {
		try {
			helper.Call("resetTargets");
			targetTrackers = new List<TargetTracker>();
		} catch (Exception e) {
			UnityEngine.Debug.Log("Platform: Error clearing targets");
		}
	}
	
	// Get current target speed
	public float getCurrentSpeed(long l) 
	{
//		try {
//			float ret = target.Call<float>("getCurrentSpeed", l);
//			UnityEngine.Debug.Log("Platform: speed obtained, currently: " + ret.ToString());
//			return ret;
//		} catch (Exception e) {
//			UnityEngine.Debug.LogWarning("Platform: Error getting current speed: " + e.Message);
//			return 0;
//		}
		return targetTrackers[0].getCurrentSpeed();
	}
	
	// Returns the target tracker
	public TargetTracker getTargetTracker(){
		TargetTracker t = new TargetTracker(helper);
		targetTrackers.Add(t);
		return t;
	}
	
	// Check if has GPS lock
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
	
	// Stop tracking 
	public void stopTrack() {
		try {
			gps.Call("stopTracking");
		} catch(Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Problem stopping tracking");
		}
	}
	
	// Authentication
	public bool authorize(string provider, string permissions) {
		try {
			return helper_class.CallStatic<bool>("authorize", activity, provider, permissions);
		} catch(Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Problem authorizing provider: " + provider);
			UnityEngine.Debug.LogException(e);
			return false;
		}
	}
	
	// Sync to server
	public void syncToServer() {
		try {
			helper_class.CallStatic("syncToServer", context);
		} catch(Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Problem syncing to server");
			UnityEngine.Debug.LogException(e);
		}
	}
	
	// Reset GPS tracker
	public void reset() {
		try {
			gps.Call("reset");
			started = false;
			UnityEngine.Debug.LogWarning("Platform: GPS has been reset");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: reset() failed: " + e.Message);
			UnityEngine.Debug.LogException(e);
		}
	}
	
	// Set the target speed
	public void setTargetSpeed(float speed)
	{
//		try {
//			target.Call("setSpeed", speed);
//			UnityEngine.Debug.LogWarning("Platform: Speed has been set to " + speed.ToString ());
//		} catch (Exception e) {
//			UnityEngine.Debug.LogWarning("Platform: setTargetSpeed() failed: " + e.Message);
//			UnityEngine.Debug.LogException(e);
//		}
		targetTrackers[0].setTargetSpeed(speed);
	}
	
	// Set the target track
	public void setTargetTrack(int trackID)
	{
//		try {
//			target.Call("setTrack", trackID);
//			UnityEngine.Debug.LogWarning("Platform: Track has been set to " + trackID.ToString ());
//		} catch (Exception e) {
//			UnityEngine.Debug.LogWarning("Platform: setTargetTrack() failed: " + e.Message);
//			UnityEngine.Debug.LogException(e);
//		}
		targetTrackers[0].setTargetTrack(trackID);
	}
	
	// Load the game blob
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
	
	// Get the device's orientation
	public Quaternion getOrientation() {
		try {
			AndroidJavaObject ajo = helper.Call<AndroidJavaObject>("getOrientation");
			Quaternion q = new Quaternion(ajo.Call<float>("getX"), ajo.Call<float>("getY"), ajo.Call<float>("getZ"), ajo.Call<float>("getW"));
			return q;
		} catch (Exception e) {
			UnityEngine.Debug.Log("Platform: Error getting orientation: " + e.Message);
			return Quaternion.identity;
		}
	}
	
	// Reset the Gyros and accelerometer
	public void resetGyro() {
		try {
			helper.Call("resetGyros");
		} catch (Exception e) {
			UnityEngine.Debug.Log("Platform: Error resetting gyros: " + e.Message);
		}
	}

	// Return a list of positions from the current track
	public List<Position> getTrackPositions() {
		try {
			int size = helper.Call<int>("getNumberPositions");
			UnityEngine.Debug.Log("Platform: get positions called Unity");
			positions = new List<Position>(size);
			try {
				for (int i=0; i<size; i++) {
					AndroidJavaObject ajo = helper.Call<AndroidJavaObject>("getPosition", i);
					Position currentPos = new Position((float)ajo.Call<double>("getLatx"), (float)ajo.Call<double>("getLngx"));
					positions.Add(currentPos);
				}
				positions.Reverse();
				return positions;
			} catch (Exception e) {
				UnityEngine.Debug.LogWarning("Platform: Error getting positions: " + e.Message);
				return null;
			}
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Error getting Track Size: " + e.Message);
			return null;
		}
	}
	
	// Load a list of tracks
	public void getTracks() {
		try {
			helper.Call("getTracks");
			UnityEngine.Debug.Log("Platform: get tracks called Unity");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Error getting Tracks: " + e.Message);
		}
	}
	
	// Select the next track
	public void getNextTrack() {
		try {
			helper.Call("getNextTrack");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Error getting next track: " + e.Message);
		}
	}
	
	// Select the previous track
	public void getPreviousTrack() {
		try {
			helper.Call("getPreviousTrack");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Error getting previous track: " + e.Message);
		}
	}
	
	// Set the chosen track
	public void setTrack() {
		try {
			helper.Call("setTrack");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Error setting track: " + e.Message);
		}
	}
	
	public void QueueAction(string json) {
		try {
			helper_class.CallStatic("queueAction", json);
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Error queueing action: " + e.Message);
		}
	}
		
	public Friend[] Friends() {
		try {
			using(AndroidJavaObject list = helper_class.CallStatic<AndroidJavaObject>("getFriends")) {
				int length = list.Call<int>("size");
				Friend[] friendList = new Friend[length];
				for (int i=0;i<length;i++) {
					using (AndroidJavaObject f = list.Call<AndroidJavaObject>("get", i)) {
						friendList[i] = new Friend(f.Get<string>("friend"));
					}
				}
				UnityEngine.Debug.LogWarning("Platform: " + friendList.Length + " friends fetched");
				return friendList;
			}
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Friends() failed: " + e.Message);
			UnityEngine.Debug.LogException(e);
		}
		return new Friend[0];
	}
		
	public Notification[] Notifications() {
		try {
			using(AndroidJavaObject list = helper_class.CallStatic<AndroidJavaObject>("getNotifications")) {
				int length = list.Call<int>("size");
				Notification[] notifications = new Notification[length];
				for (int i=0;i<length;i++) {
					using (AndroidJavaObject p = list.Call<AndroidJavaObject>("get", i)) {
						notifications[i] = new Notification(p.Get<string>("id"), p.Get<bool>("read"), p.Get<string>("message"));
					}
				}
				UnityEngine.Debug.LogWarning("Platform: " + notifications.Length + " notifications fetched");
				return notifications;
			}
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Friends() failed: " + e.Message);
			UnityEngine.Debug.LogException(e);
		}
		return new Notification[0];
	}
	
	public void ReadNotification(string id) {
		throw new NotImplementedException("Iterate through notifications and setRead(true) or add setRead(id) helper method?");
	}
		
	// Store the blob
	public void StoreBlob(string id, byte[] blob) {
		try {
			helper_class.CallStatic("storeBlob", id, blob);
			UnityEngine.Debug.LogWarning("Platform: Game blob " + id + " of size: " + blob.Length + " stored");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: StoreBlob() failed: " + e.Message);
			UnityEngine.Debug.LogException(e);
		}
	}
	
	public float getHighestDistBehind() {
		if(targetTrackers.Count <= 0)
			return 0;
		
		float h = (float)targetTrackers[0].getTargetDistance() - (float)distance;
		for(int i=0; i<targetTrackers.Count; i++) {
			if(h < targetTrackers[i].getTargetDistance() - (float)distance) {
				h = (float)targetTrackers[i].getTargetDistance() - (float)distance;
			}
		}
		return h;
	}
	
	// Update the data
	public void EraseBlob(string id) {
		try {
			helper_class.CallStatic("eraseBlob", id);
			UnityEngine.Debug.LogWarning("Platform: Game blob " + id + " erased");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: EraseBlob() failed: " + e.Message);
			UnityEngine.Debug.LogException(e);			
		}
	}
	
	public void ResetBlobs() {
		try {
			helper_class.CallStatic("resetBlobs");
			UnityEngine.Debug.LogWarning("Platform: Game blobs reset");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: ResetBlobs() failed: " + e.Message);
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
		
//		UnityEngine.Debug.Log("Platform: There are " + targetTrackers.Count + " target trackers");
		for(int i=0; i<targetTrackers.Count; i++) {
			targetTrackers[i].setTargetDistance();
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
			}
		} catch (Exception e) {
			
			UnityEngine.Debug.Log("Platform: Error getting position: " + e.Message);
//			errorLog = errorLog + "\ngetCurrentPosition|Bearing" + e.Message;
		}
		try {
			if (hasLock()) {
				bearing = gps.Call<float>("getCurrentBearing");
			}
		} catch (Exception e) {
			UnityEngine.Debug.Log("Platform: Error getting bearing: " + e.Message);
		}
		
		try {
			currentActivityPoints = points_helper.Call<long>("getCurrentActivityPoints");
			DataVault.Set("points", (int)currentActivityPoints);
		} catch (Exception e) {
			UnityEngine.Debug.Log("Platform: Error getting current acticity points: " + e.Message);
			DataVault.Set("points", -1);
		}
		
		try {
			openingPointsBalance = points_helper.Call<long>("getOpeningPointsBalance");
		} catch (Exception e) {
			UnityEngine.Debug.Log("Platform: Error getting opening points balance: " + e.Message);
		}
		
	}
	
	// Return the distance behind target
	public double DistanceBehindTarget() {
		double returnDistance = (targetTrackers[0].getTargetDistance() - distance);
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
	
	public long OpeningPointsBalance() {
		return openingPointsBalance;
	}
	
	public void setBasePointsSpeed(float speed) {
		try {
			points_helper.Call("setBaseSpeed", speed);
		} catch (Exception e) {
			UnityEngine.Debug.Log("Platform: Error setting base points speed: " + e.Message);
		}
	}
	
}