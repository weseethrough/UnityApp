using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

/// <summary>
/// First run 'challenge'.
/// See http://162.13.14.23:8080/display/RYTD/Alpha+in-challenge+Tutorial for design
/// </summary>

/// <summary>
/// enumeration of screens in this 'challenge'
/// </summary>
public enum FirstRunScreen 
{
	NoScreen,
	WelcomeScreen,
	ResetGyrosScreen,
	AwaitGPSScreen,
	SelectIndoorOutdoor,
	ConfirmIndoorScreen,
	ReadyToStartScreen,
};

public enum FirstRunHint
{
	NoHint,
	MenuHint,
	ResetGyrosHint,
};

public class FirstRun : GameBase {
	
	const float MIN_PACE = 1.5f;
	const float MAX_PACE = 6.0f;
	const int NUM_PACES = 6;
	
	protected FirstRunScreen eCurrentScreen;
	protected FirstRunHint eCurrentHint = FirstRunHint.NoHint;
 	protected GestureHelper.OnSwipeRight swipeHandler = null;
	private GestureHelper.OnSwipeLeft leftHandler = null;

	
	private bool runReadyToStart = false;
	public GameObject runner;		//a runner object. This will be cloned around for various benchmark paces.
	
	public Camera camera;
	//const float paceLabelYOffsetScreen = 0.0f;
	//const float paceLabelYOffsetWorld = 300.0f;
	
	//tuneable parameters
	const float hintCycleDelay = 4.0f;
	
	bool shouldShowPaceLabels = false;
	const float showLabelMinRange = 0.1f;
	const float showLabelMaxRange = 500.0f;
	
	public UINavProgressBar progressBar;
	
	bool hasResetGyros = false;
	
	// Use this for initialization
	void Start () {
		
		base.Start ();
		
		progressBar.numPages = 4;
		
		UnityEngine.Debug.Log("FirstRun: Start");
		
		DataVault.Set("calories", "");
		DataVault.Set("pace", "");
		DataVault.Set("distance", "");
		DataVault.Set("time", "");
		DataVault.Set("ahead_box", "");
		DataVault.Set("time_units", "");
		DataVault.Set("distanceunits", "");
		DataVault.Set("points", "");
		
		//nowhandled by the card itself.
//		swipeHandler = new GestureHelper.OnSwipeRight( () => {
//			HandleForward();
//		});
//		GestureHelper.swipeRight += swipeHandler;
//		
//		leftHandler = new GestureHelper.OnSwipeLeft( () => {
//			HandleLeftSwipe();
//		});
//		GestureHelper.swipeLeft += leftHandler;
		
		//deactivate template objects
		runner.SetActive(false);
		
		//initialise screen
		eCurrentScreen = FirstRunScreen.WelcomeScreen;
		
		//UnityEngine.Debug.Log("FirstRun: about to set instrumentation invisible");
		
//		//set the HUD not to show instrumentation
//		HUDController hudController = GameObject.FindObjectOfType(typeof(HUDController)) as HUDController;
//		if(hudController != null)
//		{
//			UnityEngine.Debug.Log("FirstRace: Attempting to hide hud instrumentation");
//			hudController.setInstrumentationVisible(false);
//			hudController.setAheadBoxVisible(false);
//		}
//		else
//		{
//			UnityEngine.Debug.LogWarning("Couldn't find HUD manager");
//		}
		
		//hide virtual track to begin with
		SetVirtualTrackVisible(false);
		
		//create target trackers for a few different paces
		float fInterval = (MAX_PACE - MIN_PACE) / NUM_PACES;
		
		Platform.Instance.ResetTargets();
		
//		for(float pace = 1.2f; pace < 5.0f; pace += fInterval)
//		{
//			TargetTracker tracker = Platform.Instance.CreateTargetTracker(pace);
//		}
		
		for(float TotalTimePace = 2.0f; TotalTimePace <= 10.0f; TotalTimePace += 1.0f)
		{
			float TotalSeconds = TotalTimePace * 60;	
			
			float speed = finish/TotalSeconds;
			
			TargetTracker tracker = Platform.Instance.CreateTargetTracker(speed);
		}
		
		//create actors for each target tracker
		InstantiateActors();
	}
	
	public override void SetReadyToStart (bool ready)
	{
		base.SetReadyToStart(ready);
		runReadyToStart = ready;
	}
	
	// Update is called once per frame
	void Update () {
		
		if(runReadyToStart)
		{
			base.Update();
		}
		
		//check for tap/back input
		bool tapped = false;
//		if(Input.touchCount > 0 && Input.GetTouch(0).phase == (TouchPhase.Began)) {
//			tapped = true;
//		}
		
#if UNITY_EDITOR
		if(Input.GetKeyDown(KeyCode.Y))	{ tapped = true; }			
#endif
		if(tapped) { 
			if(eCurrentScreen == FirstRunScreen.ResetGyrosScreen) { GyroDidReset(); }
			else HandleForward(); 
		}
			
//		//check for GPS lock
//		if(eCurrentScreen == FirstRunScreen.AwaitGPSScreen)
//		{
//			//if we've got GPS, proceed to the start
//			if(Platform.Instance.HasLock())
//			{
//				StartCoroutine(ProgressToStartOnceGPS());
//			}
//		}
	}
	
