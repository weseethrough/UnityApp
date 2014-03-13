using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using System.Diagnostics;
using System.IO;
using RaceYourself.Models;
using Newtonsoft.Json;
using Sqo;
using SiaqodbDemo;
using RaceYourself;

#if (UNITY_EDITOR || RACEYOURSELF_MOBILE)
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
[ExecuteInEditMode()] 
#endif
public class PlatformDummy : Platform
{
	const string STARTHEX_SCENE_NAME = "Assets/Scenes/Start Hex.unity";
	const string SNACKRUN_SCENE_NAME = "Assets/Scenes/SnackRun.unity";


	// Helper class for accessing the player's current position, speed and direction of movement
	private PlayerPosition _localPlayerPosition;
    public override PlayerPosition LocalPlayerPosition {
        get { return _localPlayerPosition; }
    }

	// Helper class for accessing/awarding points
	private PlayerPoints _playerPoints;
	public override PlayerPoints PlayerPoints { get { return _playerPoints; } }

	private Stopwatch timer = new Stopwatch();
	private System.Random random = new System.Random();
	private long update = 0;
//	private float distance = 0;
	private float target = 1;
	private float targetSpeed = 4;
//	private Position position = null;
//	private float bearing = 45; // degrees	
	
	private const float weight = 75.0f;
	private const float factor = 1.2f;
		
//	private const float speedIncremet = 0.5f;
	
	private string blobstore = "game-blob";
	private string blobassets = "blob";

    public float[] sensoriaSockPressure = { 0.0f, 0.2f, 0.4f, 0.6f, 0.8f, 1.0f };
	
	private PlayerOrientation playerOrientation = new PlayerOrientation();
	private float oriYaw = 0.0f;
	private float oriPitch = 0.0f;
	const float lookSensitivity = 10.0f;
	
	private static PlatformDummy _instance;
	
	private int sessionId = 0;
	
//	private bool initialised = false;	
	
	/*
	public static PlatformDummy Instance {
		get 
        {
            if (_instance == null)
            {
                PlatformDummy[] pDummies = FindObjectsOfType(typeof(PlatformDummy)) as PlatformDummy[];
                int count = pDummies.Length;
                if (count >= 1)
                {
                    if (count > 1)
                    {
                        UnityEngine.Debug.Log("Singleton: there is more than one singleton");
                    }

                    for (int i = 1; i < count; i++)
                    {
                        GameObject plat = GameObject.Find("PlatformDummy");
                        Destroy(plat);
                    }

                    _instance = FindObjectOfType(typeof(PlatformDummy)) as PlatformDummy;
                }

                if (_instance == null)
                {
                    GameObject singleton = new GameObject();
                    _instance = singleton.AddComponent<PlatformDummy>();
                    singleton.name = "PlatformDummy"; // Used as target for messages

                    DontDestroyOnLoad(singleton);
                }                
            }
			
			if (_instance != null)
			{
				if (_instance.initialised == false)
				{
					_instance.Initialize();
				}
			}
            return _instance;
		}
	}
	*/
	private static bool applicationIsQuitting = false;

    public override bool OnGlass()
    {
        return true;
    }
	public override bool IsRemoteDisplay()
	{
		return true;
	}
    public override bool IsPluggedIn() {
		return false;
	}	
	public override bool HasInternet() {
		return true;
	}	
	public override bool HasWifi() {
		return false;
	}	
	public override bool IsDisplayRemote() {
		return false;
	}	
	public override bool HasGpsProvider() {
		return false;
	}
	
	public void OnDestroy() 
	{
		applicationIsQuitting = true;
	}
	
    public override bool IsBluetoothBonded()
    {
        return false;
    }
				
	
#if UNITY_EDITOR
	[MenuItem("Race Yourself/Play from StartHex Scene, with flow at Start %0")]
	public static void PlayFromStartHex()
    {
		PlayWithSceneFlowExit(STARTHEX_SCENE_NAME, "Start Point");
	}

	[MenuItem("Race Yourself/Play from current Scene, with flow at Game Intro %[")]
	public static void PlayFromCurrentGameScene()
	{
		PlayWithSceneFlowExit(null, "Game Intro");
	}

	[MenuItem("Race Yourself/Play from SnackRun Scene, with flow at Game Intro %]")]
	public static void PlayFromSnackRunscene()
	{
		PlayWithSceneFlowExit(SNACKRUN_SCENE_NAME, "Game Intro");
	}
	
