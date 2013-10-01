using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System;

public class GlassGUI : MonoBehaviour {
	
	private Platform ji = null;

	
<<<<<<< HEAD:GlassFitUnity/Assets/GlassGUI.cs
	private const int BOX_WIDTH = 200;
	private const int BOX_HEIGHT = 100;
	private const int MAP_RADIUS = 100;
=======
	public bool countdown = false;
	public bool started = false;
	public GameObject platform;
>>>>>>> Staging-commits-for-SF-demo:GlassFitUnity/Assets/Scripts/GlassGUI.cs
	private const int MARGIN = 15;
	private const int SUBMARGIN = 5;
	
	private float countTime = 3.99f;
	private Stopwatch timer;
	
	private const float OPACITY = 0.5f;
	private float timeFromStart = 0;
	
	// Left side top
	private Rect target;	
	private string targetText;
	// Left side bottom
	private Rect distance;
	private string distanceText;
	private Rect time;
	private string timeText;
	
	// Right side top
	private Rect calories;
	private string caloriesText;
	private Rect pace;
	private string paceText;
	
	// Right side bottom
	private Rect map;
	private Rect mapSelf;
	private Rect mapTarget;
	
	// Buttons
	private Rect start;
	private string startText = "START";
	private Rect stop;
	private string stopText = "STOP";
	
	// Icons
	private Rect gpsLock;
	
	// Debug
	private Rect debug;
	private string debugText;
			
	// Slider
	private float paceSlider;
	private Rect sliderBox;
	
	private float originalWidth = 800;  // define here the original resolution
  	private float originalHeight = 500; // you used to create the GUI contents 
 	private Vector3 scale;

	// Background textures
	Texture2D normal;
	Texture2D info;
	Texture2D warning;
	
	// Map textures
	Texture2D selfIcon;
	Texture2D targetIcon;
	Texture2D mapTexture = null;
	Material mapStencil;
	const int mapAtlasRadius = 315; // API max width/height is 640
	const int mapZoom = 18;
	Position mapOrigo = new Position(0, 0);
	WWW mapWWW = null;
	Position fetchOrigo = new Position(0, 0);
	
	
	
	void Start () {
		// Left side top
<<<<<<< HEAD:GlassFitUnity/Assets/GlassGUI.cs
		target =   new Rect(MARGIN, MARGIN, BOX_WIDTH, BOX_HEIGHT);	
=======
		target =   new Rect(MARGIN, MARGIN, 200, 100);	
		
>>>>>>> Staging-commits-for-SF-demo:GlassFitUnity/Assets/Scripts/GlassGUI.cs
		// Left side bottom
		distance = new Rect(MARGIN, originalHeight-MARGIN-BOX_HEIGHT, BOX_WIDTH, BOX_HEIGHT);
		time =     new Rect(MARGIN, distance.y-SUBMARGIN-BOX_HEIGHT, BOX_WIDTH, BOX_HEIGHT);
		
		// Right side top
<<<<<<< HEAD:GlassFitUnity/Assets/GlassGUI.cs
		calories = new Rect(originalWidth-MARGIN-BOX_WIDTH, MARGIN, BOX_WIDTH, BOX_HEIGHT);
		pace =     new Rect(originalWidth-MARGIN-BOX_WIDTH, calories.y+SUBMARGIN+BOX_HEIGHT, BOX_WIDTH, BOX_HEIGHT);
=======
		calories = new Rect(originalWidth-MARGIN-200, MARGIN, 200, 100);
		pace =     new Rect(originalWidth-MARGIN-200, calories.y+SUBMARGIN+100, 200, 100);
		
>>>>>>> Staging-commits-for-SF-demo:GlassFitUnity/Assets/Scripts/GlassGUI.cs
		// Right side bottom
		map =      new Rect(originalWidth-MARGIN-MAP_RADIUS*2, originalHeight-MARGIN-MAP_RADIUS*2, MAP_RADIUS*2, MAP_RADIUS*2);
		mapSelf =  new Rect(0, 0, 30, 30);
		mapTarget = new Rect(0, 0, 30, 30);

		// Buttons
		start =    new Rect((originalWidth-BOX_WIDTH)/2, (originalHeight-BOX_HEIGHT)/2-SUBMARGIN, BOX_WIDTH, BOX_HEIGHT);
		stop =     new Rect((originalWidth-BOX_WIDTH)/2, (originalHeight+BOX_HEIGHT)/2+SUBMARGIN, BOX_WIDTH, BOX_HEIGHT);
		
		// Icons
		gpsLock =  new Rect((originalWidth/2)-150, MARGIN, 300, 100);
		
		//Slider
		sliderBox = new Rect((originalWidth/2), MARGIN, 300, 100);
		
		// *** DEBUG
		debug =    new Rect((originalWidth-200)/2, originalHeight-MARGIN-100, 200, 100);
		// *** DEBUG
		
		distanceText = "Distance\n";
		timeText = "Time\n";
		caloriesText = "Calories\n";
		paceText = "Pace/KM\n";	
		
		Color white = new Color(0.9f, 0.9f, 0.9f, OPACITY);
		normal = new Texture2D(1, 1);
		normal.SetPixel(0,0,white);
		normal.Apply();
		
		Color green = new Color(0f, 0.9f, 0f, OPACITY);
		info = new Texture2D(1, 1);
		info.SetPixel(0,0,green);
		info.Apply();
		
		Color red = new Color(0.9f, 0f, 0f, OPACITY);
		warning = new Texture2D(1, 1);
		warning.SetPixel(0,0,red);
		warning.Apply();
		
		selfIcon = Resources.Load("Self") as Texture2D;
		targetIcon = Resources.Load("Target") as Texture2D;
<<<<<<< HEAD:GlassFitUnity/Assets/GlassGUI.cs
		mapStencil = new Material(Shader.Find("Custom/MapStencil"));
				
#if UNITY_ANDROID && !UNITY_EDITOR 
		ji = new Platform();
#else
		ji = new PlatformDummy();
#endif
		
=======
		mapTexture = Resources.Load("DummyMap") as Texture2D;
		
		
		ji = new Platform();
		//EventLog.
		//ji.StartTrack(false);
>>>>>>> Staging-commits-for-SF-demo:GlassFitUnity/Assets/Scripts/GlassGUI.cs
	}
	