	/// <summary>
	/// Progresses to start once GP. This exists just to insert a short delay.
	/// </summary>
	IEnumerator ProgressToStartOnceGPS() {
		yield return new WaitForSeconds(0.75f);
		eCurrentScreen = FirstRunScreen.ReadyToStartScreen;
		progressBar.currentPage ++;
	}
	
	public override GConnector GetFinalConnection ()
	{
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		return fs.Outputs.Find(r => r.Name == "TutorialExit");
	}
	
	private void DrawTintBox() {
		//draw tint box
		float width = Screen.width;
		float height = Screen.height;
		float border = 30.0f;
		Texture tex = Resources.Load("tint_green", typeof(Texture)) as Texture;
		//Rect textureRect = new Rect(border, border, width-2*border, height-2*border);
		Rect textureRect = new Rect(0,0, Screen.width, Screen.height);
		
		GUI.DrawTexture(textureRect, tex, ScaleMode.StretchToFill, true);	
	}
	
	void OnGUI() {
		if(runReadyToStart) {
			base.OnGUI();
		}
		
		//if the user has swiped down to quit, and is seeing the quit confirmation box, dont' show anything here.
		if(maybeQuit) {
			base.OnGUI();
		}
		else
		{
			float width = Screen.width;
			float height = Screen.height;
			
			//show pace labels for closest actors in front and behind us
			//TODO this needs optimising, don't need to look at every target every frame.
			
			if(shouldShowPaceLabels)
			{
				
				TargetController closestAhead = null;
				TargetController closestBehind = null;
				float closestAheadDist = 99999;
				float closestBehindDist = -99999;
				
				float closestDistToCentre = 99999;
				TargetController closestTargetToCentre = null;
				
				foreach(GameObject actor in actors)
				{
					TargetController controller = actor.GetComponent<TargetController>();
					//set labels off by default
					controller.shouldShowOverheadLabel = false;
					float distance = controller.target.GetDistanceBehindTarget();
					
					//UnityEngine.Debug.Log("first run: dist to actor" + distance);
					
	//				//test if this is the closest ahead of us
	//				if(distance > 0 && distance < closestAheadDist)
	//				{
	//					//UnityEngine.Debug.Log("closest ahead: " + distance);
	//					closestAhead = controller;
	//					closestAheadDist = distance;
	//				}
	//				//... or the closest behind us
	//				if(distance <=0 && distance > closestBehindDist)
	//				{
	//					//UnityEngine.Debug.Log("closest behind: " + distance);
	//					closestBehind = controller;
	//					closestBehindDist = distance;
	//				}
					
					Vector3 screenPos = Camera.main.WorldToScreenPoint(actor.transform.position);
					Vector2 screenPos2D = new Vector2(screenPos.x, screenPos.y);
					Vector2 screenCentre = new Vector2(Screen.width/2, Screen.height/2);
					
					float dist = (screenPos2D - screenCentre).magnitude;
					if (screenPos.z > 0)
					{
						if(dist < closestDistToCentre)
						{
							closestDistToCentre = dist;
							closestTargetToCentre = controller;
						}
					}
					
				}
				
	//			if(closestAhead != null)
	//			{
	//				closestAhead.shouldShowOverheadLabel = true;
	//			}
	//			if(closestBehind != null)
	//			{
	//				closestAhead.shouldShowOverheadLabel = true;
	//			}
				if(closestTargetToCentre != null)
				{
					closestTargetToCentre.shouldShowOverheadLabel = true;
				}
			}
			}
	}
	
	private string GetHintString(FirstRunHint hint)
	{
		switch(hint)
		{
		case FirstRunHint.MenuHint:
			return "Tap for Menu";
		case FirstRunHint.ResetGyrosHint:
			return "Tap with two fingers to Reset Gyros";
		default:
			return "";
		}
	}
	
	protected override void OnUnpause ()
	{
		//UnityEngine.Debug.Log("First Run: exiting pause");
		//re-hide the distance
//		HUDController hudController = GameObject.FindObjectOfType(typeof(HUDController)) as HUDController;
//		if(hudController != null)
//		{
//
//			hudController.setAheadBoxVisible(false);
//		}
	}
	
	public override void GameHandleTap ()
	{
		if(started)
		{
			base.GameHandleTap();
		}
		if(eCurrentScreen == FirstRunScreen.SelectIndoorOutdoor)
		{
			bool indoor = Platform.Instance.IsIndoor();
			Platform.Instance.SetIndoor(!indoor);
		}

	}
	
	public override void QuitGame ()
	{
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		GConnector gConnect = fs.Outputs.Find(r => r.Name == "TutorialExit");
		if(gConnect != null) {
			GestureHelper.onSwipeDown -= downHandler;
			GestureHelper.onTap -= tapHandler;
			fs.parentMachine.FollowConnection(gConnect);
			AutoFade.LoadLevel("Game End", 0.1f, 1.0f, Color.black);
		} else {
			UnityEngine.Debug.Log("FirstRun: Error finding tutorial exit");
		}
	}
	
