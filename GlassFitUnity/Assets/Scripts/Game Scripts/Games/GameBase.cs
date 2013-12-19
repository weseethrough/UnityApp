﻿using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

/// <summary>
/// Game base. Base Game class which will handle aspects common to all games. Including
/// 	Indoor/Outer management
/// 	Handling when the run is finished
/// 	Updating values for the UI (cals, dist, etc.)
/// 	Countdown
/// </summary>
public class GameBase : MonoBehaviour {
	
	// Variables to set the scale
	private int originalHeight = 500;
	private int originalWidth = 800;
	private Vector3 scale;
	
	// Bools for various states
	public bool menuOpen = false;
	protected bool gameParamsChanged = false;			//has something in the settings changed which will mean we should restart
	public bool indoor = true;
	private bool hasEnded = false;
	
	// Bonus distance milestones
	protected int bonusTarget = 1000;
	
	// Final sprint bonus points.		TODO is this a feature of ALL games or just some?
	protected float finalBonus = 1000;
	
	protected double offset = 0;
	
	// Marker for the finish line
	public GameObject finishMarker;	
	
	// Distance the player wants to run
	protected int finish;
	
	List<Challenge> challenges;
	
	// Holds actor instances. Assume we will always have at least one actor in a game, even a non-AR one.
	public List<GameObject> actors = new List<GameObject>();
		
	// Start tracking and 3-2-1 countdown variables
	protected bool started = false;
	protected bool countdown = false;
	protected float countTime = 3.0f;
	protected bool readyToStart = false;	//becomes true when the user resets the gyro. Countdown can start when this is true.
	
	// Texture for black background
	public Texture blackTexture;
	
	// Bools to check if map is open and whether track has been selected
	private bool mapOpen = false;
	private bool trackSelected = false;
	
	private string indoorText = "Indoor Active";
	
	// Multiplier variables
	private float baseMultiplier;
	private String baseMultiplierString;
	private float baseMultiplierStartTime;
	
	// Target speed
	public float targSpeed = 1.8f;
	
	private bool pause = false;
	
	private GestureHelper.OnTap tapHandler = null;
	
	private GestureHelper.TwoFingerTap twoTapHandler = null;
	
	private GestureHelper.DownSwipe downHandler = null;
	
		/// <summary>
	/// Start this instance.
	/// </summary>
	public virtual void Start () 
	{
		// Calculate and set scale
		float x = (float)Screen.width/originalWidth;
		float y = (float)Screen.height/originalHeight;
		scale = new Vector3(x, y, 1);
		
		tapHandler = new GestureHelper.OnTap(() => {
			PauseGame();
		});
		
		GestureHelper.onTap += tapHandler;
		
		downHandler = new GestureHelper.DownSwipe(() => {
			FlowState fs = FlowStateMachine.GetCurrentFlowState();
			GConnector gConnect = fs.Outputs.Find(r => r.Name == "MenuExit");
			if(gConnect != null) {
				GestureHelper.onSwipeDown -= downHandler;
				fs.parentMachine.FollowConnection(gConnect);
				Platform.Instance.StopTrack();
				Platform.Instance.Reset();
				AutoFade.LoadLevel("Game End", 0.1f, 1.0f, Color.black);
			}
		});
		GestureHelper.onSwipeDown += downHandler;
		//Get target distance
#if !UNITY_EDITOR
		finish = (int)DataVault.Get("finish");

#endif	
		UnityEngine.Debug.Log("BaseGame: finish distance is " + finish);
		//retrieve or create list of challenges
		challenges = DataVault.Get("challenges") as List<Challenge>;
		if (challenges == null) challenges = new List<Challenge>(0);
				
		indoor = (bool)DataVault.Get("indoor_settings");
		
		UnityEngine.Debug.Log("GameBase: indoor set to: " + indoor.ToString());
		
		// Set indoor mode
		Platform.Instance.SetIndoor(indoor);
		Platform.Instance.StopTrack();
		Platform.Instance.Reset();
		//DataVault.Set("indoor_text", "Indoor Active");
		
		hasEnded = false;
		
		twoTapHandler = new GestureHelper.TwoFingerTap(() => {
			GyroDidReset();
			GestureHelper.onTwoTap -= twoTapHandler;
		});
		
		GestureHelper.onTwoTap += twoTapHandler;
		
		//handler for OnReset
		//this seems to get automatically called as soon as the scene loads. Trying an onTap instead.	
		
//		GestureHelper.OnTap handler = null;
//		handler = new GestureHelper.OnTap( () => {
//			Platform.Instance.ResetGyro();
//			GyroDidReset();	
//			GestureHelper.onTap -= handler;
//		});
//		GestureHelper.onTap += handler;
		
		
		UnityEngine.Debug.Log("GameBase: started");
		UnityEngine.Debug.Log("GameBase: ready = " + readyToStart);
	}
	
