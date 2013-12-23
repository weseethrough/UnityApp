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
	
	//tuneable parameters
	const float hintCycleDelay = 7.0f;
	
	
	// Use this for initialization
	void Start () {
		
		base.Start ();
		
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
		
		
//		backHandler = new GestureHelper.onSwipeDown(() => {
//			HandleBack();
//		});
//		GestureHelper.onSwipeDown += backHandler;
		
		//deactivate template objects
		runner.SetActive(false);
		
		//initialise screen
		eCurrentScreen = FirstRunScreen.WelcomeScreen;
		
		//set any necessary flags in game base to delay start, suppress normal UI etc.
		SetInstrumentationVisible(false);
		SetAheadBoxVisible(false);
		
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
		if(tapped) { HandleForward(); }
			
		//check for GPS lock
		if(eCurrentScreen == FirstRunScreen.AwaitGPSScreen)
		{
			//if we've got GPS, proceed to the start
			if(Platform.Instance.HasLock())
			{
				Platform.Instance.SetIndoor(false);
				eCurrentScreen = FirstRunScreen.ReadyToStartScreen;
			}
		}
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
	
	private void DrawTintBox() {
		//rect for the tint box
		float width = Screen.width;
		float height = Screen.height;
		
		//draw tint box
		Texture tex = Resources.Load("tint", typeof(Texture)) as Texture;

		float border = 30.0f;
		Rect textureRect = new Rect(border, border, width-2*border, height-2*border);
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
		UnityEngine.Debug.Log("First Run: exiting pause");
		//re-hide the distance
		SetAheadBoxVisible(false);
	}
	
	public override void GyroDidReset()
	{
		if (eCurrentScreen == FirstRunScreen.ResetGyrosScreen)
		{
			//progress to next screen
			eCurrentScreen = FirstRunScreen.AwaitGPSScreen;
			//reveal virtual track
			SetVirtualTrackVisible(true);
		}
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
				break;
			}
			case FirstRunScreen.AwaitGPSScreen:
			{
				eCurrentScreen = FirstRunScreen.ResetGyrosScreen;
				break;
			}
			case FirstRunScreen.ConfirmIndoorScreen:
			{
				Platform.Instance.SetIndoor(false);
				eCurrentScreen = FirstRunScreen.AwaitGPSScreen;
				break;
			}
			case FirstRunScreen.ReadyToStartScreen:
			{
				Platform.Instance.SetIndoor(false);
				eCurrentScreen = FirstRunScreen.AwaitGPSScreen;
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
		
		UnityEngine.Debug.Log("FirstRun: tap detected");
		
		switch(eCurrentScreen)
		{
		case FirstRunScreen.WelcomeScreen:
		{
			//proceed to next screen
			eCurrentScreen = FirstRunScreen.ResetGyrosScreen;	
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
				UnityEngine.Debug.Log("First Run: activating actor");
				actor.SetActive(true);	
			}
			} catch(Exception e) {
				UnityEngine.Debug.LogWarning("Error iterating actors list");	
			}	
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
		
	/// <summary>
	/// Handle the user performing the back gesture.
	/// </summary>
	void HandleBack() {
			//go back if on the confirm indoor screen
			if(eCurrentScreen == FirstRunScreen.ConfirmIndoorScreen)
			{
				eCurrentScreen = FirstRunScreen.AwaitGPSScreen;	
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
