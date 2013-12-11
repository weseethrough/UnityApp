using UnityEngine;
using System.Collections;

/// <summary>
/// Rotates the camera in-game and sets the grid
/// </summary>
public class MinimalSensorCamera : MonoBehaviour {
	
	public Quaternion offsetFromStart;
	private Quaternion bearingOffset = Quaternion.identity; // rotation between initialBearing and player's current bearing.
	                                                        // applied to camera in each update() call.
	private Quaternion? initialBearing = null;  // first valid bearing we receive. Updated on ResetGyros.
	                                            // null iff no valid bearing has been calculated yet
	
	private bool started;
	private float scaleX;
	private float scaleY;
	public GameObject grid;
	private bool gridOn = false;
	private float gridTimer = 0.0f;
	private bool timerActive = false;
	private float yRotate = 0f;
	private bool rearview = false;
	
	private GestureHelper.OnTap tapHandler = null;
	
	private GestureHelper.TwoFingerTap twoHandler = null;
	
	private GestureHelper.ThreeFingerTap threeHandler = null;
	
	private GestureHelper.OnSwipeLeft leftHandler = null;
	
	// Set the grid and scale values
	void Start () {
		grid.SetActive(false);
		scaleX = (float)Screen.width / 800.0f;
		scaleY = (float)Screen.height / 500.0f;
		
		twoHandler = new GestureHelper.TwoFingerTap(() => {
			ResetGyroGlass();
		});
		GestureHelper.onTwoTap += twoHandler;
		
		threeHandler = new GestureHelper.ThreeFingerTap(() => {
			SetRearview();
		});
		GestureHelper.onThreeTap += threeHandler;

		leftHandler = new GestureHelper.OnSwipeLeft(() => {
			FinishGame();
		});
		
		GestureHelper.swipeLeft += leftHandler;
	}
	
	void ResetGyro() 
	{
#if !UNITY_EDITOR
		// Activates the grid and reset the gyros if the timer is off, turns it off if the timer is on
		if(timerActive) {
			gridOn = false;
		} else {
			offsetFromStart = Platform.Instance.GetOrientation();
			if (Platform.Instance.Bearing() != -999.0f) {
				initialBearing = Quaternion.Euler (0.0f, Platform.Instance.Bearing(), 0.0f);
				bearingOffset = Quaternion.identity;
			} else {
				initialBearing = null;
				bearingOffset = Quaternion.identity;
			}
			Platform.Instance.ResetGyro();
			gridOn = true;
			gridTimer = 5.0f;
			timerActive = true;
		}
			
//		
//		}
//		else if(Event.current.type == EventType.Repaint)
//		{
//			// If the grid is on when the button is released, activate timer, else reset the timer and switch it off
//			if(gridOn)
//			{
//				timerActive = true;
//			} else
//			{
//				gridTimer = 0.0f;
//				timerActive = false;
//			}
//				
//		}
#endif
	}
	
	void SetRearview() {
		if((bool)DataVault.Get("rearview")) {
			rearview = !rearview;
		}
	}
	
	/// <summary>
	/// Raises the GU event. Creates a reset gyro button
	/// </summary>
	void OnGUI()
	{
		// Set the offset if it hasn't been set already, doesn't work in Start() function
		if(!started)
		{
#if !UNITY_EDITOR
			offsetFromStart = Platform.Instance.GetOrientation();
			offsetFromStart = Quaternion.Euler(0, offsetFromStart.eulerAngles.y, 0);
			Platform.Instance.ResetGyro();
			started = true;
#endif
		}
		
		// Set the new GUI matrix based on scale and the depth
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scaleX, scaleY, 1));		
		GUI.depth = 7;
		
