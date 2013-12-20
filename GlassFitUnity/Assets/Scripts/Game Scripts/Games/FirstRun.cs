using UnityEngine;
using System.Collections;

/// <summary>
/// First run 'challenge'.
/// See http://162.13.14.23:8080/display/RYTD/Alpha+in-challenge+Tutorial for design
/// </summary>

/// <summary>
/// enumeration of screens in this 'challenge'
/// </summary>
public enum FirstRunScreen 
{
	WelcomeScreen,
	ResetGyrosScreen,
	AwaitGPSScreen,
	ConfirmIndoorScreen,
	StartScreen,
};

public class FirstRun : MonoBehaviour {
	
	protected FirstRunScreen eCurrentScreen;
	protected GestureHelper.OnTap tapHandler;
 	//protected GestureHelper.OnSwipeRight backHandler;

	// Use this for initialization
	void Start () {
		
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
		
		//initialise screen
		eCurrentScreen = FirstRunScreen.WelcomeScreen;
		
		//set any necessary flags in game base to delay start, suppress normal UI etc.
	}
	
	// Update is called once per frame
	void Update () {
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
	
	void OnGUI() {
		//rect for the main 'headline'
		Rect MainMessageRect = new Rect(50, 50, 500, 600);
		
		//rect for the navigation prompt
		Rect NavMessageRect = new Rect(50, 300, 500, 600);
		
		GUIStyle MainMessageStyle = getLabelStyleLarge();
		GUIStyle NavMessageStyle = getLabelStyleNav();
		
		
		switch(eCurrentScreen)
		{
		case FirstRunScreen.WelcomeScreen:
		{
			GUI.Label(MainMessageRect, "Welcome to the First Run", MainMessageStyle); 
			GUI.Label(NavMessageRect, "Tap to Continue", NavMessageStyle);
			break;
		}
		case FirstRunScreen.ResetGyrosScreen:
		{
			GUI.Label(MainMessageRect, "Step 1: Reset Gyros", MainMessageStyle);
			GUI.Label(NavMessageRect, "Look Directly Forward and Tap", NavMessageStyle);
			break;
		}
		case FirstRunScreen.AwaitGPSScreen:
		{
			GUI.Label(MainMessageRect, "Step 2: Awaiting GPS Lock", MainMessageStyle);
			GUI.Label(NavMessageRect, "Tap to continue in Indoor Mode", NavMessageStyle);
			break;
		}
		case FirstRunScreen.ConfirmIndoorScreen:
		{
			GUI.Label(MainMessageRect, "Indoor Mode. Are you sure?", MainMessageStyle);
			GUI.Label(NavMessageRect, "Tap to continue\nSwipe down to cancel", NavMessageStyle);
			break;
		}
		case FirstRunScreen.StartScreen:
		{
			string mainMessage = Platform.Instance.IsIndoor() ? "Indoor Mode Active" : "GPS Lock Acquired";
			GUI.Label(MainMessageRect, mainMessage, MainMessageStyle);
			GUI.Label(NavMessageRect, "Tap to Begin", NavMessageStyle);
			break;
		}
			
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
			//for now, count this as gyros reset
			//proceed to next screen
			eCurrentScreen = FirstRunScreen.AwaitGPSScreen;
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
			//Begin the countdown
			//This should call up to the game base
			break;
		}	
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
	
		
}