	protected static void PlayWithSceneFlowExit(string scene, string exit)
	{
		//set the string for the exit we want to follow from the start node
		PlayerPrefs.SetString("playerStartExit", exit);
		//load scene, then play
		if(scene != null)
		{
			EditorApplication.SaveCurrentSceneIfUserWantsTo();
			EditorApplication.OpenScene(scene);
		}
		//play
		EditorApplication.isPlaying = true;

		//initialise objects for flow, datastorage, UIScene etc, if this isn't startHex
		if(scene != STARTHEX_SCENE_NAME)
		{
			InitForPreview playModePreparer = (InitForPreview)GameObject.FindObjectOfType(typeof(InitForPreview));
			if(playModePreparer != null)
			{
				playModePreparer.PrepareForPlayMode();
			}
			else
			{
				UnityEngine.Debug.LogError("PlatformDummy: Unable to initialise correctly for Play mode in editor. Ensure that a correctly configured InitForPreview component is present in the scene");
			}
		}

	}
#endif
	
	protected override void Initialize()
	{
		_localPlayerPosition = new EditorPlayerPosition();
		_playerPoints = new EditorPlayerPoints();
		try {
			initialised = false;
//		FlowState fs = FlowStateMachine.GetCurrentFlowState();
//		if(fs == null)
//		{
//			UnityEngine.Debug.Log("PlatformDummy initialise: Couldn't find flowstate, loading startHex scene");
//			//load the start hex scene
//			//Application.LoadLevel("StartHex");
//			EditorApplication.OpenScene("Assets/Scenes/Start Hex.unity");
//
//		}
//		else
//		{
//			UnityEngine.Debug.Log("PlatformDummy initialise: flowstate exists");
//		}
		
			//timer.Start();
	
			UnityEngine.Debug.Log("Creating Platform Dummy instance");
			
			blobstore = Path.Combine(Application.persistentDataPath, blobstore);
			blobassets = Path.Combine(Application.streamingAssetsPath, blobassets);
			var tag = "Player";
			if (!Application.isPlaying) {
				// Save to blob assets in editor
				blobstore = blobassets;
				tag = "Editor";
			}
			Directory.CreateDirectory(blobstore);
			UnityEngine.Debug.Log(tag + " blobstore: " + blobstore);
			if (Application.isEditor) Directory.CreateDirectory(blobassets);
			UnityEngine.Debug.Log(tag + " blobassets: " + blobassets);
				
		
			if (Application.isPlaying) {
				db = DatabaseFactory.GetInstance();
				api = new API(db);
				sessionId = Sequence.Next("session", db);
				MergeGamesList();
			}
			
			if (!initialised) {
				playerOrientation.Update(Quaternion.FromToRotation(Vector3.down,Vector3.forward));
		
				initialised = true;
			} else {
				UnityEngine.Debug.Log("Race condition in PlatformDummy!");
			}
	    } catch (Exception e) {
            UnityEngine.Debug.LogWarning("Platform: Error in constructor " + e.Message);
            UnityEngine.Debug.LogException(e);
			Application.Quit();
	    }
	}
	
	protected override void PostInit() {
		base.PostInit();
	}
	
