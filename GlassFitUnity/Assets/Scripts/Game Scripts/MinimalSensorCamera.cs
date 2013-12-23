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
	private bool noGrid = false;
	private Vector3 scale;
	
	private bool indoor = false;
	
	private GestureHelper.OnTap tapHandler = null;
	
	private GestureHelper.TwoFingerTap twoHandler = null;
	
	private GestureHelper.ThreeFingerTap threeHandler = null;
	
	private GestureHelper.OnSwipeLeft leftHandler = null;
	
	// Set the grid and scale values
	void Start () {
		// Calculate and set scale
		float x = (float)Screen.width/800f;
		float y = (float)Screen.height/500f;
		scale = new Vector3(x, y, 1);
		
		if(grid != null) {
			grid.SetActive(false);
		} else {
			noGrid = true;
		}
		scaleX = (float)Screen.width / 800.0f;
		scaleY = (float)Screen.height / 500.0f;
		
		indoor = (bool)DataVault.Get("indoor_settings");
		
		twoHandler = new GestureHelper.TwoFingerTap(() => {
			ResetGyroGlass();
		});
		GestureHelper.onTwoTap += twoHandler;
		
		threeHandler = new GestureHelper.ThreeFingerTap(() => {
			SetRearview();
		});
		GestureHelper.onThreeTap += threeHandler;

		//leftHandler = new GestureHelper.OnSwipeLeft(() => {
		//	FinishGame();
		//});
		
		GestureHelper.swipeLeft += leftHandler;
		
		bool ARCameraOn = (bool)DataVault.Get("camera_setting");
		if(!ARCameraOn)
		{
			GetComponent<QCARBehaviour>().enabled = false;
			GetComponent<DefaultInitializationErrorHandler>().enabled = false;
			GetComponent<WebCamBehaviour>().enabled = false;
			GetComponent<KeepAliveBehaviour>().enabled = false;
		}
	}
	
	void ResetGyro() 
	{
#if !UNITY_EDITOR
		// Activates the grid and reset the gyros if the timer is off, turns it off if the timer is on
		if(!noGrid) {
			if(timerActive) {
				gridOn = false;
			} else {
				// reset orientation offset
				offsetFromStart = Platform.Instance.GetOrientation();
				UnityEngine.Debug.Log("MinimalSensorCamera: Angles are: " + offsetFromStart.eulerAngles.x + ", " + offsetFromStart.eulerAngles.y + ", " + offsetFromStart.eulerAngles.z);
				//offsetFromStart = Quaternion.Euler(offsetFromStart.eulerAngles.x, 0, 0);
			
				// reset bearing offset
				if (Platform.Instance.Bearing() != -999.0f) {
					initialBearing = Quaternion.Euler (0.0f, Platform.Instance.Bearing(), 0.0f);
					bearingOffset = Quaternion.identity;
				} else {
					initialBearing = null;
					bearingOffset = Quaternion.identity;
				}
			}		
		}
#endif
	}
	
	void SetRearview() {
		if((bool)DataVault.Get("rearview_mirror")) {
			rearview = !rearview;
		}
	}
	
	/// <summary>
	/// Raises the GU event. Creates a reset gyro button
	/// </summary>
	void OnGUI()
	{		
		// Set the new GUI matrix based on scale and the depth
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);		
		GUI.depth = 7;
		
		GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
		labelStyle.alignment = TextAnchor.UpperCenter;
		labelStyle.fontSize = 40;
		labelStyle.fontStyle = FontStyle.Bold;
		
		//GUI.Label(new Rect(300, 150, 200, 200), Platform.Instance.Bearing().ToString(), labelStyle); 
		
#if !UNITY_EDITOR
		if(!noGrid) {
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
						ResetGyro();
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
		}
