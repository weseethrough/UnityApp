using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class RaceGame : MonoBehaviour {
	
	// Bools for various states
	public bool menuOpen = false;
	private bool changed = false;
	public bool indoor = true;
	private bool end = false;
	
	// Marker for the finish line
	public GameObject finishMarker;	
	
	// Distance the player wants to run
	private int finish;
	
	List<Challenge> challenges;
	
	// Enums for the actor types
	public enum ActorType
	{
		Runner			= 1,
		Cyclist			= 2,
		Mo				= 3,
		Paula 			= 4,
		Chris			= 5,
		Bradley 		= 6
	}
	
	// Minimap attributes
	private GameObject minimap;
	private const int MAP_RADIUS = 1;
	Texture2D selfIcon;
	Texture2D targetIcon;
	Texture2D mapTexture = null;
	Material mapStencil;
	const int mapAtlasRadius = 315; // API max width/height is 640
	const int mapZoom = 18;
	Position mapOrigo = new Position(0, 0);
	WWW mapWWW = null;
	Position fetchOrigo = new Position(0, 0);
	
	private ActorType currentActorType = ActorType.Runner;
	private Rect debug;
	private const int MARGIN = 15;
	private bool authenticated = false;
	
	// Holds actor templates
	public GameObject cyclistHolder;
	public GameObject runnerHolder;
	public GameObject moHolder;
	public GameObject paulaHolder;
	public GameObject chrisHolder;
	public GameObject bradleyHolder;
	
	// Holds actor instances
	public List<GameObject> actors = new List<GameObject>();
	
	// Variables to set the scale
	private int originalHeight = 500;
	private int originalWidth = 800;
	private Vector3 scale;
	
	// Start tracking and 3-2-1 countdown variables
	private bool started = false;
	private bool countdown = false;
	private float countTime = 3.0f;
	
	// Target speed
	public float targSpeed = 1.8f;
	
	// Texture for black background
	public Texture blackTexture;
	
	// Bools to check if map is open and whether track has been selected
	private bool mapOpen = false;
	private bool trackSelected = false;
	
	private string indoorText = "Indoor Active";
	
	// Multiplier variables
	private float baseMultiplier;
	private String baseMultiplierString;
	private float baseMultiplierStartTime;
	
	// Target for bonus points
	private int bonusTarget = 1000;
	
	// Final sprint bonus points
	private float finalBonus = 1000;
	
	void Start () {
		finish = (int)DataVault.Get("finish");
		UnityEngine.Debug.Log("RaceGame: finish distance is " + finish);
		challenges = DataVault.Get("challenges") as List<Challenge>;
		if (challenges == null) challenges = new List<Challenge>(0);
				
		// Set indoor mode
#if !UNITY_EDITOR
		Platform.Instance.SetIndoor(indoor);
		Platform.Instance.StopTrack();
		Platform.Instance.Reset();
		//DataVault.Set("indoor_text", "Indoor Active");
#endif
		
		end = false;
		
		//UnityEngine.Debug.Log("Settings: Initial speed set to: " + s.ToString());
		
//		minimap = GameObject.Find("minimap");
//		minimap.renderer.material.renderQueue = 3000;
		
		// Calculate and set scale
		float x = (float)Screen.width/originalWidth;
		float y = (float)Screen.height/originalHeight;
		scale = new Vector3(x, y, 1);
		
		// Set debug box
		debug = new Rect((originalWidth-100), 0, 100, 100);
		
		string tar = (string)DataVault.Get("type");
		
		switch(tar)
		{
		case "Runner":
			currentActorType = ActorType.Runner;
			break;
			
		case "Cyclist":
			currentActorType = ActorType.Cyclist;
			break;
			
		case "Mo":
			currentActorType = ActorType.Mo;
			DataVault.Set("slider_val", 0.525f);
			targSpeed = 6.059f;
			break;
			
		case "Paula":
			currentActorType = ActorType.Paula;
			DataVault.Set("slider_val", 0.4f);
			targSpeed = 4.91f;
			break;
			
		case "Chris":
			currentActorType = ActorType.Chris;
			DataVault.Set("slider_val", 0.4f);
			targSpeed = 15.686f;			
			break;
			
		case "Bradley":
			currentActorType = ActorType.Bradley;
			DataVault.Set("slider_val", 0.4f);
			targSpeed = 17.007f;	
			break;
		}
		
		// Set templates' active status
		cyclistHolder.SetActive(false);
		runnerHolder.SetActive(false);
		
#if !UNITY_EDITOR
		// TODO: Move tracker creation to a button/flow and keep this class generic
		//if (Platform.Instance.targetTrackers.Count == 0) {
		Platform.Instance.ResetTargets();
		Platform.Instance.CreateTargetTracker(targSpeed);
		//} // else trackers created earlier
#endif
		InstantiateActors();

//		GetComponent<GetTrack>().setActive(false);
		UnityEngine.Debug.Log("RaceGame: started");
	}
	
	public void SetActorType(ActorType targ) {
		currentActorType = targ;
		changed = true;		
	}
	
	public void SetIndoor() {
		if(indoor) {
			indoor = false;
			UnityEngine.Debug.Log("Outdoor mode active");
			DataVault.Set("indoor_text", "Outdoor Active");
			indoorText = "Outdoor Active";
		}
		else {
			indoor = true;
			UnityEngine.Debug.Log("Indoor mode active");
			indoorText = "Indoor Active";
			DataVault.Set("indoor_text", indoorText);
		}
		changed = true;
	}
		
	public void Back() {
		
		float temp = ((float)DataVault.Get("slider_val") * 9.15f) + 1.25f;
		//UnityEngine.Debug.Log("Settings: New speed is: " + temp.ToString());
		if(temp != targSpeed)
		{
			changed = true;
			targSpeed = temp;
		}
		
		if(changed) {
					// Reset platform, set new target speed and indoor/outdoor mode
#if !UNITY_EDITOR
			Platform.Instance.Reset();
			Platform.Instance.ResetTargets();
					
			if(!trackSelected) {
				Platform.Instance.CreateTargetTracker(targSpeed);
			}
					
#endif
			InstantiateActors();
			
#if !UNITY_EDITOR
			Platform.Instance.SetIndoor(indoor);			
#endif
			
			// Start countdown again
			started = false;
			countdown = false;
			countTime = 3.0f;
					
			// Reset bools
			trackSelected = false;
			changed = false;
		} else {
			// Else restart tracking
#if !UNITY_EDITOR
			Platform.Instance.StartTrack();
#else
			PlatformDummy.Instance.StartTrack();
#endif
		}
	}
	
	void OnGUI() {
		// Set matrix, depth and various skin sizes
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
		GUI.depth = 5;
		
		// set style for our labels
		GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
		labelStyle.alignment = TextAnchor.UpperCenter;
		labelStyle.fontSize = 40;
		labelStyle.fontStyle = FontStyle.Bold;
		
//			// Set the speed slider, if the value has changed set the new speed
//			float temp  = GUI.HorizontalSlider(new Rect((originalWidth/2)-100, 250, 200, 50), targSpeed,  1.25f, 10.4f);
//    		GUI.Label(new Rect(originalWidth/2 + 120, 250, 100, 50), temp.ToString("f2") + "m/s");
//			if(temp != targSpeed)
//			{
//				changed = true;
//				targSpeed = temp;
//			}
		
		if(countdown)
		{
			
			// Get the current time rounded up
			int cur = Mathf.CeilToInt(countTime);
			
			// Display countdown on screen
			if(countTime > 0.0f)
			{
				GUI.Label(new Rect(300, 150, 200, 200), cur.ToString(), labelStyle); 
			}
			else if(countTime > -1.0f && countTime < 0.0f)
			{
				GUI.Label(new Rect(300, 150, 200, 200), "GO!", labelStyle); 
			}
		}
		
		// Display a message if the multiplier has changed in the last second and a half
		// See NewBaseMultiplier method in this class for more detail on how this is set
		if(started && baseMultiplierStartTime > (Time.time - 1.5f)) {
			GUI.Label(new Rect(300, 150, 200, 200), baseMultiplierString, labelStyle);
		}
		
		GUI.matrix = Matrix4x4.identity;
	}
	
	protected void UpdateLeaderboard() {
		double distance = Platform.Instance.Distance();
		// TODO: Decide if we are allowed to sort in place or need to make a copy
		List<TargetTracker> trackers = Platform.Instance.targetTrackers;
		trackers.Sort(delegate(TargetTracker x, TargetTracker y) {
			return y.GetTargetDistance().CompareTo(x.GetTargetDistance());
		});
		int position = 1;
		foreach (TargetTracker tracker in trackers) {
			if (tracker.GetTargetDistance() > distance) position++;
		}

		DataVault.Set("ahead_col_box", "D20000EE");
		
		DataVault.Set("leader_header", "You are");
		if (position == 1) { 
			DataVault.Set("ahead_leader", "in the lead!");
			DataVault.Set("ahead_col_box", "19D200EE");
		} else {
			DataVault.Set("ahead_leader", "behind by " + SiDistance(trackers[0].GetDistanceBehindTarget()));
		}
		
		DataVault.Set("position_header", "Position");
		string nth = position.ToString();
		if (position == 1) nth += "st";
		if (position == 2) nth += "nd";
		if (position == 3) nth += "rd";
		if (position >= 4) nth += "th";
		if (position > 2 && position == trackers.Count + 1) nth = "Last!";
		DataVault.Set("position_box", nth);
		
		// Find closest (abs) target
		TargetTracker nemesis = null;
		TargetTracker upstream = null;
		if (position > 1) upstream = trackers[position - 2]; // 1->0 indexing
		TargetTracker downstream = null;
		if (position < trackers.Count + 1) downstream = trackers[position - 1]; // 1->0 indexing
			
		if (upstream != null && downstream != null) {
			if (Math.Abs(upstream.GetDistanceBehindTarget()) <= Math.Abs(downstream.GetDistanceBehindTarget())) nemesis = upstream;
			else nemesis = downstream;
		} 
		else if (upstream != null) nemesis = upstream;
		else if (downstream != null) nemesis = downstream;		
		
		if (nemesis != null) {
			double d = nemesis.GetDistanceBehindTarget();
			string which = " behind";
			if (d > 0) which = " ahead";
			DataVault.Set("follow_header", nemesis.name + " is"); 
			DataVault.Set("follow_box", SiDistance(Math.Abs(d)) + which);
		} else {
			DataVault.Set("follow_header", "Solo");
			DataVault.Set("follow_box", "round!");
		}
	}
	
	protected void UpdateAhead() {
#if !UNITY_EDITOR
		double targetDistance = Platform.Instance.GetHighestDistBehind();
#else
		double targetDistance = PlatformDummy.Instance.DistanceBehindTarget();
#endif
		if (targetDistance > 0) {
			DataVault.Set("ahead_header", "Behind!");
			DataVault.Set("ahead_col_header", "D20000FF");
			DataVault.Set("ahead_col_box", "D20000EE");
		} else {
			DataVault.Set("ahead_header", "Ahead!"); 
			DataVault.Set("ahead_col_box", "19D200EE");
			DataVault.Set("ahead_col_header", "19D200FF");
		}
		DataVault.Set("ahead_box", SiDistance(Math.Abs(targetDistance)));
	}
	
	void Update () {
		
#if !UNITY_EDITOR
		Platform.Instance.Poll();
		
		DataVault.Set("calories", Platform.Instance.Calories().ToString());
		DataVault.Set("pace", Platform.Instance.Pace().ToString("f2") + "m/s");
		DataVault.Set("distance", SiDistance(Platform.Instance.Distance()));
		DataVault.Set("time", TimestampMMSSdd( Platform.Instance.Time()));
		DataVault.Set("indoor_text", indoorText);
		
		DataVault.Set("rawdistance", Platform.Instance.Distance());
		DataVault.Set("rawtime", Platform.Instance.Time());
		
		UpdateLeaderboard();
#else
		PlatformDummy.Instance.Poll();
		
		DataVault.Set("calories", PlatformDummy.Instance.Calories().ToString());
		DataVault.Set("pace", PlatformDummy.Instance.Pace().ToString("f2") + "m/s");
		DataVault.Set("distance", SiDistance(PlatformDummy.Instance.Distance()));
		DataVault.Set("time", TimestampMMSSdd( PlatformDummy.Instance.Time()));
		DataVault.Set("indoor_text", indoorText);
		
		DataVault.Set("rawdistance", PlatformDummy.Instance.Distance());
		DataVault.Set("rawtime", PlatformDummy.Instance.Time());
#endif
		// TODO: Toggle based on panel type
		UpdateAhead();
		
		
		// TODO: Multiple minimap targets
#if !UNITY_EDITOR
		double targetDistance = Platform.Instance.GetHighestDistBehind();
		Position position = Platform.Instance.Position();
		float bearing = Platform.Instance.Bearing();
#else
		double targetDistance = PlatformDummy.Instance.DistanceBehindTarget();
		Position position = PlatformDummy.Instance.Position();
		float bearing = PlatformDummy.Instance.Bearing();
#endif
		double bearingRad = bearing*Math.PI/180;
//		if (position != null) {
//			// Fake target coord using distance and bearing
//			Position targetCoord = new Position(position.latitude + (float)(Math.Cos(bearingRad)*targetDistance/111229d), position.longitude + (float)(Math.Sin(bearingRad)*targetDistance/111229d));
//			GetMap(position, bearingRad, targetCoord);
//		}
		
		// If there is a GPS lock or indoor mode is active
#if !UNITY_EDITOR
		if(Platform.Instance.HasLock() || indoor)
		{
			// Initiate the countdown
			countdown = true;
		 	if(countTime <= -1.0f && !started)
			{
				Platform.Instance.StartTrack();
				UnityEngine.Debug.LogWarning("Tracking Started");
				
//				float s = (targSpeed - 1.25f) / 9.15f;
//		
//				DataVault.Set("slider_val", s);
				started = true;
			}
			else if(countTime > -1.0f)
			{
				UnityEngine.Debug.LogWarning("Counting Down");
				countTime -= Time.deltaTime;
			}
		}
		
		if(Platform.Instance.Distance() / 1000 >= finish && !end)
		{
			end = true;
			DataVault.Set("total", Platform.Instance.GetCurrentPoints() + Platform.Instance.GetOpeningPointsBalance());
			DataVault.Set("bonus", (int)finalBonus);
			Platform.Instance.StopTrack();
			GameObject h = GameObject.Find("minimap");
			if(h != null) {
				h.renderer.enabled = false;
			}
			FlowState fs = FlowStateMachine.GetCurrentFlowState();
			GConnector gConect = fs.Outputs.Find(r => r.Name == "FinishButton");
			if(gConect != null) {
				fs.parentMachine.FollowConnection(gConect);
			} else {
				UnityEngine.Debug.Log("Game: No connection found!");
			}
		}
		
		// Awards the player points for running certain milestones
		if(Platform.Instance.Distance() >= bonusTarget)
		{
			int targetToKm = bonusTarget / 1000;
			if(bonusTarget < finish) 
			{
				MessageWidget.AddMessage("Bonus Points!", "You reached " + targetToKm.ToString() + "km! 1000pts", "trophy copy");
			}
			bonusTarget += 1000;
			
		}
		
		// Gives the player bonus points for sprinting the last 100m
		if(Platform.Instance.Distance() >= finish - 100)
		{
			DataVault.Set("ending_bonus", "Keep going for " + finalBonus.ToString("f0") + " bonus points!");
			finalBonus -= 50f * Time.deltaTime;
		}
		else
		{
			DataVault.Set("ending_bonus", "");
		}
#else
		if(PlatformDummy.Instance.HasLock()) 
		{
			countdown = true;
		 	if(countTime <= -1.0f && !started)
			{
				PlatformDummy.Instance.StartTrack();
				UnityEngine.Debug.LogWarning("Tracking Started");
				
//				float s = (targSpeed - 1.25f) / 9.15f;
//		
//				DataVault.Set("slider_val", s);
				started = true;
			}
			else if(countTime > -1.0f)
			{
				UnityEngine.Debug.LogWarning("Counting Down");
				countTime -= Time.deltaTime;
			}
		}
		
		if(PlatformDummy.Instance.Distance() >= finish && !end)
		{
			end = true;
			//DataVault.Set("total", Platform.Instance.GetCurrentPoints() + Platform.Instance.OpeningPointsBalance());
			//DataVault.Set("bonus", (int)finalBonus);
			PlatformDummy.Instance.StopTrack();
			GameObject h = GameObject.Find("minimap");
			h.renderer.enabled = false;
			FlowState fs = FlowStateMachine.GetCurrentFlowState();
			GConnector gConect = fs.Outputs.Find(r => r.Name == "FinishButton");
			if(gConect != null) {
				fs.parentMachine.FollowConnection(gConect);
			} else {
				UnityEngine.Debug.Log("Game: No connection found!");
			}
		}
		
		// Awards the player points for running certain milestones
		if(PlatformDummy.Instance.Distance() >= bonusTarget)
		{
			int targetToKm = bonusTarget / 1000;
			if(bonusTarget < finish) 
			{
				MessageWidget.AddMessage("Bonus Points!", "You reached " + targetToKm.ToString() + "km! 1000pts", "trophy copy");
			}
			bonusTarget += 1000;
			
		}
		
		// Gives the player bonus points for sprinting the last 100m
		if(PlatformDummy.Instance.Distance() >= finish - 100)
		{
			DataVault.Set("ending_bonus", "Keep going for " + finalBonus.ToString("f0") + " bonus points!");
			finalBonus -= 50f * Time.deltaTime;
		}
		else
		{
			DataVault.Set("ending_bonus", "");
		}
#endif	
		
	}
	
	string SiDistance(double meters) {
		string postfix = "m";
		string final;
		float value = (float)meters;
		if (value > 1000) {
			value = value/1000;
			postfix = "km";
			final = value.ToString("f3");
		}
		else
		{
			final = value.ToString("f0");
		}
		return final+postfix;
	}
	
	long SpeedToKmPace(float speed) {
		if (speed <= 0) {
			return 0;
		}
		// m/s -> mins/Km
		return Convert.ToInt64( ((1/speed)/60) * 1000);
	}
	
	string TimestampMMSSdd(long milliseconds) {
		TimeSpan span = TimeSpan.FromMilliseconds(milliseconds);

		return string.Format("{0:00}:{1:00}:{2:00}",span.Minutes,span.Seconds,span.Milliseconds/10);	
	}
	string TimestampMMSS(long minutes) {
		TimeSpan span = TimeSpan.FromMinutes(minutes);

		return string.Format("{0:00}:{1:00}",span.Minutes,span.Seconds);	
	}
	
	// Instantiate target actors based on actor type
	void InstantiateActors() {				
		// Remove current actors
		foreach (GameObject actor in actors) {
			Destroy(actor);
		}
		actors.Clear();
		
		GameObject template;
		switch(currentActorType) {
		case ActorType.Runner:
			template = runnerHolder;
			break;
		case ActorType.Cyclist:
			template = cyclistHolder;
			break;
		case ActorType.Mo:
			template = moHolder;
			targSpeed = 6.059f;
			DataVault.Set("slider_val", 0.525f);
			finish = 10000;
			break;
		case ActorType.Paula:
			template = paulaHolder;
			DataVault.Set("slider_val", 0.4f);
			targSpeed = 4.91f;
			finish = 42195;
			break;
		case ActorType.Chris:
			template = chrisHolder;
			finish = 1000;
			targSpeed = 17.007f;
			break;
		case ActorType.Bradley:
			template = bradleyHolder;
			finish = 4000;
			targSpeed = 15.686f;			
			break;
		default:
			throw new NotImplementedException("Unknown actor type: " + currentActorType);
			break;
		}
		
#if !UNITY_EDITOR
		List<TargetTracker> trackers = Platform.Instance.targetTrackers;
		int lane = 1;
		foreach (TargetTracker tracker in trackers) {
			GameObject actor = Instantiate(template) as GameObject;
			TargetController controller = actor.GetComponent<TargetController>();
			controller.SetTracker(tracker);
			controller.SetLane(lane++);
			actor.SetActive(true);
			actors.Add(actor);
		}
#else
		GameObject actorDummy = Instantiate(template) as GameObject;
		actorDummy.SetActive(true);
		actors.Add(actorDummy);
#endif
		UnityEngine.Debug.Log("RaceGame: instantiated actors");
	}
	
	private void GetMap(Position selfCoords, double bearing, Position targetCoords) {
		Position direction = new Position(selfCoords.latitude + (float)(Math.Cos(bearing)*1000/111229d), 
		                                  selfCoords.longitude + (float)(Math.Sin(bearing)*1000/111229d));	
		double pixelBearing = Angle(MercatorToPixel(selfCoords), MercatorToPixel(direction));	
//		UnityEngine.Debug.Log("Map: pixel bearing calculated");
		bearing = pixelBearing;
		
		// Get a static map with a radius of mapAtlasRadius, cache and re-get if viewport within margin of the border
		const int margin = 15;	
		int maxdrift = (mapAtlasRadius-MAP_RADIUS-margin);
		Vector2 drift = MercatorToPixel(mapOrigo) - MercatorToPixel(selfCoords);
//		UnityEngine.Debug.Log("Map: drift calculated");
//		Debug.Log("drift: " + drift.magnitude + " .." + drift);
		if (mapWWW == null && (mapTexture == null || drift.magnitude >= maxdrift)) {
			FetchMapTexture(selfCoords);
			UnityEngine.Debug.Log("Map: fetched");
		}
		if (mapWWW != null && mapWWW.isDone) {
			if (mapWWW.error != null) {
				UnityEngine.Debug.Log(mapWWW.error);
				UnityEngine.Debug.Log("Map: error with map");
			} else {
				mapTexture = mapWWW.texture;
				mapOrigo = fetchOrigo;
				UnityEngine.Debug.Log("Map: origo error");
			}
			mapWWW = null;
		}
		
		if (mapTexture == null) {
			return;
		}
		
		// Map self coordinates into map atlas, normalize to atlas size and shift to center
		Vector2 mapNormalSelf = (MercatorToPixel(mapOrigo) - MercatorToPixel(selfCoords)) / (mapAtlasRadius*2);
		mapNormalSelf.x += 0.5f;
		mapNormalSelf.y += 0.5f;
		float normalizedRadius = (float)MAP_RADIUS/(mapAtlasRadius*2);
		// Draw a MAP_RADIUS-sized circle around self
		Rect mapCoords = new Rect(1 - mapNormalSelf.x - normalizedRadius, mapNormalSelf.y - normalizedRadius,
		                          normalizedRadius*2, normalizedRadius*2);
		Vector2 mapCenter = new Vector2(minimap.transform.position.x, minimap.transform.position.y);
		//Matrix4x4 matrixBackup = GUI.matrix;
		if (Event.current.type == EventType.Repaint) {
			// Rotation and indexing into atlas handled by shader
			//minimap.renderer.material.SetFloat("_Rotation", (float)-bearing);			
			//minimap.renderer.material.SetVector("_Rectangle", new Vector4(mapCoords.x, mapCoords.y, mapCoords.width, mapCoords.height));
//			Graphics.DrawTexture(map, mapTexture, mapCoords, 0, 0, 0, 0, mapStencil);
			minimap.renderer.material.mainTexture = mapTexture;
//			minimap2.renderer.material.mainTexture = mapTexture;
		}
		
//		// Self is always at center
//		mapSelf.x = mapCenter.x - mapSelf.width/2;
//		mapSelf.y = mapCenter.y - mapSelf.height/2;
//		GUI.DrawTexture(mapSelf, selfIcon);
//		// Target is relative to self and limited to map radius
//		Vector2 localTarget = mercatorToPixel(selfCoords) - mercatorToPixel(targetCoords);
//		if (localTarget.magnitude > MAP_RADIUS) {
//			localTarget.Normalize();
//			localTarget *= MAP_RADIUS;
//			// TODO: Change icon to indicate outside of minimap?
//		}		
		
		// Rotated so bearing is up
		double c = Math.Cos(-bearing);
		double s = Math.Sin(-bearing);
//		mapTarget.x = mapCenter.x - (float)(localTarget.x * c - localTarget.y * s)  - mapTarget.width/2;
//		mapTarget.y = mapCenter.y - (float)(localTarget.x * s + localTarget.y * c) - mapTarget.height/2;
		//GUI.DrawTexture(mapTarget, targetIcon);
				
		//GUI.matrix = matrixBackup;
		//GUI.color = original;
	}
	
	Vector2 MercatorToPixel(Position mercator) {
		// Per google maps spec: pixelCoordinate = worldCoordinate * 2^zoomLevel
		int scale = (int)Math.Pow(2, mapZoom);
		
		// Mercator to google world cooordinates
		Vector2 world = new Vector2(
			(float)(mercator.longitude+180)/360*256,
			(float)(
				(1 - Math.Log(
						Math.Tan(mercator.latitude * Math.PI / 180) +  
						1 / Math.Cos(mercator.latitude * Math.PI / 180)
					) / Math.PI
				) / 2
			) * 256
		);
//		Debug.Log(mercator.latitude + "," + mercator.longitude + " => " + world.x + "," + world.y);
		
		return world * scale;
	}
	
	Position PixelToMercator(Vector2 pixel) {
		// Per google maps spec: pixelCoordinate = worldCoordinate * 2^zoomLevel
		int scale = (int)Math.Pow(2, mapZoom);
		
		Vector2 world = pixel / scale;
		// Google world coordinates to mercator
		double n = Math.PI - 2 * Math.PI * world.y / 256;
		Position mercator = new Position(
			(float)(180 / Math.PI * Math.Atan(0.5 * (Math.Exp(n) - Math.Exp(-n)))),
			(float)world.x / 256 * 360 - 180
		);
			
		return mercator;			
	}
	
	private double Angle(Vector2 pos1, Vector2 pos2) {
	    Vector2 from = pos2 - pos1;
	    Vector2 to = new Vector2(0, 1);
	 
	    float result = Vector2.Angle( from, to );
//		Debug.Log(result);
	    Vector3 cross = Vector3.Cross( from, to );
	 
	    if (cross.z > 0)
	       result = 360f - result;
		
		result += 180.0f;
		
	    return result*Math.PI/180;
	}
	
	private void FetchMapTexture(Position origo) {					
		const string API_KEY = "AIzaSyBj_iHOwteDxJ8Rj_bPsoslxIquy--y9nI";
		const string endpoint = "http://maps.googleapis.com/maps/api/staticmap";
		string url = endpoint + "?center="
		                      + origo.latitude + "," + origo.longitude
//		                      + "&markers=color:blue%7Clabel:S%7C" + origo.latitude + "," + origo.longitude
//		                      + "&markers=color:green%7Clabel:E%7C" + (origo.latitude+0.0001f) + "," + (origo.longitude+0.0001f)
//		                      + "&markers=color:red%7Clabel:W%7C" + (origo.latitude) + "," + (origo.longitude+0.0003f)
		                      + "&zoom=" + mapZoom
		                      + "&size=" + (mapAtlasRadius*2) + "x" + (mapAtlasRadius*2)
		                      + "&maptype=roadmap"
		                      + "&sensor=true&key=" + API_KEY;
		mapWWW = new WWW(url);		
//		debugText = "Fetching map..";
		UnityEngine.Debug.Log("Fetching map.. " + url);
		fetchOrigo = origo;
	}
	
	public void OnSliderValueChange() 
	{
		changed = true;
		targSpeed = UISlider.current.value * 10.4f;
		UnityEngine.Debug.Log(UISlider.current.value);
	}
	
	// Listen for UnitySendMessage with multiplier updates
	// Display the ner multiplier on screen for a second or so
	public void NewBaseMultiplier(String message) {
		// format the multiplier to 2 sig figs:
		float f = 0.0f;
		float.TryParse(message, out f);
		if (f == 1.0f && this.baseMultiplier > 1.0f) {
			this.baseMultiplierString = "Multiplier lost!";  // multiplier reset
		} else if (f > 1.0f && f != this.baseMultiplier) {
			this.baseMultiplierString = f.ToString("G") + "x Multiplier!";  // multiplier increased
		}
		// save the value for next time
		this.baseMultiplier = f;
		// remember the current time so we know how long to display for:
		this.baseMultiplierStartTime = Time.time;
		UnityEngine.Debug.Log("New base multiplier received:" + this.baseMultiplier);
	}
	
}
