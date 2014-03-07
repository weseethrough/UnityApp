using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using System;
using System.Runtime.CompilerServices;
using SimpleJSON;
using RaceYourself;
using SiaqodbDemo;
using Sqo;
using RaceYourself.Models;
using Newtonsoft.Json;

public abstract class Platform : MonoBehaviour {
	private double targetElapsedDistance = 0;

    private string intent = "";

	private PlayerOrientation playerOrientation = new PlayerOrientation();

	// Holds the local player's position and bearing
	public abstract PlayerPosition LocalPlayerPosition { get; }

	// Helper class for accessing/awarding points
	public abstract PlayerPoints PlayerPoints { get; }

	// Player state - STOPPED, STEADY_GPS_SPEED etc. Set from Java via Unity Messages.
	// This should probably move into PlayerPosition at some point..
	internal float playerStateEntryTime = UnityEngine.Time.time;
	internal string playerState = "";

	protected float yaw = -999.0f;
	private bool started = false;
	protected bool initialised = false;

	public int currentTrack { get; set; }
	public float[] sensoriaSockPressure { get; private set;}
	
	private List<Track> trackList;
	private List<Game> gameList;
	

	private AndroidJavaObject helper;


	private AndroidJavaClass helper_class;

	private AndroidJavaObject activity;
	private AndroidJavaObject context;
	private AndroidJavaObject sensoriaSock;
	
	public List<TargetTracker> targetTrackers { get; private set; }
	
	// Are we authenticated? Note: we mark it false at init and true when any auth call passes
	public bool authenticated { get; private set; }	
	public bool connected { get; private set; } // ditto
	
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
	public delegate void OnGroupCreated(int groupId);
	public OnGroupCreated onGroupCreated = null;
	
	// The current user and device
	private User user = null;
	private Device device = null;
	
	private static Platform _instance;
	private static Type platformType;

	protected static Log log = new Log("Platform");  // for use by subclasses
	
	private Siaqodb db;	

	/// <summary>
	/// Gets the single instance of the right kind of platform for the OS we're running on,
	/// or creates one if it doesn't exist.
	/// </summary>
	/// <value>
	/// The instance.
	/// </value>
    public static Platform Instance 
    {
		get 
        {
            if(applicationIsQuitting) 
            {
            	log.info("Application is quitting - won't create a new instance of Platform");
                return null;
            }

			// work out which type of platform we need
			#if UNITY_EDITOR
        	    platformType = typeof(PlatformDummy);
			#elif UNITY_ANDROID
				platformType = typeof(AndroidPlatform);
			#elif UNITY_IPHONE
				platformType = typeof(IosPlatform);
			#endif

			// find, or create, an instance of the right type
            if(_instance == null || !_instance.GetType().Equals(platformType))
            {
				// if an instance exists, use it
				var instance = (Platform) FindObjectOfType(platformType);
				var owner = false;

				// otherwise initialise a new one
				if(instance == null)
                {
					log.info("Creating new " + platformType.Name);
					GameObject singleton = new GameObject();
                	instance = (Platform)singleton.AddComponent(platformType);
                	singleton.name = "Platform"; // Used as target for messages
					instance.enabled = true;
					singleton.SetActive(true);
                    DontDestroyOnLoad(singleton);
					owner = true;
            	}
				else
				{
					log.info("Found existing " + platformType.Name + ", won't create a new one. This is unlikely to happen..");
				}

				// make sure the instance is initialized before returning
				while (instance.initialised == false)
                {
                    //yield return null;
					continue;
                }
				_instance  = instance;
				if (owner) _instance.PostInit();

			}

            return _instance;
     	}
    }
	
	private static bool applicationIsQuitting = false;
	
	public void OnDestroy() {
		applicationIsQuitting = true;
	}
	
	void Awake()
    {
		//Application.targetFrameRate = 25;
        if (initialised == false)
        {
            Initialize();
        }
    }
        