	public void PauseGame()
	{
		if(!pause)
		{
			pause = true;
			Platform.Instance.StopTrack();
			FlowState fs = FlowStateMachine.GetCurrentFlowState();
			GConnector gConnect = fs.Outputs.Find(r => r.Name == "PauseExit");
		 	if(gConnect != null)
			{
				fs.parentMachine.FollowConnection(gConnect);
			} else
			{
				UnityEngine.Debug.Log("GameBase: Can't find exit - PauseExit");
			}
		} else {
			pause = false;
			FlowState fs = FlowStateMachine.GetCurrentFlowState();
			GConnector gConnect = fs.Outputs.Find(r => r.Name == "ReturnExit");
		 	if(gConnect != null)
			{
				fs.parentMachine.FollowConnection(gConnect);
			} else
			{
				UnityEngine.Debug.Log("GameBase: Can't find exit - PauseExit");
			}
			
			if(started)
			{
				Platform.Instance.StartTrack();
			} 
			else
			{
				countdown = false;
				countTime = 3.0f;
			}
		}
	}
	
	/// <summary>
	/// Toggles whether we are indoors. Name should be changed to 'ToggleIndoor()';
	/// Not sure where this is called from. AH
	/// </summary>
		public void SetIndoor() {
		if(indoor) {
			indoor = false;
			UnityEngine.Debug.Log("Outdoor mode active");
			DataVault.Set("indoor_text", "Outdoor Active");
			indoorText = "Outdoor Active";
		}
		else {
			indoor = true;
			UnityEngine.Debug.Log("Indoor mode active");
			indoorText = "Indoor Active";
			DataVault.Set("indoor_text", indoorText);
		}
		gameParamsChanged = true;
	}
	
	/// <summary>
	/// Called on returning to game when exiting the Settings menu.
	/// Should maybe be called 'OnResume()' instead? AH
	/// </summary>
	public void Back() 
	{
		
		float settingsTargetSpeed = ((float)DataVault.Get("slider_val") * 9.15f) + 1.25f;
		//UnityEngine.Debug.Log("Settings: New speed is: " + settingsTargetSpeed.ToString());
		if(settingsTargetSpeed != targSpeed)
		{
			gameParamsChanged = true;
			targSpeed = settingsTargetSpeed;
		}
		
		//restart if settings changed.
		if(gameParamsChanged)
		{
// Reset platform, set new target speed and indoor/outdoor mode
			Platform.Instance.Reset();
			Platform.Instance.ResetTargets();
					
			if(!trackSelected) {
				Platform.Instance.CreateTargetTracker(targSpeed);
			}
			
			Platform.Instance.SetIndoor(indoor);			
			
			// Start countdown again
			started = false;
			countdown = false;
			countTime = 3.0f;
					
			// Reset bools
			trackSelected = false;
			gameParamsChanged = false;
		} 
		else //game params didn't change
		{
			// Else restart tracking

			Platform.Instance.StartTrack();
		}
	}
	
