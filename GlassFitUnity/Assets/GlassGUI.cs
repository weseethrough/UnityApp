using UnityEngine;
using System.Collections;
using System;

public class GlassGUI : MonoBehaviour {
	
#if UNITY_ANDROID && !UNITY_EDITOR 
	private Platform ji = null;
#else
	private PlatformDummy ji = null;
#endif
	private Boolean started = false;
	private Boolean buttonOn = false;
	private float timeOut = 0;
	private int touchCount = 0;
	
	private const int BOX_WIDTH = 200;
	private const int BOX_HEIGHT = 100;
	private const int MAP_RADIUS = 100;
	private const int MARGIN = 15;
	private const int SUBMARGIN = 5;
	
	private const float OPACITY = 0.5f;
	
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
			
	private int originalWidth = 800;  // define here the original resolution
  	private int originalHeight = 500; // you used to create the GUI contents 
 	private Vector3 scale;

	// Background textures
	Texture2D normal;
	Texture2D info;
	Texture2D warning;
	
	// Map textures
	Texture2D selfIcon;
	Texture2D targetIcon;
	Texture2D mapTexture = null;
	const int mapAtlasRadius = 315; // API max width/height is 640
	const int mapZoom = 18;
	Position mapOrigo = new Position(0, 0);
	WWW mapWWW = null;
	Position fetchOrigo = new Position(0, 0);
	
	
	
	void Start () {
		// Left side top
		target =   new Rect(MARGIN, MARGIN, BOX_WIDTH, BOX_HEIGHT);	
		// Left side bottom
		distance = new Rect(MARGIN, originalHeight-MARGIN-BOX_HEIGHT, BOX_WIDTH, BOX_HEIGHT);
		time =     new Rect(MARGIN, distance.y-SUBMARGIN-BOX_HEIGHT, BOX_WIDTH, BOX_HEIGHT);
		
		// Right side top
		calories = new Rect(originalWidth-MARGIN-BOX_WIDTH, MARGIN, BOX_WIDTH, BOX_HEIGHT);
		pace =     new Rect(originalWidth-MARGIN-BOX_WIDTH, calories.y+SUBMARGIN+BOX_HEIGHT, BOX_WIDTH, BOX_HEIGHT);
		// Right side bottom
		map =      new Rect(originalWidth-MARGIN-MAP_RADIUS*2, originalHeight-MARGIN-MAP_RADIUS*2, MAP_RADIUS*2, MAP_RADIUS*2);
		mapSelf =  new Rect(0, 0, 30, 30);
		mapTarget = new Rect(0, 0, 30, 30);

		// Buttons
		start =    new Rect((originalWidth-BOX_WIDTH)/2, (originalHeight-BOX_HEIGHT)/2-SUBMARGIN, BOX_WIDTH, BOX_HEIGHT);
		stop =     new Rect((originalWidth-BOX_WIDTH)/2, (originalHeight+BOX_HEIGHT)/2+SUBMARGIN, BOX_WIDTH, BOX_HEIGHT);
		
		// Icons
		gpsLock =  new Rect((originalWidth-50)/2, MARGIN, 50, 50);
		
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
				
#if UNITY_ANDROID && !UNITY_EDITOR 
		ji = new Platform();
#else
		ji = new PlatformDummy();
#endif
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		// TODO: Replace with timed poll or callback
		ji.Poll();
		
		foreach (Touch touch in Input.touches) {
            if (touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled)
                touchCount++;
        }
		
		if(touchCount > 0) {
			buttonOn = true;
			timeOut = 3;
			touchCount = 0;
		}
		if(timeOut < 0) {
			buttonOn = false;
		}
		
		timeOut -= Time.deltaTime;		
	}
	
