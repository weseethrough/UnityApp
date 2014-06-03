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
using System.IO;

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
    private PositionMessageListener _positionMessageListener;
    public PositionMessageListener PositionMessageListener { get { return _positionMessageListener; } }
    private NetworkMessageListener _networkMessageListener;
    public NetworkMessageListener NetworkMessageListener { get { return _networkMessageListener; } }
    private BluetoothMessageListener _bluetoothMessageListener;
    public BluetoothMessageListener BluetoothMessageListener { get { return _bluetoothMessageListener; } }
    private BleMessageListener _bleMessageListener;
    public BleMessageListener BleMessageListener { get { return _bleMessageListener; } }
	private RemoteTextureManager _remoteTextureManager;
	public RemoteTextureManager RemoteTextureManager { get { return _remoteTextureManager; } }

    // internal platform tools
    public PlatformPartner partner;  // MonoBehavior that passes unity calls through to platform
    protected static Log log = new Log("Platform");  // for use by subclasses
    public Siaqodb db;
    public API api;

    // internal platform state
    protected static bool applicationIsQuitting = false;
    protected bool initialised = false;
    private int sessionId = 0;
    public bool connected { get; protected set; }
    public int syncInterval = 10;  // Other components may change this to disable sync temporarily?
	public DateTime lastSync = DateTime.Now;

    // TODO: fields that almost certainly want removing
    protected float yaw = -999.0f;
    protected bool started = false;
	public int currentTrack { get; set; }
	public float[] sensoriaSockPressure { get; protected set;}
	protected List<Track> trackList;
	protected List<Game> gameList;
	public List<TargetTracker> targetTrackers { get; protected set; }
	
	bool hasRegisteredUserForUXCam = false;

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
            return (Platform)GetInstance<MinimalAndroidPlatform>();