	// Update is called once per frame
	void Update ()
	{
		timeFromStart += Time.deltaTime;
	
//		if(countTime == 3.99f && ji.hasLock() && !started)
//		{
//			started = true;
//		}
//		
//		if(started && countTime <= 0.0f)
//		{
//			ji.StartTrack(false);
//		}
//		else if(started && countTime > 0.0f)
//		{
//			countTime -= Time.deltaTime;
//		}
		
		if(Input.GetKeyDown(KeyCode.Escape))
		{
			if(countTime < 0.0f)
			{
				Application.LoadLevel(Application.loadedLevel);
			} else
			{
				Application.LoadLevel(0);
			}
		}
		
<<<<<<< HEAD:GlassFitUnity/Assets/GlassGUI.cs
		timeOut -= Time.deltaTime;		
=======
		ji.Poll();
>>>>>>> Staging-commits-for-SF-demo:GlassFitUnity/Assets/Scripts/GlassGUI.cs
	}
	
	void OnGUI ()
	{
<<<<<<< HEAD:GlassFitUnity/Assets/GlassGUI.cs
		// Scale UI to fit screen
		scale.x = (float)Screen.width/originalWidth; // calculate hor scale
	    scale.y = (float)Screen.height/originalHeight; // calculate vert scale
	    scale.z = 1;
	    var svMat = GUI.matrix; // save current matrix
	    // substitute matrix - only scale is altered from standard
	    GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
		
		// Style elements
=======
		// Scale for devices
		scale.x = (float)Screen.width / originalWidth;
		scale.y = (float)Screen.height / originalHeight;
    	scale.z = 1;
		
    	var svMat = GUI.matrix; // save current matrix
    	
		// substitute matrix - only scale is altered from standard
    	GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
		
		if(countdown)
		{
			GUI.skin.label.fontSize = 40;
			GUI.Label(new Rect(200, 200, 500, 500), "COUNTDOWN ON");
			//float currentTime = 3.0f - timer.Elapsed.Seconds;
			if(countTime > 0.99f)
			{
				GUI.Label(new Rect(300, 200, 200, 200), countTime.ToString("f0")); 
			}
			else if(countTime > 0.0f && countTime < 1.0f)
			{
				GUI.Label(new Rect(300, 200, 200, 200), "GO!"); 
			}
		}
		
		// Setting label font size
>>>>>>> Staging-commits-for-SF-demo:GlassFitUnity/Assets/Scripts/GlassGUI.cs
		GUI.skin.label.fontSize = 15;
		GUI.skin.label.alignment = TextAnchor.MiddleCenter;
		
		// Setting GUI box attributes
		GUI.skin.box.wordWrap = true;
		GUI.skin.box.fontSize = 30;
		GUI.skin.box.fontStyle = FontStyle.Bold;
		GUI.skin.box.alignment = TextAnchor.MiddleCenter;				
		GUI.skin.box.normal.background = normal;
		GUI.skin.box.normal.textColor = Color.black;
		
<<<<<<< HEAD:GlassFitUnity/Assets/GlassGUI.cs
		// *** DEBUG? TODO: Icon? Message?
		float bearing = ji.Bearing();
		double bearingRad = bearing*Math.PI/180;
		GUI.Label(gpsLock, "Bear: " + (int)(bearing) + "\u00B0");
				
		// Style top-left box depending on content
=======
		if(!ji.hasLock())
		{
			GUI.Label(gpsLock, "Waiting for GPS Lock...");
		}
		
		// Target Distance
>>>>>>> Staging-commits-for-SF-demo:GlassFitUnity/Assets/Scripts/GlassGUI.cs
		GUIStyle targetStyle = new GUIStyle(GUI.skin.box);
		double targetDistance = ji.DistanceBehindTarget();
		if (targetDistance > 0) {
			targetStyle.normal.background = warning; 
			targetText = "Behind!\n";
		} else {
			targetStyle.normal.background = info; 
			targetText = "Ahead\n";
		}
		targetStyle.normal.textColor = Color.white;		
		GUI.Box(target, targetText+"<i>"+SiDistance( Math.Abs(targetDistance) )+"</i>", targetStyle);
		
		// Distance
		double selfDistance = ji.Distance();
		GUI.Box(distance, distanceText+SiDistance( selfDistance));
		
		// Time
		GUI.Box(time, timeText+TimestampMMSSdd( ji.Time() ));
<<<<<<< HEAD:GlassFitUnity/Assets/GlassGUI.cs
		GUI.Box(calories, caloriesText + ji.Calories());		
		GUI.Box(pace, paceText+TimestampMMSS(speedToKmPace( ji.Pace() )) );
		
		// Draw minimap
		Position position = ji.Position();
		if (position != null) {
			// Fake target coord using distance and bearing
			Position targetCoord = new Position(position.latitude + (float)(Math.Cos(bearingRad)*targetDistance/111229d), position.longitude + (float)(Math.Sin(bearingRad)*targetDistance/111229d));
			GUIMap(position, bearingRad, targetCoord);
		} else {
			GUI.Label(map, "No GPS lock");
		}
		
		/// Draw and handle buttons
		if(started && buttonOn && GUI.Button(start, "Pause")) {
			ji.Start(false);
			started = true;
		}
		if(started && buttonOn && GUI.Button(stop, stopText)) {
			ji.Start(false);
			started = true;
		}		
		
		if (!started && GUI.Button (start, startText)) {
			ji.Start(false);
			started = true;
		}
		// *** DEBUG		
		if (!started && GUI.Button (stop, "START indoor")) {			
			ji.Start(true);
			started = true;
		}
=======
		
		// Calories
		GUI.Box(calories, caloriesText + ji.Calories());
		
		// pace
		GUI.Box(pace, paceText+TimestampMMSS(speedToKmPace( ji.Pace() )) );
		
		// Map
		// TODO: Stencil out circle
		Color original = GUI.color;
		GUI.color = new Color(1f, 1f, 1f, OPACITY);
				
		float selfOnMap = (float)selfDistance/map.height;
		Rect mapCoords = new Rect(0, selfOnMap, 1, selfOnMap+0.3f);
		GUI.DrawTextureWithTexCoords(map, mapTexture, mapCoords);

		mapSelf.x = map.x + map.width/2 - mapSelf.width/2;
		mapSelf.y = map.y + map.height/2 - mapSelf.height/2;
		
		int targetDistanceOnMap = Convert.ToInt32(targetDistance);
		int maxDistanceOnMap = Convert.ToInt32(map.height/2);
		if (targetDistanceOnMap > maxDistanceOnMap) targetDistanceOnMap = maxDistanceOnMap; 
		if (-targetDistanceOnMap > maxDistanceOnMap) targetDistanceOnMap = -maxDistanceOnMap; 
		mapTarget.x = map.x + map.width/2 - mapTarget.width/2;
		mapTarget.y = map.y - targetDistanceOnMap + map.height/2 - mapTarget.height/2;
		
		GUI.DrawTexture(mapSelf, selfIcon);
		GUI.DrawTexture(mapTarget, targetIcon);
		GUI.color = original;
		
		// *** DEBUG
		//GUI.TextArea(debug, debugText + ji.DebugLog());
>>>>>>> Staging-commits-for-SF-demo:GlassFitUnity/Assets/Scripts/GlassGUI.cs
		// *** DEBUG
		GUI.matrix = svMat; // restore matrix
	}
	
<<<<<<< HEAD:GlassFitUnity/Assets/GlassGUI.cs
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
		debugText = "Fetching map..";
		Debug.Log("Fetching map.. " + url);
		fetchOrigo = origo;
	}
	
	private void GUIMap(Position selfCoords, double bearing, Position targetCoords) {
		Position direction = new Position(selfCoords.latitude + (float)(Math.Cos(bearing)*1000/111229d), 
		                                  selfCoords.longitude + (float)(Math.Sin(bearing)*1000/111229d));	
		double pixelBearing = Angle(mercatorToPixel(selfCoords), mercatorToPixel(direction));	
		bearing = pixelBearing;
		
		// Get a static map with a radius of mapAtlasRadius, cache and re-get if viewport within margin of the border
		const int margin = 15;	
		int maxdrift = (mapAtlasRadius-MAP_RADIUS-margin);
		Vector2 drift = mercatorToPixel(mapOrigo) - mercatorToPixel(selfCoords);
//		Debug.Log("drift: " + drift.magnitude + " .." + drift);
		if (mapWWW == null && (mapTexture == null || drift.magnitude >= maxdrift)) {
			FetchMapTexture(selfCoords);
		}
		if (mapWWW != null && mapWWW.isDone) {
			if (mapWWW.error != null) {
				debugText = mapWWW.error;
				Debug.Log(mapWWW.error);
			} else {
				debugText = "";
				mapTexture = mapWWW.texture;
				mapOrigo = fetchOrigo;
			}
			mapWWW = null;
		}
		if (mapTexture == null) {
			GUI.Label(map, "Fetching map..");
			return;
		}
		
		// TODO: Stencil out circle
		Color original = GUI.color;
		GUI.color = new Color(1f, 1f, 1f, OPACITY);
		
		// Map self coordinates into map atlas, normalize to atlas size and shift to center
		Vector2 mapNormalSelf = (mercatorToPixel(mapOrigo) - mercatorToPixel(selfCoords)) / (mapAtlasRadius*2);
		mapNormalSelf.x += 0.5f;
		mapNormalSelf.y += 0.5f;
		float normalizedRadius = (float)MAP_RADIUS/(mapAtlasRadius*2);
		// Draw a MAP_RADIUS-sized circle around self
		Rect mapCoords = new Rect(1 - mapNormalSelf.x - normalizedRadius, mapNormalSelf.y - normalizedRadius,
		                          normalizedRadius*2, normalizedRadius*2);
		Vector2 mapCenter = new Vector2(map.x + map.width/2, map.y + map.height/2);
		Matrix4x4 matrixBackup = GUI.matrix;
		if (Event.current.type == EventType.Repaint) {
			// Rotation and indexing into atlas handled by shader
			mapStencil.SetFloat("_Rotation", (float)-bearing);			
			mapStencil.SetVector("_Rectangle", new Vector4(mapCoords.x, mapCoords.y, mapCoords.width, mapCoords.height));
//			Graphics.DrawTexture(map, mapTexture, mapCoords, 0, 0, 0, 0, mapStencil);
			Graphics.DrawTexture(map, mapTexture, mapStencil);
		}
		
		// Self is always at center
		mapSelf.x = mapCenter.x - mapSelf.width/2;
		mapSelf.y = mapCenter.y - mapSelf.height/2;
		GUI.DrawTexture(mapSelf, selfIcon);
		// Target is relative to self and limited to map radius
		Vector2 localTarget = mercatorToPixel(selfCoords) - mercatorToPixel(targetCoords);
		if (localTarget.magnitude > MAP_RADIUS) {
			localTarget.Normalize();
			localTarget *= MAP_RADIUS;
			// TODO: Change icon to indicate outside of minimap?
		}		
		// Rotated so bearing is up
		double c = Math.Cos(-bearing);
		double s = Math.Sin(-bearing);
		mapTarget.x = mapCenter.x - (float)(localTarget.x * c - localTarget.y * s)  - mapTarget.width/2;
		mapTarget.y = mapCenter.y - (float)(localTarget.x * s + localTarget.y * c) - mapTarget.height/2;
		GUI.DrawTexture(mapTarget, targetIcon);
				
		GUI.matrix = matrixBackup;
		GUI.color = original;
	}
	
	string SiDistance(long meters) {
		string postfix = "M";
		float value = meters;
		if (value > 100) {
=======
	string SiDistance(double meters) {
		string postfix = "m";
		string final;
		int value = (int)meters;
		if (value > 1000) {
>>>>>>> Staging-commits-for-SF-demo:GlassFitUnity/Assets/Scripts/GlassGUI.cs
			value = value/1000;
			postfix = "km";
			final = value.ToString("f3");
		}
		else
		{
			final = value.ToString("f0");
		}
		return value+postfix;
	}
	
	long speedToKmPace(float speed) {
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
	
	Vector2 mercatorToPixel(Position mercator) {
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
	
	Position pixelToMercator(Vector2 pixel) {
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
		Debug.Log(result);
	    Vector3 cross = Vector3.Cross( from, to );
	 
	    if (cross.z > 0)
	       result = 360f - result;
		
		result += 180.0f;
		
	    return result*Math.PI/180;
	}	
}

