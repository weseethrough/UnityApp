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
		
	//constant values for states. New states should be added here or in a subclass in the same manner.
	public const string GAMESTATE_AWAITING_USER_READY = "awaiting_ready";
	public const string GAMESTATE_COUNTING_DOWN = "counting_down";
	public const string GAMESTATE_PAUSED = "paused";
	public const string GAMESTATE_RUNNING = "running";
	public const string GAMESTATE_FINISHED = "finished";
	public const string GAMESTATE_QUIT_CONFIRMATION = "quit_confirmation";
	
	//string to track the current state. Should ONLY use the const values defined above or in a subclass
	protected string gameState = GAMESTATE_AWAITING_USER_READY;
	
	//flag for whether we are auto-aused
	protected bool bAutoPaused = false;
	
	// Bonus distance milestones
	protected int bonusTarget = 1000;
	// Final sprint bonus points.		TODO is this a feature of ALL games or just some?
	protected float finalBonus = 1000;
	
	// Distance the player wants to run
	protected int finish;
	
	// Start tracking and 3-2-1 countdown variables
	protected bool started = false;
	
	// Multiplier variables
	private float baseMultiplier;
	private String baseMultiplierString;
	private float baseMultiplierStartTime;
	
	// Selected Track
	public Track selectedTrack = null;
	
	// Target speed
	public float targSpeed = 1.8f;
	
	protected GestureHelper.OnTap tapHandler = null;
	private GestureHelper.TwoFingerTap twoTapHandler = null;
	protected GestureHelper.OnBack backHandler = null;
	private GestureHelper.OnSwipeLeft leftHandler = null;
	private GestureHelper.OnSwipeRight rightHandler = null;

	protected GameObject theVirtualTrack;
	
	//two vars used in controlling the jog-on-the spot prompt.
	private int lastDistance;
	private float indoorTime;
	
	public bool showFps = false;
	
	/// <summary>
	/// Start this instance.
	/// </summary>
	public virtual void Start () 
	{
		//blank out strings
		DataVault.Set("countdown_subtitle", " ");
		DataVault.Set("indoor_move", " ");
		DataVault.Set("distance_position", " ");
		DataVault.Set("target_units", "");
		DataVault.Set("ahead_box", " ");
		
		//gesture handlers. All gestures should be handled by these delegates by editing/extending the GameHandleXXXX method.
		tapHandler = new GestureHelper.OnTap(() => {
			GameHandleTap();
		});
		GestureHelper.onTap += tapHandler;
		
		twoTapHandler = new GestureHelper.TwoFingerTap(() => {
			GameHandleTwoTap();
		});
		GestureHelper.onTwoTap += twoTapHandler;
		
		leftHandler = new GestureHelper.OnSwipeLeft(() => {
			GameHandleLeftSwipe();
		});
		GestureHelper.onSwipeLeft += leftHandler;
		
		rightHandler = new GestureHelper.OnSwipeRight(() => {
			GameHandleRightSwipe();
		});
		GestureHelper.onSwipeRight += rightHandler;
		
		backHandler = new GestureHelper.OnBack(() => {
			GameHandleBack();
		});
		GestureHelper.onBack += backHandler;
		
			
		//Get target distance
		UnityEngine.Debug.Log("GameBase: getting track");
		selectedTrack = (Track)DataVault.Get("current_track");
		
		UnityEngine.Debug.Log("GameBase: checking if track is null");
		if(selectedTrack != null) {
			finish = (int)selectedTrack.distance;
			UnityEngine.Debug.Log("GameBase: track distance is: " + finish.ToString());
		} else {
			if(Application.isEditor)
			{
				finish = 1000;
			}
			else
			{
				finish = (int)DataVault.Get("finish");
			}
		}
	
		
#if RY_INDOOR
		SetReadyToStart(true);
		SetVirtualTrackVisible(true);
#endif
		//set distance with units for the menu cards
		DataVault.Set("finish_km", UnitsHelper.SiDistanceUnitless(finish, string.Empty) );
	
		UnityEngine.Debug.Log("GameBase: resetting platform");
		Platform.Instance.Reset();
		
		//initialise units for HUD
		DataVault.Set("distance_units", "M");
        DataVault.Set("minutes_unit_short", "MIN");
        DataVault.Set("seconds_unit_short", "SEC");
        DataVault.Set("pace_unit_short", "MIN/KM");
        DataVault.Set("calories_unit_short", "KCAL");
        DataVault.Set("sweat_points_unit", "SWEAT POINTS");
		
		//set up fps counter
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
		
		CleanUp();
		
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
			FlowState fs = FlowStateMachine.GetCurrentFlowState();
			GConnector gConnect = fs.Outputs.Find(r => r.Name == "QuitExit");
			if(gConnect != null) {
				fs.parentMachine.FollowConnection(gConnect);
				Platform.Instance.StopTrack();
			}
	}
	
	//UNUSED?
