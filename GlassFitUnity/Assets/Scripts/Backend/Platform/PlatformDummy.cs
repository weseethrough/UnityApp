using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using System.Diagnostics;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;

[ExecuteInEditMode()] 
public class PlatformDummy : Platform
{
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
		
	private const float speedIncremet = 0.5f;

	private string blobassets = "blob";

    public float[] sensoriaSockPressure = { 0.0f, 0.2f, 0.4f, 0.6f, 0.8f, 1.0f };
	
	private PlayerOrientation playerOrientation = new PlayerOrientation();
	private float oriYaw = 0.0f;
	private float oriPitch = 0.0f;
	const float lookSensitivity = 10.0f;
	
	private static PlatformDummy _instance;

//	private bool initialised = false;	
	private List<Game> games;
	
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
        return false;
    }
	public override bool IsRemoteDisplay()
	{
		return true;
	}
	
	public void OnDestroy() 
	{
		applicationIsQuitting = true;
	}

    public override float GetDistance()
    {
        return (float)distance;
    }

    public override bool IsPluggedIn()
    {
        return false;
    }

	//if there is a platform dummy about on the device, kill it.
//#if !UNITY_EDITOR
//	void Awake()
//	{
//		Destroy(gameObject);			
//	}
//#endif
	
	[MenuItem("Race Yourself/Play from StartHex Scene, with flow at Start %0")]
	public static void PlayFromStartHex()
    {
		PlayWithSceneFlowExit("Assets/Scenes/Start Hex.unity", "Start Point");
	}

	[MenuItem("Race Yourself/Play from current Scene, with flow at Game Intro %[")]
	public static void PlayFromCurrentGameScene()
	{
		PlayWithSceneFlowExit(null, "Game Intro");
	}

	[MenuItem("Race Yourself/Play from SnackRun Scene, with flow at Game Intro %]")]
	public static void PlayFromSnackRunscene()
	{
		PlayWithSceneFlowExit("Assets/Scenes/SnackRun.unity", "Game Intro");
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
		//initialise objects for datastorage etc
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
	
	public override void Initialize()
	{
		//create list of target trackers, since this is a public member, an empty list is preferable to a null list.
		targetTrackers = new List<TargetTracker>();

		//take us to the start hex scene if appropriate
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
		
		//blobstore = Path.Combine(Application.persistentDataPath, blobstore);
		//Directory.CreateDirectory(blobstore);
		blobassets = Path.Combine(Application.streamingAssetsPath, blobassets);
		Directory.CreateDirectory(blobassets);
		UnityEngine.Debug.Log("Editor blobassets: " + blobassets);
		
		games = new List<Game>();
		
		Game current = new Game("Race Yourself (run)", "activity_run", "activity_run", "run", "Run against an avatar that follows your previous track","unlocked",1,0,0, "Race", 1, 0, "Race Mode");
		games.Add(current);
		
		//current = new Game("Switch to cycle mode (run)","activity_bike", "activity_bike", "run","Switch to cycle mode","locked",1,1000,0, "Race", 1, 0, "Race Mode");
        //games.Add(current);
		
		current = new Game("Zombies 1","activity_zombie", "activity_zombie","run","We all want to see if we could survive the zombie apocalypse, and now you can! Remember the #1 rule - cardio.","Unlocked",2,50000,0, "Snack", 0, -1, "ZombieSnack");
        games.Add(current);
		
		current = new Game("Boulder 1","activity_boulder", "activity_boulder", "run","Relive that classic moment in Indiana Jones, run from the boulder! No treasure this time though.","Unocked",1,10000,0, "Snack", -1, 0, "BoulderSnack");
        games.Add(current);
		
		current = new Game("Dinosaur 1","activity_dinosaurs", "activity_dinosaurs","run","Remember that time in Jurassic Park when the T-Rex ate those guys? Try to avoid the same fate!","locked",3,100000,0, "Pursuit", 0, 1, "Race Mode");
        games.Add(current);
		
		current = new Game("Eagle 1","activity_eagle", "activity_eagle","run","You stole her eggs, now the giant eagle is after you! It's not your fault the eggs are really tasty...","locked",2,70000,0, "Pursuit", -1, 1, "Race Mode");
        games.Add(current);
		
		current = new Game("Train 1","activity_train", "activity_train","run","Run away from a train!","Unlocked",2,20000,0, "Snack", 1, 1, "TrainSnack");
		games.Add(current);

		current = new Game("100m Sprint", "activity_bolt_level1", "ctivity_bolt_level1", "run", "RSprint for 100m against opponents in a stadium", "unlocked", 2, 70000, 0, "Celebrity", 2, 0, "Race Mode");
        games.Add(current);
		
		current = new Game("Paula Radcliffe","activity_paula_radcliffe", "activity_paula_radcliffe", "run","Run a marathon with Paula Radcliffe! Try and beat her time at the 2007 NYC Marathon!","unlocked",2,20000,0, "Celebrity", 2, 1, "Race Mode");
		games.Add(current);

        current = new Game("Chris Hoy", "activity_chris_hoy", "activity_chris_hoy","run", "Cycle with Chris Hoy, in his almost record breaking 1km cycle in 2007", "unlocked", 2, 10000, 0, "Celebrity", 2, -1, "Race Mode");
        games.Add(current);

        current = new Game("Bradley Wiggins", "activity_bradley_wiggins", "activity_bradley_wiggins","cycle", "Participate in a 4km pursuit race with Bradley Wiggins on his 2008 Olympics gold medal time", "unlocked", 2, 10000, 0, "Celebrity", 1, -1, "Race Mode");
        games.Add(current);

        current = new Game("Fire", "activity_fire", "activity_fire","run", "Know what's good on a barbeque? Burgers. Know what isn't? You. So run before you get burned.", "unlocked", 2, 10000, 0, "Pursuit", 1, 2, "Race Mode");
        games.Add(current);
		
		current = new Game("Settings", "settings", "settings", "run", "Settings for Indoor mode", "unlocked", 2, 0, 0, "Mode", -1, 2, "Race Mode");
		games.Add(current);
		
		//init player orientation
		playerOrientation.Reset();
		playerOrientation.Update(Quaternion.FromToRotation(Vector3.down,Vector3.forward));
		
		initialised = true;
		
	}
	
    public override Device Device()
    {
        return null;
    }

	public override void StartTrack() {
		UnityEngine.Debug.Log("PlatformDummy:StartTrack");
		timer.Start();
	}
	
	public override void SetIndoor(bool indoor) {
		//nothing to do in dummy.
		return;	
	}
	
	public override bool IsIndoor() {
		//always indoor in editor
		return true;
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

	public override Boolean HasLock() {
		//always report that we have gps lock in editor
		return true;
	}
	
	public override Track StopTrack() {
		timer.Stop();
		return null;
	}

	public override void Authorize(string provider, string permissions) {
		//ignore in dummy
		return;
	}
	
	public override bool HasPermissions(string provider, string permissions) {
		//assume always have permissions in dummy
		return true;
	}
	
	public override void SyncToServer() {
		//do nothing for dummy
		return;	
	}

	public override void Reset() {
		timer.Stop();
		timer.Reset();
		distance = 0;
		update = 0;
		target = 1;
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
		//don't need any challenges in the dummy
		return null;	
	}

	public override Track FetchTrack(int deviceID, int trackID) {
		//don't need any tracks in the dummy.
		return null;	
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
		//don't need any tracks in the dummy
		return null;
	}
	
	/// <summary>
	/// Not too sure how TempGames differs from Games. AH
	/// </summary>
	/// <returns>
	/// This function will simply return the games list for Platform Dummy, for now.
	/// </returns>
	public override List<Game> GetTempGames() {
		return games;	
	}
	
	public override List<Game> GetGames() {
		return games;
	}
	
	public override void QueueAction(string json) {
		//do nothing
		return;
	}
	
	public override List<Friend> Friends() {
		var friend = @"{
	        ""_id"": ""gplus107650962788507404146"",
	        ""has_glass"": false,
	        ""image"": ""https://lh6.googleusercontent.com/-c89V0_0E6tM/AAAAAAAAAAI/AAAAAAAAAFE/9oLaR0rjbog/photo.jpg?sz=50"",
	        ""name"": ""Aaron Aycock"",
	        ""photo"": null,
	        ""uid"": ""107650962788507404146"",
	        ""user_id"": null
	      }";
		List<Friend> friends = new List<Friend>(1);
		friends[0] = new Friend(friend);
		return friends;
	}

	public override Notification[] Notifications ()
	{
		return null;
	}
	
	public override void ReadNotification(string id) {
		return;	
	}
	
	public override byte[] LoadBlob(string id) {
		try {
			UnityEngine.Debug.Log("PlatformDummy: Trying id: " + id);
			
			return File.ReadAllBytes(Path.Combine(blobassets, id));
			
		} catch (FileNotFoundException e) {
			return new byte[0];
		}
	}

    public override void StoreBlob(string id, byte[] blob)
    {
        File.WriteAllBytes(Path.Combine(blobassets, id), blob);
    }

	public override void ResetBlobs ()
	{
		//Not entirely sure what this is supposed to do. Wil do nothing for now. AH
		return;
	}
	
	public override void Poll() {
		if (!timer.IsRunning) return;
		//if (Time() - update > 1000) { 

		//move forward
		distance += targetSpeed * UnityEngine.Time.deltaTime;


		update = Time();
		position = new Position((float)(51.400+Math.Cos(bearing*Math.PI/180)*distance/111229d), (float)(-0.15+Math.Sin(bearing*Math.PI/180)*distance/111229d));

	}
	
	public override User User() {
		return null;	
	}
	
	public override User GetUser(int userID) {
		return null;	
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
		return target - distance;
	}
	
	public override long Time() {
		return timer.ElapsedMilliseconds;
	}
	
	public override double Distance() {
		return distance;
	}
	
	public override int Calories() {
		return (int)(factor * weight * distance/1000);
	}
	
	public override float Pace() {
		return 1.0f;
	}
	
	public Position Position() {
		return position;
	}	
	
	public float Bearing() {
		return bearing;
	}

	public int GetCurrentGemBalance ()
	{
		//give lots of gems for testing in editor
		return 100;
	}
	
	public override float GetCurrentMetabolism ()
	{
		//return a default value
		return 1.0f;
	}
	
	public override void SetBasePointsSpeed (float speed)
	{
		//do nothing
		return;
	}
	
	public override void AwardPoints (string reason, string gameId, long points)
	{
		//do nothing
		return;
	}
	
	public override void AwardGems (string reason, string gameId, int gems)
	{
		//do nothing
		return;
	}

	
	public override void Update ()
	{
		//fake head movement
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

		//fake speed up/ slow down
		if(Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadPlus))
		{
			targetSpeed += speedIncremet;
		}
		if(Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
		{
			targetSpeed -= speedIncremet;
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

}
#endif