	private void MergeGamesList() {
		var defaultGames = new List<Game>();
		defaultGames.Add(new Game("activity_monster",		"Giant Monster Challenge",	"activity_monster",		"run",	"You have woken up a giant monster - and he's hungry",	"Locked",	3,5000,4,	"N/A",		-2,0,	"Race Mode"));
		defaultGames.Add(new Game("activity_press_up",		"Press-ups",				"activity_press_up",	"all",	"Learn the proper technique for press ups.",			"Locked",	3,5000,4,	"N/A",		-2,1,	"Race Mode"));
		defaultGames.Add(new Game("activity_train",			"The train game",			"activity_train",		"all",	"There's a damsel in distress on the tracks - save her!","Locked",	0,10000,5,	"Snack",	-2,-1,	"TrainSnack"));
		defaultGames.Add(new Game("activity_bike",			"Race Yourself",			"activity_bike",		"cycle","Cycle against your own avatar for points",				"Locked",	0,500,0,	"N/A",		-1,-1,	"Race Mode"));
		defaultGames.Add(new Game("activity_boulder",		"Boulder Dash",				"activity_boulder",		"run",	"Run away from the boulder!",							"Locked",	1,1000,1,	"Snack",	-1,0,	"BoulderSnack"));
		defaultGames.Add(new Game("activity_versus",		"Challenges",				"activity_versus",		"all",	"Race against your friends!",							"Unlocked",	1,5000,3,	"Challenge",-1,1,	"Race Mode"));
		defaultGames.Add(new Game("activity_race_yourself",	"Race Yourself",			"activity_run",			"run",	"Race against your own avatar",							"Unlocked",	0,0,0,		"Race",		0,-1,	"Race Mode"));
		defaultGames.Add(new Game("activity_achievement",	"Achievements",				"activity_achievement",	"run",	"View your achievements and progress",					"Locked",	2,5000,3,	"N/A",		0,1,	"Race Mode"));
		defaultGames.Add(new Game("activity_bolt_level1",	"Beat Bolt",				"activity_bolt_level1",	"run",	"Try to beat Bolt's 100m time",							"Locked",	2,5000,3,	"Snack",	1,1,	"UsainSnack"));
		defaultGames.Add(new Game("activity_zombie",		"Zombie mode",				"activity_zombie",		"all",	"How long can you survive against zombies?",			"Locked",	1,0,0,		"Snack",	1,-1,	"ZombieSnack"));
		defaultGames.Add(new Game("activity_heart",			"Heart-rate monitor",		"activity_heart",		"all",	"Connect to a heart-rate monitor",						"Locked",	3,5000,4,	"N/A",		2,0,	"Race Mode"));
		defaultGames.Add(new Game("activity_food_burn",		"Snack Run",				"activity_food_burn",	"all",	"Go on a fun run packed with mini games!",				"Unlocked",	1,0,0,		"Race",		1,0,	"SnackRun"));	
		
		var games = GetGames();
		// Add default games
		foreach (var game in defaultGames) {
			if (!games.Exists(g => g.gameId == game.gameId)) {
				games.Add(game);
				db.StoreObject(game);
			}
		}
		// Remove non-default games
		foreach (var game in games) {
			if (!defaultGames.Exists(g => g.gameId == game.gameId)) {
				games.Remove(game);
				db.Delete(game);
			}
		}
	}
	
    public override Device Device()
    {
		return db.Cast<Device>().Where(d => d.self == true).FirstOrDefault();
    }

	
	public override void ResetTargets() {
		//Nothing to do?
		return;	
	}
	
	public override TargetTracker CreateTargetTracker(float constantSpeed){
		//don't want to create any target trackers in dummy.
		return null;
	}
	
	public override TargetTracker CreateTargetTracker(int deviceId, int trackId){
		//don't want to create any target trackers in dummy.
		return null;	
	}

	public override void Authorize(string provider, string permissions) {
		StartCoroutine(api.Login("janne.husberg@gmail.com", "testing123"));
	}
	
	public override bool HasPermissions(string provider, string permissions) {
		return authenticated;
	}
	
	public override void SyncToServer() {
		lastSync = DateTime.Now;
		StartCoroutine(api.Sync());
	}
	
	public void SetTargetSpeed(float speed)
	{
		throw new NotImplementedException();
	}
	
	public void SetTargetTrack(int trackID)
	{
		throw new NotImplementedException();
	}
	
	public override PlayerOrientation GetPlayerOrientation() {
		return playerOrientation;
	}
	
	public override Challenge FetchChallenge(string id) {
		Challenge challenge = null;
		IEnumerator e = api.get("challenges/" + id, (body) => {
			challenge = JsonConvert.DeserializeObject<RaceYourself.API.SingleResponse<RaceYourself.Models.Challenge>>(body).response;
		});
		while(e.MoveNext()) {}; // block until finished
		return challenge;
	}

