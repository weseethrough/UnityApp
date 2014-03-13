using System;
using System.Collections;
using System.Collections.Generic;
using RaceYourself.Models;
using UnityEngine;
using Newtonsoft.Json;

#if UNITY_ANDROID && !RACEYOURSELF_MOBILE
/// <summary>
/// Android platform. Overrides platform functionality with android-specific functionality where necessary. Usually this means JNI calls to the GlassfitPlatform libr
/// </summary>
public class AndroidPlatform : Platform
{
	private AndroidJavaClass build_class;	
	private AndroidJavaObject helper;
	private AndroidJavaClass helper_class;
	private AndroidJavaObject activity;
	private AndroidJavaObject context;
	private AndroidJavaObject sensoriaSock;
	
	
	private PlayerPosition _localPlayerPosition;
    public override PlayerPosition LocalPlayerPosition {
        get { return _localPlayerPosition; }
    }

	// Helper class for accessing/awarding points
	private PlayerPoints _playerPoints;
	public override PlayerPoints PlayerPoints { get { return _playerPoints; } }

	protected override void Initialize() {
		base.Initialize();
	    try {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
    		activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            context = activity.Call<AndroidJavaObject>("getApplicationContext");
            helper_class = new AndroidJavaClass("com.glassfitgames.glassfitplatform.gpstracker.Helper");
			build_class = new AndroidJavaClass("android.os.Build");

            UnityEngine.Debug.LogWarning("Platform: helper_class created OK");
	                        
            // call the following on the UI thread
            activity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
	                                
				try {
	                // Get the singleton helper objects
                    helper = helper_class.CallStatic<AndroidJavaObject>("getInstance", context);

                    UnityEngine.Debug.LogWarning("Platform: unique helper instance returned OK");

					// get reference to Sensoria Socks
					log.info("Initializing Sensoria socks");
					try {
						sensoriaSock = new AndroidJavaObject("com.glassfitgames.glassfitplatform.sensors.SensoriaSock", context);
					} catch (Exception e) {
						log.error("Error attaching to Sensoria Socks: " + e.Message);
					}
		                                        
					//ExportCSV();
	
					// Log screen dimensions - for debug only, can be commented out
					log.info("Screen dimensions are " + GetScreenDimensions().x.ToString() + "x" + GetScreenDimensions().y.ToString());
			    } catch (Exception e) {
		            log.error("Error in android initialisation thread " + e.Message);
					Application.Quit();
			    }
				initialised = true;
				log.info("Initialise complete");
	    	}));
	            
			log.info("Initializing AndroidPlayerPosition");
			_localPlayerPosition = new AndroidPlayerPosition();
			log.info("Initializing AndroidPlayerPoints");
			_playerPoints = new LocalDbPlayerPoints();
	    } catch (Exception e) {
            log.error("Error in initialisation " + e.Message);
			Application.Quit();
	    }		
	}
	
	public override Device Device() {
		try {
			UnityEngine.Debug.Log("Platform: Getting user details");
			AndroidJavaObject ajo = helper_class.CallStatic<AndroidJavaObject>("getDevice");
			if (ajo.GetRawObject().ToInt32() == 0) return null;
			return new Device();
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: failed to fetch device " + e.Message);
			UnityEngine.Debug.LogException(e);
			return null;
		}
	}

	public override User User() {
		try {
			UnityEngine.Debug.Log("Platform: getting user");
			AndroidJavaObject ajo = helper_class.CallStatic<AndroidJavaObject>("getUser");
			if (ajo.GetRawObject().ToInt32() == 0) return null;
			return new User{id = ajo.Get<int>("guid"), username = ajo.Get<string>("username"), name = ajo.Get<string>("name")};
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: failed to fetch user " + e.Message);
			UnityEngine.Debug.LogException(e);
			return null;
		}
	}

	public override User GetUser(int userId) {
		try {
			AndroidJavaObject ajo = helper_class.CallStatic<AndroidJavaObject>("fetchUser", userId);
			if(ajo.GetRawObject().ToInt32() == 0) return null;
			return new User{id = ajo.Get<int>("guid"), username = ajo.Get<string>("username"), name = ajo.Get<string>("name")};
		} catch (Exception) {
			UnityEngine.Debug.LogWarning("Platform: error getting user");
			return null;
		}
	}
	
	public override void ResetTargets() {
		try {
			helper.Call("resetTargets");
			targetTrackers.Clear();
		} catch (Exception) {
			UnityEngine.Debug.LogWarning("Platform: Error clearing targets");
		}
	}

	// Returns the target tracker
	public override TargetTracker CreateTargetTracker(float constantSpeed){
		FauxTargetTracker t;
		try {
			AndroidJavaObject ajo = helper.Call<AndroidJavaObject>("getFauxTargetTracker", constantSpeed);
			if (ajo.GetRawObject().ToInt32() == 0) return null;
			UnityEngine.Debug.Log("TargetTracker: faux target tracker obtained");
			t = new FauxTargetTracker(ajo);
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("TargetTracker: Helper.getFauxTargetTracker() failed" + e.Message);
			UnityEngine.Debug.LogException(e);
			return null;
		}
		if (t == null) return null;
		targetTrackers.Add(t);
		return t;
	}

	public override TargetTracker CreateTargetTracker(int deviceId, int trackId){
		TargetTracker t = TargetTracker.Build(helper, deviceId, trackId);
		if (t == null) return null;
		targetTrackers.Add(t);
		return t;
	}
	
	public override bool OnGlass() 
    {
		try {
			//UnityEngine.Debug.Log("Platform: seeing if glass");
			return helper_class.CallStatic<bool>("onGlass");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: onGlass() failed: " + e.Message);
			UnityEngine.Debug.LogException(e);
			return false;
		}
	}

	public override bool IsRemoteDisplay() 
    {
		try {
			//UnityEngine.Debug.Log("Platform: seeing if glass");
			return helper_class.CallStatic<bool>("isRemoteDisplay");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: isRemoteDisplay() failed: " + e.Message);
			UnityEngine.Debug.LogException(e);
			return false;
		}
	}

    public override bool IsPluggedIn()
    {
		try {
			UnityEngine.Debug.Log("Platform: calling IsPluggedIn");
			return helper.Call<bool>("isPluggedIn");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: isPluggedIn() failed: " + e.Message);
			UnityEngine.Debug.LogException(e);
			return false;
		}
	}
	
	public override bool HasInternet() {
		try {
			return helper.Call<bool>("hasInternet");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: hasInternet() failed: " + e.Message);
			UnityEngine.Debug.LogException(e);
			return false;
		}
	}
	
	public override bool HasWifi() {
		try {
			return helper.Call<bool>("hasWifi");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: hasWifi() failed: " + e.Message);
			UnityEngine.Debug.LogException(e);
			return false;
		}
	}
	
	public override bool IsDisplayRemote() {
		foreach(string peer in BluetoothPeers()) {
			UnityEngine.Debug.LogWarning("Platform: BT peer: " + peer);
			if (peer.Contains("Glass") || peer.Contains("Display")) return true;
		}
		return false;
	}
	
	public override bool HasGpsProvider() {
		try {
			return helper.Call<bool>("hasGps");
		} catch (Exception e) {
			log.error("Error calling hasGps over JNI " + e.Message);
			return false;
		}
	}

    public override bool IsBluetoothBonded()
    {
        try {
            return helper.Call<bool>("isBluetoothBonded");
        } catch (Exception e) {
            log.error("Error calling isBluetoothBonded over JNI " + e.Message);
            return false;
        }
    }
	

	
	// Authentication 
	// result returned through onAuthenticated
	public override void Authorize(string provider, string permissions) {
		try {
			UnityEngine.Debug.Log("Platform: authorizing");
			bool auth = helper.Call<bool>("authorize", activity, provider, permissions);
			if (!authenticated && auth) authenticated = true;
			UnityEngine.Debug.Log("Platform: authorize end");
		} catch(Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Problem authorizing provider: " + provider);
			UnityEngine.Debug.LogException(e);
		}
	}
	
	public override bool HasPermissions(string provider, string permissions) {
		try {
			UnityEngine.Debug.Log("Platform: checking permissions");
			bool auth = helper_class.CallStatic<bool>("hasPermissions", provider, permissions);
			if (!authenticated && auth) authenticated = true;
			UnityEngine.Debug.Log("Platform: permissions complete");
			return auth;
		} catch(Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Problem checking permissions for provider: " + provider);
			UnityEngine.Debug.LogException(e);
			return false;
		}		
	}
	
	// Sync to server
	public override void SyncToServer() {
		lastSync = DateTime.Now;
		try {
			UnityEngine.Debug.Log("Platform: sync to server calling");
			helper_class.CallStatic("syncToServer", context);
			UnityEngine.Debug.Log("Platform: sync to server called");
		} catch(Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Problem syncing to server");
			UnityEngine.Debug.LogException(e);
		}
	}
		
	// Load the game blob
	public override byte[] LoadBlob(string id) {
		try {
			byte[] blob = helper_class.CallStatic<byte[]>("loadBlob", id);
			UnityEngine.Debug.Log("Platform: Game blob " + id + " of size: " + blob.Length + " loaded");
			return blob;
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: LoadBlob() failed: " + e.Message);
			UnityEngine.Debug.LogException(e);			
		}
		return null;
	}
	
	public override Challenge FetchChallenge(string id) {
		try {
			UnityEngine.Debug.Log("Platform: fetching challenge");
			using (AndroidJavaObject rawch = helper_class.CallStatic<AndroidJavaObject>("fetchChallenge", id)) {
				return JsonConvert.DeserializeObject<Challenge>(rawch.Get<string>("json"));
			}
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Error getting Track: " + e.Message);
			return null;
		}
	}
	
	private void ExportCSV() {
		try {
			helper.Call("exportDatabaseToCsv");
			UnityEngine.Debug.Log("Platform: Database Successfully exported to CSV");
		} catch (Exception e)
		{
			UnityEngine.Debug.LogWarning("Platform: Error exporting database");
			UnityEngine.Debug.Log(e.Message);
		}
	}
	

	public override Track FetchTrack(int deviceId, int trackId) {
		try {
			UnityEngine.Debug.Log("Platform: fetching track");
			using (AndroidJavaObject rawtrack = helper_class.CallStatic<AndroidJavaObject>("fetchTrack", deviceId, trackId)) {
				Track track = new AndroidTrack(rawtrack);
				return track;
			}
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Error getting Track: " + e.Message);
			return null;
		}
	}
	
	// Obtain tracks based on distance
	public override List<Track> GetTracks(double distance, double minDistance) {
		try {
			UnityEngine.Debug.Log("Platform: getting tracks with distance");
			using(AndroidJavaObject list = helper.Call<AndroidJavaObject>("getTracks", distance, minDistance)) {
				int size = list.Call<int>("size");
				trackList = new List<Track>(size);
				try {
					for(int i=0; i<size; i++) {
						using(AndroidJavaObject rawTrack = list.Call<AndroidJavaObject>("get", i)) {
							Track currentTrack = new AndroidTrack(rawTrack);
							trackList.Add(currentTrack);
						}
					}
					trackList.Reverse();
					this.currentTrack = 0;
					return trackList;
				} catch (Exception e) {
					UnityEngine.Debug.LogWarning("Platform: Error getting track based on distance: " + e.Message);
					return null;
				}
			}
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Error getting Tracks based on distance: " + e.Message);
			return null;
		}
	}
	
	// Load a list of tracks
	public override List<Track> GetTracks() {
		try {
			UnityEngine.Debug.Log("Platform: getting tracks");
			using(AndroidJavaObject list = helper_class.Call<AndroidJavaObject>("getTracks")) {
				int size = list.Call<int>("size");
				trackList = new List<Track>(size);
				try {
					for(int i=0; i<size; i++) {
						using (AndroidJavaObject rawtrack = list.Call<AndroidJavaObject>("get", i)) {
							Track currentTrack = new AndroidTrack(rawtrack);
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
		
	public override void ResetGames() 
	{
		try {
			UnityEngine.Debug.Log("Platform: resetting games");
			helper.Call("loadDefaultGames");
			gameList = null;
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Error resetting games: ");
			UnityEngine.Debug.Log(e.Message);
		}
	}
	
	/// <summary>
	/// Load a list of games from the java database, together with lock state, cost, description etc.
	/// Typically used when building the main hex menu
	/// </summary>
	public override List<Game> GetGames()
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
					AndroidGame csGame = new AndroidGame();
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
	
	public override void QueueAction(string json) {
		try {
			UnityEngine.Debug.Log("Platform: queueing action");
			helper_class.CallStatic("queueAction", json);
			UnityEngine.Debug.Log("Platform: action queued");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Error queueing action: " + e.Message);
		}
	}
		
	public override List<Friend> Friends() {
		try {
			UnityEngine.Debug.Log("Platform: getting friends list");
			using(AndroidJavaObject list = helper_class.CallStatic<AndroidJavaObject>("getFriends")) {
				UnityEngine.Debug.Log("Platform: getting friends list size");
				int length = list.Call<int>("size");
                UnityEngine.Debug.Log("Platform: Friends to fetch " + length);
				List<Friend> friendList = new List<Friend>();                
				for (int i=0;i<length;i++) {
					using (AndroidJavaObject f = list.Call<AndroidJavaObject>("get", i)) {
						UnityEngine.Debug.Log("Platform: getting a friend ("+i+")");
						Friend friend = JsonConvert.DeserializeObject<Friend>(f.Get<string>("friend"));
						friendList.Add(friend);
						UnityEngine.Debug.Log("Platform: got a friend ("+i+")");
					}
				}
				UnityEngine.Debug.Log("Platform: " + friendList.Count + " friends fetched");
				return friendList;
			}
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Friends() failed: " + e.Message);
			UnityEngine.Debug.LogException(e);
		}
		return new List<Friend>();
	}
	
	public override Notification[] Notifications() {
		try {
			UnityEngine.Debug.Log("Platform: getting notifications");
			using(AndroidJavaObject list = helper_class.CallStatic<AndroidJavaObject>("getNotifications")) {
				int length = list.Call<int>("size");
				Notification[] notifications = new Notification[length];
				for (int i=0;i<length;i++) {
					AndroidJavaObject p = list.Call<AndroidJavaObject>("get", i);
					notifications[i] = new AndroidNotification{id = p.Get<string>("id"), read = p.Get<bool>("read"), message = JsonConvert.DeserializeObject<Message>(p.Get<string>("message")), ajo = p};
				}
				UnityEngine.Debug.Log("Platform: " + notifications.Length + " notifications fetched");
				return notifications;
			}
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Friends() failed: " + e.Message);
			UnityEngine.Debug.LogException(e);
		}
		return new Notification[0];
	}
	
	public override void ReadNotification(string id) {
		var notifications = Notifications();
		foreach (Notification note in notifications) {
			if (note.id == id) {
				if (note is AndroidNotification) {
					var n = note as AndroidNotification;
					n.ajo.Call("setRead", true);
					n.ajo.Call<int>("save");
					n.read = true;
				} else base.ReadNotification(id);
			}
		}
	}
		
	// Store the blob
	public override void StoreBlob(string id, byte[] blob) {
		try {
			UnityEngine.Debug.Log("Platform: storing blob");
			helper_class.CallStatic("storeBlob", id, blob);
			UnityEngine.Debug.Log("Platform: Game blob " + id + " of size: " + blob.Length + " stored");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: StoreBlob() failed: " + e.Message);
			UnityEngine.Debug.LogException(e);
		}
	}
	
	public override float GetHighestDistBehind() {
		if(targetTrackers.Count <= 0)
			return 0;
		
		float h = (float)targetTrackers[0].GetTargetDistance() - (float)LocalPlayerPosition.Distance;
		for(int i=0; i<targetTrackers.Count; i++) {
			if(h < targetTrackers[i].GetTargetDistance() - (float)LocalPlayerPosition.Distance) {
				h = (float)targetTrackers[i].GetTargetDistance() - (float)LocalPlayerPosition.Distance;
			}
		}
		return h;
	}
	
	
	public override float GetLowestDistBehind() {
		if(targetTrackers.Count <= 0)
			return 0;
		
		float l = (float)targetTrackers[0].GetTargetDistance() - (float)LocalPlayerPosition.Distance;
		for(int i=0; i<targetTrackers.Count; i++) {
			if(l > targetTrackers[i].GetTargetDistance() - (float)LocalPlayerPosition.Distance) {
				l = (float)targetTrackers[i].GetTargetDistance() - (float)LocalPlayerPosition.Distance;
			}
		}
		return l;
	}
	
	// Update the data
	public override void EraseBlob(string id) {
		try {
			helper_class.CallStatic("eraseBlob", id);
			UnityEngine.Debug.Log("Platform: Game blob " + id + " erased");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: EraseBlob() failed: " + e.Message);
			UnityEngine.Debug.LogException(e);			
		}
	}
	
	public override void ResetBlobs() {
		try {
			helper_class.CallStatic("resetBlobs");
			UnityEngine.Debug.Log("Platform: Game blobs reset");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: ResetBlobs() failed: " + e.Message);
			UnityEngine.Debug.LogException(e);			
		}
	}
	
	public override void Update() {
		base.Update();
		//UnityEngine.Debug.Log("Platform: updating");
		//UnityEngine.Debug.Log("Platform: about to sync");
//		if (authenticated && syncInterval > 0 && DateTime.Now.Subtract(lastSync).TotalSeconds > syncInterval && IsPluggedIn()) {
//			//UnityEngine.Debug.Log("Platform: about to sync properly");
//			SyncToServer();
//			//UnityEngine.Debug.Log("Platform: sync complete");
//		}

		// Update player orientation
		try {
			//UnityEngine.Debug.Log("Platform: getting orientation");
			AndroidJavaObject q = helper.Call<AndroidJavaObject>("getOrientation");
            playerOrientation.Update(new Quaternion(q.Call<float>("getX"), q.Call<float>("getY"), q.Call<float>("getZ"), q.Call<float>("getW")));
		} catch (Exception e) {
			UnityEngine.Debug.Log("Platform: Error getting orientation: " + e.Message);
		}

//		// Test - print log messages for touch input
//		if (GetTouchInput() != null)
//		{
//			UnityEngine.Debug.Log("Touch input: x:" + ((Vector2)GetTouchInput())[0] + ", y:" + ((Vector2)GetTouchInput())[1]);
//		}

	}	
	
	public override void Poll() {

		this.LocalPlayerPosition.Update();  // update current position
		this.PlayerPoints.Update(); // update current points

		//UnityEngine.Debug.Log("Platform: poll There are " + targetTrackers.Count + " target trackers");
		for(int i=0; i<targetTrackers.Count; i++) {
			targetTrackers[i].PollTargetDistance();
		}

		try {
			yaw = helper.Call<float>("getAzimuth");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Error getting bearing: " + e.Message);
		}
		
		try {
			sensoriaSockPressure = sensoriaSock.Call<float[]>("getPressureSensorValues", ((long)(UnityEngine.Time.time*1000)));
			//UnityEngine.Debug.Log("Platform: poll Sensoria pressure = " + sensoriaSockPressure[0]);
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Error getting sensoria sock pressure data: " + e.Message);
		}
	}
	
	// Return the distance behind target
	public override double DistanceBehindTarget(TargetTracker tracker) {
		double returnDistance = (tracker.GetTargetDistance() - LocalPlayerPosition.Distance);
		return returnDistance;
	}
	
	public override double DistanceBehindTarget() {
		return GetLowestDistBehind();
	}

	public override float Yaw() {
		return yaw;
	}
	

	
	/// <summary>
	/// Gets the target speed. Only used by Eagle? Will be better done by a dummy targetController.
	/// </summary>
	/// <returns>
	/// The target speed.
	/// </returns>
	public override float GetTargetSpeed() {
		return 0.0f;
	}
	


	/// <summary>
	/// Use this method to record events for analytics, e.g. a user action.
	/// </summary>
	/// <param name='json'>
	/// Json-encoded event values such as current game state, what the user action was etc
	/// </param>
	public override void LogAnalytics(JSONObject json)
	{
		try
		{
			helper.CallStatic("logEvent", json.ToString());
			UnityEngine.Debug.Log("Platform: logged analytic event " + json.ToString());
		}
		catch (Exception e)
		{
			UnityEngine.Debug.LogWarning("Platform: Error logging analytic event. " + e.Message);
		}
	}


	// Poll java for touch input
	// Returns the int number of fingers touching the pad
	public override int GetTouchCount()
	{
		if (OnGlass ()) {
			try
			{
				return activity.Call<int> ("getTouchCount");
	
			}
			catch(Exception e)
			{
				UnityEngine.Debug.LogWarning("Platform: Error getting touch count " + e.Message);
				return 0;
			}
		} else {
			//use unity's built-in input for now
			return Input.touchCount;
		}
	}

	// Poll java for touch input
	// Returns (x,y) as floats between 0 and 1
	public override Vector2? GetTouchInput()
	{
		if (OnGlass ()) {
		try
		{
			//UnityEngine.Debug.Log("Platform: Checking touch input..");
			int touchCount = activity.Call<int> ("getTouchCount");
			if (touchCount > 0)
			{
				float x = 1 - activity.Call<float> ("getTouchX");  // glass swipe forward === tablet swipe left
				float y = activity.Call<float> ("getTouchY");
				return new Vector2(x,y);
			} else {
				return null;
			}
		}
		catch(Exception e)
		{
			UnityEngine.Debug.LogWarning("Platform: Error getting touch input " + e.Message);
			return null;
		}
		} else {
			//use unity's built-in input for now
			if (Input.touchCount == 1)
			{
				float x = Input.touches[0].position.x / Screen.width;
				float y = Input.touches[0].position.y / Screen.height;
				return new Vector2(x,y);
			}
			else
			{
				return null;
			}
		}
	}
	
	public override void BluetoothServer()
	{
		try
		{
			activity.Call("startBluetoothServer");
			UnityEngine.Debug.Log("Platform: starting Bluetooth server");
		}
		catch (Exception e)
		{
			UnityEngine.Debug.LogWarning("Platform: Error starting Bluetooth server. " + e.Message);
		}
	}

	public override void BluetoothClient()
	{
		try
		{
			activity.Call("startBluetoothClient");
			UnityEngine.Debug.Log("Platform: starting Bluetooth client");
		}
		catch (Exception e)
		{
			UnityEngine.Debug.LogWarning("Platform: Error starting Bluetooth client. " + e.Message);
		}
	}
	
	public override void BluetoothBroadcast(JSONObject json) {
		try
		{
			activity.Call("broadcast", json.ToString());
			UnityEngine.Debug.Log("Platform: broadcasted Bluetooth message: " + json.ToString());
		}
		catch (Exception e)
		{
			UnityEngine.Debug.LogWarning("Platform: Error broadcasting Bluetooth message. " + e.Message);
		}
	}

	public override string[] BluetoothPeers() {
		try
		{
			return activity.Call<string[]>("getBluetoothPeers");
		}
		catch (Exception e)
		{
			UnityEngine.Debug.LogWarning("Platform: Error getting Bluetooth peers. " + e.Message);
			return new string[0];
		}
	}
	
	public override bool ConnectSocket() {
		try
		{
			AndroidJavaObject socket = helper.Call<AndroidJavaObject>("getSocket");
			if (socket.GetRawObject().ToInt32() == 0) return false;
			connected = true;
			return true;
		}
		catch (Exception e)
		{
			UnityEngine.Debug.LogWarning("Platform: Error connecting to socket server. " + e.Message);
			return false;
		}
	}

	public override bool DisconnectSocket() {
		try
		{
			helper.Call("disconnectSocket");
			connected = false;
			return true;
		}
		catch (Exception e)
		{
			UnityEngine.Debug.LogWarning("Platform: Error disconnecting from socket server. " + e.Message);
			return false;
		}
	}

	public override bool MessageUser(int userId, string message) {
		try
		{
			AndroidJavaObject socket = helper.Call<AndroidJavaObject>("getSocket");
			if (socket.GetRawObject().ToInt32() == 0) return false;
			connected = true;
			socket.Call("messageUser", userId, System.Text.Encoding.UTF8.GetBytes(message));
			return true;
		}
		catch (Exception e)
		{
			UnityEngine.Debug.LogWarning("Platform: Error sending socket message. " + e.Message);
			return false;
		}
	}

	public override bool MessageGroup(int groupId, string message) {
		try
		{
			AndroidJavaObject socket = helper.Call<AndroidJavaObject>("getSocket");
			if (socket.GetRawObject().ToInt32() == 0) return false;
			connected = true;
			socket.Call("messageGroup", groupId, System.Text.Encoding.UTF8.GetBytes(message));
			return true;
		}
		catch (Exception e)
		{
			UnityEngine.Debug.LogWarning("Platform: Error sending group socket message. " + e.Message);
			return false;
		}
	}

	public override bool CreateGroup() {
		try
		{
			AndroidJavaObject socket = helper.Call<AndroidJavaObject>("getSocket");
			if (socket.GetRawObject().ToInt32() == 0) return false;
			connected = true;
			socket.Call("createGroup");
			return true;
		}
		catch (Exception e)
		{
			UnityEngine.Debug.LogWarning("Platform: Error creating messaging group. " + e.Message);
			return false;
		}
	}

	public override bool JoinGroup(int groupId) {
		try
		{
			AndroidJavaObject socket = helper.Call<AndroidJavaObject>("getSocket");
			if (socket.GetRawObject().ToInt32() == 0) return false;
			connected = true;
			socket.Call("joinGroup", groupId);
			return true;
		}
		catch (Exception e)
		{
			UnityEngine.Debug.LogWarning("Platform: Error joining messaging group. " + e.Message);
			return false;
		}
	}

	public override bool LeaveGroup(int groupId) {
		try
		{
			AndroidJavaObject socket = helper.Call<AndroidJavaObject>("getSocket");
			if (socket.GetRawObject().ToInt32() == 0) return false;
			connected = true;
			socket.Call("leaveGroup", groupId);
			return true;
		}
		catch (Exception e)
		{
			UnityEngine.Debug.LogWarning("Platform: Error leaving messaging group. " + e.Message);
			return false;
		}
	}

	private Vector2i GetScreenDimensions()
	{
		try
		{
			AndroidJavaObject displayMetrics = context.Call<AndroidJavaObject>("getResources").Call<AndroidJavaObject>("getDisplayMetrics");
			int height = displayMetrics.Get<int>("heightPixels");
			int width = displayMetrics.Get<int>("widthPixels");
			return new Vector2i(width, height);
		}
		catch (Exception e)
		{
			UnityEngine.Debug.LogWarning("Platform: Error getting screen dimensions. " + e.Message);
			return new Vector2i(0,0);
		}
	}	
	
	public override Device DeviceInformation() 
	{
		return new Device(build_class.GetStatic<string>("MANUFACTURER"), build_class.GetStatic<string>("MODEL"));
	}	
}
#endif

