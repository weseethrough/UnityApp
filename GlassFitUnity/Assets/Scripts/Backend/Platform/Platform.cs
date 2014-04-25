using UnityEngine;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using System;
using System.Runtime.CompilerServices;
using SimpleJSON;
using RaceYourself;
using SiaqodbUtils;
using Sqo;
using RaceYourself.Models;
using Newtonsoft.Json;
using System.Collections;

public abstract class Platform : SingletonBase
{
	protected double targetElapsedDistance = 0;

    // Services that probably don't need platform-specific overrides
	protected PlayerOrientation playerOrientation = new PlayerOrientation();
    public abstract PlayerPoints PlayerPoints { get; }  // Helper class for accessing/awarding points

    // Services that need platform-specific implementations
    public abstract PlayerPosition LocalPlayerPosition { get; }  // Holds the local player's position and bearing
    public abstract BleController BleController { get; }  // Interface to BLE devices

    // Listeners for unity messages, attached to Platform game object in GetMonoBehavioursPartner
    public PositionMessageListener _positionMessageListener;
    public PositionMessageListener PositionMessageListener { get { return _positionMessageListener; } }
    public NetworkMessageListener _networkMessageListener;
    public NetworkMessageListener NetworkMessageListener { get { return _networkMessageListener; } }
    public BluetoothMessageListener _bluetoothMessageListener;
    public BluetoothMessageListener BluetoothMessageListener { get { return _bluetoothMessageListener; } }
    public BleMessageListener _bleMessageListener;
    public BleMessageListener BleMessageListener { get { return _bleMessageListener; } }

    // internal platform tools
    private PlatformPartner partner;  // MonoBehavior that passes unity calls through to platform
    protected static Log log = new Log("Platform");  // for use by subclasses
    protected Siaqodb db;
    public API api;

    // internal platform state
    protected static bool applicationIsQuitting = false;
    protected bool initialised = false;
    private int sessionId = 0;
    public bool connected { get; protected set; }
    public int syncInterval = 10;  // Other components may change this to disable sync temporarily?
    public DateTime lastSync = new DateTime(0);

    // TODO: fields that almost certainly want removing
    protected float yaw = -999.0f;
    protected bool started = false;
	public int currentTrack { get; set; }
	public float[] sensoriaSockPressure { get; protected set;}
	protected List<Track> trackList;
	protected List<Game> gameList;
	public List<TargetTracker> targetTrackers { get; protected set; }
	


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
   
	public virtual void OnDestroy() {
		log.info("OnDestroy");
		applicationIsQuitting = true;
	}
	
	public override void Awake()
    {
        base.Awake();

		if (initialised == false)
        {
            Initialize();
        }

        log.info("awake, ensuring attachment to Platform game object for MonoBehaviours support");        
    }

	protected virtual void Initialize()
	{
		connected = false;
	    targetTrackers = new List<TargetTracker>();	                
		// Set initialised=true in overriden method
	}
	