	public override Track FetchTrack(int deviceId, int trackId) {
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
		
//	public void SetTargetSpeed(float speed)
//	{
//		throw new NotImplementedException();
//	}
//	
//	public void SetTargetTrack(int trackID)
//	{
//		throw new NotImplementedException();
//	}
		
	public override List<Track> GetTracks() {
		// TODO: Change signature to IList<Track>
		var tracks = new List<Track>(db.LoadAll<Track>());
		foreach (var track in tracks) {
			IncludePositions(track);
		}
		return tracks;
	}
	
	public override List<Game> GetGames() {
		// TODO: Change signature to IList<Game>
		var games = new List<Game>(db.LoadAll<Game>());
		return games;
	}
	
	public override void QueueAction(string json) {
		var action = new RaceYourself.Models.Action(json);
		db.StoreObject(action);
	}
	
	public override List<Friend> Friends() {
		// TODO: Change signature to IList<Friend>
		return new List<Friend>(db.LoadAll<Friend>());
	}

	public override Notification[] Notifications () {
		// TODO: Change signature to IList<Notification>
		var list = db.LoadAll<Notification>();
		var array = new Notification[list.Count];
		list.CopyTo(array, 0);
		return array;
	}
	
	public override void ReadNotification(string id) {
		throw new NotImplementedException();
	}
	
	public override byte[] LoadBlob(string id) {
		try {
			UnityEngine.Debug.Log("PlatformDummy: Loading blob id: " + id);			
			return File.ReadAllBytes(Path.Combine(blobstore, id));			
		} catch (FileNotFoundException e) {
			return LoadDefaultBlob(id);
		}
	}

	public byte[] LoadDefaultBlob(string id) {
		try {
			UnityEngine.Debug.Log("PlatformDummy: Loading default blob id: " + id);
			if (blobassets.Contains("://")) {
				var www = new WWW(Path.Combine(blobassets, id));
				while(!www.isDone) {}; // block until finished
				return www.bytes;
			} else {
				return File.ReadAllBytes(Path.Combine(blobassets, id));			
			}
		} catch (FileNotFoundException e) {
			return new byte[0];
		}
	}

    public override void StoreBlob(string id, byte[] blob)
    {
        File.WriteAllBytes(Path.Combine(blobstore, id), blob);
		UnityEngine.Debug.Log("PlatformDummy: Stored blob id: " + id);
    }

	public override void ResetBlobs ()
	{
		//Not entirely sure what this is supposed to do. Wil do nothing for now. AH
		return;
	}
	
	public override void Poll() {

		LocalPlayerPosition.Update();

//		target += targetSpeed * UnityEngine.Time.deltaTime;
//		if (random.Next() % 5 == 0) target += 1 * UnityEngine.Time.deltaTime;
//		if (random.Next() % 5 == 4) {
//				target -= 1 * UnityEngine.Time.deltaTime;
//		}

	}
	
	public override User User() {
		if (api == null) return null;
		return api.user;
	}
	
	public override User GetUser(int userId) {
		User user = null;
		IEnumerator e = api.get("users/" + userId, (body) => {
			user = JsonConvert.DeserializeObject<RaceYourself.API.SingleResponse<RaceYourself.Models.User>>(body).response;
		});
		while(e.MoveNext()) {}; // block until finished
		return user;
	}
	
	public override List<Track> GetTracks (double distance, double minDistance)
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
		
	//specific to the platform dummy (ideally this would be provided by a TargetTracker dummy object)
	public override float GetTargetSpeed() {
		return targetSpeed;
	}
	
	public override float GetHighestDistBehind ()
	{
		return (float)DistanceBehindTarget();
	}
	
	public override float GetLowestDistBehind ()
	{
		return (float)DistanceBehindTarget();
	}
	
	public override double DistanceBehindTarget() {
		return target - LocalPlayerPosition.Distance;
	}

	
	public override void BluetoothClient ()
	{
	}
	
	public override void BluetoothServer ()
	{
	}
	
	public override string[] BluetoothPeers ()
	{
		return new string[0];
	}
	
	public override void BluetoothBroadcast (JSONObject json)
	{
		return;
	}
	
	public override void LogAnalytics (JSONObject json) {
		var e = new RaceYourself.Models.Event(json.ToString(), sessionId);
		db.StoreObject(e);
	}
	
	public override void Update ()
	{	
		base.Update();		
		if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
		{
			//check for input and update player orientation as appropriate
			float x = Input.GetAxis("Mouse X");
			float y = Input.GetAxis("Mouse Y");
			
			oriYaw -= x * lookSensitivity;
			oriPitch += y * lookSensitivity;
			
			//construct quaternion and update player ori
			Quaternion fromForward = Quaternion.Euler(oriPitch, oriYaw, 0.0f);
			Quaternion fromDown = Quaternion.FromToRotation(Vector3.down,Vector3.forward) * fromForward;
			
			playerOrientation.Update(fromDown);
		}
	}
	
	public override Vector2? GetTouchInput ()
	{
		if(GetTouchCount() > 0)
		{
			float x = Input.mousePosition.x / Screen.width;
			float y = Input.mousePosition.y / Screen.height;
			return new Vector2(x,y);
		}
		else return null;
	}
	
	public override int GetTouchCount ()
	{
		int touchCount = 0;
		//simulate multiple touchers by holding more than one modifier key
		if(Input.GetKey(KeyCode.LeftControl)) { touchCount++;}
		if(Input.GetKey(KeyCode.LeftCommand)) { touchCount++;}
		if(Input.GetKey(KeyCode.RightControl)) { touchCount++;}
		if(Input.GetKey(KeyCode.RightCommand)) { touchCount++;}
		return touchCount;
	}
	
	public override Device DeviceInformation() 
	{
		return new Device("Unknown", "Device");
	}	

}
#endif