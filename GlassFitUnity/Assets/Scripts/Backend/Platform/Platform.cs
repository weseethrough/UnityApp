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
	public int currentTrack { get; set; }
	
	
	private List<Track> trackList;
	private List<Game> gameList;
	
	private Boolean tracking = false;
	
	private AndroidJavaObject helper;
	private AndroidJavaObject points_helper;
	private AndroidJavaObject gps;
	private AndroidJavaClass helper_class;
	private AndroidJavaClass points_helper_class;
	private AndroidJavaObject activity;
	private AndroidJavaObject context;
	
	public List<TargetTracker> targetTrackers { get; private set; }
	
	public bool authenticated { get; private set; }	
	
	// Other components may change this to disable sync temporarily?
	public int syncInterval = 10;
	private DateTime lastSync = new DateTime(0);
	
	// Events
	public delegate void OnAuthenticated(bool success);
	public OnAuthenticated onAuthenticated = null;
	public delegate void OnSync();
	public OnSync onSync = null;
	
	// TEMP
	private string notesLabel = "";
	// TEMP
	
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
						singleton.name = "Platform"; // Used as target for messages
						
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
	
	/// Message receivers
	public void OnAuthentication(string message) {
		if (string.Equals(message, "Success")) {
			authenticated = true;
			if (onAuthenticated != null) onAuthenticated(true);
		}
		if (string.Equals(message, "Failure")) {
			authenticated = false;
			if (onAuthenticated != null) onAuthenticated(false);
		}
		UnityEngine.Debug.Log("Platform: authentication " + message.ToLower()); 
	}
	
	public void OnSynchronization(string message) {
		lastSync = DateTime.Now;
		if (onSync != null) onSync();
		/// TEMP
		Notification[] notes = Notifications();
		if (notes.Length > 0) {
			notesLabel = notes[notes.Length-1].ToString() + "\n" + notes.Length + " notifications";
		} else {
			notesLabel = "No notifications";
		}
		/// TEMP
	}
	
	public void OnGUI() {
		GUI.Label(new Rect(Screen.width/2 - 150, Screen.height - 50, 300, 50), notesLabel);
	}
	
	protected Platform() {
		
		authenticated = false;
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
				AwardPoints("Free points for devs", "Platform.cs", 10000);
				// Cache the list of games and states from java
		        GetGames();
				initialised = true;
        	}));
			
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Error in constructor" + e.Message);
			UnityEngine.Debug.LogException(e);
		}
		
		
		
	}
	
	public AndroidJavaObject GetHelper() {
		return helper;
	}
	
	public bool HasStarted() {
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
	
	public User GetUser(int userId) {
		// TODO: Implement me!
		string[] names = { "Cain", "Elijah", "Jake", "Finn", "Todd", "Juno", "Bubblegum", "Ella", "May", "Sofia" };
		string name = names[userId % names.Length];
		
		return new User(userId, name, name + " Who");
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
	public void SetIndoor(bool indoor) {
		try {
			AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
    	    AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
			
			activity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
				gps.Call("setIndoorMode", indoor);
				if (indoor) {
				    gps.Call("setIndoorSpeed", 7.0f);
				    UnityEngine.Debug.LogWarning("Platform: Indoor mode set to true, indoor speed = 2.0m/s");
				} else {
					UnityEngine.Debug.LogWarning("Platform: Indoor mode set to false, will use true GPS speed");
				}
			}));
		} catch(Exception e) {
			UnityEngine.Debug.Log("Platform: Error setting indoor mode " + e.Message);
		}
	}
	
	public void ResetTargets() {
		try {
			helper.Call("resetTargets");
			targetTrackers.Clear();
		} catch (Exception e) {
			UnityEngine.Debug.Log("Platform: Error clearing targets");
		}
	}
	
	// Returns the target tracker
	public TargetTracker CreateTargetTracker(float constantSpeed){
		TargetTracker t = TargetTracker.Build(helper, constantSpeed);
		if (t == null) return null;
		targetTrackers.Add(t);
		return t;
	}
	public TargetTracker CreateTargetTracker(int deviceId, int trackId){
		TargetTracker t = TargetTracker.Build(helper, deviceId, trackId);
		if (t == null) return null;
		targetTrackers.Add(t);
		return t;
	}
	
	// Check if has GPS lock
	public Boolean HasLock() {
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
	public Track StopTrack() {
		try {
			gps.Call("stopTracking");
			using (AndroidJavaObject rawtrack = gps.Call<AndroidJavaObject>("getTrack")) {
				return convertTrack(rawtrack);
			}
		} catch(Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Problem stopping tracking");
			return null;
		}
	}
	
	// Authentication 
	// result returned through onAuthenticated
	public void Authorize(string provider, string permissions) {
		try {
			authenticated = helper_class.CallStatic<bool>("authorize", activity, provider, permissions);
		} catch(Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Problem authorizing provider: " + provider);
			UnityEngine.Debug.LogException(e);
		}
	}
	
	public bool HasPermissions(string provider, string permissions) {
		try {
			return helper_class.CallStatic<bool>("hasPermissions", provider, permissions);
		} catch(Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Problem checking permissions for provider: " + provider);
			UnityEngine.Debug.LogException(e);
			return false;
		}		
	}
	
	// Sync to server
	public void SyncToServer() {
		lastSync = DateTime.Now;
		try {
			helper_class.CallStatic("syncToServer", context);
		} catch(Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Problem syncing to server");
			UnityEngine.Debug.LogException(e);
		}
	}
	
	// Reset GPS tracker
	public void Reset() {
		try {
			gps.Call("reset");
			started = false;
			UnityEngine.Debug.LogWarning("Platform: GPS has been reset");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: reset() failed: " + e.Message);
			UnityEngine.Debug.LogException(e);
		}
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
	public Quaternion GetOrientation() {
		try {
			AndroidJavaObject ajo = helper.Call<AndroidJavaObject>("getOrientation");
            Quaternion q = new Quaternion(ajo.Call<float>("getX"), ajo.Call<float>("getY"), ajo.Call<float>("getZ"),ajo.Call<float>("getW"));
			return q;
		} catch (Exception e) {
			UnityEngine.Debug.Log("Platform: Error getting orientation: " + e.Message);
			return Quaternion.identity;
		}
	}
	
	// Reset the Gyros and accelerometer
	public void ResetGyro() {
		try {
			helper.Call("resetGyros");
		} catch (Exception e) {
			UnityEngine.Debug.Log("Platform: Error resetting gyros: " + e.Message);
		}
	}

	public Challenge FetchChallenge(string id) {
		try {
			using (AndroidJavaObject rawch = helper_class.CallStatic<AndroidJavaObject>("fetchChallenge", id)) {
				return Challenge.Build(rawch.Get<string>("json"));
			}
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Error getting Track: " + e.Message);
			return null;
		}
	}
	
	private Track convertTrack(AndroidJavaObject rawtrack) {
		string name = rawtrack.Call<string>("getName");
		int[] ids = rawtrack.Call<int[]>("getIDs"); 
		using(AndroidJavaObject poslist = rawtrack.Call<AndroidJavaObject>("getTrackPositions")) {
			int numPositions = poslist.Call<int>("size");
			List<Position> pos = new List<Position>(numPositions);
			for(int j=0; j<numPositions; j++) {
				AndroidJavaObject position = poslist.Call<AndroidJavaObject>("get", j);
				Position current = new Position((float)position.Call<double>("getLatx"), (float)position.Call<double>("getLngx"));
				pos.Add(current);
			}
			return new Track(name, ids[0], ids[1], pos);
		}
	}
	
	public Track FetchTrack(int deviceId, int trackId) {
		try {
			using (AndroidJavaObject rawtrack = helper_class.CallStatic<AndroidJavaObject>("fetchTrack", deviceId, trackId)) {
					Track track = convertTrack(rawtrack);
					return track;
			}
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Error getting Track: " + e.Message);
			return null;
		}
	}
	
	// Load a list of tracks
	public List<Track> GetTracks() {
		try {
			using(AndroidJavaObject list = helper_class.CallStatic<AndroidJavaObject>("getTracks")) {
				int size = list.Call<int>("size");
				trackList = new List<Track>(size);
				try {
					for(int i=0; i<size; i++) {
						using (AndroidJavaObject rawtrack = list.Call<AndroidJavaObject>("get", i)) {
							Track currentTrack = convertTrack(rawtrack);
							trackList.Add(currentTrack);
						}
					}
					trackList.Reverse();
					this.currentTrack = 0;
					return trackList;
				} catch (Exception e) {
					UnityEngine.Debug.LogWarning("Platform: Error getting track: " + e.Message);
					return null;
				}
			}
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Error getting Tracks: " + e.Message);
			return null;
		}
	}
	
	/// <summary>
	/// Load a list of games from the java database, together with lock state, cost, description etc.
	/// Typically used when building the main hex menu
	/// </summary>
	public List<Game> GetGames()
	{
		// if we already have a copy, return it. Games are unlikely to update except through Game.unlock.
		if (gameList != null)
		{
			return gameList;
		}
		// otherwise, get the games from java
		try
		{
			UnityEngine.Debug.Log("Platform: Getting games from java...");
			AndroidJavaObject javaGameList = helper.Call<AndroidJavaObject>("getGames");
			int size = javaGameList.Call<int>("size");
			UnityEngine.Debug.Log("Platform: Retrieved " + size + " games from java");
			gameList = new List<Game>(size);
			try
			{
				for(int i=0; i<size; i++)
				{
					AndroidJavaObject javaGame = javaGameList.Call<AndroidJavaObject>("get", i);
					Game csGame = new Game();
					csGame.Initialise(javaGame);
					gameList.Add(csGame);
				}
				UnityEngine.Debug.Log("Platform: Successfully imported " + size + " games.");
				return gameList;
			}
			catch (Exception e)
			{
				UnityEngine.Debug.LogWarning("Platform: Error getting game!");
				UnityEngine.Debug.LogWarning(e.Message);
				UnityEngine.Debug.LogException(e);
				return null;
			}
		}
		catch (Exception e)
		{
			UnityEngine.Debug.LogWarning("Platform: Error getting Games!");
			UnityEngine.Debug.LogWarning(e.Message);
			UnityEngine.Debug.LogException(e);
			return null;
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
	
	public float GetHighestDistBehind() {
		if(targetTrackers.Count <= 0)
			return 0;
		
		float h = (float)targetTrackers[0].GetTargetDistance() - (float)distance;
		for(int i=0; i<targetTrackers.Count; i++) {
			if(h < targetTrackers[i].GetTargetDistance() - (float)distance) {
				h = (float)targetTrackers[i].GetTargetDistance() - (float)distance;
			}
		}
		return h;
	}
	
	public float GetLowestDistBehind() {
		if(targetTrackers.Count <= 0)
			return 0;
		
		float l = (float)targetTrackers[0].GetTargetDistance() - (float)distance;
		for(int i=0; i<targetTrackers.Count; i++) {
			if(l > targetTrackers[i].GetTargetDistance() - (float)distance) {
				l = (float)targetTrackers[i].GetTargetDistance() - (float)distance;
			}
		}
		return l;
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
			targetTrackers[i].PollTargetDistance();
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
			if (HasLock()) {
				AndroidJavaObject ajo = gps.Call<AndroidJavaObject>("getCurrentPosition");
				position = new Position((float)ajo.Call<double>("getLatx"), (float)ajo.Call<double>("getLngx"));
			}
		} catch (Exception e) {
			
			UnityEngine.Debug.Log("Platform: Error getting position: " + e.Message);
//			errorLog = errorLog + "\ngetCurrentPosition|Bearing" + e.Message;
		}
		try {
			if (HasLock()) {
				bearing = gps.Call<float>("getCurrentBearing");
			}
		} catch (Exception e) {
			UnityEngine.Debug.Log("Platform: Error getting bearing: " + e.Message);
		}
		
		try {
			currentActivityPoints = points_helper.Call<long>("getCurrentActivityPoints");
			DataVault.Set("points", (int)currentActivityPoints);
		} catch (Exception e) {
			UnityEngine.Debug.Log("Platform: Error getting current activity points: " + e.Message);
			DataVault.Set("points", -1);
		}
		
		try {
			openingPointsBalance = points_helper.Call<long>("getOpeningPointsBalance");
		} catch (Exception e) {
			UnityEngine.Debug.Log("Platform: Error getting opening points balance: " + e.Message);
		}
		
		if (authenticated && syncInterval > 0 && DateTime.Now.Subtract(lastSync).TotalSeconds > syncInterval) {
			SyncToServer();
		}		
	}
	
	// Return the distance behind target
	public double DistanceBehindTarget(TargetTracker tracker) {
		double returnDistance = (tracker.GetTargetDistance() - distance);
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
	
	public long GetCurrentPoints() {
		return currentActivityPoints;
	}
	
	public void SetBasePointsSpeed(float speed) {
		try {
			points_helper.Call("setBaseSpeed", speed);
		} catch (Exception e) {
			UnityEngine.Debug.Log("Platform: Error setting base points speed: " + e.Message);
		}
	}
	
	/// <summary>
	/// Use this method to award the user points.
	/// </summary>
	/// <param name='reason'>
	/// Reason that the points are being awarded, e.g. "1km bonus".
	/// </param>
	/// <param name='gameId'>
	/// Game identifier so we can log which game the points came from.
	/// </param>
	/// <param name='points'>
	/// Number of points to award.
	/// </param>
	public void AwardPoints(String reason, String gameId, long points)
	{
		try
		{
			points_helper.Call("awardPoints", "in-game bonus", reason, gameId, points);
			UnityEngine.Debug.Log("Platform: " + gameId + " awarded " + points + " points for " + reason);
		}
		catch (Exception e)
		{
			UnityEngine.Debug.Log("Platform: Error awarding " + reason + " of " + points + " points in " + gameId);
		}
	}
	
}