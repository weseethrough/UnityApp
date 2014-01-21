using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using System;
using System.Runtime.CompilerServices;

public class Platform : MonoBehaviour {
	private double targetElapsedDistance = 0;
	private long time = 0;
	protected double distance = 0.0;
	private int calories = 0;
	private float pace = 0;
	protected Position position = null;
	private PlayerOrientation playerOrientation = new PlayerOrientation();
	protected float bearing = -999.0f;
	private bool started = false;
	protected bool initialised = false;
	private long currentActivityPoints = 0;
	private long openingPointsBalance = 0;
	public int currentTrack { get; set; }
	public float[] sensoriaSockPressure { get; private set;}
	
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
	private AndroidJavaObject sensoriaSock;
	
	public List<TargetTracker> targetTrackers { get; private set; }
	
	// Are we authenticated? Note: we mark it false at init and true when any auth call passes
	public bool authenticated { get; private set; }	
	
	// Other components may change this to disable sync temporarily?
	public int syncInterval = 10;
	private DateTime lastSync = new DateTime(0);
	
	// Events
	public delegate void OnAuthenticated(bool success);
	public OnAuthenticated onAuthenticated = null;
	public delegate void OnSync(string message);
	public OnSync onSync = null;
	public delegate void OnSyncProgress(string message);
	public OnSyncProgress onSyncProgress = null;
	public delegate void OnRegistered(string message);
	public OnRegistered onDeviceRegistered = null;
	
	public delegate void OnResetGyro();
	public OnResetGyro onResetGyro = null;
	
	// The current user and device
	private User user = null;
	private Device device = null;
	
	private static Platform _instance;
        

    public static Platform Instance 
    {
#if !UNITY_EDITOR        
		get 
        {
            if(applicationIsQuitting) 
            {
            	UnityEngine.Debug.Log("Singleton: already destroyed on application quit - won't create again");
                return null;
            }
                        
            if(_instance == null) 
            {
            	_instance = (Platform) FindObjectOfType(typeof(Platform));
                                
                /* Too heavy operation to be called by instance reference
                 * if(FindObjectsOfType(typeof(Platform)).Length > 1) 
                {
                	UnityEngine.Debug.Log("Singleton: there is more than one singleton");
                                        //return _instance;
                }*/
                
				if(_instance == null || (_instance is PlatformDummy) )
                {
                	GameObject singleton = new GameObject();
                	_instance = singleton.AddComponent<Platform>();
                	singleton.name = "Platform"; // Used as target for messages										
					// Enable Update() function
					_instance.enabled = true; 
					singleton.SetActive(true);
                                                
                    DontDestroyOnLoad(singleton);
            	} 
                else 
                {
               		UnityEngine.Debug.Log("Singleton: already exists!!");
                }
       		}

            if (_instance != null)
            {
                while (_instance.initialised == false)
                {
                    continue;
                }
            }
            return _instance;
                        
     	}
#else
		//create an instance of platformdummy
		get
		{
			if(_instance == null)
			{
				//Look for an instance already in existence
				//_instance = (PlatformDummy)FindObjectOfType(typeof(PlatformDummy));
				_instance = FindObjectOfType(typeof(PlatformDummy)) as PlatformDummy;
			}
			if(_instance == null)
			{
				//create instance
				GameObject singleton = new GameObject();
                _instance = singleton.AddComponent<PlatformDummy>();
                singleton.name = "PlatformDummy"; // Used as target for messages
				
				_instance.enabled = true;
				singleton.SetActive(true);
			}
			return _instance;	
		}
		
#endif
    }
	
	private static bool applicationIsQuitting = false;
	
	public void OnDestroy() {
		applicationIsQuitting = true;
	}
	
	 void Awake()
    {
		Application.targetFrameRate = 25;
        if (initialised == false)
        {
            Initialize();
        }
    }
        
