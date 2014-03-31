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

public abstract class Platform : SingletonBase
{
	protected double targetElapsedDistance = 0;

	protected PlayerOrientation playerOrientation = new PlayerOrientation();

	// Holds the local player's position and bearing
	public abstract PlayerPosition LocalPlayerPosition { get; }

	// Helper class for accessing/awarding points
	public abstract PlayerPoints PlayerPoints { get; }

    // Listeners for unity messages, attached to Platform game object in GetMonoBehavioursPartner
    public PositionMessageListener _positionMessageListener;
    public PositionMessageListener PositionMessageListener { get { return _positionMessageListener; } }
    public NetworkMessageListener _networkMessageListener;
    public NetworkMessageListener NetworkMessageListener { get { return _networkMessageListener; } }
    public BluetoothMessageListener _bluetoothMessageListener;
    public BluetoothMessageListener BluetoothMessageListener { get { return _bluetoothMessageListener; } }


	protected float yaw = -999.0f;
	protected bool started = false;
	protected bool initialised = false;

	public int currentTrack { get; set; }
	public float[] sensoriaSockPressure { get; protected set;}
	
	protected List<Track> trackList;
	protected List<Game> gameList;
	
	public List<TargetTracker> targetTrackers { get; protected set; }
	
	public bool connected { get; protected set; } // ditto
	
	// Other components may change this to disable sync temporarily?
	public int syncInterval = 10;
    public DateTime lastSync = new DateTime(0);

	protected static Log log = new Log("Platform");  // for use by subclasses
	
    protected Siaqodb db;
    public API api;


    private PlatformPartner partner;

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
#if UNITY_EDITOR
            return (Platform)GetInstance<PlatformDummy>();
#elif UNITY_ANDROID && RACEYOURSELF_MOBILE
            return (Platform)GetInstance<AndroidPlatform>();
#elif UNITY_ANDROID
            return (Platform)GetInstance<AndroidPlatform>();
#elif UNITY_IPHONE
            return (Platform)GetInstance<IosPlatform>();
#endif
            return null;
        }
    }
   
	
	protected static bool applicationIsQuitting = false;
	
	public virtual void OnDestroy() {
		log.info("OnDestroy");
		applicationIsQuitting = true;
	}
	
	public override void Awake()
    {
        base.Awake();

		/*if (gameObject != null && gameObject.name != "New Game Object") 
		{
			log.error("Scene " + Application.loadedLevelName + " contains a platform gameobject called " + gameObject.name + ", quitting..");
//			DestroyImmediate(gameObject); // Doesn't always work, force developer to manually fix scene
			Application.Quit();
#if UNITY_EDITOR
    		UnityEditor.EditorApplication.isPlaying = false;
#endif			
			return;
		}*/
		if (initialised == false)
        {
            Initialize();
        }

        log.info("awake, ensuring attachment to Platform game object for MonoBehaviours support");
        GetMonoBehavioursPartner();
    }
        
	protected virtual void Initialize()
	{        	                
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
	
	public virtual Device DeviceInformation() 
	{
		throw new NotImplementedException();
	}	
	
	public void OnApplicationQuit ()
	{
		if (db != null) {
			DatabaseFactory.CloseDatabase();
		}
//		Destroy(gameObject);
	}

    public void SetMonoBehavioursPartner(PlatformPartner obj)
    {
        if (partner == null)
        {
            //named object to identify platform game object reprezentation
            GameObject go = new GameObject("Platform");
            partner = go.AddComponent<PlatformPartner>();
            _positionMessageListener = go.AddComponent<PositionMessageListener>();  // listenes for NewTrack and NewPosition messages from Java
            _networkMessageListener = go.AddComponent<NetworkMessageListener>();  // listenes for NewTrack and NewPosition messages from Java
            _bluetoothMessageListener = go.AddComponent<BluetoothMessageListener>();  // listenes for NewTrack and NewPosition messages from Java

            //post initialziation procedure
            partner = obj;
            PostInit();
        }
    }

    public PlatformPartner GetMonoBehavioursPartner()
    {
        if (partner == null)
        {
            UnityEngine.Debug.LogError("Partner is not set yet. scene not constructed, you cant refer scene objects. Do you try to do so from another thread?");
        }

        return partner;
    }
}