	protected virtual void Initialize()
	{        
	                
	    authenticated = false;
		connected = false;
	    targetTrackers = new List<TargetTracker>();
	                
	    try {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
    		activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            context = activity.Call<AndroidJavaObject>("getApplicationContext");
            helper_class = new AndroidJavaClass("com.glassfitgames.glassfitplatform.gpstracker.Helper");

            UnityEngine.Debug.LogWarning("Platform: helper_class created OK");
	                        
            // call the following on the UI thread
            activity.Call("runOnUiThread", new AndroidJavaRunnable(() => {
	                                
				try {
	                // Get the singleton helper objects
                    helper = helper_class.CallStatic<AndroidJavaObject>("getInstance", context);

                    UnityEngine.Debug.LogWarning("Platform: unique helper instance returned OK");

	                // Cache the list of games and states from java
					log.info("Initializing Games");
		            GetGames();
					
					// get reference to Sensoria Socks
					log.info("Initializing Sensoria socks");
					try {
						sensoriaSock = new AndroidJavaObject("com.glassfitgames.glassfitplatform.sensors.SensoriaSock", context);
					} catch (Exception e) {
						log.error("Error attaching to Sensoria Socks: " + e.Message);
					}

		                                        
					if (OnGlass() && HasInternet()) {
						log.info("Attempting authorize");
						Authorize("any", "login");
						log.info("Authorize complete");
					}

					log.info("Initializing bluetooth");
					if (IsRemoteDisplay()) {
						BluetoothServer();
					} else {
						BluetoothClient();
					}
					if (false && HasInternet() && HasPermissions("any", "login")) {
						// TODO: Non-blocking connect?
						ConnectSocket();
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
	                        
	    } catch (Exception e) {
            log.error("Error in initialisation " + e.Message);
			Application.Quit();
	    }
		// start listening for 2-tap gestures to reset gyros
		GestureHelper.onTwoTap += new GestureHelper.TwoFingerTap(() => {
            if (IsRemoteDisplay())
            {
                Platform.Instance.GetPlayerOrientation().Reset();
            }
            else
            {
                GUICamera[] scripts = GameObject.FindObjectsOfType(typeof(GUICamera)) as GUICamera[];
                foreach(GUICamera cameraScript in scripts)
                {
                    cameraScript.ResetCamera();
                }

            }
		});
	}
	
	protected virtual void PostInit() {
		UnityEngine.Debug.Log("Platform: post init");
		if (Application.isPlaying) {
			db = DatabaseFactory.GetInstance();
			var api = new API(db);
			StartCoroutine(api.Login("janne.husberg@gmail.com", "testing123"));
			Platform.Instance.onAuthenticated += new Platform.OnAuthenticated((authenticated) => {
				Device self = db.Cast<Device>().Where(d => d.self == true).FirstOrDefault();
				if (self == null) {
					StartCoroutine(api.RegisterDevice());
				} else {
					UnityEngine.Debug.Log ("DEBUG: device id: " + self._id);	
					StartCoroutine(api.Sync());
					StartCoroutine(api.get("users", (body) => {
						UnityEngine.Debug.Log(body);
						UnityEngine.Debug.Log(JsonConvert.DeserializeObject<RaceYourself.API.ListResponse<RaceYourself.Models.Account>>(body).response.Count);
					}));
				}
			});
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
		if (onDeviceRegistered != null) onDeviceRegistered(message);
	}	
	
	public void OnActionIntent(string message) {
		UnityEngine.Debug.Log("Platform: action " + message); 
		MessageWidget.AddMessage("Internal", "App opened with intent " + message, "settings");
        intent = message;
	}
	
	public void OnBluetoothConnect(string message) {
		MessageWidget.AddMessage("Bluetooth", message, "settings");
	}
	
	public void OnBluetoothMessage(string message) {
//		MessageWidget.AddMessage("Bluetooth", message, "settings"); // DEBUG
		UnityEngine.Debug.Log("Platform: OnBluetoothMessage " + message.Length + "B"); 
		JSONNode json = JSON.Parse(message);
		OnBluetoothJson(json);
	}
	
	public void OnUserMessage(string message) {
		JSONNode json = JSON.Parse(message);
		MessageWidget.AddMessage("Network", "<" + json["from"] + "> " + json["data"], "settings"); // DEBUG
	}
	
	public void OnGroupMessage(string message) {
		JSONNode json = JSON.Parse(message);
		MessageWidget.AddMessage("Network", "#" + json["group"] + " <" + json["from"] + "> " + json["data"], "settings"); // DEBUG
	}
	
	public void OnGroupCreation(string message) {
		if (onGroupCreated != null) onGroupCreated(int.Parse(message));
		// TODO: Potential hanging deferral. What do we do if socket is disconnected before a group is created?
	}
	
	
	private void OnBluetoothJson(JSONNode json) {
		UnityEngine.Debug.Log("Platform: OnBluetoothJson"); 
		switch(json["action"]) {
		case "LoadLevelFade":
			if (IsRemoteDisplay()) {
//				DataVaultFromJson(json["data"]);
//				if (json["levelName"] != null) AutoFade.LoadLevel(json["levelName"], 0f, 1.0f, Color.black); 			
//				if (json["levelIndex"] != null) AutoFade.LoadLevel(json["levelIndex"].AsInt, 0f, 1.0f, Color.black); 			
			}
			break;
		case "LoadLevelAsync":
			if (IsRemoteDisplay()) {
				DataVaultFromJson(json["data"]);
				FlowStateMachine.Restart("Restart Point");
			}
			break;
		case "InitiateSnack":
			if (IsRemoteDisplay()) {
				//find SnackRun Object
				SnackRun snackRunGame = (SnackRun)FindObjectOfType(typeof(SnackRun));
				string gameID = json["snack_gameID"];
				if(snackRunGame != null)
				{
					UnityEngine.Debug.Log("Platform: Received InitiateSnack message. Initiating game: " + gameID);
					snackRunGame.OfferPlayerSnack(gameID);
				}
				else
				{
					UnityEngine.Debug.LogWarning("Platform: Received InitiateSnack message for " + gameID + " but not currently on a snack run");
				}
			}
			break;
		case "ReturnToMainMenu":
			if(IsDisplayRemote()) {
				FlowStateMachine.Restart("Start Point");	
			}
			break;
		case "OnSnackFinished":
			if(IsDisplayRemote()) {
				UnityEngine.Debug.Log("Platform: Received bluetooth snack finished message");
				//find SnackRemoteControlPanel
				SnackRemoteControlPanel remotePanel = (SnackRemoteControlPanel)FlowStateMachine.GetCurrentFlowState();
				if(remotePanel != null)
				{
					remotePanel.ClearCurrentSnackHex();
				}
				else
				{
					UnityEngine.Debug.LogWarning("Platform: Couldn't find Snack remote panel");
				}
			}
			break;
		default:
			UnityEngine.Debug.Log("Platform: unknown Bluetooth message: " + json);
			break;
		}
		
		// TODO: Start challenge
		// TODO: Toggle outdoor/indoor
	}


	// Called by unity messages on each state change
	void PlayerStateChange(string message) {
		//UnityEngine.Debug.Log("Player state message received from Java: " + message);
		playerState = message;
		playerStateEntryTime = UnityEngine.Time.time;
	}
	
	private void DataVaultFromJson(JSONNode json) {
		JSONNode track = json["current_track"];
		if (track != null) {
			Track t = FetchTrack(track["device_id"].AsInt, track["track_id"].AsInt);
			if (t != null) DataVault.Set("current_track", t);
			else track = null;
		} 
		if (track == null) DataVault.Remove("current_track");
		if (json["race_type"] != null) DataVault.Set("race_type", json["race_type"] as string);
		else DataVault.Remove("race_type");
		if (json["type"] != null) DataVault.Set("type", json["type"] as string);
		else DataVault.Remove("type");
		if (json["finish"] != null) DataVault.Set("finish", json["finish"].AsInt);
		else DataVault.Remove("finish");
		if (json["lower_finish"] != null) DataVault.Set("lower_finish", json["lower_finish"].AsInt);
		else DataVault.Remove("lower_finish");
		if (json["challenger"] != null) DataVault.Set("challenger", json["challenger"] as string);
		else DataVault.Remove("challenger");
		if (json["current_game_id"] != null)
		{
			UnityEngine.Debug.Log("Bluetooth: Current Game ID received: " + json["current_game_id"]);
			DataVault.Set("current_game_id", json["current_game_id"] as string);
		}
		else DataVault.Remove("current_game_id");
		JSONNode challengeNotification = json["current_challenge_notification"];
		if (challengeNotification != null) {
			// TODO: fetch challenge notification and store in datavault
		} 
		if (challengeNotification == null) DataVault.Remove("current_challenge_notification");
		ResetTargets();
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
			return new Device();
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("Platform: failed to fetch device " + e.Message);
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

	public virtual bool IsRemoteDisplay() 
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
	
	public bool IsDisplayRemote() {
		foreach(string peer in BluetoothPeers()) {
			UnityEngine.Debug.LogWarning("Platform: BT peer: " + peer);
			if (peer.Contains("Glass") || peer.Contains("Display")) return true;
		}
		return false;
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
	

	
	[MethodImpl(MethodImplOptions.Synchronized)]
	public virtual Track FetchTrack(int deviceId, int trackId) {
		try {
			UnityEngine.Debug.Log("Platform: fetching track");
			using (AndroidJavaObject rawtrack = helper_class.CallStatic<AndroidJavaObject>("fetchTrack", deviceId, trackId)) {
				Track track = new Track(rawtrack);
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
							Track currentTrack = new Track(rawTrack);
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
							Track currentTrack = new Track(rawtrack);
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
                UnityEngine.Debug.Log("Platform: Friends to fetch " + length);
				List<Friend> friendList = new List<Friend>();                
				for (int i=0;i<length;i++) {
					using (AndroidJavaObject f = list.Call<AndroidJavaObject>("get", i)) {
						UnityEngine.Debug.Log("Platform: getting a friend ("+i+")");
						Friend friend = new Friend(f.Get<string>("friend"));
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
	
	public virtual float GetHighestDistBehind() {
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
	
	
	public virtual float GetLowestDistBehind() {
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
	
	public virtual void Poll() {

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
	public virtual double DistanceBehindTarget(TargetTracker tracker) {
		double returnDistance = (tracker.GetTargetDistance() - LocalPlayerPosition.Distance);
		return returnDistance;
	}
	
	public virtual double DistanceBehindTarget() {
		return GetLowestDistBehind();
	}

	public virtual float Yaw() {
		return yaw;
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


	// Poll java for touch input
	// Returns the int number of fingers touching the pad
	[MethodImpl(MethodImplOptions.Synchronized)]
	public virtual int GetTouchCount()
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
	[MethodImpl(MethodImplOptions.Synchronized)]
	public virtual Vector2? GetTouchInput()
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
			UnityEngine.Debug.LogWarning("Platform: Error toggling screen recording. " + e.Message);
		}
	}

	public virtual void BluetoothServer()
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

	public virtual void BluetoothClient()
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
	
	public virtual void BluetoothBroadcast(JSONObject json) {
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

	public virtual string[] BluetoothPeers() {
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
	
	public virtual bool ConnectSocket() {
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

	public virtual bool DisconnectSocket() {
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

	public virtual bool MessageUser(int userId, string message) {
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

	public virtual bool MessageGroup(int groupId, string message) {
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

	public virtual bool CreateGroup() {
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

	public virtual bool JoinGroup(int groupId) {
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

	public virtual bool LeaveGroup(int groupId) {
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

	public Vector2i GetScreenDimensions()
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

	void OnApplicationFocus(bool paused) {
		if (initialised && paused && OnGlass()) {
			Application.Quit();
		}
	}

    public string GetIntent()
    {
        return intent;
    }
	
	public void OnApplicationQuit ()
	{
		if (db != null) {
			DatabaseFactory.CloseDatabase();
		}
		Destroy(gameObject);
	}	
}