#if !UNITY_EDITOR
		if(!started) {
			if(GUI.Button(new Rect(200, 0, 400, 500), "", GUIStyle.none)) {
				started = true;
			}
		} else {
			// Check if the button is being held
			if(GUI.RepeatButton(new Rect(200, 0, 400, 250), "", GUIStyle.none))
			{ 
				// Activates the grid and reset the gyros if the timer is off, turns it off if the timer is on
				if(timerActive) {
					gridOn = false;
				} else {
					offsetFromStart = Platform.Instance.GetOrientation();
					Platform.Instance.ResetGyro();
					gridOn = true;
				}
				gridTimer = 5.0f;
			
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
		}
#endif
		GUI.matrix = Matrix4x4.identity;
	}
	
	void ResetGyroGlass()
	{
		ResetGyro();
	}
	
	/// <summary>
	/// Delegate function for Glass - when the user swipes back this is called to end the game
	/// </summary>
	void FinishGame()
	{
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		GConnector gConnect = fs.Outputs.Find(r => r.Name == "FinishButton");
		if(gConnect != null) {
			DataVault.Set("total", Platform.Instance.GetCurrentPoints() + Platform.Instance.GetOpeningPointsBalance());
			DataVault.Set("bonus", 0);
			Platform.Instance.StopTrack();
			GestureHelper.onTap -= tapHandler;
			tapHandler = new GestureHelper.OnTap(() => {
				Continue();
			});
			GestureHelper.onTap += tapHandler;
			fs.parentMachine.FollowConnection(gConnect);
		} else {
			UnityEngine.Debug.Log("Camera: No connection found - FinishButton");
		}
	}
	
	/// <summary>
	/// Part of the delegate function for Glass. When the user taps the screen it presses the continue button.
	/// </summary>
	void Continue() {
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		GConnector gConnect = fs.Outputs.Find(r => r.Name == "ContinueButton");
		if(gConnect != null) {
			(gConnect.Parent as Panel).CallStaticFunction(gConnect.EventFunction, null);
			fs.parentMachine.FollowConnection(gConnect);
		} else {
			UnityEngine.Debug.Log("Camera: No connection found - ContinueButton");
		}
	}
	
	/// <summary>
	/// Update this instance. Updates the rotation
	/// </summary>
	void Update () {
		
#if !UNITY_EDITOR
		// Check for changes in the player's bearing
		if (Platform.Instance.Bearing() != -999.0f) {
			Quaternion currentBearing = Quaternion.Euler (0.0f, Platform.Instance.Bearing(), 0.0f);
			if (initialBearing.HasValue == false) {
				// if this is the first valid bearing we've had, use it as the reference point and return identity
				initialBearing = currentBearing;
			}
			bearingOffset = initialBearing.Value * Quaternion.Inverse (currentBearing);
		}
		UnityEngine.Debug.Log("Bearing w-component: " + bearingOffset);
		
		// Set the new rotation of the camera
		Quaternion newOffset;
		if(rearview) {
			newOffset = bearingOffset * Quaternion.Inverse(Quaternion.Euler(0, 180, 0)) * (Quaternion.Inverse(offsetFromStart) * Platform.Instance.GetOrientation());
		} else {
			newOffset = bearingOffset * Quaternion.Inverse(offsetFromStart) * Platform.Instance.GetOrientation();
		}
#else
		if(Input.GetKeyDown(KeyCode.B)) {
			yRotate += 180f;
			if(yRotate >= 360f) {
				yRotate -= 360f;
			}
			transform.rotation = Quaternion.Euler(0, yRotate, 0);
		}
		
		if(Input.GetKeyDown(KeyCode.G))
		{
			FlowState fs = FlowStateMachine.GetCurrentFlowState();
			GConnector gConect = fs.Outputs.Find(r => r.Name == "straightPursuitExit");
			if(gConect != null) {
				fs.parentMachine.FollowConnection(gConect);
			} else {
				UnityEngine.Debug.Log("Game: No connection found!");
			}
		}
#endif
		
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
		
		grid.SetActive(gridOn);
		
#if !UNITY_EDITOR
		transform.rotation =  newOffset;	
#endif
	}
	
	void OnDestroy() 
	{
		GestureHelper.onTwoTap -= twoHandler;
		GestureHelper.onThreeTap -= threeHandler;
		GestureHelper.swipeLeft -= leftHandler;
		GestureHelper.onTap -= tapHandler;
	}
}