	public virtual void Initialize()
	{        
	                
	    authenticated = false;
	    targetTrackers = new List<TargetTracker>();
	    UnityEngine.Debug.Log("Platform: Initialize() called");
	                
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
                
                // Cache the list of games and states from java
	            GetGames();
				
				// get reference to Sensoria Socks
				try {
					sensoriaSock = new AndroidJavaObject("com.glassfitgames.glassfitplatform.sensors.SensoriaSock", context);
					UnityEngine.Debug.Log("Platform: socks obtained");
				} catch (Exception e) {
					UnityEngine.Debug.LogWarning("Platform: Error attaching to Sensoria Socks: " + e.Message);
				}
			                       
				//Poll();
	            UnityEngine.Debug.Log("Platform: Opening points: " + GetOpeningPointsBalance());
	            UnityEngine.Debug.Log("Platform: Current game points: " + GetCurrentPoints());
	            UnityEngine.Debug.Log("Platform: Current gems: " + GetCurrentGemBalance());
	            UnityEngine.Debug.Log("Platform: Current metabolism: " + GetCurrentMetabolism());
	                                        
				if (OnGlass() && HasInternet()) {
					UnityEngine.Debug.Log("Platform: attempting authorize");
					Authorize("any", "login");
					UnityEngine.Debug.Log("Platform: authorize complete");
				}
				
	            initialised = true;
				
				UnityEngine.Debug.Log("Platform: initialise complete");
				//ExportCSV();
	    	}));
	                        
	    } catch (Exception e) {
            UnityEngine.Debug.LogWarning("Platform: Error in constructor" + e.Message);
            UnityEngine.Debug.LogException(e);
	    }
	}
	
	/// Message receivers
	public void OnAuthentication(string message) {
		UnityEngine.Debug.Log("Platform: starting to authenticate");
		if (string.Equals(message, "Success")) {
			if (authenticated == false) {
				User me = User();
				if (me != null) MessageWidget.AddMessage("Logged in", "Welcome " + me.name, "settings");
			}
			authenticated = true;
			if (onAuthenticated != null) onAuthenticated(true);
		}
		if (string.Equals(message, "Failure")) {
			if (onAuthenticated != null) onAuthenticated(false);
		}
		if (string.Equals(message, "OutOfBand")) {
			if (onAuthenticated != null) onAuthenticated(false);
			// TODO: Use confirmation dialog instead of message
			MessageWidget.AddMessage("Notice", "Please use the web interface to link your account to this provider", "settings");
		}
		UnityEngine.Debug.Log("Platform: authentication " + message.ToLower()); 
	}
	
	public void OnSynchronization(string message) {
		UnityEngine.Debug.Log("Platform: synchronize finished with " + message);
		lastSync = DateTime.Now;
		if (onSync != null) onSync(message);
	}

	public void OnSynchronizationProgress(string message) {
		if (onSyncProgress != null) onSyncProgress(message);
	}
	
	public void OnRegistration(string message) {
		onDeviceRegistered(message);
	}	
	
	public void OnActionIntent(string message) {
		UnityEngine.Debug.Log("Platform: action " + message); 
		MessageWidget.AddMessage("Internal", "App opened with intent " + message, "settings");
	}
	
	public virtual AndroidJavaObject GetHelper() {
		return helper;
	}
	
	public virtual bool HasStarted() {
		return started;
	}
	
	public virtual Device Device() {
		try {
			UnityEngine.Debug.Log("Platform: Getting user details");
			AndroidJavaObject ajo = helper_class.CallStatic<AndroidJavaObject>("getDevice");
			if (ajo.GetRawObject().ToInt32() == 0) return null;
			return new Device(ajo.Get<int>("id"), ajo.Get<string>("manufacturer"), ajo.Get<string>("model"), ajo.Get<int>("glassfit_version"));
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: failed to fetch user " + e.Message);
			UnityEngine.Debug.LogException(e);
			return null;
		}
	}	
	
	[MethodImpl(MethodImplOptions.Synchronized)]
	public virtual User User() {
		try {
			UnityEngine.Debug.Log("Platform: getting user");
			AndroidJavaObject ajo = helper_class.CallStatic<AndroidJavaObject>("getUser");
			if (ajo.GetRawObject().ToInt32() == 0) return null;
			return new User(ajo.Get<int>("guid"), ajo.Get<string>("username"), ajo.Get<string>("name"));
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: failed to fetch user " + e.Message);
			UnityEngine.Debug.LogException(e);
			return null;
		}
	}
	
	public virtual User GetUser(int userId) {
		// TODO: Implement me!
		try {
			AndroidJavaObject ajo = helper_class.CallStatic<AndroidJavaObject>("fetchUser", userId);
			if(ajo.GetRawObject().ToInt32() == 0) return null;
			return new User(ajo.Get<int>("guid"), ajo.Get<string>("username"), ajo.Get<string>("name"));
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: error getting user");
			string[] names = { "Cain", "Elijah", "Jake", "Finn", "Todd", "Juno", "Bubblegum", "Ella", "May", "Sofia" };
			string name = names[userId % names.Length];
			
			return new User(userId, name, name + " Who");
		}
	}
	
	// Starts tracking
	[MethodImpl(MethodImplOptions.Synchronized)]
	public virtual void StartTrack() {
		try {
			gps.Call("startTracking");
			tracking = true;
			started = true;
			UnityEngine.Debug.Log("Platform: StartTrack succeeded");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: StartTrack failed " + e.Message);
			UnityEngine.Debug.LogException(e);
		}
	}
	
	// Set the indoor mode
	[MethodImpl(MethodImplOptions.Synchronized)]
	public virtual void SetIndoor(bool indoor) {
		try {
			UnityEngine.Debug.Log("Platform: setting indoor");
			AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
    	    AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
			
			activity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
				gps.Call("setIndoorMode", indoor);
				if (indoor) {
				    gps.Call("setIndoorSpeed", 4.5f);
				    UnityEngine.Debug.Log("Platform: Indoor mode set to true, indoor speed = 4 min/km");
				} else {
					UnityEngine.Debug.Log("Platform: Indoor mode set to false, will use true GPS speed");
				}
			}));
		} catch(Exception e) {
			UnityEngine.Debug.Log("Platform: Error setting indoor mode " + e.Message);
		}
	}
	
	[MethodImpl(MethodImplOptions.Synchronized)]
	public virtual bool IsIndoor() {
  		try {
			//UnityEngine.Debug.Log("Platform: checking indoor");
		    return gps.Call<bool>("isIndoorMode");
		  } catch (Exception e) {
		   UnityEngine.Debug.LogWarning("Platform: Error returning isIndoor");
		   UnityEngine.Debug.Log(e.Message);
		   return false;
		  }
	}
	
	[MethodImpl(MethodImplOptions.Synchronized)]
	public virtual void ResetTargets() {
		try {
			helper.Call("resetTargets");
			targetTrackers.Clear();
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Error clearing targets");
		}
	}
	
	// Returns the target tracker
	[MethodImpl(MethodImplOptions.Synchronized)]
	public virtual TargetTracker CreateTargetTracker(float constantSpeed){
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
	[MethodImpl(MethodImplOptions.Synchronized)]
	public virtual TargetTracker CreateTargetTracker(int deviceId, int trackId){
		TargetTracker t = TargetTracker.Build(helper, deviceId, trackId);
		if (t == null) return null;
		targetTrackers.Add(t);
		return t;
	}
	
	public virtual bool OnGlass() 
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

    public virtual bool IsPluggedIn()
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
	
	public bool HasInternet() {
		try {
			UnityEngine.Debug.Log("Platform: checking internet");
			return helper.Call<bool>("hasInternet");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: hasInternet() failed: " + e.Message);
			UnityEngine.Debug.LogException(e);
			return false;
		}
	}
	
	public bool HasWifi() {
		try {
			UnityEngine.Debug.Log("Platform: checking wifi");
			return helper.Call<bool>("hasWifi");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: hasWifi() failed: " + e.Message);
			UnityEngine.Debug.LogException(e);
			return false;
		}
	}
	
	// Check if has GPS lock
	[MethodImpl(MethodImplOptions.Synchronized)]
	public virtual Boolean HasLock() {
		try {
			//UnityEngine.Debug.Log("Platform: checking GPS");
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
	[MethodImpl(MethodImplOptions.Synchronized)]
	public virtual Track StopTrack() {
		try {
			UnityEngine.Debug.Log("Platform: calling stop track");
			gps.Call("stopTracking");
			using (AndroidJavaObject rawtrack = gps.Call<AndroidJavaObject>("getTrack")) {
				return convertTrack(rawtrack);
			}
		} catch(Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Problem stopping tracking");
			UnityEngine.Debug.LogException(e);
			return null;
		}
	}
	
	// Authentication 
	// result returned through onAuthenticated
	[MethodImpl(MethodImplOptions.Synchronized)]
	public virtual void Authorize(string provider, string permissions) {
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
	
	[MethodImpl(MethodImplOptions.Synchronized)]
	public virtual bool HasPermissions(string provider, string permissions) {
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
	[MethodImpl(MethodImplOptions.Synchronized)]
	public virtual void SyncToServer() {
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

	
	// Reset GPS tracker
	[MethodImpl(MethodImplOptions.Synchronized)]
	public virtual void Reset() {
		try {
			gps.Call("startNewTrack");
			started = false;
			UnityEngine.Debug.Log("Platform: GPS has been reset");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: reset() failed: " + e.Message);
			UnityEngine.Debug.LogException(e);
		}
	}
		
	// Load the game blob
	public virtual byte[] LoadBlob(string id) {
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
	
	// Get the device's orientation
	public virtual PlayerOrientation GetPlayerOrientation() {
		return playerOrientation;
	}
	
	// Reset the Gyros and accelerometer
	public virtual void ResetGyro() {
		try {
			UnityEngine.Debug.Log("Platform: resetting gyros");
			helper.Call("resetGyros");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Error resetting gyros: " + e.Message);
		}
		//call handlers
		if (onResetGyro != null)
		{
			UnityEngine.Debug.Log("Platform: calling reset gyros delegate");
			onResetGyro();
		}
		UnityEngine.Debug.Log("Platform: reset gyros");
	}
	
	[MethodImpl(MethodImplOptions.Synchronized)]
	public virtual Challenge FetchChallenge(string id) {
		try {
			UnityEngine.Debug.Log("Platform: fetching challenge");
			using (AndroidJavaObject rawch = helper_class.CallStatic<AndroidJavaObject>("fetchChallenge", id)) {
				return Challenge.Build(rawch.Get<string>("json"));
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
	
	private Track convertTrack(AndroidJavaObject rawtrack) {
		UnityEngine.Debug.Log("Platform: converting track");
		string name = rawtrack.Call<string>("getName");
		int[] ids = rawtrack.Call<int[]>("getIDs"); 
		double trackDistance = rawtrack.Call<double>("getDistance");
		long trackTime = rawtrack.Call<long>("getTime");
		string date = rawtrack.Call<string>("getDate");
		using(AndroidJavaObject poslist = rawtrack.Call<AndroidJavaObject>("getTrackPositions")) {
			int numPositions = poslist.Call<int>("size");
			List<Position> pos = new List<Position>(numPositions);
			for(int j=0; j<numPositions; j++) {
				AndroidJavaObject position = poslist.Call<AndroidJavaObject>("get", j);
				Position current = new Position((float)position.Call<double>("getLatx"), (float)position.Call<double>("getLngx"));
				pos.Add(current);
			}
			return new Track(name, ids[0], ids[1], pos, trackDistance, trackTime, date);
		}
	}
	
	[MethodImpl(MethodImplOptions.Synchronized)]
	public virtual Track FetchTrack(int deviceId, int trackId) {
		try {
			UnityEngine.Debug.Log("Platform: fetching track");
			using (AndroidJavaObject rawtrack = helper_class.CallStatic<AndroidJavaObject>("fetchTrack", deviceId, trackId)) {
				Track track = convertTrack(rawtrack);
				return track;
			}
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Error getting Track: " + e.Message);
			return null;
		}
	}
	
	// Obtain tracks based on distance
	public virtual List<Track> GetTracks(double distance, double minDistance) {
		try {
			UnityEngine.Debug.Log("Platform: getting tracks with distance");
			using(AndroidJavaObject list = helper.Call<AndroidJavaObject>("getTracks", distance, minDistance)) {
				int size = list.Call<int>("size");
				trackList = new List<Track>(size);
				try {
					for(int i=0; i<size; i++) {
						using(AndroidJavaObject rawTrack = list.Call<AndroidJavaObject>("get", i)) {
							Track currentTrack = convertTrack(rawTrack);
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
	[MethodImpl(MethodImplOptions.Synchronized)]
	public virtual List<Track> GetTracks() {
		try {
			UnityEngine.Debug.Log("Platform: getting tracks");
			using(AndroidJavaObject list = helper_class.Call<AndroidJavaObject>("getTracks")) {
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
	/// Gets the hardcoded games from Java.
	/// </summary>
	/// <returns>
	/// The games.
	/// </returns>
	public virtual List<Game> GetTempGames() {
		if(gameList != null)
		{
			return gameList;
		}
		
		try 
		{
			UnityEngine.Debug.Log("Platform: Getting temp games from java...");
			AndroidJavaObject javaGameList = helper.Call<AndroidJavaObject>("getTempGames");
			int size = javaGameList.Call<int>("size");
			UnityEngine.Debug.Log("Platform: Retrieved " + size + " temp games from java");
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
				UnityEngine.Debug.Log("Platform: Successfully imported " + size + " temp games.");
				return gameList;
			}
			catch (Exception e)
			{
				UnityEngine.Debug.LogWarning("Platform: Error getting temp game!");
				UnityEngine.Debug.LogWarning(e.Message);
				UnityEngine.Debug.LogException(e);
				return null;
			}
		} catch (Exception e)
		{
			UnityEngine.Debug.LogWarning("Platform: Error getting temp Games!");
			UnityEngine.Debug.LogWarning(e.Message);
			UnityEngine.Debug.LogException(e);
			return null;
		}
	}
	
	public virtual void ResetGames() 
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
	public virtual List<Game> GetGames()
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
	
	public virtual void QueueAction(string json) {
		try {
			UnityEngine.Debug.Log("Platform: queueing action");
			helper_class.CallStatic("queueAction", json);
			UnityEngine.Debug.Log("Platform: action queued");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Error queueing action: " + e.Message);
		}
	}
		
	[MethodImpl(MethodImplOptions.Synchronized)]
	public virtual List<Friend> Friends() {
		try {
			UnityEngine.Debug.Log("Platform: getting friends list");
			using(AndroidJavaObject list = helper_class.CallStatic<AndroidJavaObject>("getFriends")) {
				UnityEngine.Debug.Log("Platform: getting friends list size");
				int length = list.Call<int>("size");
				List<Friend> friendList = new List<Friend>();
				for (int i=0;i<length;i++) {
					using (AndroidJavaObject f = list.Call<AndroidJavaObject>("get", i)) {
						UnityEngine.Debug.Log("Platform: getting a friend");
						Friend friend = new Friend(f.Get<string>("friend"));
						friendList.Add(friend);
						UnityEngine.Debug.Log("Platform: got a friend");
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
	
	[MethodImpl(MethodImplOptions.Synchronized)]
	public virtual Notification[] Notifications() {
		try {
			UnityEngine.Debug.Log("Platform: getting notifications");
			using(AndroidJavaObject list = helper_class.CallStatic<AndroidJavaObject>("getNotifications")) {
				int length = list.Call<int>("size");
				Notification[] notifications = new Notification[length];
				for (int i=0;i<length;i++) {
					AndroidJavaObject p = list.Call<AndroidJavaObject>("get", i);
					notifications[i] = new Notification(p.Get<string>("id"), p.Get<bool>("read"), p.Get<string>("message"));
					notifications[i].ajo = p; // Store java handle, TODO: Only when not read so as to save handles?
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
	
	public virtual void ReadNotification(string id) {
		throw new NotImplementedException("Platform: Iterate through notifications and setRead(true) or add setRead(id) helper method?");
	}
		
	// Store the blob
	public virtual void StoreBlob(string id, byte[] blob) {
		try {
			UnityEngine.Debug.Log("Platform: storing blob");
			helper_class.CallStatic("storeBlob", id, blob);
			UnityEngine.Debug.Log("Platform: Game blob " + id + " of size: " + blob.Length + " stored");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: StoreBlob() failed: " + e.Message);
			UnityEngine.Debug.LogException(e);
		}
	}
	
	 /**
	 * Editor-specific function.
	 * Now defunct
	 */
	public virtual void StoreBlobAsAsset(string id, byte[] blob) {
		return;
	}
	
	public virtual float GetDistance() {
		return (float)distance;
	}
	
	
	public virtual float GetHighestDistBehind() {
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
	
	
	public virtual float GetLowestDistBehind() {
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
	public virtual void EraseBlob(string id) {
		try {
			helper_class.CallStatic("eraseBlob", id);
			UnityEngine.Debug.Log("Platform: Game blob " + id + " erased");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: EraseBlob() failed: " + e.Message);
			UnityEngine.Debug.LogException(e);			
		}
	}
	
	public virtual void ResetBlobs() {
		try {
			helper_class.CallStatic("resetBlobs");
			UnityEngine.Debug.Log("Platform: Game blobs reset");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: ResetBlobs() failed: " + e.Message);
			UnityEngine.Debug.LogException(e);			
		}
	}
	
	public virtual void Update() {
		//UnityEngine.Debug.Log("Platform: updating");
		if (device == null) device = Device();
		if (user == null) user = User();
		//UnityEngine.Debug.Log("Platform: about to sync");
		if (authenticated && syncInterval > 0 && DateTime.Now.Subtract(lastSync).TotalSeconds > syncInterval && IsPluggedIn()) {
			//UnityEngine.Debug.Log("Platform: about to sync properly");
			SyncToServer();
			//UnityEngine.Debug.Log("Platform: sync complete");
		}

		// Update player orientation
		try {
			//UnityEngine.Debug.Log("Platform: getting orientation");
			AndroidJavaObject q = helper.Call<AndroidJavaObject>("getOrientation");
            playerOrientation.Update(new Quaternion(q.Call<float>("getX"), q.Call<float>("getY"), q.Call<float>("getZ"), q.Call<float>("getW")));
		} catch (Exception e) {
			UnityEngine.Debug.Log("Platform: Error getting orientation: " + e.Message);
		}
	}	
	
	public virtual void Poll() {
		//UnityEngine.Debug.Log("Platform: poll now");
		
//		if (!hasLock ()) return;
		try {
			time = gps.Call<long>("getElapsedTime");
			//UnityEngine.Debug.Log("Platform: poll time");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: getElapsedTime() failed: " + e.Message);
			UnityEngine.Debug.LogException(e);
		}
		
		//UnityEngine.Debug.Log("Platform: poll There are " + targetTrackers.Count + " target trackers");
		for(int i=0; i<targetTrackers.Count; i++) {
			targetTrackers[i].PollTargetDistance();
		}
		
		try {
			distance = gps.Call<double>("getElapsedDistance");
			//UnityEngine.Debug.Log("Platform: poll distance");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: getElapsedDistance() failed: " + e.Message);
			UnityEngine.Debug.LogException(e);
		}
		try {
			pace = gps.Call<float>("getCurrentSpeed");
			//UnityEngine.Debug.Log("Platform: poll speed");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: getCurrentSpeed() failed: " + e.Message);            
			UnityEngine.Debug.LogException(e);
		}
		try {
			if (HasLock()) {
				AndroidJavaObject ajo = gps.Call<AndroidJavaObject>("getCurrentPosition");
				//UnityEngine.Debug.Log("Platform: poll position");
				position = new Position((float)ajo.Call<double>("getLatx"), (float)ajo.Call<double>("getLngx"));
			}
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Error getting position: " + e.Message);
//			errorLog = errorLog + "\ngetCurrentPosition|Bearing" + e.Message;
		}

		try {
			if (gps.Call<bool>("hasBearing")) {
				bearing = gps.Call<float>("getCurrentBearing");
				//UnityEngine.Debug.Log("Platform: poll bearing");
			} else {
				bearing = -999.0f;
			}
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Error getting bearing: " + e.Message);
		}
		
		try {
			currentActivityPoints = points_helper.Call<long>("getCurrentActivityPoints");
			//UnityEngine.Debug.Log("Platform: poll current points");
			string pointsFormatted = currentActivityPoints.ToString("n0");
			DataVault.Set ("points", pointsFormatted/* + "RP"*/);
			
			//UnityEngine.Debug.Log("Platform: poll points vault set");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Error getting current activity points: " + e.Message);
			DataVault.Set("points", -1);
		}		
		
		try {
			openingPointsBalance = points_helper.Call<long>("getOpeningPointsBalance");
			//UnityEngine.Debug.Log("Platform: poll opening points");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Error getting opening points balance: " + e.Message);
		}
		
		try {
			sensoriaSockPressure = sensoriaSock.Call<float[]>("getPressureSensorValues", ((long)(UnityEngine.Time.time*1000)));
			//UnityEngine.Debug.Log("Platform: poll Sensoria pressure = " + sensoriaSockPressure[0]);
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Error getting sensoria sock pressure data: " + e.Message);
		}
	}
	
	// Return the distance behind target
	public virtual double DistanceBehindTarget(TargetTracker tracker) {
		double returnDistance = (tracker.GetTargetDistance() - distance);
		return returnDistance;
	}
	
	public virtual double DistanceBehindTarget() {
		return GetLowestDistBehind();
	}
	
	public virtual long Time() {
		return time;
	}
	
	public virtual double Distance() {
		return distance;
	}
	
	public virtual int Calories() {
		double cal = 76.0 / 1000.0 * distance;
		return (int)cal;
	}
	
	public virtual float Pace() {
		return pace;
	}
	
	public virtual Position Position() {
		return position;
	}
	
	public virtual float Bearing() {
		return bearing;
	}
	
	public virtual long GetOpeningPointsBalance() {
		return openingPointsBalance;
	}
	
	public virtual long GetCurrentPoints() {
		return currentActivityPoints;
	}
	
	public virtual int GetCurrentGemBalance() {
		try {
			return points_helper.Call<int>("getCurrentGemBalance");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Error getting current gem balance: " + e.Message);
			return 0;
		}
	}
	
	/// <summary>
	/// Gets the target speed. Only used by Eagle? Will be better done by a dummy targetController.
	/// </summary>
	/// <returns>
	/// The target speed.
	/// </returns>
	public virtual float GetTargetSpeed() {
		return 0.0f;
	}
	
	public virtual float GetCurrentMetabolism() {
		try {
			return points_helper.Call<float>("getCurrentMetabolism");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Error getting current metabolism: " + e.Message);
			return 0;
		}
	}
	
	public virtual void SetBasePointsSpeed(float speed) {
		try {
			points_helper.Call("setBaseSpeed", speed);
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: Error setting base points speed: " + e.Message);
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
	public virtual void AwardPoints(String reason, String gameId, long points)
	{
		try
		{
			points_helper.Call("awardPoints", "in-game bonus", reason, gameId, points);
			UnityEngine.Debug.Log("Platform: " + gameId + " awarded " + points + " points for " + reason);
		}
		catch (Exception e)
		{
			UnityEngine.Debug.LogWarning("Platform: Error awarding " + reason + " of " + points + " points in " + gameId);
		}
	}
	
	/// <summary>
	/// Use this method to award the user gems.
	/// </summary>
	/// <param name='reason'>
	/// Reason that the gems are being awarded, e.g. "race completion".
	/// </param>
	/// <param name='gameId'>
	/// Game identifier so we can log which game the gems came from.
	/// </param>
	/// <param name='gems'>
	/// Number of gems to award.
	/// </param>
	public virtual void AwardGems(String reason, String gameId, int gems)
	{
		try
		{
			points_helper.Call("awardGems", "in-game bonus", reason, gameId, gems);
			UnityEngine.Debug.Log("Platform: " + gameId + " awarded " + gems + " gem(s) for " + reason);
		}
		catch (Exception e)
		{
			UnityEngine.Debug.LogWarning("Platform: Error awarding " + reason + " of " + gems + " gems in " + gameId);
		}
	}

	/// <summary>
	/// Use this method to record events for analytics, e.g. a user action.
	/// </summary>
	/// <param name='json'>
	/// Json-encoded event values such as current game state, what the user action was etc
	/// </param>
	public virtual void LogAnalytics(JSONObject json)
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
	
	/// <summary>
	/// Stores the BLOB. Called by 
	/// </summary>
	/// <param name='id'>
	/// Identifier.
	/// </param>
	/// <param name='blob'>
	/// BLOB.
	/// </param>

	public virtual void ToggleScreenCapture()
	{
		try
		{
			helper.Call("screenrecord", activity);
			UnityEngine.Debug.Log("Platform: toggling screen recording");
		}
		catch (Exception e)
		{
			UnityEngine.Debug.LogWarning("Platform: Error logging analytic event. " + e.Message);
		}
	}
}