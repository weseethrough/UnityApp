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
	
	// Background textures
	Texture2D normal;
	Texture2D info;
	Texture2D warning;
	
	// Map textures
	Texture2D selfIcon;
	Texture2D targetIcon;
	Texture2D mapTexture;
	
	void Start () {
		// Left side top
		target =   new Rect(MARGIN, MARGIN, 200, 100);	
		// Left side bottom
		distance = new Rect(MARGIN, Screen.height-MARGIN-100, 200, 100);
		time =     new Rect(MARGIN, distance.y-SUBMARGIN-100, 200, 100);
		
		// Right side top
		calories = new Rect(Screen.width-MARGIN-200, MARGIN, 200, 100);
		pace =     new Rect(Screen.width-MARGIN-200, calories.y+SUBMARGIN+100, 200, 100);
		// Right side bottom
		map =      new Rect(Screen.width-MARGIN-200, Screen.height-MARGIN-200, 200, 150);
		mapSelf =  new Rect(0, 0, 30, 30);
		mapTarget = new Rect(0, 0, 30, 30);
		
		// Buttons
		start =    new Rect((Screen.width-200)/2, (Screen.height-100)/2-MARGIN, 200, 100);
		stop =     new Rect((Screen.width-200)/2, (Screen.height+100)/2+MARGIN, 200, 100);
		
		// Icons
		gpsLock =  new Rect((Screen.width-50)/2, MARGIN, 50, 50);
		
		// *** DEBUG
		debug =    new Rect(250, Screen.height-100-MARGIN, Screen.width-250*2, 100);
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
		mapTexture = Resources.Load("DummyMap") as Texture2D;
		
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
	}
	
	void OnGUI ()
	{
		GUI.skin.label.fontSize = 15;
		
		GUI.skin.box.wordWrap = true;
		GUI.skin.box.fontSize = 30;
		GUI.skin.box.fontStyle = FontStyle.Bold;
		GUI.skin.box.alignment = TextAnchor.MiddleCenter;				
		GUI.skin.box.normal.background = normal;
		GUI.skin.box.normal.textColor = Color.black;
		
		GUI.Label(gpsLock, "GPS: " + ji.hasLock());
				
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
		
		// Map
		// TODO: Stencil out circle
		Color original = GUI.color;
		GUI.color = new Color(1f, 1f, 1f, OPACITY);
				
		float selfOnMap = selfDistance/map.height;
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
}