#elif UNITY_ANDROID
            return (Platform)GetInstance<MinimalAndroidPlatform>();
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

		Input.location.Start(10,1);

		// Set initialised=true in overriden method
	}
	
	protected virtual void PostInit()
    {
        log.info("Starting PostInit");
        

        if (Application.isPlaying) {
            db = DatabaseFactory.GetInstance();
            api = new API(db, GetMonoBehavioursPartner());
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

        log.info("Starting sync co-routine");
        GetMonoBehavioursPartner().StartCoroutine(SyncLoop());

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
		Hashtable eventProperties = new Hashtable();
		eventProperties.Add("event_name", "launch");
		Platform.Instance.LogAnalyticEvent(JsonConvert.SerializeObject(eventProperties));

		//tag the user for UXCam - will only do anything on iOS for now
		User user = User();
		if(user != null)
		{
			tagUserForUXCam(user.username, user.DisplayName);
			hasRegisteredUserForUXCam = true;
		}

        //GetMonoBehavioursPartner().StartCoroutine(api.Login("cats", "dogs"));
	}



    /// <summary>
    /// Called every frame by PlatformPartner to update internal state
    /// </summary>
    public virtual void Update() {
        // overridden in subclasses to update orientation

		// see if we've got the user yet. Consider moving this to a periodic check, not per frame.
		if(!hasRegisteredUserForUXCam)
		{
			User user = User();
			if(user != null)
			{
				tagUserForUXCam(user.username, user.DisplayName);
				hasRegisteredUserForUXCam = true;
			}
		}
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
            _bleMessageListener = obj.gameObject.AddComponent<BleMessageListener>();  // listenes for BLE messages - new devices, services and characteristics data, e.g. heart-rate or cadence
			_remoteTextureManager = obj.gameObject.AddComponent<RemoteTextureManager>();

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
    
	[Obsolete]
    public virtual PlayerConfig GetPlayerConfig()
    {
		log.error("GetPlayerConfig(): Illegal call to synchronous method. Need to use Asynchronous GetUser(int userId, Action<User> callback)");

        // TODO put in synchronise call.
        PlayerConfig cfg = null;
        IEnumerator e = api.get("configurations/unity", (body) => {
            cfg = JsonConvert.DeserializeObject<RaceYourself.API.SingleResponse<RaceYourself.Models.PlayerConfig>>(body).response;
        });
        while(e.MoveNext()) {}; // block until finished
        return cfg;
    }

	public virtual void GetUser(int userId, Action<User> callback)
	{
		User user = null;
		Platform.Instance.partner.StartCoroutine(api.get("users/" + userId, (body) => {
			user = JsonConvert.DeserializeObject<RaceYourself.API.SingleResponse<RaceYourself.Models.User>>(body).response;
			callback(user);
		}));
	}

	public virtual IEnumerator GetUserCoroutine(int userId, Action<User> callback)
	{
		User user = null;
		UnityEngine.Debug.Log("Platform Getting user in coroutine");
		Coroutine e = Platform.Instance.partner.StartCoroutine( api.get ("users/" + userId, (body) => {
			if(body == null) user = null;
			else {
				user = JsonConvert.DeserializeObject<RaceYourself.API.SingleResponse<RaceYourself.Models.User>>(body).response;
			}

		} ));
		yield return e;
		if(user != null)
		{
//			UnityEngine.Debug.Log("Platform got user " + userId + " from coroutine: " + user.DisplayName);
		}
		else
		{
			log.error("Couldn't get user: " + userId);
		}
		callback(user);
	}

	[Obsolete]
    public virtual User GetUser(int userId)
    {
		log.error("GetUser(): Illegal call to synchronous method. Need to use Asynchronous GetUser(int userId, Action<User> callback)");

		User user = null;
        IEnumerator e = api.get("users/" + userId, (body) => {
            user = JsonConvert.DeserializeObject<RaceYourself.API.SingleResponse<RaceYourself.Models.User>>(body).response;
        });
		while(e.MoveNext()) {}; // block until finished
        return user;
    }

	[Obsolete]
    public virtual List<Track> GetTracks()
    {
		log.error("GetTracks(): Illegal call to synchronous method. Need to use Asynchronous GetUser(int userId, Action<User> callback)");

        // TODO: Change signature to IList<Track>
		//	Actually, best not to, iOS doesn't deal well with generics.
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

    public virtual List<Notification> Notifications () {
        // TODO: Change signature to IList<Notification>
		var list = new List<Notification>(db.LoadAll<Notification>());
		//        var array = new Notification[list.Count];
//        list.CopyTo(array, 0);
        return list;
    }

    public virtual void ReadNotification (int id) {
        var notifications = Notifications();
        foreach (Notification note in notifications) {
            if (note.id == id) {
                note.read = true;
				note.dirty = true;
				db.UpdateObjectBy("id", note);
            }
        }
    }

    /// <summary>
    /// Use this method to record key events in the user's journey
    /// </summary>
    /// <param name='json'>
    /// Json-encoded event values. Currently supported:
        //    Launch app (event_name = "launch")
        //    Successfully signed up via facebook (event_name = "signup", provider = "facebook")
        //    Successfully signed up via email (event_name = "signup", provider = "facebook")
        //    Start race (event_name = "start_race", track_id = "xxx")
        //    End race (event_name = "end_race", result = "win/loss", track_id="xxx")
        //    Send challenge (event_name = "send_challenge", challenge_id = "xxx")
        //    Accept challenge (event_name = "accept_challenge", challenge_id = "xxx")
        //    Reject challenge (event_name = "reject_challenge", challenge_id = "xxx")
        //    Invite new user (event_name = "invite", invite_code = "xxx345x", provider = "facebook/email")
        //    Share (event_name = "share", provider = "facebook/twitter/google+")
        //    Rate (event_name = "rate", provider = "Apple store / Android store / Like on facebook")
    /// </param>
    public virtual void LogAnalyticEvent (string jsonString) {
        Hashtable jsonObject = JsonConvert.DeserializeObject<Hashtable>(jsonString);
        jsonObject.Add("event_type", "event");
		jsonString = JsonConvert.SerializeObject(jsonObject);
        log.warning("Analytic Event: " + jsonString);
        var e = new RaceYourself.Models.Event(jsonString, sessionId);
        db.StoreObject(e);
    }

    /// <summary>
    /// Use this method to record screen transitions so we can understand how users interact with the app
    /// </summary>
    /// <param name='json'>
    /// Json-encoded event values such as time on screen
    /// </param>
    public virtual void LogScreenView (string jsonString) {
        Hashtable jsonObject = JsonConvert.DeserializeObject<Hashtable>(jsonString);
        jsonObject.Add("event_type", "screen");
		jsonString = JsonConvert.SerializeObject(jsonObject);
        log.info("Screen View: " + jsonString);
        var e = new RaceYourself.Models.Event(jsonString, sessionId);
        db.StoreObject(e);
    }

    public virtual void FetchChallenge(int id, Action<Challenge> callback) {
        Challenge challenge = null;
        Platform.Instance.partner.StartCoroutine(api.get("challenges/" + id, (body) => {
            challenge = JsonConvert.DeserializeObject<RaceYourself.API.SingleResponse<RaceYourself.Models.Challenge>>(body).response;

			callback(challenge);
		}));
    }

	public virtual IEnumerator FetchChallengeCoroutine(int id, Action<Challenge> callback) {
		Challenge challenge = null;
		Coroutine e = Platform.Instance.partner.StartCoroutine(api.get("challenges/" + id, (body => {
			challenge = JsonConvert.DeserializeObject<RaceYourself.API.SingleResponse<RaceYourself.Models.Challenge>>(body).response;
		}) ));
		yield return e;
		callback(challenge);
	}

	public virtual IEnumerator FetchChallengeFromNotification(int id, Notification note, Action<Challenge, Notification> callback) {
		Challenge challenge = null;

		yield return Platform.Instance.partner.StartCoroutine(api.get("challenges/" + id, (body) => {

			if(body != null)
			{
				challenge = JsonConvert.DeserializeObject<RaceYourself.API.SingleResponse<RaceYourself.Models.Challenge>>(body).response;
			}

			callback(challenge, note);
		}));
	}
	
	public virtual IList<Challenge> Challenges() {
		IList<DistanceChallenge> distanceList = db.LoadAll<DistanceChallenge>();
		IList<DurationChallenge> durationList = db.LoadAll<DurationChallenge>();
		List<Challenge> allChallengeList = new List<Challenge>();
		if(distanceList != null) {
			for(int i=0; i<distanceList.Count; i++) {
				allChallengeList.Add(distanceList[i]);
			}
		}
		if(durationList != null) {
			for(int i=0; i<durationList.Count; i++) {
				allChallengeList.Add(durationList[i]);
			}
		}
		return allChallengeList;
	}

	public virtual void FetchTrack(int deviceId, int trackId, Action<Track> callback)
	{
		Track track = db.Cast<Track>().Where<Track>(t => t.deviceId == deviceId && t.trackId == trackId).FirstOrDefault();
		if (track != null) {
			IncludePositions(track);
			callback(track);
		}
		else
		{
			Platform.Instance.partner.StartCoroutine(api.get("tracks/" + deviceId + "-" + trackId, (body) => {
				track = JsonConvert.DeserializeObject<RaceYourself.API.SingleResponse<RaceYourself.Models.Track>>(body).response;
				callback(track);
			}));
		}
		// or fetch from API
//		log.error("Illegal call to synchronous method. Need to use Asynchronous GetUser(int userId, Action<User> callback)");


	}

	public virtual IEnumerator FetchTrackCoroutine(int deviceId, int trackId, Action<Track> callback)
	{
		Track track = null;
		Coroutine e = Platform.Instance.partner.StartCoroutine(api.get("tracks/" + deviceId + "-" + trackId, (body) => {
			track = JsonConvert.DeserializeObject<RaceYourself.API.SingleResponse<RaceYourself.Models.Track>>(body).response;
		}));
		yield return e;
		callback(track);
	}

	[Obsolete]
    public virtual Track FetchTrack(int deviceId, int trackId) {
        // Check db
        Track track = db.Cast<Track>().Where<Track>(t => t.deviceId == deviceId && t.trackId == trackId).FirstOrDefault();
        if (track != null) {
            IncludePositions(track);
            return track;
        }
        // or fetch from API
		log.error("Illegal call to synchronous method. Need to use Asynchronous GetUser(int userId, Action<User> callback)");

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
                // GetMonoBehavioursPartner().StartCoroutine(api.Login("raceyourself@mailinator.com", "exerciseIsChanging123!"));
                //GetMonoBehavioursPartner().StartCoroutine(api.Login("ry.beta@mailinator.com", "b3tab3ta"));
                if (NetworkMessageListener.authenticated) NetworkMessageListener.OnAuthentication("Success");
                else NetworkMessageListener.OnAuthentication("Failure");
                return;
            }
			}
		} else {
			NetworkMessageListener.OnAuthentication("Failure");
        }
    }

    public virtual bool HasPermissions(string provider, string permissions) {
		if (api == null || api.user == null) return false;
		var authentication = api.user.authentications.Find(auth => auth.provider == provider);
		if (authentication == null) return false;

		var perms = permissions.Split(',');
		foreach(var perm in perms) {
			var trimmed = perm.Trim();
			if (trimmed.Length == 0) continue;
			if (!authentication.permissions.Contains(trimmed)) return false;
		}
		return true;
    }

    public virtual void SyncToServer() {
        log.info("SyncToServer called");
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
        //No longer need this function
		//TODO remove
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
	
    /// <summary>
    // Does the platform provide a back button? Some of the possible scenarios:
    //
    // a. Android - Samsung Galaxy SII/S3/S4 - physical back button built into device.
    // b. Android - Nexus 5 - no physical back button, but OS provides one when not full-screen.
    // c. All iPhone models at time of writing - no physical back button and nothing provided by the OS by default.
    // Left to individual apps to provide this.
    //
    /// </summary>
    /// <returns><c>true</c>, in scenarios (a) and (b) above, <c>false</c> in scenario (c).</returns>
	public virtual bool ProvidesBackButton() {
        return true;
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
	}
	
	private void OnHideUnity(bool isGameShown)
	{
		log.info("Facebook: Is game showing? " + isGameShown);
	}
	
	private void FBLogin(string permissions)
	{
		string scope = "email"; // Default/"" = login only
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
			NetworkMessageListener.OnAuthentication("Cancelled");
		}
		else
		{
			log.info("Facebook: Login was successful! " + FB.UserId + " " + FB.AccessToken);
			if (NetworkMessageListener.authenticated) {
				GetMonoBehavioursPartner().StartCoroutine(api.LinkProvider(new ProviderToken("facebook", FB.AccessToken, FB.UserId), null));
				NetworkMessageListener.OnAuthentication("Success");
			} else {
				// OnAuthentication sent from coroutine
				GetMonoBehavioursPartner().StartCoroutine(api.Login(FB.UserId + "@facebook", FB.AccessToken));
			}
		}
	}
	
	private void FacebookMeCallback(FBResult result) {
        if (result.Error == null) {
            FacebookMe me = JsonConvert.DeserializeObject<FacebookMe> (result.Text);
			
        log.info("Facebook me: " + JsonConvert.SerializeObject(me));
		}
	}

    private IEnumerator SyncLoop()
    {
        while (true) {
            SyncToServer ();
            yield return new WaitForSeconds(60.0f);
        }
    }

	// UXCam methods
	public virtual void startUXCam()
	{
		//do nothing, except on iOS
		return;
	}

	public virtual void stopUXCam()
	{
		//do nothing, except on iOS
		return;
	}

	public virtual void tagScreenForUXCam(string tag)
	{
		//do nothing, except on iOS
		return;
	}

	public virtual void tagUserForUXCam(string tag, string additionalData)
	{
		//do nothing, except on iOS
		return;
	}

	public virtual byte[] ReadAssets(string filename) 
	{
		string assetspath = Application.streamingAssetsPath;
		if (assetspath.Contains("://")) {
			var www = new WWW(Path.Combine(assetspath, filename));
			while(!www.isDone) {}; // block until finished
			return www.bytes;
		} else {
			return File.ReadAllBytes(Path.Combine(assetspath, filename));			
		}
	}

	public string ReadAssetsString(string filename) 
	{
		return new System.Text.UTF8Encoding().GetString(ReadAssets(filename));
	}
}