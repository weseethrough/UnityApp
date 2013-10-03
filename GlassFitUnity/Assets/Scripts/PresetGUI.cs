using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System;

public class PresetGUI : MonoBehaviour {

	private Platform ji = null;
	
	public bool countdown = false;
	public bool started = false;
	private const int MARGIN = 15;
	private const int SUBMARGIN = 5;
	
	private float countTime = 3.0f;
	
	private const float OPACITY = 0.5f;
	private float timeFromStart = 0;
	
	// Left side top
	private Rect target;	
	
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
	Texture2D mapTexture;

	void Start () {
		// Left side top
		target =   new Rect(MARGIN, MARGIN, 200, 100);	
		
		// Left side bottom
		distance = new Rect(MARGIN, originalHeight-MARGIN-100, 200, 100);
		time =     new Rect(MARGIN, distance.y-SUBMARGIN-100, 200, 100);
		
		// Right side top
		calories = new Rect(originalWidth-MARGIN-200, MARGIN, 200, 100);
		pace =     new Rect(originalWidth-MARGIN-200, calories.y+SUBMARGIN+100, 200, 100);
		
		// Right side bottom
		map =      new Rect(originalWidth-MARGIN-200, originalHeight-MARGIN-200, 200, 200);
		mapSelf =  new Rect(0, 0, 30, 30);
		mapTarget = new Rect(0, 0, 30, 30);

		// Buttons
		start =    new Rect((originalWidth-200)/2, (originalHeight-100)/2-SUBMARGIN, 200, 100);
		stop =     new Rect((originalWidth-200)/2, (originalHeight+100)/2+SUBMARGIN, 200, 100);
		
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
		mapTexture = Resources.Load("DummyMap") as Texture2D;
		
		
		ji = new Platform();
	}
	
	// Update is called once per frame
	void Update ()
	{
		timeFromStart += Time.deltaTime;
		
		if(started && countTime > -1.0f && ji.hasLock())
		{
			countTime -= Time.deltaTime;
		}
		
		if(Input.GetKeyDown(KeyCode.Escape))
		{
			if(countTime < 0.0f)
			{
				ji.reset();
				Application.LoadLevel(Application.loadedLevel);
			} else
			{
				ji.reset();
				Application.LoadLevel(0);
			}
		}
				
		ji.Poll();
	}
	
	
	
	void OnGUI ()
	{
		// Scale for devices
		scale.x = (float)Screen.width / originalWidth;
		scale.y = (float)Screen.height / originalHeight;
    	scale.z = 1;
		
    	var svMat = GUI.matrix; // save current matrix
    	
		// substitute matrix - only scale is altered from standard
    	GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
		
		// Setting label font size
		GUI.skin.label.fontSize = 15;
		
		// Setting GUI box attributes
		GUI.skin.box.wordWrap = true;
		GUI.skin.box.fontSize = 30;
		GUI.skin.box.fontStyle = FontStyle.Bold;
		GUI.skin.box.alignment = TextAnchor.MiddleCenter;				
		GUI.skin.box.normal.background = normal;
		GUI.skin.box.normal.textColor = Color.black;
		
		if(!ji.hasLock())
		{
			GUI.Label(gpsLock, "Waiting for GPS Lock...");
		}
		
		// Distance
		double selfDistance = ji.Distance();
		
		GUI.Box(distance, distanceText+SiDistance( selfDistance));
		
		// Time
		GUI.Box(time, timeText+TimestampMMSSdd( ji.Time() ));
		
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
		
		double targetDistance = ji.DistanceBehindTarget();
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
		// *** DEBUG
		GUI.matrix = svMat; // restore matrix
	}
	
	string SiDistance(double meters) {
		string postfix = "m";
		string final;
		int value = (int)meters;
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
}
