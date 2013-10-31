using UnityEngine;
using System.Collections;

public class SettingsScreen : MonoBehaviour {
	
	// Bools for various states
	public bool menuOpen = false;
	private bool changed = false;
	public bool indoor = true;
	
	// Enums for the targets
	public enum Targets
	{
		Runner			= 1,
		Eagle			= 2,
		Train			= 3,
		Zombie			= 4
	}
	
	private Targets currentTarget = Targets.Runner;
	private Rect debug;
	private const int MARGIN = 15;
	private bool authenticated = false;
	
	// Holds game objects to set them to active/inactive
	public GameObject eagleHolder;
	public GameObject runnerHolder;
	public GameObject zombieHolder;
	public GameObject trainHolder;
	
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
	
	void Start () {
		// Set indoor mode
		Platform.Instance.setIndoor(indoor);
		
		// Calculate and set scale
		float x = (float)Screen.width/originalWidth;
		float y = (float)Screen.height/originalHeight;
		scale = new Vector3(x, y, 1);
		
		// Set debug box
		debug = new Rect((originalWidth-100), 0, 100, 100);
		
		// Set holders active status
		setTargets();
		GetComponent<GetTrack>().setActive(false);
	}
	
	void OnGUI() {
		// Set matrix, depth and various skin sizes
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
		GUI.depth = 5;
		GUI.skin.button.fontSize = 15;
		GUI.skin.horizontalSliderThumb.fixedWidth = 60;
		GUI.skin.horizontalSliderThumb.fixedHeight = 60;
		
		// If the menu is open
		if(menuOpen)
		{			
			// Draw the black texture
			GUI.DrawTexture(new Rect(0,0,originalWidth,originalHeight), blackTexture);
				
			// Set increased label size for speed guide
			GUI.skin.label.fontSize = 30;		
			GUI.Label(new Rect(originalWidth/2-100, 10, 200, 40), "Speed Guide");
			
			// Reduce label size and set rest of speed guide
			GUI.skin.label.fontSize = 15;
			GUI.Label(new Rect(originalWidth/2 -100, 50, 200, 40), "Walking: 1.25m/s");
			GUI.Label (new Rect(originalWidth/2 - 100, 90, 200, 40), "Jogging: 2.2m/s");
			GUI.Label(new Rect(originalWidth/2 - 100, 130, 200, 40), "Running: 4.2m/s");
			GUI.Label(new Rect(originalWidth/2 - 100, 170, 200, 40), "Usain Bolt: 10.4m/s");
			
			// If runner is pressed, deactivate the rest and set a new offset
			if(GUI.Button(new Rect(0, originalHeight-200, 200, 200) , "Runner"))
			{
				currentTarget = Targets.Runner;
				changed = true;
				GetComponent<TargetDisplay>().setOffset(0);
			}
			
			// If an eagle is pressed, deactivate the rest and set the new offset
			if(GUI.Button(new Rect (200, originalHeight-200, 200, 200), "Eagle"))
			{				
				currentTarget = Targets.Eagle;
				changed = true;
				GetComponent<TargetDisplay>().setOffset(50);
			}
			
			// If an zombie is pressed, deactivate the rest and set the new offset
			if(GUI.Button(new Rect (400, originalHeight-200, 200, 200), "Zombie"))
			{
				currentTarget = Targets.Zombie;
				changed = true;
				GetComponent<TargetDisplay>().setOffset(20);
			}
			
			// If an train is pressed, deactivate the rest and set the new offset
			if(GUI.Button(new Rect(600, originalHeight-200, 200, 200), "Train"))
			{
				currentTarget = Targets.Train;
				changed = true;
				GetComponent<TargetDisplay>().setOffset(50);
			}
			
			
			// Set the indoor/outdoor mode
			if(GUI.Button(new Rect(0, 0, 100, 100), indoorText))
			{
				if(indoor) {
					indoor = false;
					UnityEngine.Debug.Log("Outdoor mode active");
					indoorText = "Outdoor Active";
				}
				else {
					indoor = true;
					UnityEngine.Debug.Log("Indoor mode active");
					indoorText = "Indoor Active";
				}
				changed = true;
			}
			
			// Set the speed slider, if the value has changed set the new speed
			float temp  = GUI.HorizontalSlider(new Rect((originalWidth/2)-100, 250, 200, 50), targSpeed,  1.25f, 10.4f);
    		GUI.Label(new Rect(originalWidth/2 + 120, 250, 100, 50), temp.ToString("f2") + "m/s");
			if(temp != targSpeed)
			{
				changed = true;
				targSpeed = temp;
			}
			
			// When the back button is pressed
			if (GUI.Button(new Rect(10, ((originalHeight)/2)-50, 100, 50), "Back"))
			{
        	    menuOpen = false;
				
				// If anything has changed
				if(changed) {
					// Reset platform, set new target speed and indoor/outdoor mode
					Platform.Instance.reset();
					Platform.Instance.resetTargets();
					
					setTargets();
					
					if(!trackSelected) {
						Platform.Instance.setTargetSpeed(targSpeed);
					}
					
					Platform.Instance.setIndoor(indoor);
					
					// Start countdown again
					started = false;
					countdown = false;
					countTime = 3.0f;
					
					// Reset bools
					trackSelected = false;
					changed = false;
				} else {
					// Else restart tracking
					Platform.Instance.StartTrack();
				}
			}
			
			// If not authenticated and button pressed, authenticate
			if (!authenticated && GUI.Button(debug, "Authenticate")) {
				Platform.Instance.authenticate();
				// TODO: check result
				authenticated = true;
			}
			
			// If authenticated and button pressed, sync to server
			if (authenticated && GUI.Button(debug, "Sync to server")) {
				Platform.Instance.syncToServer();
			}
			
			// If set track button pressed, open map selection menu
			if(GUI.Button(new Rect(originalWidth-100, originalHeight/2 - 50, 100, 100), "Set Track")) {
				menuOpen = false;
				mapOpen = true;
				GetComponent<GetTrack>().setActive(true);
			}
		} 
		// Else if map selection screen open
		else if(mapOpen) 
		{
			// Draw black texture
			GUI.DrawTexture(new Rect(0,0,originalWidth,originalHeight), blackTexture);
			
			// Set the back button
			if (GUI.Button(new Rect(0, originalHeight-50, 100, 50), "Back")){
				// Set bools
        		mapOpen = false;
				menuOpen = true;
				
				// Deactivate component, and if track is changed set boolean to true
				GetComponent<GetTrack>().setActive(false);
				if(GetComponent<GetTrack>().isChanged()) {
					GetComponent<GetTrack>().setChanged(false);
					changed = true;
					trackSelected = true;
				}
			}			
		}
		else 
		{
			// Else display options button
			if (GUI.Button(new Rect(10, ((originalHeight)/2)-50, 100, 50), "Options")){
				// Open the menu and pause tracking
        		menuOpen = true;
				Platform.Instance.stopTrack();
			}
		}
		
		if(countdown)
		{
			GUI.skin.label.fontSize = 40;
			
			// Get the current time rounded up
			int cur = Mathf.CeilToInt(countTime);
			
			// Display countdown on screen
			if(countTime > 0.0f)
			{
				GUI.Label(new Rect(300, 150, 200, 200), cur.ToString()); 
			}
			else if(countTime > -1.0f && countTime < 0.0f)
			{
				GUI.Label(new Rect(300, 150, 200, 200), "GO!"); 
			}
		}
		GUI.matrix = Matrix4x4.identity;
	}
	
	void Update () {
		// If there is a GPS lock or indoor mode is active
		if(Platform.Instance.hasLock() || indoor)
		{
			// Initiate the countdown
			countdown = true;
		 	if(countTime <= -1.0f && !started)
			{
				Platform.Instance.StartTrack();
				UnityEngine.Debug.LogWarning("Tracking Started");
				started = true;
			}
			else if(countTime > -1.0f)
			{
				UnityEngine.Debug.LogWarning("Counting Down");
				countTime -= Time.deltaTime;
			}
		}
	}
	
	// Set the targets based on the enums
	void setTargets() {
		eagleHolder.SetActive(false);
		runnerHolder.SetActive(false);
		trainHolder.SetActive(false);
		zombieHolder.SetActive(false);
				
		switch(currentTarget) {
		case Targets.Eagle:
			eagleHolder.SetActive(true);
			break;
		case Targets.Runner:
			runnerHolder.SetActive(true);
			break;
		case Targets.Train:
			trainHolder.SetActive(true);
			break;
		case Targets.Zombie:
			zombieHolder.SetActive(true);
			break;
		default:
			break;
		}
	}
}
