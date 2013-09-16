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
	
	private const int MARGIN = 15;
	
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
	private string mapText;
	
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

	void Start () {
		


		// Left side top
		target =   new Rect(MARGIN, MARGIN, 150, 100);	
		// Left side bottom
		distance = new Rect(MARGIN, originalHeight-100, 150, 100);
		time =     new Rect(MARGIN, distance.y-MARGIN-100, 150, 100);
		
		// Right side top
		calories = new Rect(originalWidth-MARGIN, MARGIN, 150, 100);
		pace =     new Rect(originalWidth-MARGIN, calories.y+MARGIN+100, 150, 100);
		// Right side bottom
		map =      new Rect(originalWidth-MARGIN, originalHeight-MARGIN-150, 150, 150);
		
		// Buttons
		start =    new Rect((originalWidth)/2, (originalHeight-100)/2-MARGIN, 200, 100);
		stop =     new Rect((originalWidth)/2, (originalHeight+100)/2+MARGIN, 200, 100);
		
		// Icons
		gpsLock =  new Rect(originalWidth/2+75, MARGIN, 50, 50);
		
		// *** DEBUG
		debug =    new Rect(originalWidth/2+50, originalHeight-MARGIN, 100, 100);
		

		// *** DEBUG
		
		targetText = "Behind\n";
		distanceText = "Distance\n";
		timeText = "Time\n";
		caloriesText = "Calories\n";
		paceText = "Pace/KM\n";	
		
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
		
		
		
	scale.x = Screen.width/originalWidth; // calculate hor scale
    scale.y = Screen.height/originalHeight; // calculate vert scale
    scale.z = 1;
    var svMat = GUI.matrix; // save current matrix
    // substitute matrix - only scale is altered from standard
    GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
		
		GUI.skin.box.wordWrap = true;
		
		GUI.Box(gpsLock, "GPS: " + ji.hasLock());
		GUI.Box(target, targetText+SiDistance( ji.DistanceBehindTarget() ));
		GUI.Box(distance, distanceText+SiDistance( ji.Distance() ));
		GUI.Box(time, timeText+Timestamp( ji.Time() ));
		GUI.Box(calories, caloriesText + ji.Calories());
		GUI.Box(pace, paceText+Timestamp(speedToKmPace( ji.Pace() )) );
		GUI.Box(map, "TODO");
		
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
		GUI.Box(debug, debugText + ji.DebugLog());
		// *** DEBUG
			GUI.matrix = svMat; // restore matrix
	
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
		return Convert.ToInt64(1000*1/(speed));
	}
	
	string Timestamp(long milliseconds) {
		TimeSpan span = TimeSpan.FromMilliseconds(milliseconds);

		return string.Format("{0:00}:{1:00}:{2:00}",span.Minutes,span.Seconds,span.Milliseconds/10);	
	}
}