	protected GUIStyle getLabelStyle()
	{
		// set style for our labels
		GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
		labelStyle.alignment = TextAnchor.UpperCenter;
		labelStyle.fontSize = 40;
		labelStyle.fontStyle = FontStyle.Bold;
		
		return labelStyle;
	}
	
	public virtual void OnGUI() {
		// Set matrix, depth and various skin sizes
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
		GUI.depth = 5;
		
		GUIStyle labelStyle = getLabelStyle();
		
		if(pause)
		{
			labelStyle.fontSize = 60;
			GUI.Label(new Rect(250, 150, 300, 200), "PAUSED", labelStyle);
		}
		
		labelStyle.fontSize = 40;
		
		Rect messageRect = new Rect(250, 150, 300, 200);
		
		if(countdown && !pause)
		{
			// Get the current time rounded up
			int cur = Mathf.CeilToInt(countTime);
			
			// Display countdown on screen
			if(countTime > 0.0f)
			{
				GUI.Label(messageRect, cur.ToString(), labelStyle); 
			}
			else if(countTime > -1.0f && countTime < 0.0f)
			{
				GUI.Label(messageRect, "GO!", labelStyle); 
			}
		}
		
		//feedback to user
		if(!readyToStart && !pause && !hasEnded)
		{
			//are we waiting for GPS?
			if(!Platform.Instance.HasLock() && !indoor)
			{
				GUI.Label(messageRect, "Awaiting GPS lock...", labelStyle);
			}
			else
			{
				//notify if we're indoor
				if(indoor)
				{
					GUIStyle indoorTextStyle = new GUIStyle(labelStyle);
					indoorTextStyle.fontSize -= 10;
					GUI.Label(new Rect(messageRect.xMin, 15, messageRect.width, 100), "Indoor mode", indoorTextStyle);
				}
				
				GUI.Label(messageRect, "Centre View to start", labelStyle);
			}
		}
		
		GUI.matrix = Matrix4x4.identity;
		
		// Display a message if the multiplier has changed in the last second and a half
		// See NewBaseMultiplier method in this class for more detail on how this is set
		if(started && baseMultiplierStartTime > (Time.time - 1.5f)) {
			GUI.Label(messageRect, baseMultiplierString, labelStyle);
		}
	}
	
		protected void UpdateAhead() {

		double targetDistance = Platform.Instance.GetHighestDistBehind() - offset;

		if (targetDistance > 0) {
			DataVault.Set("ahead_header", "Behind!");
			//DataVault.Set("ahead_col_header", "D20000FF");
			DataVault.Set("ahead_col_box", "D20000EE");
		} else {
			DataVault.Set("ahead_header", "Ahead!"); 
			DataVault.Set("ahead_col_box", "19D200EE");
			//DataVault.Set("ahead_col_header", "19D200FF");
		}
		DataVault.Set("ahead_box", SiDistance(Math.Abs(targetDistance)));
	}
	
