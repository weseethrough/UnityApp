using UnityEngine;
using System.Collections;
using System;

public class GlassDemoGUI : MonoBehaviour {
	
	// The started variables
	public bool started = false;
	private float countTime = 3.0f;
	
	// The margin and submargin to pad the boxes
	private const int MARGIN = 15;
	private const int SUBMARGIN = 5;
	
	// Opacity of the boxes
	private const float OPACITY = 0.5f;
	
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
	
	// Icons
	private Rect gpsLock;
	
	// Debug
	private Rect debug;
	private string debugText;
	
	private float originalWidth = 800;  // define here the original resolution
  	private float originalHeight = 500; // you used to create the GUI contents 
 	private Vector3 scale;

	// Background textures
	Texture2D normal;
	Texture2D info;
	Texture2D warning;
	
	private bool authenticated = false;
	
	void Start () {
		
		// Left side bottom
		distance = new Rect(MARGIN, originalHeight-MARGIN-100, 200, 100);
		time =     new Rect(MARGIN, distance.y-SUBMARGIN-100, 200, 100);
		
		// Right side top
		calories = new Rect(originalWidth-MARGIN-200, MARGIN, 200, 100);
		pace =     new Rect(originalWidth-MARGIN-200, calories.y+SUBMARGIN+100, 200, 100);
		
		// Icons
		gpsLock =  new Rect((originalWidth/2)-150, MARGIN, 300, 100);
		
		// *** DEBUG
		debug =    new Rect((originalWidth-200)/2, originalHeight-MARGIN-100, 200, 100);
		// *** DEBUG

		distanceText = "Distance\n";
		timeText = "Time\n";
		caloriesText = "Calories\n";
		paceText = "Pace\n";	
		
		// White coloured box
		Color white = new Color(0.9f, 0.9f, 0.9f, OPACITY);
		normal = new Texture2D(1, 1);
		normal.SetPixel(0,0,white);
		normal.Apply();
		
		// Green coloured box
		Color green = new Color(0f, 0.9f, 0f, OPACITY);
		info = new Texture2D(1, 1);
		info.SetPixel(0,0,green);
		info.Apply();
		
		// Red coloured box
		Color red = new Color(0.9f, 0f, 0f, OPACITY);
		warning = new Texture2D(1, 1);
		warning.SetPixel(0,0,red);
		warning.Apply();
		
	}
	
	// Update is called once per frame
	void Update ()
	{		
		// Reduces the countdown timer if there is a lock
		if(started && countTime > -1.0f && Platform.Instance.HasLock())
		{
			countTime -= Time.deltaTime;
		}
		
		// Resets the game if back is pressed.
		if(Input.GetKeyDown(KeyCode.Escape))
		{
			if(countTime < 0.0f)
			{
				Platform.Instance.Reset();
				Application.LoadLevel(Application.loadedLevel);
			} else
			{
				Platform.Instance.Reset();
				Application.LoadLevel(0);
			}
		}
				
		Platform.Instance.Poll();
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
		GUI.depth = 10;
		// Setting label font size
		GUI.skin.label.fontSize = 15;
		
		// Setting GUI box attributes
		GUI.skin.box.wordWrap = true;
		GUI.skin.box.fontSize = 30;
		GUI.skin.box.fontStyle = FontStyle.Bold;
		GUI.skin.box.alignment = TextAnchor.MiddleCenter;				
		GUI.skin.box.normal.background = normal;
		GUI.skin.box.normal.textColor = Color.black;
		
		if(!Platform.Instance.HasLock())
		{
			GUI.Label(gpsLock, "Waiting for GPS Lock...");
		}
		
		// *** DEBUG? TODO: Icon? Message?
		float bearing = Platform.Instance.Bearing();
		double bearingRad = bearing*Math.PI/180;
		
		// Distance
		double selfDistance = Platform.Instance.Distance();
		
		GUI.Box(distance, distanceText+SiDistance( selfDistance));
		
		// Time
		GUI.Box(time, timeText+TimestampMMSSdd( Platform.Instance.Time() ));
		
		// Calories
		GUI.Box(calories, caloriesText + Platform.Instance.Calories());
		
		// pace
		//GUI.Box(pace, paceText+TimestampMMSS(speedToKmPace( Platform.Instance.Pace() )) );
		GUI.Box(pace, paceText + Platform.Instance.Pace().ToString("f2") + " Min/Mile");
		
		GUI.matrix = svMat; // restore matrix
	}
	
	/// <summary>
	/// Converts the distance to miles.
	/// </summary>
	/// <returns>
	/// The distance in miles.
	/// </returns>
	/// <param name='meters'>
	/// The distance in meters.
	/// </param>
	string SiDistance(double meters) {
		string postfix = " Miles";
		string final;
		float value = (float)meters/1609.344f;
		final = value.ToString("f2");
		return final+postfix;
	}
	
	/// <summary>
	/// converts the speed to minutes per mile.
	/// </summary>
	/// <returns>
	/// The converted pace.
	/// </returns>
	/// <param name='speed'>
	/// The initial speed.
	/// </param>
	float SpeedToMilesPace(float speed) {
		if (speed <= 0) {
			return 0;
		}
		// m/s -> mins/Km
		float x = (speed / 1609.344f) * 60.0f;
		return 1.0f/x;
	}
	
	/// <summary>
	/// Changes the time to the correct format.
	/// </summary>
	/// <returns>
	/// The time in minutes-seconds-milliseconds.
	/// </returns>
	/// <param name='milliseconds'>
	/// The time in milliseconds.
	/// </param>
	string TimestampMMSSdd(long milliseconds) {
		TimeSpan span = TimeSpan.FromMilliseconds(milliseconds);

		return string.Format("{0:00}:{1:00}:{2:00}",span.Minutes,span.Seconds,span.Milliseconds/10);	
	}
	
	/// <summary>
	/// Converts the time to the correct format.
	/// </summary>
	/// <returns>
	/// The time in minutes-seconds.
	/// </returns>
	/// <param name='minutes'>
	/// Time in minutes.
	/// </param>
	string TimestampMMSS(long minutes) {
		TimeSpan span = TimeSpan.FromMinutes(minutes);

		return string.Format("{0:00}:{1:00}",span.Minutes,span.Seconds);	
	}
}
