using UnityEngine;
using System.Collections;

public class TrainingCamera : MonoBehaviour {
	
	// Rotation offset
	public Quaternion offsetFromStart;
	
	// 
	private bool gridSet = false;
	private bool avatarSet = false;
	private float scaleX;
	private float scaleY;
	public GameObject grid;
	private bool gridOn = false;
	private float gridTimer = 0.0f;
	private bool timerActive = false;
	private float subtitleTime = 0.0f;
	private Quaternion newOffset;
	public GameObject runner;
	
	Texture2D normal;

	// Set the grid, scale values and the initial offset
	void Start () {
		scaleX = (float)Screen.width / 800.0f;
		scaleY = (float)Screen.height / 500.0f;
		Color black = new Color(0.0f, 0.0f, 0.0f, 0.4f);
		normal = new Texture2D(1, 1);
		normal.SetPixel(0,0,black);
		normal.Apply();
	}
	
	void OnGUI()
	{		
		// Set the new GUI matrix based on scale and the depth
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scaleX,scaleY, 1));		
		GUI.depth = 7;
		GUI.skin.label.normal.background = normal;
		
		// If the grid has not been set.
		if(!gridSet)
		{
			// Show subtitles to make the user set the grid which change after a certain amount of time.
			if(subtitleTime < 5.0f) {
				GUI.Label(new Rect(300, 100, 200, 60), "Welcome to Race Yourself! In this tutorial we will teach you how to play the game.");
			} else if(subtitleTime < 12.0f) {
				GUI.Label(new Rect(300, 100, 200, 60), "Let's start with placing the grid. Align the grid with the floor and tap the touchpad to set it.");
			}
		} else if(!avatarSet) {
			// Next prompt the user to set the avatar by looking at it.
			if(transform.rotation.eulerAngles.y < 200 && transform.rotation.eulerAngles.y > 160) {
				// If the player looks at the avatar, set the reset the subtitle time and make the avatar go to the start position.
				subtitleTime = 0.0f;
				Platform.Instance.Reset();
				runner.GetComponent<TrainingController>().SetMove(true);
				avatarSet = true;
			} else {
				// Else prompt the user.
				GUI.Label(new Rect(300, 100, 200, 60), "Great! Now take a look around and try and spot the avatar you will be racing against");
			}
		} else if(!Platform.Instance.HasStarted()) {
			// Once the player has set the avatar, it checks for a GPS lock.
			if(!Platform.Instance.HasLock()){
				// If there is no lock, tell the player.
				GUI.Label(new Rect(300, 100, 200, 80), "There he is, he's just going to get into position and we can start once we have a lock on the GPS.");
				subtitleTime = 0.0f;
			} else {
				// Else prompt the player to start the game.
				if(subtitleTime < 8.0f) {
					GUI.Label(new Rect(300, 100, 200, 100), "We now have a lock. Press start whenever you are ready, the avatar will run at a constant jogging speed of 2.2m/s");
				}
			}
		}
		
		GUI.skin.label.normal.background = null;
		
		// Check if the button is being held.
		if(GUI.RepeatButton(new Rect(200, 0, 400, 250), "", GUIStyle.none))
		{ 
			// Activates the grid and reset the gyros if the timer is off, turns it off if the timer is on
			if(gridSet) {
				if(timerActive) {
					gridOn = false;
				} else {
					offsetFromStart = Platform.Instance.GetPlayerOrientation().AsQuaternion();
					Platform.Instance.ResetGyro();
					gridOn = true;
				}
				gridTimer = 5.0f;
			} else {
				gridSet = true;
				subtitleTime = 0.0f;
			}
		}
		else if(Event.current.type == EventType.Repaint)
		{
			// If the grid is on when the button is released, activate timer, else reset the timer and switch it off
			if(gridOn)
			{
				timerActive = true;
			} else
			{
				gridTimer = 0.0f;
				timerActive = false;
			}
		}	
		GUI.matrix = Matrix4x4.identity;
	}
	
	void Update () {
		// Set the new rotation of the camera
		newOffset = Quaternion.Inverse(offsetFromStart) * Platform.Instance.GetPlayerOrientation().AsQuaternion();
		
		subtitleTime += Time.deltaTime;
		
		// If the timer and grid are on, countdown the timer and switch it off if the timer runs out
		if(timerActive && gridOn)
		{
			gridTimer -= Time.deltaTime;
			if(gridTimer < 0.0f)
			{
				gridOn = false;
				timerActive = false;
			}
		}
		
		if(gridSet) {
			grid.SetActive(gridOn);
		}
		
		transform.rotation =  newOffset;		
	}
}