	public override void GyroDidReset()
	{
		if (eCurrentScreen == FirstRunScreen.ResetGyrosScreen)
		{
			//StartCoroutine(ProgressPastResetGyroScreen());	//don't progress automatically. Allow the user to swipe.
			//reveal virtual track
			SetVirtualTrackVisible(true);
			//assume outdoor and try that initially.
			Platform.Instance.SetIndoor(false);
			hasResetGyros = true;
		}
	}
	
	//want a delay for this so the reticle and virtual track are together briefly
	IEnumerator ProgressPastResetGyroScreen() {
		//shuffle the progress bar along midway through the delay
		yield return new WaitForSeconds(0.4f);
		progressBar.currentPage++;
		yield return new WaitForSeconds(0.5f);
		
		//progress to next screen
		eCurrentScreen = FirstRunScreen.SelectIndoorOutdoor;
	}
	
	public override void HandleLeftSwipe ()
	{
		if(started || countdown)
		{
			base.HandleLeftSwipe ();
		}
		else
		{
			//look for a 'back' connector on the current flow state
			FlowState fs = FlowStateMachine.GetCurrentFlowState();
			GConnector gConnect = fs.Outputs.Find(r => r.Name == "Back");
			
			if(gConnect != null)
			{
				fs.parentMachine.FollowConnection(gConnect);
				progressBar.currentPage--;
			}
			else
			{
				UnityEngine.Debug.LogWarning("FirstRun: Swiped back but nowhere to swipe back to");
			}
		}
	}
	
	/// <summary>
	/// Handle the user performing a tap gesture
	/// </summary>
	void HandleForward() {
		//look for a 'Swipe' connector on the current flow state
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		GConnector gConnect = fs.Outputs.Find(r => r.Name == "Swipe");
		
		if(gConnect != null)
		{
			fs.parentMachine.FollowConnection(gConnect);
			progressBar.currentPage++;
		}
		else
		{
			UnityEngine.Debug.LogWarning("FirstRun: Swiped forward but no link to follow");
		}
	}
	
	IEnumerator ProgressInfoOnceStarted() {
		//wait for the countdown
		yield return new WaitForSeconds(hintCycleDelay);
		
		//cycle the hints
		while(eCurrentHint != FirstRunHint.NoHint)
		{
			//pause for a few seconds on the current hint
			yield return new WaitForSeconds(hintCycleDelay);
			//cycle to next hint
			GetNextHint();
		}
		
		//reveal the benchmark actors
		yield return new WaitForSeconds(hintCycleDelay);
		
		ShowActors();
	}
	
	private void ShowActors() {
			try{
			//show all of the actors
			foreach (GameObject actor in actors)
			{
				//UnityEngine.Debug.Log("First Run: activating actor");
				actor.SetActive(true);
			}
			} catch(Exception e) {
				UnityEngine.Debug.LogWarning("Error iterating actors list");	
			}
			shouldShowPaceLabels = true;
	}
	
	private void GetNextHint() {
		switch(eCurrentHint)
			{
			case FirstRunHint.NoHint:
				eCurrentHint = FirstRunHint.MenuHint;
				break;
			case FirstRunHint.MenuHint:
				eCurrentHint = FirstRunHint.ResetGyrosHint;
				break;
			case FirstRunHint.ResetGyrosHint:
				eCurrentHint = FirstRunHint.NoHint;
				break;
			default:
				break;
				//do nothing;
			}
		}
		

	
	void OnDestroy() {
		//deregister handlers
//		if(tapHandler != null)
//		{
//			GestureHelper.onTap -= tapHandler;
//		}
//		if(backHandler != null)
//		{
//			GestureHelper.onSwipeDown -= backHandler;
//		}
		if(swipeHandler != null)
		{
			GestureHelper.swipeRight -= swipeHandler;
		}
		if(leftHandler != null)
		{
			GestureHelper.swipeLeft -= leftHandler;
		}
	}
	
	private void InstantiateActors() {
		//create an actor for each active target tracker
		List<TargetTracker> trackers = Platform.Instance.targetTrackers;
		int lane = 1;
		foreach (TargetTracker tracker in trackers) {
			GameObject actor = Instantiate(runner) as GameObject;
			TargetController controller = actor.GetComponent<TargetController>();
			controller.SetLane(1);
			controller.SetTracker(tracker);
			controller.SetLane(lane++);
			//actor.SetActive(true);
			actors.Add(actor);
			
			//determine pace and set string
			float speed = tracker.PollCurrentSpeed();
			long totalTime = (long)((float)finish/speed)*1000;
			//UnityEngine.Debug.Log("FirstRun: Speed = " + speed);
			//UnityEngine.Debug.Log("FirstRun: totalTime = " + totalTime);
			string paceString = TimestampMMSSFromMS(totalTime) + "min/km";
			//UnityEngine.Debug.Log("FirstRun: pace = " + paceString);
			controller.overheadLabelString = paceString;
		}
		
	}
	
		
}