//	public virtual void QuitGame() {
//		FlowState fs = FlowStateMachine.GetCurrentFlowState();
//		GConnector gConnect = fs.Outputs.Find(r => r.Name == "MenuExit");
//		if(gConnect != null) {
//			fs.parentMachine.FollowConnection(gConnect);
//			CleanUp();
//			AutoFade.LoadLevel("Game End", 0.1f, 1.0f, Color.black);
//		}
//	}
	
	public void ReturnGame() {
		
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		GConnector gConnect = fs.Outputs.Find(r => r.Name == "GameExit");
		if(gConnect != null) {
			fs.parentMachine.FollowConnection(gConnect);
		}
	}
	
	public virtual GConnector GetFinalConnection() {
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		return fs.Outputs.Find(r => r.Name == "FinishButton");
	}
	
	/// <summary>
	/// Sets the state of the game. Argument must be one of the const values in GameBase or a subclass
	/// </summary>
	/// <param name='state'>
	/// State.
	/// </param>
	protected void SetGameState(string state)
	{ 
		if(state != gameState)
		{
			UnityEngine.Debug.Log("GameBase: Transitioning from state: " + gameState + " to: " + state);
			OnExitState(gameState);
			OnEnterState(state);
			gameState = state;
		}
	}
	
	/// <summary>
	/// Called when entering the given state. Called automatically 
	/// Should be overriden in any subclasses which add new game states, to handle those states.
	/// </summary>
	/// <param name='state'>
	/// State we are entering
	/// </param>
	protected virtual void OnEnterState(string state)
	{
		UnityEngine.Debug.Log("GameBase: Entering State: " + state);
		switch(state)
		{
		case GAMESTATE_COUNTING_DOWN:
			//start the countdown
			UnityEngine.Debug.Log("GameBase: About to start countdown");
			StartCoroutine("DoCountDown");
			break;
		case GAMESTATE_FINISHED:
			FinishGame();
			break;
		case GAMESTATE_PAUSED:
			EnterPause();
			break;
		case GAMESTATE_QUIT_CONFIRMATION:
			ConsiderQuit();
			break;
		}
	}
	
	protected virtual void OnExitState(string state)
	{
		UnityEngine.Debug.Log("GameBase: Exiting State: " + state);
		switch(state)
		{
		case GAMESTATE_COUNTING_DOWN:
			//start the race
			StartRace();
			break;
		case GAMESTATE_PAUSED:
			//switch flow back to main flow
			ExitPause();
			break;
		case GAMESTATE_QUIT_CONFIRMATION:
			ReturnGame();
			break;
		}
	}

	
	/// <summary>
	/// Call on quit confirmed.
	/// </summary>
	protected void FinishGame()
	{
		//stop tracking
		Platform.Instance.StopTrack();
		
		//invoke any game-specific behaviour
		OnFinishedGame();
		
		UnityEngine.Debug.Log("GameBase: Ending game");
		
		//follow the flow link to the results page
		GConnector gConnect = GetFinalConnection();
		if(gConnect != null) {
			UnityEngine.Debug.Log("GameBase: final connection found");
			DataVault.Set("total", (Platform.Instance.GetCurrentPoints() + Platform.Instance.GetOpeningPointsBalance() + finalBonus).ToString("n0"));
			DataVault.Set("distance_with_units", UnitsHelper.SiDistance(Platform.Instance.GetDistance()));
			UnityEngine.Debug.Log("GameBase: setting points");
			if(finish >= 1000) {
				DataVault.Set("bonus", finalBonus.ToString("n0")); 
			} else {
				DataVault.Set("bonus", 0);
			}
			
			//follow the connector
			FlowState fs = FlowStateMachine.GetCurrentFlowState();
			fs.parentMachine.FollowConnection(gConnect);
		} else {
			UnityEngine.Debug.Log("GameBase: No connection found for FinishGame()");
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
			(gConnect.Parent as Panel).CallStaticFunction(gConnect.EventFunction, null);
			SoundManager.PlaySound(SoundManager.Sounds.Tap);
			fs.parentMachine.FollowConnection(gConnect);
			AutoFade.LoadLevel("Game End", 0.1f, 1.0f, Color.black);
		} else {
			UnityEngine.Debug.Log("GameBase: No connection found - ContinueButton");
		}
		
		CleanUp();
	}
	
	//handle a tap. Default is just to pause/unpause but games (especially tutorial, can customise this by overriding)
	public virtual void GameHandleTap() {
		if(started)
		{
			UnityEngine.Debug.Log("GameBase: tap detected");
			SoundManager.PlaySound(SoundManager.Sounds.Tap);
						
			switch (gameState)
			{
			case GAMESTATE_AWAITING_USER_READY:
				//nothing. We can't interpret a tap as ready yet, user might not be on that panel.
				break;
			case GAMESTATE_COUNTING_DOWN:
				//nothing
			case GAMESTATE_RUNNING:
				//toggle pause
				SetGameState(GAMESTATE_PAUSED);
				break;
			case GAMESTATE_PAUSED:
				SetGameState(GAMESTATE_RUNNING);
				break;
			case GAMESTATE_QUIT_CONFIRMATION:
				SetGameState(GAMESTATE_FINISHED);
				FinishGame();
				break;
				
			case GAMESTATE_FINISHED:
				FlowState fs = FlowStateMachine.GetCurrentFlowState();
				GConnector gConnect = fs.Outputs.Find(r => r.Name == "ContinueButton");
				if(gConnect != null) {
					(gConnect.Parent as Panel).CallStaticFunction(gConnect.EventFunction, null);
					SoundManager.PlaySound(SoundManager.Sounds.Tap);
					fs.parentMachine.FollowConnection(gConnect);
					AutoFade.LoadLevel("Game End", 0.1f, 1.0f, Color.black);
				} else {
					UnityEngine.Debug.Log("GameBase: No connection found - ContinueButton");
				}
				break;
			}
		}
	}
	
	public virtual void GameHandleBack() {
		switch (gameState)
		{
		case GAMESTATE_AWAITING_USER_READY:
			QuitImmediately();
			break;
		case GAMESTATE_COUNTING_DOWN:
			QuitImmediately();
			break;
		case GAMESTATE_RUNNING:
			SetGameState(GAMESTATE_QUIT_CONFIRMATION);
			break;
		case GAMESTATE_PAUSED:
			//do nothing
			break;
		case GAMESTATE_QUIT_CONFIRMATION:
			//cancel quit
			SetGameState(GAMESTATE_RUNNING);
			break;
		case GAMESTATE_FINISHED:
			//continue
			//do nothing
			break;
		}
	}
	
	public virtual void GameHandleTwoTap() {
		//do nothing
	}
	
	public virtual void GameHandleLeftSwipe() {
		//do nothing
	}
	
	public virtual void GameHandleRightSwipe() {
		//do nothing
	}
	
	protected void EnterPause()
	{
		//transition to pause screen
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		GConnector gConnect = fs.Outputs.Find(r => r.Name == "PauseExit");
		if(gConnect != null)
		{
			fs.parentMachine.FollowConnection(gConnect);
			//set string for GUI
			DataVault.Set("paused", "Paused");
			
			Time.timeScale = 0.0f;
			
			//stop tracking
			Platform.Instance.StopTrack();
		} else
		{
			UnityEngine.Debug.Log("GameBase: Can't find exit - PauseExit");
		}
		
	}
	
	protected virtual void ExitPause()
	{
		UnityEngine.Debug.Log("GameBase: Unpausing");

		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		fs.parentMachine.FollowBack();
		Time.timeScale = 1.0f;
	
		//resume tracking
		Platform.Instance.StartTrack();
	}
	
	protected virtual void UpdateAhead() {
		
		double targetDistance = GetDistBehindForHud();

		if (targetDistance > 0) {
			DataVault.Set("distance_position", "BEHIND");
			//DataVault.Set("ahead_col_header", "D20000FF");
			DataVault.Set("ahead_col_box", UIColour.red);
		} else {
			DataVault.Set("distance_position", "AHEAD");
			DataVault.Set("ahead_col_box", UIColour.green);
			//DataVault.Set("ahead_col_header", "19D200FF");
		}
		//UnityEngine.Debug.Log("GameBase: distance behind is " + GetDistBehindForHud().ToString());
		string siDistance = UnitsHelper.SiDistanceUnitless(Math.Abs(targetDistance), "target_units");
		//UnityEngine.Debug.Log("GameBase: setting target distance to: " + siDistance);
		DataVault.Set("ahead_box", siDistance);
		
	}
	
	protected virtual double GetDistBehindForHud() {
		return Platform.Instance.GetHighestDistBehind();
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
		SetGameState(GAMESTATE_RUNNING);
	}
	
	// Update is called once per frame
	public virtual void Update ()
	{
		//accept 'o' on keyboard as 'tap to start'
		if( Input.GetKeyDown(KeyCode.O) )
		{
			TriggerUserReady();
		}
		
		switch(gameState)
		{
		case GAMESTATE_AWAITING_USER_READY:
			//nothing to do
			break;
			
		case GAMESTATE_COUNTING_DOWN:
			//no updates to do. All handled by coroutine.
			//poll only while running or counting down
			Platform.Instance.Poll();
			break;
			
		case GAMESTATE_RUNNING:
			
			//poll only while running or counting down
			Platform.Instance.Poll();
			
			//check for auto-pause
			if(!Platform.Instance.IsIndoor() && Platform.Instance.playerState == "STOPPED")
			{
				//state is applicable, check timer
				if(UnityEngine.Time.time - Platform.Instance.playerStateEntryTime > 1.0f)
				{
					bAutoPaused = true;
					SetGameState(GAMESTATE_PAUSED);
					UnityEngine.Debug.Log("Auto-pausing");
					//set string for GUI label
					DataVault.Set("paused", "Auto-Paused");
				}
			}
				
			UpdateIndoorPrompts();
				
			//check for finished
			if(Platform.Instance.Distance() >= finish)
			{
				
				SetGameState(GAMESTATE_FINISHED);
			}
			
			// Award the player points for running certain milestones
			if(Platform.Instance.Distance() >= bonusTarget)
			{
				int targetToKm = bonusTarget / 1000;
				if(bonusTarget < finish) 
				{
					MessageWidget.AddMessage("Bonus Points!", "You reached " + targetToKm.ToString() + "km! 1000pts", "trophy copy");
				}
				bonusTarget += 1000;
			}
			
			//update ahead for HUD			
			UpdateAhead();

			break;
			
		case GAMESTATE_PAUSED:
			//check for auto-resume
			if( bAutoPaused && !Platform.Instance.IsIndoor() && Platform.Instance.playerState != "STOPPED" )
			{
				//can unpause - check for min time in this state
				if(UnityEngine.Time.time-Platform.Instance.playerStateEntryTime > 0.5f)
				{
					bAutoPaused = false;
					SetGameState(GAMESTATE_RUNNING);
					UnityEngine.Debug.Log("Auto-unpausing");
				}
			}
			break;
			
		case GAMESTATE_FINISHED:
			//nothing to do
			break;
		
		case GAMESTATE_QUIT_CONFIRMATION:
			//nothing to do
			break;
			
		default:
			break;
		}
		

	}
	
	protected void UpdateIndoorPrompts()
	{
		//Show prompt to run on spot indoor
		if(Platform.Instance.IsIndoor()) {
			if((int)Platform.Instance.Distance() == lastDistance) 
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
				lastDistance = (int)Platform.Instance.Distance();
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
		Platform.Instance.StartTrack();
		UnityEngine.Debug.Log("Tracking Started");
		started = true;
	}
	
	/// <summary>
	/// Called externally to show that the user is ready to start. Probably called from scripto on the tap-to-start UI panel
	/// </summary>
	public virtual void TriggerUserReady()
	{
		if(gameState!=GAMESTATE_AWAITING_USER_READY)
		{
			UnityEngine.Debug.Log("GameBase: Received User Ready trigger, when not waiting for it");
			//move to countdown state
		}
		else
		{
			UnityEngine.Debug.Log("GameBase: Received User ready trigger");
			SetGameState(GAMESTATE_COUNTING_DOWN);
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
	
	/// <summary>
	/// Determines whether this instance is running.
	/// </summary>
	/// <returns>
	/// <c>true</c> if this instance is running; otherwise, <c>false</c>.
	/// </returns>
	public bool IsRunning()
	{
		return gameState == GAMESTATE_RUNNING;
	}
	
	protected void CleanUp()
	{
		//stop tracking
		Platform.Instance.StopTrack();
			
		//release handlers
		GestureHelper.onBack -= backHandler;
		GestureHelper.onTap -= tapHandler;
		GestureHelper.onTwoTap -= twoTapHandler;
		GestureHelper.onSwipeLeft -= leftHandler;
		GestureHelper.onSwipeRight -= rightHandler;
		
	}
}
