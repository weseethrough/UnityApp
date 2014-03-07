using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
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
	
	//private bool indoor = true;
	

	//public bool indoor = true;

	protected bool hasEnded = false;
	
	protected bool maybeQuit = false;
	
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
	protected bool countdown = true;
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
	
	// Selected Track
	public Track selectedTrack = null;
	
	// Target speed
	public float targSpeed = 1.8f;
	
	private bool pause = false; // true when the game is paused, either due to a user-tap or auto-pause
	private bool autopause = false; // true when the game has automatically paused due to the user stopping runing
	
	protected GestureHelper.OnTap tapHandler = null;
	private GestureHelper.TwoFingerTap twoTapHandler = null;
	protected GestureHelper.OnBack backHandler = null;
	private GestureHelper.OnSwipeLeft leftHandler = null;
	private GestureHelper.OnSwipeRight rightHandler = null;

	protected GameObject theVirtualTrack;
	
	private int lastDistance;
	
	private float indoorTime;
	
	public bool showFps = false;
	
	//subclasses can override this if they are doing their own custom countdown (e.g. train game)
	protected virtual bool shouldDoGameBaseCountdown()
	{
		return true;
	}
	
	/// <summary>
	/// Start this instance.
	/// </summary>
	public virtual void Start () 
	{
		UnityEngine.Debug.Log("GameBase: setting scale");
		// Calculate and set scale
		float x = (float)Screen.width/originalWidth;
		float y = (float)Screen.height/originalHeight;
		scale = new Vector3(x, y, 1);
		
		UnityEngine.Debug.Log("GameBase: setting countdown subtitle");
		DataVault.Set("countdown_subtitle", " ");
		
		UnityEngine.Debug.Log("GameBase: setting tap handler");
		tapHandler = new GestureHelper.OnTap(() => {
			GameHandleTap();
		});
		GestureHelper.onTap += tapHandler;
		
		UnityEngine.Debug.Log("GameBase: setting down swipe handler");
		backHandler = new GestureHelper.OnBack(() => {
			QuitImmediately();
		});
		GestureHelper.onBack += backHandler;
		
		UnityEngine.Debug.Log("GameBase: setting indoor move text");
		DataVault.Set("indoor_move", " ");
		
//		leftHandler = new GestureHelper.OnSwipeLeft(() => {
//			HandleLeftSwipe();
//		});
//		GestureHelper.onSwipeLeft += leftHandler;
		
		//Get target distance
#if !UNITY_EDITOR
		
		UnityEngine.Debug.Log("GameBase: getting track");
		selectedTrack = (Track)DataVault.Get("current_track");
		
		UnityEngine.Debug.Log("GameBase: checking if track is null");
		if(selectedTrack != null) {
			finish = (int)selectedTrack.distance;
			UnityEngine.Debug.Log("GameBase: track distance is: " + finish.ToString());
		} else {
			finish = (int)DataVault.Get("finish");
		}

#endif	
		
#if RY_INDOOR
		SetReadyToStart(true);
		SetVirtualTrackVisible(true);
#endif
		UnityEngine.Debug.Log("GameBase: setting finish km");
		DataVault.Set("finish_km", SiDistanceUnitless(finish, string.Empty) );
		
		UnityEngine.Debug.Log("GameBase: finish distance is " + finish);
		UnityEngine.Debug.Log("GameBase: getting list of challenges");
		//retrieve or create list of challenges
		challenges = DataVault.Get("challenges") as List<Challenge>;
		if (challenges == null) challenges = new List<Challenge>(0);
		
		//indoor = Convert.ToBoolean(DataVault.Get("indoor_settings"));
		
		UnityEngine.Debug.Log("GameBase: resetting platform");
			
		//indoor = true;
		
		// Set indoor mode
		//Platform.Instance.LocalPlayerPosition.SetIndoor(indoor);
		Platform.Instance.LocalPlayerPosition.Reset();
		//DataVault.Set("indoor_text", "Indoor Active");
		
		hasEnded = false;
		
		//handler for OnReset
		//this seems to get automatically called as soon as the scene loads. Trying an onTap instead.	

//		GestureHelper.OnTap handler = null;
//		handler = new GestureHelper.OnTap( () => {
//			GyroDidReset();	
//			GestureHelper.onTap -= handler;
//		});
//		GestureHelper.onTap += handler;
		
		DataVault.Set("distance_units", "M");
		//DataVault.Set("time_units", "m:s:ds");

        DataVault.Set("minutes_unit_short", "MIN");
        DataVault.Set("seconds_unit_short", "SEC");
        DataVault.Set("pace_unit_short", "MIN/KM");
        DataVault.Set("calories_unit_short", "KCAL");
        
		
		UnityEngine.Debug.Log("GameBase: started");
		UnityEngine.Debug.Log("GameBase: ready = " + readyToStart);

        DataVault.Set("sweat_points_unit", "SWEAT POINTS");
		
		UpdateFPS fps = GetComponent<UpdateFPS>();
		
		if(fps != null)
		{
			if(showFps)
			{
				fps.enabled = true;
			} 
			else
			{
				fps.enabled = false;
				DataVault.Set("fps", "");
			}
		}
		else
		{
			DataVault.Set("fps", "");
		}
	}
	
	/// <summary>
	/// Quits immediately back to main menu, without user confirmation.
	/// </summary>
	protected void QuitImmediately()
	{
		//Flow to main hex menu
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		GConnector gc = fs.Outputs.Find( r => r.Name == "QuitImmediately");
		if(gc != null)
		{
			fs.parentMachine.FollowConnection(gc);
		}
		
		//clean up handlers
		GestureHelper.onBack -= backHandler;
		GestureHelper.onTap -= tapHandler;
		GestureHelper.onSwipeLeft -= leftHandler;
		GestureHelper.onSwipeRight -= rightHandler;
		
		//load env
		AutoFade.LoadLevel("Game End", 0.1f, 1.0f, Color.black);
	}
	
	public void SetVirtualTrackVisible(bool visible)
	{
		if(theVirtualTrack == null)
		{
			theVirtualTrack = GameObject.Find("VirtualTrack");
		}
		if(theVirtualTrack != null)
		{
			theVirtualTrack.SetActive(visible);
		}
		else
		{
			UnityEngine.Debug.Log("GameBase: Couldn't find virtual track to set visiblity");
		}
	}

	public void ConsiderQuit() {
		if(pause) {
			PauseGame();
		} else {
			FlowState fs = FlowStateMachine.GetCurrentFlowState();
			GConnector gConnect = fs.Outputs.Find(r => r.Name == "QuitExit");
			if(gConnect != null) {
				maybeQuit = true;
				Platform.Instance.LocalPlayerPosition.StopTrack();
				GestureHelper.onBack -= backHandler;
				fs.parentMachine.FollowConnection(gConnect);
				
				backHandler = new GestureHelper.OnBack(() => {
					ReturnGame();
				});
				GestureHelper.onBack += backHandler;
				
				leftHandler = new GestureHelper.OnSwipeLeft(() => {
					ReturnGame();
				});
				GestureHelper.onSwipeLeft += leftHandler;
				
				rightHandler = new GestureHelper.OnSwipeRight(() => {
					ReturnGame();
				});
				GestureHelper.onSwipeRight += rightHandler;
				
				GestureHelper.onTap -= tapHandler;
				
				tapHandler = new GestureHelper.OnTap(() => {
					hasEnded = true;
					finalBonus = 0;
					FinishGame();
					GestureHelper.onTap -= tapHandler;
				});
				GestureHelper.onTap += tapHandler;
			}		
		}
	}
	
	public virtual void QuitGame() {
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		GConnector gConnect = fs.Outputs.Find(r => r.Name == "MenuExit");
		if(gConnect != null) {
			GestureHelper.onBack -= backHandler;
			GestureHelper.onTap -= tapHandler;
			fs.parentMachine.FollowConnection(gConnect);
			AutoFade.LoadLevel("Game End", 0.1f, 1.0f, Color.black);
		}
	}
	
	public void ReturnGame() {
		
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		GConnector gConnect = fs.Outputs.Find(r => r.Name == "GameExit");
		if(gConnect != null) {
			maybeQuit = false;
			if(started) {
				Platform.Instance.LocalPlayerPosition.StartTrack();
			} else {
				countTime = 3.0f;
				countdown = false;
			}
			fs.parentMachine.FollowConnection(gConnect);
			GestureHelper.onBack -= backHandler;
			GestureHelper.onSwipeLeft -= leftHandler;
			GestureHelper.onSwipeRight -= rightHandler;
		
			backHandler = new GestureHelper.OnBack(() => {
				ConsiderQuit();
			});
			GestureHelper.onBack += backHandler;
			
			GestureHelper.onTap -= tapHandler;
			
			tapHandler = new GestureHelper.OnTap(() => {
				GameHandleTap();
			});
			
			GestureHelper.onTap += tapHandler;
			
			OnUnpause();
		}
	}
	
	/// <summary>
	/// Handles a left swipe gesture. Finishes the run by default. Can be overridden by game modes, esp tutorials.
	/// </summary>
	public virtual void HandleLeftSwipe() {
		//Taking this out for now. We don't need both the exit via swipe down AND this shortcut. The exit should go via the 'Finished' Menu.
		//FinishGame();	
	}
	
	public virtual GConnector GetFinalConnection() {
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		return fs.Outputs.Find(r => r.Name == "FinishButton");
	}
	
		/// <summary>
	/// Delegate function for Glass - when the user swipes back this is called to end the game
	/// </summary>
	protected void FinishGame()
	{
		OnFinishedGame();
		UnityEngine.Debug.Log("GameBase: Ending game");
		GConnector gConnect = GetFinalConnection();
		if(gConnect != null) {
			UnityEngine.Debug.Log("GameBase: final connection found");
			countdown = false;
			DataVault.Set("total", (Platform.Instance.PlayerPoints.CurrentActivityPoints + Platform.Instance.PlayerPoints.OpeningPointsBalance + finalBonus).ToString("n0"));
			DataVault.Set("distance_with_units", SiDistance(Platform.Instance.LocalPlayerPosition.Distance));
			UnityEngine.Debug.Log("GameBase: setting points");
			if(finish >= 1000) {
				DataVault.Set("bonus", finalBonus.ToString("n0")); 
			} else {
				DataVault.Set("bonus", 0);
			}
			Platform.Instance.LocalPlayerPosition.StopTrack();
			GestureHelper.onBack -= backHandler;
			GestureHelper.onTap -= tapHandler;
			GestureHelper.onSwipeLeft -= leftHandler;
			GestureHelper.onSwipeRight -= rightHandler;
			
			if(gConnect.Name == "TutorialExit") {
				AutoFade.LoadLevel("Game End", 0.1f, 1.0f, Color.black);
			} else {
				tapHandler = new GestureHelper.OnTap(() => {
					Continue();
				});
				GestureHelper.onTap += tapHandler;
			}
			FlowState fs = FlowStateMachine.GetCurrentFlowState();
			fs.parentMachine.FollowConnection(gConnect);
		} else {
			UnityEngine.Debug.Log("GameBase: No connection found - FinishButton");
		}
	}
	
	/// <summary>
	/// Raises the finished game event.
	/// Can be implemented by subclasses to do custom behaviour on finishing the run.
	/// </summary>
	protected virtual void OnFinishedGame()
	{
		return;
	}
	
	/// <summary>
	/// Part of the delegate function for Glass. When the user taps the screen it presses the continue button.
	/// </summary>
	void Continue() {
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		GConnector gConnect = fs.Outputs.Find(r => r.Name == "ContinueButton");
		if(gConnect != null) {
			//(gConnect.Parent as Panel).CallStaticFunction(gConnect.EventFunction, null);
			SoundManager.PlaySound(SoundManager.Sounds.Tap);
			fs.parentMachine.FollowConnection(gConnect);
			AutoFade.LoadLevel("Game End", 0.1f, 1.0f, Color.black);
		} else {
			UnityEngine.Debug.Log("GameBase: No connection found - ContinueButton");
		}
		GestureHelper.onTap -= tapHandler;
	}
	
	//handle a tap. Default is just to pause/unpause but games (especially tutorial, can customise this by overriding)
	public virtual void GameHandleTap() {
		if(started)
		{
			UnityEngine.Debug.Log("GameBase: tap detected");
			SoundManager.PlaySound(SoundManager.Sounds.Tap);
			PauseGame();
		}
	}
	
	public void PauseGame()
	{
		if(!pause)
		{
			//only pause if we've actually started, or the countdown has started
			if(started || countdown)
			{
				FlowState fs = FlowStateMachine.GetCurrentFlowState();
				GConnector gConnect = fs.Outputs.Find(r => r.Name == "PauseExit");
			 	if(gConnect != null)
				{
					pause = true;
					Platform.Instance.LocalPlayerPosition.StopTrack();
					fs.parentMachine.FollowConnection(gConnect);
				} else
				{
					UnityEngine.Debug.Log("GameBase: Can't find exit - PauseExit");
				}
			    DataVault.Set ("paused", "paused");
			}
		} else {
			UnityEngine.Debug.Log("GameBase: Pause pressed, turning off");
			pause = false;
			autopause = false; // user can exit auto-pause by swiping down
			FlowState fs = FlowStateMachine.GetCurrentFlowState();
			UnityEngine.Debug.Log("GameBase: flowstate obtained");
			GConnector gConnect = fs.Outputs.Find(r => r.Name == "ReturnExit");
		 	if(gConnect != null)
			{
				UnityEngine.Debug.Log("GameBase: found connection, following");
				fs.parentMachine.FollowConnection(gConnect);
			} else
			{
				UnityEngine.Debug.Log("GameBase: Can't find exit - PauseExit");
			}
			
			if(started)
			{
				UnityEngine.Debug.Log("GameBase: Starting to track");
				Platform.Instance.LocalPlayerPosition.StartTrack();
				UnityEngine.Debug.Log("GameBase: Track started successfully");
			} 
			else
			{
				countdown = false;
				countTime = 3.0f;
			}
			OnUnpause();
		}
	}
	
	protected virtual void OnUnpause() {
		//any extra steps which might be needed when we unpause. Intended to be overriden in child classes.
		return;
	}
	
	/// <summary>
	/// Toggles whether we are indoors. Name should be changed to 'ToggleIndoor()';
	/// Not sure where this is called from. AH