	// Update is called once per frame
	public virtual void Update () 
	{
		//Update variables for GUI	
		Platform.Instance.Poll();
		
		DataVault.Set("calories", Platform.Instance.Calories().ToString());
		DataVault.Set("pace", Platform.Instance.Pace().ToString("f2") + "m/s");
		DataVault.Set("distance", SiDistance(Platform.Instance.Distance()));
		DataVault.Set("time", TimestampMMSSdd( Platform.Instance.Time()));
		DataVault.Set("indoor_text", indoorText);
		
		DataVault.Set("rawdistance", Platform.Instance.Distance());
		DataVault.Set("rawtime", Platform.Instance.Time());
		
		//This is currently in the derived game class.
		//UpdateLeaderboard();

		// TODO: Toggle based on panel type
		UpdateAhead();
		
		//detect the touch and reset/start if it's there
		// Non-Glass devices
		if(!readyToStart && (Platform.Instance.HasLock() || indoor) )
		{
			//UnityEngine.Debug.Log("GameBase: Update: Not ready to start");
			if(Input.touchCount > 0)
			{
				UnityEngine.Debug.Log("GameBase: Update: Touch detected");
					if(Platform.Instance.HasLock() || indoor)
					{
						UnityEngine.Debug.Log("GameBase: Now ready to start");
						readyToStart = true;
					}
			}
			//In the editor, just go straight away
#if UNITY_EDITOR
			readyToStart = true;
#endif
		}
	
		//start the contdown once we've got reset the gyro		
		if(readyToStart)
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
		
		// Awards the player points for running certain milestones
		if(Platform.Instance.Distance() >= bonusTarget)
		{
			int targetToKm = bonusTarget / 1000;
			if(bonusTarget < finish) 
			{
				MessageWidget.AddMessage("Bonus Points!", "You reached " + targetToKm.ToString() + "km! 1000pts", "trophy copy");
			}
			bonusTarget += 1000;
			
		}
		
		//if we're finished, hide the map, and progress to the finished menu
		//currently firing when targetDistance is 0...
		if(Platform.Instance.Distance() / 1000 >= finish && !hasEnded)
		{
			hasEnded = true;
			DataVault.Set("total", Platform.Instance.GetCurrentPoints() + Platform.Instance.GetOpeningPointsBalance());
			DataVault.Set("bonus", (int)finalBonus);
			Platform.Instance.StopTrack();
			
			//hide the  map
			GameObject h = GameObject.Find("minimap");
			if(h != null)
			{
				h.renderer.enabled = false;
			}
			
			//go to the 'finish' menu
			FlowState fs = FlowStateMachine.GetCurrentFlowState();
			GConnector gConect = fs.Outputs.Find(r => r.Name == "FinishButton");
			if(gConect != null) {
				//finish = finish * 10;
				fs.parentMachine.FollowConnection(gConect);
			} else {
				UnityEngine.Debug.Log("Game: No connection found!");
			}
		}
		
	}
	
	//TODO move these to a utility class
	protected string SiDistance(double meters) {
		string postfix = "m";
		string final;
		float value = (float)meters;
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
	
	protected long SpeedToKmPace(float speed) {
		if (speed <= 0) {
			return 0;
		}
		// m/s -> mins/Km
		return Convert.ToInt64( ((1/speed)/60) * 1000);
	}
	
	protected string TimestampMMSSdd(long milliseconds) {
		TimeSpan span = TimeSpan.FromMilliseconds(milliseconds);

		return string.Format("{0:00}:{1:00}:{2:00}",span.Minutes,span.Seconds,span.Milliseconds/10);	
	}
	protected string TimestampMMSS(long minutes) {
		TimeSpan span = TimeSpan.FromMinutes(minutes);

		return string.Format("{0:00}:{1:00}",span.Minutes,span.Seconds);	
	}
	
	public void NewBaseMultiplier(String message) {
		// format the multiplier to 2 sig figs:
		float f = 0.0f;
		float.TryParse(message, out f);
		if (f == 1.0f && this.baseMultiplier > 1.0f) {
			this.baseMultiplierString = "Multiplier lost!";  // multiplier reset
		} else if (f > 1.0f && f != this.baseMultiplier) {
			this.baseMultiplierString = f.ToString("G") + "x Multiplier!";  // multiplier increased
		}
		// save the value for next time
		this.baseMultiplier = f;
		// remember the current time so we know how long to display for:
		this.baseMultiplierStartTime = Time.time;
		UnityEngine.Debug.Log("New base multiplier received:" + this.baseMultiplier);
	}
	
	/// <summary>
	/// handler for when the gyro is reset.
	/// In this case, indicate that we're ready to start, if we previously weren't
	/// </summary>
	public void GyroDidReset()
	{
		UnityEngine.Debug.Log("GameBase: Gyro did reset");
		if(!readyToStart)
		{
			if(Platform.Instance.HasLock() || indoor)
			{
				UnityEngine.Debug.Log("GameBase: Now ready to start");
				readyToStart = true;
			}
		}
	}
}
