using UnityEngine;
using System.Collections;
using System;

public class GlassGUI : MonoBehaviour {
	
#if UNITY_ANDROID && !UNITY_EDITOR 
	private Platform ji = new Platform();
#else
	private PlatformDummy ji = new PlatformDummy();
#endif
	private Boolean started = false;
	
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

	// Debug
	private Rect debug;
	private string debugText;
	
	void Start () {
		// Left side top
		target =   new Rect(MARGIN, MARGIN, 150, 100);	
		// Left side bottom
		distance = new Rect(MARGIN, Screen.height-MARGIN-100, 150, 100);
		time =     new Rect(MARGIN, distance.y-MARGIN-100, 150, 100);
		
		// Right side top
		calories = new Rect(Screen.width-MARGIN-150, MARGIN, 150, 100);
		pace =     new Rect(Screen.width-MARGIN-150, calories.y+MARGIN+100, 150, 100);
		// Right side bottom
		map =      new Rect(Screen.width-MARGIN-150, Screen.height-MARGIN-150, 150, 150);
		
		// Buttons
		start =    new Rect((Screen.width-200)/2, (Screen.height-100)/2-MARGIN, 200, 100);
		stop =     new Rect((Screen.width-200)/2, (Screen.height+100)/2+MARGIN, 200, 100);
		
		// *** DEBUG
		debug =    new Rect(200, Screen.height-100-MARGIN, Screen.width-200*2, 100);
		// *** DEBUG
		
		targetText = "Behind\n";
		distanceText = "Distance\n";
		timeText = "Time\n";
		caloriesText = "Calories\n";
		paceText = "Pace/KM\n";
	
		ji.Start();
	}
	
	// Update is called once per frame
	void Update ()
	{
#if UNITY_EDITOR
		ji.Simulate();
#endif	
	}
	
	void OnGUI ()
	{
		GUI.skin.box.wordWrap = true;
		
		GUI.Box(target, targetText+SiDistance( ji.DistanceBehindTarget() ));
		GUI.Box(distance, distanceText+SiDistance( ji.Distance() ));
		GUI.Box(time, timeText+Timestamp( ji.Time() ));
		GUI.Box(calories, caloriesText + ji.Calories());
		GUI.Box(pace, paceText+Timestamp( ji.Pace() ));
		GUI.Box(map, "TODO");
/*		if (GUI.Button (start, startText)) {
			print ("You clicked the start!");
		}
		if (GUI.Button (stop, stopText)) {
			print ("You clicked the stop!");
		}
*/		// *** DEBUG
		GUI.Box(debug, debugText + ji.DebugLog());
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
	
	string Timestamp(long milliseconds) {
		TimeSpan span = TimeSpan.FromMilliseconds(milliseconds);

		return string.Format("{0:00}:{1:00}:{2:00}",span.Minutes,span.Seconds,span.Milliseconds/10);	
	}
}