	void OnGUI ()
	{
		// Scale UI to fit screen
		scale.x = (float)Screen.width/originalWidth; // calculate hor scale
	    scale.y = (float)Screen.height/originalHeight; // calculate vert scale
	    scale.z = 1;
	    var svMat = GUI.matrix; // save current matrix
	    // substitute matrix - only scale is altered from standard
	    GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
		
		// Style elements
		GUI.skin.label.fontSize = 15;
		GUI.skin.label.alignment = TextAnchor.MiddleCenter;
		
		GUI.skin.box.wordWrap = true;
		GUI.skin.box.fontSize = 30;
		GUI.skin.box.fontStyle = FontStyle.Bold;
		GUI.skin.box.alignment = TextAnchor.MiddleCenter;				
		GUI.skin.box.normal.background = normal;
		GUI.skin.box.normal.textColor = Color.black;
		
		// *** DEBUG? TODO: Icon? Message?
		GUI.Label(gpsLock, "GPS: " + ji.hasLock());
				
		// Style top-left box depending on content
		GUIStyle targetStyle = new GUIStyle(GUI.skin.box);
		long targetDistance = ji.DistanceBehindTarget();
		if (targetDistance > 0) {
			targetStyle.normal.background = warning; 
			targetText = "Behind!\n";
		} else {
			targetStyle.normal.background = info; 
			targetText = "Ahead\n";
		}
		targetStyle.normal.textColor = Color.white;		
		GUI.Box(target, targetText+"<i>"+SiDistance( Math.Abs(targetDistance) )+"</i>", targetStyle);
		long selfDistance = ji.Distance();
		GUI.Box(distance, distanceText+SiDistance( selfDistance));
		GUI.Box(time, timeText+TimestampMMSSdd( ji.Time() ));
		GUI.Box(calories, caloriesText + ji.Calories());		
		GUI.Box(pace, paceText+TimestampMMSS(speedToKmPace( ji.Pace() )) );
		
		// Draw minimap
		Position position = ji.Position();
		if (position != null) {
			// Fake target coord using distance
			float bearing = 0;
			Position targetCoord = new Position(position.latitude + (float)(targetDistance/111229d), position.longitude - (float)(targetDistance/111229d));
			GUIMap(position, bearing, targetCoord);
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
		// *** DEBUG
		GUI.TextArea(debug, debugText + ji.DebugLog());
		// *** DEBUG
	
		GUI.matrix = svMat; // restore matrix
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
		debugText = url;
		Debug.Log(url);
		fetchOrigo = origo;
	}
	
	private void GUIMap(Position selfCoords, float bearing, Position targetCoords) {
		// Get a static map with a radius of mapAtlasRadius, cache and re-get if viewport within margin of the border
		const int margin = 15;
		int maxdrift = (mapAtlasRadius-MAP_RADIUS-margin);
		Vector2 drift = mercatorToPixel(mapOrigo) - mercatorToPixel(selfCoords);
//		Debug.Log("drift: " + drift.magnitude + " .." + drift);
		if (mapWWW == null && (mapTexture == null || drift.magnitude >= 50)) {
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
//		Debug.Log(mapCoords);
		// TODO: Rotate map so bearing is up
		GUI.DrawTextureWithTexCoords(map, mapTexture, mapCoords);
				
		Vector2 mapCenter = new Vector2(map.x + map.width/2, map.y + map.height/2);
		
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
		mapTarget.x = mapCenter.x - localTarget.x - mapTarget.width/2;
		mapTarget.y = mapCenter.y - localTarget.y - mapTarget.height/2;
		GUI.DrawTexture(mapTarget, targetIcon);
		GUI.color = original;
	}
	
	string SiDistance(long meters) {
		string postfix = "M";
		float value = meters;
		if (value > 100) {
			value = value/1000;
			postfix = "KM";
		}
		return value+postfix;
	}
	
	long speedToKmPace(float speed) {
		if (speed <= 0) return 0;
		// m/s -> ms/km
		return Convert.ToInt64(1000*(1/speed));
	}
	
	string TimestampMMSSdd(long milliseconds) {
		TimeSpan span = TimeSpan.FromMilliseconds(milliseconds);

		return string.Format("{0:00}:{1:00}:{2:00}",span.Minutes,span.Seconds,span.Milliseconds/10);	
	}
	string TimestampMMSS(long seconds) {
		TimeSpan span = TimeSpan.FromSeconds(seconds);

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
//			(float)(180 / Math.PI * (2*Math.Atan( Math.Exp(world.y / 256 * Math.PI/180)) - Math.PI/2 )),
			(float)world.x / 256 * 360 - 180
		);
			
		return mercator;			
	}
}

