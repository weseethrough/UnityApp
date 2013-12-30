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
 	protected GestureHelper.OnSwipeRight swipeHandler;
	
	private bool runReadyToStart = false;
	public GameObject runner;		//a runner object. This will be cloned around for various benchmark paces.
	
	public Camera camera;
	const float paceLabelYOffsetScreen = 0.0f;
	const float paceLabelYOffsetWorld = 300.0f;
	
	//tuneable parameters
	const float hintCycleDelay = 4.0f;
	
	bool shouldShowPaceLabels = false;
	const float showLabelMinRange = 0.1f;
	const float showLabelMaxRange = 500.0f;
	
	public UINavProgressBar progressBar;
	
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
		
		
		swipeHandler = new GestureHelper.OnSwipeRight( () => {
			HandleForward();
		});
		GestureHelper.swipeRight += swipeHandler;
		
		
		//deactivate template objects
		runner.SetActive(false);
		
		//initialise screen
		eCurrentScreen = FirstRunScreen.WelcomeScreen;
		
		//UnityEngine.Debug.Log("FirstRun: about to set instrumentation invisible");
		
		//set any necessary flags in game base to delay start, suppress normal UI etc.
		SetInstrumentationVisible(false);
		
		//UnityEngine.Debug.Log("FirstRun: about to make ahead box invisible");
		
		SetAheadBoxVisible(false);
		
		//UnityEngine.Debug.Log("FirstRun: about to make virtual track invisible");
		
		//hide virtual track to begin with
		SetVirtualTrackVisible(false);
		
		//create target trackers for a few different paces
		float fInterval = (MAX_PACE - MIN_PACE) / NUM_PACES;
		
		Platform.Instance.ResetTargets();
		
		for(float pace = 1.2f; pace < 5.0f; pace += fInterval)
		{
			Platform.Instance.CreateTargetTracker(pace);
		}
		
		//create actors for each target tracker
		InstantiateActors();
	}
	
	// Update is called once per frame
	void Update () {
		
		if(runReadyToStart)
		{
			base.Update();
		}
		
		//check for tap/back input
		bool tapped = false;
		if(Input.touchCount > 0 && Input.GetTouch(0).phase == (TouchPhase.Began)) {
			tapped = true;
		}
		
#if UNITY_EDITOR
		if(Input.GetKeyDown(KeyCode.Y))	{ tapped = true; }			
#endif
		if(tapped) { 
			if(eCurrentScreen == FirstRunScreen.ResetGyrosScreen) { GyroDidReset(); }
			else HandleForward(); 
		}
			
		//check for GPS lock
		if(eCurrentScreen == FirstRunScreen.AwaitGPSScreen)
		{
			//if we've got GPS, proceed to the start
			if(Platform.Instance.HasLock())
			{
				StartCoroutine(ProgressToStartOnceGPS());
			}
		}
	}
	
	/// <summary>
	/// Progresses to start once GP. This exists just to insert a short delay.
	/// </summary>
	/// <returns>
	/// The to start once GP.
	/// </returns>
	IEnumerator ProgressToStartOnceGPS() {
		yield return new WaitForSeconds(0.75f);
		eCurrentScreen = FirstRunScreen.ReadyToStartScreen;
		progressBar.currentPage ++;
	}
	
	protected GUIStyle getLabelStyleLarge()
	{
		// set style for our labels
		GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
		labelStyle.alignment = TextAnchor.UpperCenter;
		labelStyle.fontSize = 55;
		labelStyle.fontStyle = FontStyle.Normal;
		//labelStyle.clipping = TextClipping.Overflow;
				
		return labelStyle;
	}
		
	protected GUIStyle getLabelStyleNav()
	{
		// set style for our labels
		GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
		labelStyle.alignment = TextAnchor.LowerCenter;
		labelStyle.fontSize = 35;
		labelStyle.fontStyle = FontStyle.Normal;
		//labelStyle.clipping = TextClipping.Overflow;
		
		return labelStyle;
	}
	
	protected GUIStyle getLabelStyleHint()
	{
		// set style for our labels
		GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
		labelStyle.alignment = TextAnchor.UpperLeft;
		labelStyle.fontSize = 35;
		labelStyle.fontStyle = FontStyle.Normal;
		//labelStyle.clipping = TextClipping.Overflow;

		return labelStyle;
	}
	
	protected GUIStyle getLabelStylePace()
	{
		// set style for our labels
		GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
		labelStyle.alignment = TextAnchor.MiddleCenter;
		labelStyle.fontSize = 25;
		labelStyle.fontStyle = FontStyle.Normal;
		labelStyle.clipping = TextClipping.Overflow;

		return labelStyle;
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
		
		float width = Screen.width;
		float height = Screen.height;
		float border = 35.0f;
		
		//rect for the main 'headline'
		Rect MainMessageRect = new Rect(border, border, width - 2*border , height - 2*border);
		//rect for the navigation prompt
		Rect NavMessageRect = new Rect(border, border, width - 2*border, height - 2*border);
		Rect HintRect = new Rect(border, border, width*0.5f - border, height - 2*border);
		
		GUIStyle MainMessageStyle = getLabelStyleLarge();
		GUIStyle NavMessageStyle = getLabelStyleNav();
		GUIStyle HintStyle = getLabelStyleHint();
		GUIStyle PaceLabelStyle = getLabelStylePace();
		
		switch(eCurrentScreen)
		{
		case FirstRunScreen.WelcomeScreen:
		{
			DrawTintBox();
			GUI.Label(MainMessageRect, "Welcome to the First Run", MainMessageStyle); 
			GUI.Label(NavMessageRect, "Swipe Right to Continue", NavMessageStyle);
			break;
		}
		case FirstRunScreen.ResetGyrosScreen:
		{
			DrawTintBox();
			GUI.Label(MainMessageRect, "Step 1: Reset Gyros", MainMessageStyle);
			GUI.Label(NavMessageRect, "Look Directly Forward and Tap with two fingers", NavMessageStyle);
			
			//draw reticle
			Texture tex = Resources.Load("FirstRace_HorizonReticle", typeof(Texture)) as Texture;
			float halfWidth = 130.0f;
			Rect ReticleRect = new Rect(Screen.width/2 - halfWidth, Screen.height/2 - halfWidth, 2*halfWidth, 2*halfWidth);
			GUI.DrawTexture(ReticleRect, tex);
			
			break;
		}
		case FirstRunScreen.AwaitGPSScreen:
		{
			DrawTintBox();
			GUI.Label(MainMessageRect, "Step 2: Awaiting GPS Lock", MainMessageStyle);
			GUI.Label(NavMessageRect, "Swipe Right to continue in Indoor Mode", NavMessageStyle);
			
			//draw gps icon
			Texture tex = Resources.Load("FirstRace_gpsLogo", typeof(Texture)) as Texture;
			float halfWidth = 30.0f;
			Rect GPSRect = new Rect(Screen.width/2 - halfWidth, Screen.height/2 - halfWidth, 2*halfWidth, 2*halfWidth);
			GUI.DrawTexture(GPSRect, tex);
			
			break;
		}
		case FirstRunScreen.ConfirmIndoorScreen:
		{
			DrawTintBox();
			GUI.Label(MainMessageRect, "Indoor Mode\nAre you sure?", MainMessageStyle);
			GUI.Label(NavMessageRect, "Swipe Right to continue\nSwipe down to cancel", NavMessageStyle);
			break;
		}
		case FirstRunScreen.ReadyToStartScreen:
		{
			DrawTintBox();
			string mainMessage = Platform.Instance.IsIndoor() ? "Indoor Mode Active" : "GPS Lock Acquired";
			GUI.Label(MainMessageRect, mainMessage, MainMessageStyle);
			GUI.Label(NavMessageRect, "Swipe Right to Begin", NavMessageStyle);
			break;
		}
		default:
		{
			//show the normal UI
			//Add appropriate hint
			string hintMessage = GetHintString(eCurrentHint);
			GUI.Label(HintRect, hintMessage, HintStyle);
			break;
		}

		}
		
		//show pace labels for closest actors in front and behind us
		//TODO this needs optimising, don't need to look at every target every frame.
		if(shouldShowPaceLabels)
		{

			GameObject closestAhead = null;
			GameObject closestBehind = null;
			float closestAheadDist = 99999;
			float closestBehindDist = -99999;
			
			foreach(GameObject actor in actors)
			{
				TargetController controller = actor.GetComponent<TargetController>();
				float distance = controller.target.GetDistanceBehindTarget();
				
				//UnityEngine.Debug.Log("first run: dist to actor" + distance);
				
				//test if this is the closest ahead of us
				if(distance > 0 && distance < closestAheadDist)
				{
					//UnityEngine.Debug.Log("closest ahead: " + distance);
					closestAhead = actor;
					closestAheadDist = distance;
				}
				//... or the closest behind us
				if(distance <=0 && distance > closestBehindDist)
				{
					//UnityEngine.Debug.Log("closest behind: " + distance);
					closestBehind = actor;
					closestBehindDist = distance;
				}
			}
			
			if(closestAhead != null)
			{
				showPaceLabel(closestAhead);
			}
			if(closestBehind != null)
			{
				showPaceLabel(closestBehind);
			}
			
		}
	}
	
	private void showPaceLabel(GameObject labelActor)
	{
		if(labelActor == null)
		{
			UnityEngine.Debug.LogWarning("FirstRun: can't show label for null actor.");
			return;
		}
		
		Vector3 actorPos = labelActor.transform.position;
		
		
		Vector3 headPos = actorPos + new Vector3(0, paceLabelYOffsetWorld, 0);
		//UnityEngine.Debug.Log("FirstRun: actor height: " + actorTop);
		//UnityEngine.Debug.Log("FirstRun: actor world pos y: " + actor.transform.position);
		Vector3 screenPos = camera.WorldToScreenPoint(headPos);
		
		//only shown actors in front of us.
		if(screenPos.z < 0) return;
		
		//create label
		GUIStyle paceStyle = getLabelStylePace();
		float paceHalfWidth = 200;
	
		//calculate yPos. Note, camera screen pos calculation comes out with y inverted.
		float yPos = Screen.height - screenPos.y - paceLabelYOffsetScreen;
	
		Rect paceRect = new Rect(screenPos.x - paceHalfWidth, yPos, 2*paceHalfWidth, 1);
		
		//determine pace
		TargetController controller = labelActor.GetComponent<TargetController>();
		float speed = controller.target.PollCurrentSpeed();
		long totalTime = (long)((float)finish*1000/speed);
		string paceString = TimestampMMSSFromMS(totalTime);
		//UnityEngine.Debug.Log("speed:"+speed+" totalTime:"+totalTime + " distancePace:" + paceString);
		
		GUI.Label(paceRect, paceString, paceStyle);
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
		SetAheadBoxVisible(false);
	}
	
	public override void GyroDidReset()
	{
		if (eCurrentScreen == FirstRunScreen.ResetGyrosScreen)
		{
			StartCoroutine(ProgressPastResetGyroScreen());
			//reveal virtual track
			SetVirtualTrackVisible(true);
			//assume outdoor and try that initially.
			Platform.Instance.SetIndoor(false);
		}
	}
	
	//want a delay for this so the reticle and virtual track are together briefly
	IEnumerator ProgressPastResetGyroScreen() {
		//shuffle the progress bar along midway through the delay
		yield return new WaitForSeconds(0.4f);
		progressBar.currentPage++;
		yield return new WaitForSeconds(0.5f);
		
		//progress to next screen
		eCurrentScreen = FirstRunScreen.AwaitGPSScreen;
	}
	
	public override void HandleLeftSwipe ()
	{
		if(started || countdown)
		{
			base.HandleLeftSwipe ();
		}
		else
		{
			//navigate backwards through the order of cards
			// N.B. The most important step here is the ability to step back from Indoor Ready to awaiting GPS
			switch(eCurrentScreen)
			{
			case FirstRunScreen.WelcomeScreen:
			{
				//nothing to do
				break;
			}
			case FirstRunScreen.ResetGyrosScreen:
			{
				eCurrentScreen = FirstRunScreen.WelcomeScreen;
				progressBar.currentPage--;
				break;
			}
			case FirstRunScreen.AwaitGPSScreen:
			{
				eCurrentScreen = FirstRunScreen.ResetGyrosScreen;
				progressBar.currentPage--;
				break;
			}
			case FirstRunScreen.ConfirmIndoorScreen:
			{
				Platform.Instance.SetIndoor(false);
				eCurrentScreen = FirstRunScreen.AwaitGPSScreen;
				progressBar.currentPage--;
				break;
			}
			case FirstRunScreen.ReadyToStartScreen:
			{
				Platform.Instance.SetIndoor(false);
				eCurrentScreen = FirstRunScreen.AwaitGPSScreen;
				progressBar.currentPage--;
				break;
			}
			default:
			{
				//do nothing
				break;
			}
			}
		}
	}
	
	/// <summary>
	/// Handle the user performing a tap gesture
	/// </summary>
	void HandleForward() {
		
		//UnityEngine.Debug.Log("FirstRun: tap detected");
		
		switch(eCurrentScreen)
		{
		case FirstRunScreen.WelcomeScreen:
		{
			//proceed to next screen
			eCurrentScreen = FirstRunScreen.ResetGyrosScreen;	
			progressBar.currentPage++;
			break;
		}
		case FirstRunScreen.ResetGyrosScreen:
		{
			//Do nothing for a tap/swipe on this screen
			break;
		}
		case FirstRunScreen.AwaitGPSScreen:
		// In fact, don't use the confirmation screen. Go straight to the start.
		//{
		//	//if we tap here, go to the confirm indoor mode screen
		//	eCurrentScreen = FirstRunScreen.ConfirmIndoorScreen;
		//	break;
		//}
		case FirstRunScreen.ConfirmIndoorScreen:
		{
			//tap means confirmation. Set indoor mode and proceed.
			Platform.Instance.SetIndoor(true);
			eCurrentScreen = FirstRunScreen.ReadyToStartScreen;
			progressBar.currentPage++;
			break;
		}
		case FirstRunScreen.ReadyToStartScreen:
		{
			//flag that the countdown can begin
			runReadyToStart = true;
			
			try{
			//show instrumentation numbers
			SetInstrumentationVisible(true);
			} catch(Exception e) {
				UnityEngine.Debug.LogWarning("Error turning on instrumentation");
			}
			
			//set no first run screen
			eCurrentScreen = FirstRunScreen.NoScreen;
			progressBar.show = false;			
			
			//set menu hint
			eCurrentHint = FirstRunHint.MenuHint;
			//fire off coroutine to cycle hint, show other actors etc
			StartCoroutine(ProgressInfoOnceStarted());
			
			//flag to gameBase, that the countdown can commence
			readyToStart = true;
			
			break;
		}
		case FirstRunScreen.NoScreen:
		default:
		{
			//show nothing
			break;
		}
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
	}
	
	private void InstantiateActors() {
		//create an actor for each active target tracker
		List<TargetTracker> trackers = Platform.Instance.targetTrackers;
		int lane = 1;
		foreach (TargetTracker tracker in trackers) {
			GameObject actor = Instantiate(runner) as GameObject;
			TargetController controller = actor.GetComponent<TargetController>();
			controller.SetTracker(tracker);
			controller.SetLane(lane++);
			//actor.SetActive(true);
			actors.Add(actor);
		}
		
	}
	
		
}
