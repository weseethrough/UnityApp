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
	StartScreen,
};

public enum FirstRunHint
{
	NoHint,
	MenuHint,
	ResetGyrosHint,
};

public class FirstRun : GameBase {
	
	protected FirstRunScreen eCurrentScreen;
	protected FirstRunHint eCurrentHint;
	protected GestureHelper.OnTap tapHandler;
 	//protected GestureHelper.OnSwipeRight backHandler;
	
	private bool runReadyToStart = false;
	public GameObject runner;		//a runner object. This will be cloned around for various benchmark paces.
	
	//tuneable parameters
	const float hintCycleDelay = 5.0f;
	
	
	// Use this for initialization
	void Start () {
		
		base.Start ();
		
		UnityEngine.Debug.Log("FirstRun: Start");
		
		DataVault.Set("calories", "");
		DataVault.Set("pace", "");
		DataVault.Set("distance", "");
		DataVault.Set("time", "");
		DataVault.Set("ahead_box", "");
		
		// create and register handlers for tap, double tap, back
		tapHandler = new GestureHelper.OnTap(() => {
			HandleTap();
		});
		GestureHelper.onTap += tapHandler;
		
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
		
		//create target tracker - TODO create ones for various different pace benchmarks
		Platform.Instance.ResetTargets();
		Platform.Instance.CreateTargetTracker(3.0f);
		
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
		if(tapped) { HandleTap(); }
			
		//check for GPS lock
		if(eCurrentScreen == FirstRunScreen.AwaitGPSScreen)
		{
			//if we've got GPS, proceed to the start
			if(Platform.Instance.HasLock())
			{
				Platform.Instance.SetIndoor(false);
				eCurrentScreen = FirstRunScreen.StartScreen;
			}
		}
	}
	
	protected GUIStyle getLabelStyleLarge()
	{
		// set style for our labels
		GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
		labelStyle.alignment = TextAnchor.UpperCenter;
		labelStyle.fontSize = 65;
		labelStyle.fontStyle = FontStyle.Normal;
				
		return labelStyle;
	}
		
	protected GUIStyle getLabelStyleNav()
	{
		// set style for our labels
		GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
		labelStyle.alignment = TextAnchor.UpperCenter;
		labelStyle.fontSize = 35;
		labelStyle.fontStyle = FontStyle.Normal;
		
		return labelStyle;
	}
	
	protected GUIStyle getLabelStyleHint()
	{
		// set style for our labels
		GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
		labelStyle.alignment = TextAnchor.UpperLeft;
		labelStyle.fontSize = 35;
		labelStyle.fontStyle = FontStyle.Normal;
		
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
		
		float width = Screen.width;
		float height = Screen.height;
		float border = 10.0f;
		float mainHeight = height * 0.4f;
		float navHeight = height * 0.15f;
		//rect for the main 'headline'
		Rect MainMessageRect = new Rect(border, border, width - 2*border , mainHeight);
		//rect for the navigation prompt
		Rect NavMessageRect = new Rect(border, height - border - navHeight, width - 2*border, navHeight);
		Rect HintRect = new Rect(border, border, width - 2*border, mainHeight);
		
		GUIStyle MainMessageStyle = getLabelStyleLarge();
		GUIStyle NavMessageStyle = getLabelStyleNav();
		GUIStyle HintStyle = getLabelStyleHint();
		
		switch(eCurrentScreen)
		{
		case FirstRunScreen.WelcomeScreen:
		{
			DrawTintBox();
			GUI.Label(MainMessageRect, "Welcome to the First Run", MainMessageStyle); 
			GUI.Label(NavMessageRect, "Tap to Continue", NavMessageStyle);
			break;
		}
		case FirstRunScreen.ResetGyrosScreen:
		{
			DrawTintBox();
			GUI.Label(MainMessageRect, "Step 1: Reset Gyros", MainMessageStyle);
			GUI.Label(NavMessageRect, "Look Directly Forward and Tap", NavMessageStyle);
			
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
			GUI.Label(NavMessageRect, "Tap to continue in Indoor Mode", NavMessageStyle);
			
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
			GUI.Label(NavMessageRect, "Tap to continue\nSwipe down to cancel", NavMessageStyle);
			break;
		}
		case FirstRunScreen.StartScreen:
		{
			DrawTintBox();
			string mainMessage = Platform.Instance.IsIndoor() ? "Indoor Mode Active" : "GPS Lock Acquired";
			GUI.Label(MainMessageRect, mainMessage, MainMessageStyle);
			GUI.Label(NavMessageRect, "Tap to Begin", NavMessageStyle);
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
	
	protected void DidResetGyros()
	{
		if (eCurrentScreen == FirstRunScreen.ResetGyrosScreen)
		{
			//progress to next screen
			eCurrentScreen = FirstRunScreen.AwaitGPSScreen;
			//reveal virtual track
			SetVirtualTrackVisible(true);
		}
	}
	
	/// <summary>
	/// Handle the user performing a tap gesture
	/// </summary>
	void HandleTap() {
		
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
			//consider this a tap for now
			DidResetGyros();
			break;
		}
		case FirstRunScreen.AwaitGPSScreen:
		{
			//if we tap here, go to the confirm indoor mode screen
			eCurrentScreen = FirstRunScreen.ConfirmIndoorScreen;
			break;
		}
		case FirstRunScreen.ConfirmIndoorScreen:
		{
			//tap means confirmation. Set indoor mode and proceed.
			Platform.Instance.SetIndoor(true);
			eCurrentScreen = FirstRunScreen.StartScreen;			
			break;
		}
		case FirstRunScreen.StartScreen:
		{
			//flag that the countdown can begin
			runReadyToStart = true;
			
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
		
		foreach( GameObject actor in actors)
		{
			actor.SetActive(true);
		}
		
	}

	private void GetNextHint() {
		switch(eCurrentHint)
			{
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
		if(tapHandler != null)
		{
			GestureHelper.onTap -= tapHandler;
		}
//		if(backHandler != null)
//		{
//			GestureHelper.onSwipeDown -= backHandler;
//		}
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