	protected virtual void PostInit()
    {
        log.info("Starting PostInit");
        
        if (Application.isPlaying) {
            db = DatabaseFactory.GetInstance();
            api = new API(db);
            sessionId = Sequences.Instance.Next("session", db);
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
		
		FB.Init(OnInitComplete, OnHideUnity);
		
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

    /// <summary>
    /// Called every frame by PlatformPartner to update internal state
    /// </summary>
    public virtual void Update() {
        // nothing, but overridden in subclasses to update orientation
    }   

    /// <summary>
    /// Called every frame DURING A RACE by RaceGame to update position, speed etc
    /// Not called outside races to save battery life
    /// </summary>
    public virtual void Poll() {
        LocalPlayerPosition.Update();  // update current position
        PlayerPoints.Update(); // update current points
    }

    public void OnApplicationFocus(bool paused) {
        if (initialised && paused && OnGlass()) {
            Application.Quit();
        }
    }

    public void OnApplicationQuit ()
    {
        if (db != null) {
            DatabaseFactory.CloseDatabase();
        }
        //      Destroy(gameObject);
    }

    public void SetMonoBehavioursPartner(PlatformPartner obj)
    {
        if (partner == null)
        {
            //named object to identify platform game object reprezentation
            //GameObject go = new GameObject("Platform");
            //partner = go.AddComponent<PlatformPartner>();
            _positionMessageListener = obj.gameObject.AddComponent<PositionMessageListener>();  // listenes for NewTrack and NewPosition messages from Java
            _networkMessageListener = obj.gameObject.AddComponent<NetworkMessageListener>();  // listenes for NewTrack and NewPosition messages from Java
            _bluetoothMessageListener = obj.gameObject.AddComponent<BluetoothMessageListener>();  // listenes for NewTrack and NewPosition messages from Java

            //post initialziation procedure
            partner = obj;
            PostInit();
        }
    }

    public PlatformPartner GetMonoBehavioursPartner()
    {
        if (partner == null)
        {
            log.warning("Partner is not set yet. scene not constructed, you cant refer scene objects. Do you try to do so from another thread?");
        }

        return partner;
    }

    //***** Convenience methods that mostly return values from the database  ****

    // Get the device's orientation
    public virtual PlayerOrientation GetPlayerOrientation() {
        return playerOrientation;
    }

    // TODO: kill this method, only used by training camera
    public virtual bool HasStarted()
    {
		return started;
	}

    public virtual Device Device()
    {
        return db.Cast<Device>().Where(d => d.self == true).FirstOrDefault();
    }

    public virtual User User()
    {
        if (api == null) return null;
        return api.user;
    }
    
    public virtual PlayerConfig GetPlayerConfig()
    {
        // TODO put in synchronise call.
        PlayerConfig cfg = null;
        IEnumerator e = api.get("configurations/unity", (body) => {
            cfg = JsonConvert.DeserializeObject<RaceYourself.API.SingleResponse<RaceYourself.Models.PlayerConfig>>(body).response;
        });
        while(e.MoveNext()) {}; // block until finished
        return cfg;
    }

    public virtual User GetUser(int userId)
    {
        User user = null;
        IEnumerator e = api.get("users/" + userId, (body) => {
            user = JsonConvert.DeserializeObject<RaceYourself.API.SingleResponse<RaceYourself.Models.User>>(body).response;
        });
        while(e.MoveNext()) {}; // block until finished
        return user;
    }

    public virtual List<Track> GetTracks()
    {
        // TODO: Change signature to IList<Track>
        var tracks = new List<Track>(db.LoadAll<Track>());
        foreach (var track in tracks) {
            IncludePositions(track);
        }
        return tracks;
    }

    public virtual List<Track> GetTracks (double distance, double minDistance)
    {
        var tracks = new List<Track>(db.Cast<Track>().Where<Track>(t => t.distance > minDistance && t.distance <= distance));        
        foreach (var track in tracks) {
            IncludePositions(track);
        }
        return tracks;
    }

    private void IncludePositions(Track track)
    {
        if (track == null) return;
        if (track.positions != null) return;
        track.positions = new List<Position>(db.Cast<Position>().Where<Position>(p => p.deviceId == track.deviceId && p.trackId == track.trackId));
    }

    /// <summary>
    /// Load a list of games from the java database, together with lock state, cost, description etc.
    /// Typically used when building the main hex menu
    /// </summary>
    public virtual List<Game> GetGames() {
        // TODO: Change signature to IList<Game>
        var games = new List<Game>(db.LoadAll<Game>());
        return games;
    }

    public virtual void ResetGames() 
    {
        throw new NotImplementedException();
    }

    public virtual void QueueAction(string json) {
        var action = new RaceYourself.Models.Action(json);
        db.StoreObject(action);
    }

    public virtual List<Friend> Friends() {
        // TODO: Change signature to IList<Friend>
        return new List<Friend>(db.LoadAll<Friend>());
    }

    public virtual Notification[] Notifications () {
        // TODO: Change signature to IList<Notification>
        var list = db.LoadAll<Notification>();
        var array = new Notification[list.Count];
        list.CopyTo(array, 0);
        return array;
    }

    public virtual void ReadNotification (string id) {
        var notifications = Notifications();
        foreach (Notification note in notifications) {
            if (note.id == id) {
                note.read = true;
            }
        }
    }

    /// <summary>
    /// Use this method to record events for analytics, e.g. a user action.
    /// </summary>
    /// <param name='json'>
    /// Json-encoded event values such as current game state, what the user action was etc
    /// </param>
    public virtual void LogAnalytics (string json) {
        var e = new RaceYourself.Models.Event(json, sessionId);
        db.StoreObject(e);
    }

    public virtual Challenge FetchChallenge(string id) {
        Challenge challenge = null;
        IEnumerator e = api.get("challenges/" + id, (body) => {
            challenge = JsonConvert.DeserializeObject<RaceYourself.API.SingleResponse<RaceYourself.Models.Challenge>>(body).response;
        });
        while(e.MoveNext()) {}; // block until finished
        return challenge;
    }

    public virtual Track FetchTrack(int deviceId, int trackId) {
        // Check db
        Track track = db.Cast<Track>().Where<Track>(t => t.deviceId == deviceId && t.trackId == trackId).FirstOrDefault();
        if (track != null) {
            IncludePositions(track);
            return track;
        }
        // or fetch from API
        IEnumerator e = api.get("tracks/" + deviceId + "-" + trackId, (body) => {
            track = JsonConvert.DeserializeObject<RaceYourself.API.SingleResponse<RaceYourself.Models.Track>>(body).response;
        });
        while(e.MoveNext()) {}; // block until finished
        return track;
    }

    // *** Networking methods ***

    public virtual void Authorize(string provider, string permissions) {
        if (Application.isPlaying) {
			switch(provider) {
			case "facebook": {
				FBLogin(permissions);
				return;
			}
			case "twitter": {
				NetworkMessageListener.OnAuthentication("Failure");
                throw new NotImplementedException("Implement Twitter authorization in native platform or webview");
				return;
			}
			case "google+": {
				NetworkMessageListener.OnAuthentication("Failure");
				throw new NotImplementedException("Implement Google+ authorization in native platform");
                return;
            }
			case "any":
			default: {
	            // TODO should this be in PlatformDummy? If on device, email/password should come from OS, no? 
				//      Yes
	            GetMonoBehavioursPartner().StartCoroutine(api.Login("raceyourself@mailinator.com", "exerciseIsChanging123!"));
	            //GetMonoBehavioursPartner().StartCoroutine(api.Login("ry.beta@mailinator.com", "b3tab3ta"));
				return;
			}
			}
        } else {
			NetworkMessageListener.OnAuthentication("Failure");
        }
    }

    public virtual bool HasPermissions(string provider, string permissions) {
        return NetworkMessageListener.authenticated;
    }

    public virtual void SyncToServer() {
        lastSync = DateTime.Now;
        GetMonoBehavioursPartner().StartCoroutine(api.Sync());
    }

    // *** Methods that need platform-specific overrides ***
	
    public abstract bool OnGlass ();

    public abstract bool IsRemoteDisplay ();

    public abstract bool IsPluggedIn ();
	
    public abstract bool HasInternet ();
	
    public abstract bool HasWifi ();
	
    public abstract bool IsDisplayRemote ();
	
    public abstract bool HasGpsProvider ();

    public abstract bool IsBluetoothBonded ();
	
    public abstract byte[] LoadBlob (string id);
	
    public abstract void StoreBlob (string id, byte[] blob);

    public abstract void EraseBlob (string id);

    public abstract void ResetBlobs ();

    // Returns the int number of fingers touching glass's trackpad
    public abstract int GetTouchCount ();

    // Returns (x,y) as floats between 0 and 1
    public abstract Vector2? GetTouchInput ();

    public abstract float Yaw ();

    public abstract void BluetoothServer ();

    public abstract void BluetoothClient ();

    public abstract void BluetoothBroadcast (string json);

    public abstract string[] BluetoothPeers ();

    public abstract Device DeviceInformation ();




    // *** Target tracking - to be refactored out of platform ***
	
	// Return the distance behind target
	public virtual double DistanceBehindTarget(TargetTracker tracker) {
		double returnDistance = (tracker.GetTargetDistance() - LocalPlayerPosition.Distance);
		return returnDistance;
	}
	
	public virtual double DistanceBehindTarget() {
		return GetLowestDistBehind();
	}

    public virtual float GetHighestDistBehind() {
        throw new NotImplementedException();
    }   

    public virtual float GetLowestDistBehind() {
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
        
    /// Only used by Eagle? Will be better done by a dummy targetController.
    public virtual float GetTargetSpeed() {
        return 0.0f;
    }
	
	
	






    // *** Networking methods, currently overridden for android but not other platforms ***
	
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

	// Facebook methods
	private void OnInitComplete()
	{
		log.info("Facebook: FB.Init completed: Is user logged in? " + FB.IsLoggedIn);
		if (FB.IsLoggedIn) {
			log.info("Facebook: Logged in as " + FB.UserId + " " + FB.AccessToken);
			FB.API("/me", Facebook.HttpMethod.GET, FacebookMeCallback);
		}
    }
	
	private void OnHideUnity(bool isGameShown)
	{
		log.info("Facebook: Is game showing? " + isGameShown);
    }
    
	private void FBLogin(string permissions)
	{
		string scope = ""; // Default/"" = login only
		// TODO: Convert our permissions to Facebook permissions
		FB.Login(scope, FacebookLoginCallback);
	}
	
	private void FacebookLoginCallback(FBResult result)
	{
		if (result.Error != null) {
			log.error("Facebook: Error Response:\n" + result.Error);
			NetworkMessageListener.OnAuthentication("Failure");
        }
		else if (!FB.IsLoggedIn)
		{
			log.info("Facebook: Login cancelled by Player");
			NetworkMessageListener.OnAuthentication("Failure");
		}
		else
		{
			log.info("Facebook: Login was successful! " + FB.UserId + " " + FB.AccessToken);
			NetworkMessageListener.OnAuthentication("Success");
		}
	}

	private void FacebookMeCallback(FBResult result) {
		if (result.Error == null) {
			FacebookMe me = JsonConvert.DeserializeObject<FacebookMe>(result.Text);
			log.info("Facebook me: " + JsonConvert.SerializeObject(me));
		}
	}    
}