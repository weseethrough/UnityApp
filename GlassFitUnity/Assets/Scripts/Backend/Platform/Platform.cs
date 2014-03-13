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
	protected double targetElapsedDistance = 0;

    protected string intent = "";

	protected PlayerOrientation playerOrientation = new PlayerOrientation();

	// Holds the local player's position and bearing
	public abstract PlayerPosition LocalPlayerPosition { get; }

	// Helper class for accessing/awarding points
	public abstract PlayerPoints PlayerPoints { get; }

	// Player state - STOPPED, STEADY_GPS_SPEED etc. Set from Java via Unity Messages.
	// This should probably move into PlayerPosition at some point..
	internal float playerStateEntryTime;
	internal string playerState = "";

	protected float yaw = -999.0f;
	protected bool started = false;
	protected bool initialised = false;

	public int currentTrack { get; set; }
	public float[] sensoriaSockPressure { get; protected set;}
	
	protected List<Track> trackList;
	protected List<Game> gameList;
	
	public List<TargetTracker> targetTrackers { get; protected set; }
	
	// Are we authenticated? Note: we mark it false at init and true when any auth call passes
	public bool authenticated { get; protected set; }	
	public bool connected { get; protected set; } // ditto
	
	// Other components may change this to disable sync temporarily?
	public int syncInterval = 10;
	protected DateTime lastSync = new DateTime(0);
	
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
	
	protected static Platform _instance;
	protected static Type platformType;

	protected static Log log = new Log("Platform");  // for use by subclasses
	
	protected Siaqodb db;	
	protected API api;

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
			#elif UNITY_ANDROID && RACEYOURSELF_MOBILE
				platformType = typeof(PlatformDummy);
			#elif UNITY_ANDROID
				platformType = typeof(AndroidPlatform);
			#elif UNITY_IPHONE
				platformType = typeof(IosPlatform);
			#endif

			// find, or create, an instance of the right type
            if(ReferenceEquals(null, _instance) || !_instance.GetType().Equals(platformType))
            {
				// if an instance exists, use it
				var instance = (Platform) FindObjectOfType(platformType);
				var owner = false;

				// otherwise initialise a new one
				if(ReferenceEquals(null, instance))
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
					if (applicationIsQuitting) return null;
                    //yield return null;
					continue;
                }
				_instance  = instance;
				if (owner) _instance.PostInit();

			}

            return _instance;
     	}
    }
	
	protected static bool applicationIsQuitting = false;
	
	public virtual void OnDestroy() {
		log.info("OnDestroy");
		applicationIsQuitting = true;
	}
	
	public virtual void Awake()
    {
		if (gameObject != null && gameObject.name != "New Game Object") 
		{
			log.error("Scene " + Application.loadedLevelName + " contains a platform gameobject called " + gameObject.name + ", quitting..");
//			DestroyImmediate(gameObject); // Doesn't always work, force developer to manually fix scene
			Application.Quit();
#if UNITY_EDITOR
    		UnityEditor.EditorApplication.isPlaying = false;
#endif			
			return;
		}
		if (initialised == false)
        {
			playerStateEntryTime = UnityEngine.Time.time;
            Initialize();
        }
    }
        
	protected virtual void Initialize()
	{        	                
	    authenticated = false;
		connected = false;
	    targetTrackers = new List<TargetTracker>();	                
		// Set initialised=true in overriden method
	}
	
	protected virtual void PostInit() {
		UnityEngine.Debug.Log("Platform: post init");

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
		
		if (Application.isPlaying) {
//			db = DatabaseFactory.GetInstance();
//			api = new API(db);
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
	
	
	protected void OnBluetoothJson(JSONNode json) {
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

	// Called by native platform with a push notification id
	public void OnPushId(string id) {
		if (db != null && api != null) {
			Device self = Device();
			self.push_id = id;
			db.StoreObject(self);
		}
	}	
	
	protected void DataVaultFromJson(JSONNode json) {
		JSONNode track = json["current_track"];
		if (track != null) {
			Track t = FetchTrack(track["device_id"].AsInt, track["track_id"].AsInt);
			if (t != null) DataVault.Set("current_track", t);
			else track = null;
		} 
		if (track == null) DataVault.Remove("current_track");
		if (json["race_type"] != null) DataVault.Set("race_type", json["race_type"].Value);
		else DataVault.Remove("race_type");
		if (json["type"] != null) DataVault.Set("type", json["type"].Value);
		else DataVault.Remove("type");
		if (json["finish"] != null) DataVault.Set("finish", json["finish"].AsInt);
		else DataVault.Remove("finish");
		if (json["lower_finish"] != null) DataVault.Set("lower_finish", json["lower_finish"].AsInt);
		else DataVault.Remove("lower_finish");
		if (json["challenger"] != null) DataVault.Set("challenger", json["challenger"].Value);
		else DataVault.Remove("challenger");
		if (json["current_game_id"] != null)
		{
			UnityEngine.Debug.Log("Bluetooth: Current Game ID received: " + json["current_game_id"]);
			DataVault.Set("current_game_id", json["current_game_id"].Value);
		}
		else DataVault.Remove("current_game_id");
		JSONNode challengeNotification = json["current_challenge_notification"];
		if (challengeNotification != null) {
			// TODO: fetch challenge notification and store in datavault
		} 
		if (challengeNotification == null) DataVault.Remove("current_challenge_notification");
		ResetTargets();
	}
	
	public virtual bool HasStarted() {
		return started;
	}

	public virtual Device Device() {
		throw new NotImplementedException();
	}

	public virtual User User() {
		throw new NotImplementedException();
	}

	public virtual User GetUser(int userId) {
		throw new NotImplementedException();
	}
	
	public virtual void ResetTargets() {
		throw new NotImplementedException();
	}

	// Returns the target tracker
	public virtual TargetTracker CreateTargetTracker(float constantSpeed){
		throw new NotImplementedException();
	}

	public virtual TargetTracker CreateTargetTracker(int deviceId, int trackId){
		throw new NotImplementedException();
	}
	
	public virtual bool OnGlass() {
		throw new NotImplementedException();
	}

	public virtual bool IsRemoteDisplay() {
		throw new NotImplementedException();
	}

    public virtual bool IsPluggedIn() {
		throw new NotImplementedException();
	}
	
	public virtual bool HasInternet() {
		throw new NotImplementedException();
	}
	
	public virtual bool HasWifi() {
		throw new NotImplementedException();
	}
	
	public virtual bool IsDisplayRemote() {
		throw new NotImplementedException();
	}
	
	public virtual bool HasGpsProvider() {
		throw new NotImplementedException();
	}

	public virtual bool IsBluetoothBonded() {
		throw new NotImplementedException();
	}
	

	
	// Authentication 
	// result returned through onAuthenticated
	public virtual void Authorize(string provider, string permissions) {
		throw new NotImplementedException();
	}
	
	public virtual bool HasPermissions(string provider, string permissions) {
		throw new NotImplementedException();
	}
	
	// Sync to server
	public virtual void SyncToServer() {
		throw new NotImplementedException();
	}
		
	// Load the game blob
	public virtual byte[] LoadBlob(string id) {
		throw new NotImplementedException();
	}
	
	// Get the device's orientation
	public virtual PlayerOrientation GetPlayerOrientation() {
		return playerOrientation;
	}

	
	public virtual Challenge FetchChallenge(string id) {
		throw new NotImplementedException();
	}
	
	public virtual Track FetchTrack(int deviceId, int trackId) {
		throw new NotImplementedException();
	}
	
	// Obtain tracks based on distance
	public virtual List<Track> GetTracks(double distance, double minDistance) {
		throw new NotImplementedException();
	}
	
	// Load a list of tracks
	public virtual List<Track> GetTracks() {
		throw new NotImplementedException();
	}
		
	public virtual void ResetGames() 
	{
		throw new NotImplementedException();
	}
	
	/// <summary>
	/// Load a list of games from the java database, together with lock state, cost, description etc.
	/// Typically used when building the main hex menu
	/// </summary>
	public virtual List<Game> GetGames()
	{
		throw new NotImplementedException();
	}
	
	public virtual void QueueAction(string json) {
		throw new NotImplementedException();
	}
		
	public virtual List<Friend> Friends() {
		throw new NotImplementedException();
	}
	
	public virtual Notification[] Notifications() {
		throw new NotImplementedException();
	}
	
	public virtual void ReadNotification(string id) {
		throw new NotImplementedException("Platform: Iterate through notifications and setRead(true) or add setRead(id) helper method?");
	}
		
	// Store the blob
	public virtual void StoreBlob(string id, byte[] blob) {
		throw new NotImplementedException();
	}
		
	public virtual float GetHighestDistBehind() {
		throw new NotImplementedException();
	}	
	
	public virtual float GetLowestDistBehind() {
		throw new NotImplementedException();
	}
	
	public virtual void EraseBlob(string id) {
		throw new NotImplementedException();
	}
	
	public virtual void ResetBlobs() {
		throw new NotImplementedException();
	}
	
	public virtual void Update() {
		if (PlayerPoints != null) PlayerPoints.Update();
	}	
	
	public virtual void Poll() {
		throw new NotImplementedException();
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
	public virtual void LogAnalytics(string json)
	{
		throw new NotImplementedException();
	}


	// Poll java for touch input
	// Returns the int number of fingers touching the pad
	public virtual int GetTouchCount()
	{
		throw new NotImplementedException();
	}

	// Poll java for touch input
	// Returns (x,y) as floats between 0 and 1
	public virtual Vector2? GetTouchInput()
	{
		throw new NotImplementedException();
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

	public virtual void BluetoothServer()
	{
		throw new NotImplementedException();
	}

	public virtual void BluetoothClient()
	{
		throw new NotImplementedException();
	}
	
	public virtual void BluetoothBroadcast(string json) {
		throw new NotImplementedException();
	}

	public virtual string[] BluetoothPeers() {
		throw new NotImplementedException();
	}
	
	public virtual bool ConnectSocket() {
		throw new NotImplementedException();
	}

	public virtual bool DisconnectSocket() {
		throw new NotImplementedException();
	}

	public virtual bool MessageUser(int userId, string message) {
		throw new NotImplementedException();
	}

	public virtual bool MessageGroup(int groupId, string message) {
		throw new NotImplementedException();
	}

	public virtual bool CreateGroup() {
		throw new NotImplementedException();
	}

	public virtual bool JoinGroup(int groupId) {
		throw new NotImplementedException();
	}

	public virtual bool LeaveGroup(int groupId) {
		throw new NotImplementedException();
	}

	public void OnApplicationFocus(bool paused) {
		if (initialised && paused && OnGlass()) {
			Application.Quit();
		}
	}

    public string GetIntent()
    {
        return intent;
    }
	
	public virtual Device DeviceInformation() 
	{
		throw new NotImplementedException();
	}	
	
	public void OnApplicationQuit ()
	{
		if (db != null) {
			DatabaseFactory.CloseDatabase();
		}
		Destroy(gameObject);
	}	
}