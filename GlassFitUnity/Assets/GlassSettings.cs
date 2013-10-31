using UnityEngine;
using System.Collections;

public class GlassSettings : MonoBehaviour {
	// Bools for various states
	public bool menuOpen = false;
	private bool changed = false;
	public bool indoor = true;
	
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
	
	private string indoorText = "Indoor Active";
	
	void Start () {
		// Set indoor mode
		//Platform.Instance.reset();
		Platform.Instance.setIndoor(false);
		//Platform.Instance.resetTargets();
		// Calculate and set scale
		float x = (float)Screen.width/originalWidth;
		float y = (float)Screen.height/originalHeight;
		scale = new Vector3(x, y, 1);
		
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
										
					Platform.Instance.setTargetSpeed(targSpeed);
										
					// Start countdown again
					started = false;
					countdown = false;
					countTime = 3.0f;
					
					// Reset bools
					changed = false;
				} else {
					// Else restart tracking
					Platform.Instance.StartTrack();
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
		
		if(Scriptholder.Instance.isTapped) {
			GUI.Label(new Rect(350, 0, 100, 50), "TOUCHPAD TAPPED");
		}
		if(Scriptholder.Instance.up) {
			GUI.Label(new Rect(350, 50, 100, 50), "FAST SWIPE UP");
		}
		if(Scriptholder.Instance.down) {
			GUI.Label(new Rect(350, 100, 100, 50), "FAST SWIPE DOWN");
		}
		if(Scriptholder.Instance.left) {
			GUI.Label(new Rect(350, 150, 100, 50), "FAST SWIPE LEFT");
		}
		if(Scriptholder.Instance.right) {
			GUI.Label(new Rect(350, 200, 100, 50), "FAST SWIPE RIGHT");
		}
		
		if(countdown)
		{
			GUI.skin.label.fontSize = 40;
			
			// Get the current time rounded up
			int cur = Mathf.CeilToInt(countTime);
			
			// Display countdown on screen
			if(countTime > 0.0f)
			{
				GUI.Label(new Rect(400, 200, 200, 200), cur.ToString()); 
			}
			else if(countTime > -1.0f && countTime < 0.0f)
			{
				GUI.Label(new Rect(400, 200, 200, 200), "GO!"); 
			}
		}
		
		if((GUI.Button(new Rect(275, 400, 100, 100), "START") || Scriptholder.Instance.right) && !countdown && Platform.Instance.hasLock()) {
			countdown = true;
		}
		
		if((GUI.Button(new Rect(425, 400, 100, 100), "RESET")  || Scriptholder.Instance.left)) {
			countdown = false;
			Platform.Instance.stopTrack();
			Platform.Instance.reset();
			countTime = 3.0f;
			started = false;
		}
		GUI.matrix = Matrix4x4.identity;
	}
	
	void Update () {
		
		// Initiate the countdown
		if(countdown) {
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
}