#endif
		GUI.matrix = Matrix4x4.identity;
	}
	
	void ResetGyroGlass()
	{
		if(!noGrid) {
			if(timerActive) {
				gridOn = false;
			} else {
			
			// reset orientation offset
				offsetFromStart = Platform.Instance.GetOrientation();
			
			// reset bearing offset
			if (Platform.Instance.Bearing() != -999.0f) {
				initialBearing = Quaternion.Euler (0.0f, Platform.Instance.Bearing(), 0.0f);
				bearingOffset = Quaternion.identity;
			} else {
				initialBearing = null;
				bearingOffset = Quaternion.identity;
			}
			
				Platform.Instance.ResetGyro();
				//gridOn = true;
				//gridTimer = 5.0f;
				//timerActive = true;
			}
		}
	}
	
	//Moving this to GameBase, since individual game modes may need to customise the behaviour.
//	/// <summary>
//	/// Delegate function for Glass - when the user swipes back this is called to end the game
//	/// </summary>
//	void FinishGame()
//	{
//		FlowState fs = FlowStateMachine.GetCurrentFlowState();
//		GConnector gConnect = fs.Outputs.Find(r => r.Name == "FinishButton");
//		if(gConnect != null) {
//			DataVault.Set("total", Platform.Instance.GetCurrentPoints() + Platform.Instance.GetOpeningPointsBalance());
//			DataVault.Set("bonus", 0);
//			Platform.Instance.StopTrack();
//			GestureHelper.onTap -= tapHandler;
//			tapHandler = new GestureHelper.OnTap(() => {
//				Continue();
//			});
//			GestureHelper.onTap += tapHandler;
//			fs.parentMachine.FollowConnection(gConnect);
//		} else {
//			UnityEngine.Debug.Log("Camera: No connection found - FinishButton");
//		}
//	}
	
//	/// <summary>
//	/// Part of the delegate function for Glass. When the user taps the screen it presses the continue button.
//	/// </summary>
//	void Continue() {
//		FlowState fs = FlowStateMachine.GetCurrentFlowState();
//		GConnector gConnect = fs.Outputs.Find(r => r.Name == "ContinueButton");
//		if(gConnect != null) {
//			(gConnect.Parent as Panel).CallStaticFunction(gConnect.EventFunction, null);
//			fs.parentMachine.FollowConnection(gConnect);
//		} else {
//			UnityEngine.Debug.Log("Camera: No connection found - ContinueButton");
//		}
//	}
	
	/// <summary>
	/// Update this instance. Updates the rotation
	/// </summary>
	void Update () {

#if !UNITY_EDITOR		
		// Set the offset if it hasn't been set already, doesn't work in Start() function
		if(!started)
		{

			offsetFromStart = Platform.Instance.GetOrientation();
			//offsetFromStart = Quaternion.Euler(0, offsetFromStart.eulerAngles.y, 0);
			Platform.Instance.ResetGyro();
			started = true;

		}
		
		// Check for changes in the player's bearing
		if (Platform.Instance.Bearing() != -999.0f) {
			Quaternion currentBearing = Quaternion.Euler (0.0f, Platform.Instance.Bearing(), 0.0f);
			if (initialBearing.HasValue == false) {
				// if this is the first valid bearing we've had, use it as the reference point and return identity
				initialBearing = currentBearing;
			}
			bearingOffset = Quaternion.Inverse (currentBearing) * initialBearing.Value;
		}
//		UnityEngine.Debug.Log("Bearing w-component: " + bearingOffset);
		
		// Check for changes in players head orientation
		Quaternion headOffset = Quaternion.Inverse(offsetFromStart) * Platform.Instance.GetOrientation();
		
		// Check for rearview
		Quaternion rearviewOffset = Quaternion.Euler(0, (rearview ? 180 : 0), 0);
				
		// Rotate the camera
		if(!indoor) {
			transform.rotation = bearingOffset * rearviewOffset * headOffset;
		} else {
			transform.rotation = rearviewOffset * headOffset;
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
		if(!noGrid) {
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
		}
		
	}
	
	void OnDestroy() 
	{
		GestureHelper.onTwoTap -= twoHandler;
		GestureHelper.onThreeTap -= threeHandler;
		//GestureHelper.swipeLeft -= leftHandler;
		GestureHelper.onTap -= tapHandler;
	}
}