//	/// </summary>
//		public void SetIndoor() {
//		if(indoor) {
//			indoor = false;
//			UnityEngine.Debug.Log("Outdoor mode active");
//			DataVault.Set("indoor_text", "Outdoor Active");
//			indoorText = "Outdoor Active";
//		}
//		else {
//			indoor = true;
//			UnityEngine.Debug.Log("Indoor mode active");
//			indoorText = "Indoor Active";
//			DataVault.Set("indoor_text", indoorText);
//		}
//		gameParamsChanged = true;
//	}
	
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
			Platform.Instance.LocalPlayerPosition.Reset();
			Platform.Instance.ResetTargets();
					
			if(selectedTrack == null) {
				Platform.Instance.CreateTargetTracker(targSpeed);
			} else {
				Platform.Instance.CreateTargetTracker(selectedTrack.deviceId, selectedTrack.trackId);
			}
			
			//Platform.Instance.LocalPlayerPosition.SetIndoor(indoor);			
			
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

			Platform.Instance.LocalPlayerPosition.StartTrack();
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
	
//	public virtual void OnGUI() {
//		// Set matrix, depth and various skin sizes
//		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
//		GUI.depth = 5;
//		
//		GUIStyle labelStyle = getLabelStyle();
////		
////		if(pause)
////		{
////			labelStyle.fontSize = 60;
////			GUI.Label(new Rect(250, 150, 300, 200), "PAUSED", labelStyle);
////		}
//		
//		labelStyle.fontSize = 40;
//		
//		Rect messageRect = new Rect(250, 150, 300, 200);
//		
////		if(countdown && !pause)
////		{
////			// Get the current time rounded up
////			int cur = Mathf.CeilToInt(countTime);
////			
////			// Display countdown on screen
////			if(countTime > 0.0f)
////			{
////				GUI.Label(messageRect, cur.ToString(), labelStyle); 
////			}
////			else if(countTime > -1.0f && countTime < 0.0f)
////			{
////				GUI.Label(messageRect, "GO!", labelStyle); 
////			}
////		}
//		
//		GUI.matrix = Matrix4x4.identity;
//		
//		// Display a message if the multiplier has changed in the last second and a half
//		// See NewBaseMultiplier method in this class for more detail on how this is set
//		if(started && baseMultiplierStartTime > (Time.time - 1.5f)) {
//			GUI.Label(messageRect, baseMultiplierString, labelStyle);
//		}
//	
//	}
	
	protected virtual void UpdateAhead() {

		double targetDistance = GetDistBehindForHud();

		if (targetDistance > 0) {
			DataVault.Set("distance_position", "BEHIND");
			//DataVault.Set("ahead_col_header", "D20000FF");
            DataVault.Set("ahead_col_box", "E5312FFF");
		} else {
			DataVault.Set("distance_position", "AHEAD");
            DataVault.Set("ahead_col_box", "009540FF");
			//DataVault.Set("ahead_col_header", "19D200FF");
		}
		//UnityEngine.Debug.Log("GameBase: distance behind is " + GetDistBehindForHud().ToString());
		string siDistance = SiDistanceUnitless(Math.Abs(targetDistance), "target_units");
		//UnityEngine.Debug.Log("GameBase: setting target distance to: " + siDistance);
		DataVault.Set("ahead_box", siDistance);
	}
	
	protected virtual double GetDistBehindForHud() {
		return Platform.Instance.GetHighestDistBehind() - offset;
	}
	
	IEnumerator DoCountDown()
	{
		UnityEngine.Debug.Log("GameBase: Starting Countdown Coroutine");
		for(int i=3; i>=0; i--)
		{
			//set value for subtitle. 0 = GO
			string displayString = (i==0) ? "GO !" : i.ToString();
			DataVault.Set("countdown_subtitle", displayString);
			
			//wait half a second
			yield return new WaitForSeconds(1.0f);
		}
		//start the game
		DataVault.Set("countdown_subtitle", " ");
		StartRace();
	}
	
	// Update is called once per frame
	public virtual void Update () 
	{

		// Auto-pause if player has stopped for > 0.5secs
		if (started && !autopause && !pause
			&& !Platform.Instance.LocalPlayerPosition.IsIndoor()
			&& Platform.Instance.playerState == "STOPPED"
			&& (UnityEngine.Time.time-Platform.Instance.playerStateEntryTime > 1.0f))
		{
			UnityEngine.Debug.Log("Auto-pausing");
			PauseGame();
			if (pause) // if pausegame suceeded (sometimes it doesn't, e.g. if considering quit)
			{
				autopause = true;
				DataVault.Set ("paused","auto-paused");
			}
		}

		// Auto-un-pause if player has started moving again
		else if (started && autopause
			&& !Platform.Instance.LocalPlayerPosition.IsIndoor()
			&& Platform.Instance.playerState != "STOPPED"
			&& (UnityEngine.Time.time-Platform.Instance.playerStateEntryTime > 0.5f))
		{
			UnityEngine.Debug.Log("Auto-un-pausing");
			PauseGame();
			if (!pause) // if pausegame suceeded (sometimes it doesn't, e.g. if considering quit)
			{
				autopause = false;
			}

		}

		//Update variables for GUI	
		Platform.Instance.Poll();
		
		DataVault.Set("calories", Platform.Instance.LocalPlayerPosition.Calories.ToString()/* + "kcal"*/);
		float pace = SpeedToKmPace(Platform.Instance.LocalPlayerPosition.Pace);
		string paceString = (pace > 20.0f || pace == 0.0f) ? "--:--" : TimestampMMSSnearestTenSecs(pace); // show dashes if slower than slow walk, otherwise round to nearest 10s
		DataVault.Set("pace", paceString/* + "min/km"*/);
		DataVault.Set("distance", SiDistanceUnitless(Platform.Instance.LocalPlayerPosition.Distance, "distance_units"));
		DataVault.Set("time", TimestampMMSSfromMillis(Platform.Instance.LocalPlayerPosition.Time));
		DataVault.Set("indoor_text", indoorText);
		
		DataVault.Set("rawdistance", Platform.Instance.LocalPlayerPosition.Distance);
		DataVault.Set("rawtime", Platform.Instance.LocalPlayerPosition.Time);
        DataVault.Set("sweat_points", string.Format("{0:N0}", Platform.Instance.PlayerPoints.CurrentActivityPoints));


        TimeSpan span = TimeSpan.FromMilliseconds(Platform.Instance.LocalPlayerPosition.Time);
						
			/*return string.Format("{0:0}:{1:00}", span.Minutes, span.Seconds);
		} else {
			DataVault.Set("time_units", "h:m:s");
			return string.Format("{0:0}:{1:00}:{2:00}", span.Hours, span.Minutes, span.Seconds);
		}*/

        DataVault.Set("time_minutes_only", (int)(span.Minutes + span.Hours * 60));
        DataVault.Set("time_seconds_only", string.Format("{0:00}" ,span.Seconds));
		
		//This is currently in the derived game class.
		//UpdateLeaderboard();

		// TODO: Toggle based on panel type
		UpdateAhead();
		
		//start the contdown once we've got reset the gyro		
		if(readyToStart && shouldDoGameBaseCountdown() && countdown)
		{
			// Initiate the countdown
//			countdown = true;
//		 	if(countTime <= -1.0f && !started)
//			{
//				StartRace();
//			}
//			else if(countTime > -1.0f)
//			{
//				//UnityEngine.Debug.Log("Counting Down");
//				countTime -= Time.deltaTime;
//			}
			countdown = false;
			StartCoroutine("DoCountDown");
		}
		
		// Awards the player points for running certain milestones
		if(Platform.Instance.LocalPlayerPosition.Distance >= bonusTarget)
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
		if(Platform.Instance.LocalPlayerPosition.Distance >= finish && !hasEnded)
		{
			hasEnded = true;

			FinishGame();
		}
		
		if(Platform.Instance.LocalPlayerPosition.IsIndoor()) {
			if((int)Platform.Instance.LocalPlayerPosition.Distance == lastDistance) 
			{
				//UnityEngine.Debug.Log("GameBase: distance is the same, increasing time");
				if(started)
				{
					indoorTime += Time.deltaTime;
				}
				if(indoorTime > 10f) {
					//UnityEngine.Debug.Log("GameBase: setting text for indoor jogging");
					DataVault.Set("indoor_move", "Jog on the spot to move!");
				}
			} else {
				//UnityEngine.Debug.Log("GameBase: distance not the same, resetting");
				DataVault.Set("indoor_move", " ");
				lastDistance = (int)Platform.Instance.LocalPlayerPosition.Distance;
				indoorTime = 0f;
			}
		} else {
			//UnityEngine.Debug.Log("GameBase: outdoor is active");
			DataVault.Set("indoor_move", " ");
		}
		
	}
	
	/// <summary>
	/// Starts the race.
	/// To be called when the countdown completes
	/// </summary>
	protected void StartRace()
	{
		Platform.Instance.LocalPlayerPosition.StartTrack();
		UnityEngine.Debug.Log("Tracking Started");
		started = true;
		
		//from this point onward, swipe down should quit via confirmation
		GestureHelper.onBack -= backHandler;
		backHandler = new GestureHelper.OnBack(() => {
			ConsiderQuit();
		});
		GestureHelper.onBack += backHandler;
	}
	
	/// <summary>
	/// Sets the ready to start flag. Allows external scripts to trigger the start (e.g. reset gyros menu screen)
	/// </summary>
	/// <param name='ready'>
	/// Ready.
	/// </param>
	public virtual void SetReadyToStart(bool ready)
	{
		readyToStart = ready;
		
		//start countdown
	}
	
	
	
	//TODO move these to a utility class
	protected string SiDistance(double meters) {
		return SiDistanceUnitless(meters, "distanceunits") + DataVault.Get("distance_units");
	}
	
	protected string SiDistanceUnitless(double meters, string units) {
		string postfix = "m";
		string final;
		float value = (float)meters;
		if (value > 1000) {
			value = value/1000;
			postfix = "km";
			if(value >= 10) {
				final = value.ToString("f1");
			} else {
				final = value.ToString("f2");
			}
		}
		else
		{
			final = value.ToString("f0");
		}
		//set the units string for the HUD
		DataVault.Set(units, postfix);
		return final;
	}
			
	protected float SpeedToKmPace(float speed) {
		if (speed <= 0) {
			return 0;
		}
		// m/s -> mins/Km
		return ((1/speed)/60) * 1000;
	}
	
	protected string TimestampMMSSdd(long milliseconds) {
		TimeSpan span = TimeSpan.FromMilliseconds(milliseconds);
		//if we're into hours, show them
		if(span.Hours > 0)
		{
			return string.Format("{0:0}:{1:00}:{2:00}:{3:00}", span.Hours, span.Minutes, span.Seconds, span.Milliseconds/10);
			//set units string for HUD
			DataVault.Set("time_units", "h:m:s");
		}
		else
		{				
			return string.Format("{0:0}:{1:00}:{2:00}",span.Hours*60 + span.Minutes, span.Seconds, span.Milliseconds/10);
			//set units string for HUD
			DataVault.Set("time_units", "m:s:ds");
		}
			
	}
	protected string TimestampMMSS(long minutes) {
		TimeSpan span = TimeSpan.FromMinutes(minutes);

		return string.Format("{0:00}:{1:00}",span.Minutes,span.Seconds);	
	}

	protected string TimestampMMSSnearestTenSecs(float mins) {
		TimeSpan span = TimeSpan.FromMinutes(mins);
		int minutes = span.Minutes;
		int seconds = (int)Math.Ceiling(span.Seconds / 10.0f) * 10; // ceil to nearest 10
		return string.Format("{0:00}:{1:00}", minutes, seconds);
	}
	
	protected string TimestampMMSSfromMillis(long milliseconds) {
		TimeSpan span = TimeSpan.FromMilliseconds(milliseconds);
		
		if(span.Hours > 0)
		{
			DataVault.Set("time_units", "m:s");
			return string.Format("{0:0}:{1:00}", span.Minutes, span.Seconds);
		} else {
			DataVault.Set("time_units", "h:m:s");
			return string.Format("{0:0}:{1:00}:{2:00}", span.Hours, span.Minutes, span.Seconds);
		}
	}
	
	/// <summary>
	/// Show a timestamp in the form MM:SS, without milliseconds
	/// </summary>
	/// <returns>
	/// The MMSS from M.
	/// </returns>
	protected string TimestampMMSSFromMS(long milliseconds) {
		//UnityEngine.Debug.Log("Converting Timestamp in milliseconds" + milliseconds);
		TimeSpan span = TimeSpan.FromMilliseconds(milliseconds);
		//if we're into hours, show them
		if(span.Hours > 0)
		{
			return string.Format("{0:0}:{1:00}:{2:00}", span.Hours, span.Minutes, span.Seconds);
		}
		else
		{
			return string.Format("{0:0}:{1:00}",span.Hours*60 + span.Minutes, span.Seconds);
		}
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
	
	public bool IsReady()
	{
		return readyToStart;
	}
}
