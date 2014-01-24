using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Rotates the camera in-game and sets the grid
/// </summary>
public class MinimalSensorCamera : MonoBehaviour {
	
	public const Boolean DEBUG_ORIENTATION = false;

	public Quaternion offsetFromStart;
	private Quaternion bearingOffset = Quaternion.identity; // rotation between initialBearing and player's current bearing.
	                                                        // applied to camera in each update() call.
	private Quaternion? initialBearing = null;  // first valid bearing we receive. Updated on 2-tap (along with gyros)
	                                            // null iff no valid bearing has been calculated yet
	private bool started;
	public GameObject grid;
	private bool gridOn = false;
	private float gridTimer = 0.0f;
	private bool timerActive = false;
	private float yRotate = 0f;
	private bool rearview = false;
	private bool noGrid = false;
	private Vector3 scale;
	
	private bool indoor = false;
	
	private GestureHelper.TwoFingerTap twoHandler = null;
	
	private GestureHelper.ThreeFingerTap threeHandler = null;
	
	static bool sensorRotationPaused = false;
	
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
		
		twoHandler = new GestureHelper.TwoFingerTap(() => {
			ResetBearing();
		});
		GestureHelper.onTwoTap += twoHandler;
		
		threeHandler = new GestureHelper.ThreeFingerTap(() => {
			SetRearview();
		});
		GestureHelper.onThreeTap += threeHandler;

		bool ARCameraOn = Convert.ToBoolean(DataVault.Get("camera_setting"));
		if(!ARCameraOn)
		{
			GetComponentInChildren<QCARBehaviour>().enabled = false;
			GetComponentInChildren<DefaultInitializationErrorHandler>().enabled = false;
			GetComponentInChildren<WebCamBehaviour>().enabled = false;
			GetComponentInChildren<KeepAliveBehaviour>().enabled = false;
		}
	}
	
	void SetRearview() {
		if(Convert.ToBoolean(DataVault.Get("rearview_mirror"))) {
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

		if (DEBUG_ORIENTATION)
		{
			// print orientation (x,y,z)
			Quaternion realWorldOffset = Platform.Instance.GetPlayerOrientation().AsRealWorldQuaternion();
//			GUI.Label(new Rect(200, 050, 400, 50), "Euler x: " + ((int)realWorldOffset.eulerAngles.x).ToString(), labelStyle);
//			GUI.Label(new Rect(200, 100, 400, 50), "Euler y: " + ((int)realWorldOffset.eulerAngles.y).ToString(), labelStyle);
//			GUI.Label(new Rect(200, 150, 400, 50), "Euler z: " + ((int)realWorldOffset.eulerAngles.z).ToString(), labelStyle);

			// print orientation (yaw, pitch roll)
			Vector3 YPR = OrientationUtils.QuaternionToYPR(realWorldOffset);
//			GUI.Label(new Rect(200, 250, 400, 50), "Euler yaw: "   + ((int)Mathf.Rad2Deg*YPR.x).ToString(), labelStyle);
//			GUI.Label(new Rect(200, 300, 400, 50), "Euler pitch: " + ((int)Mathf.Rad2Deg*YPR.y).ToString(), labelStyle);
			GUI.Label(new Rect(200, 350, 400, 50), "Euler roll: "  + ((int)Mathf.Rad2Deg*YPR.z).ToString(), labelStyle);

			// print bearing debug data
		    GUI.Label(new Rect(200, 150, 400, 50), "Bearing: " + ((int)Platform.Instance.Bearing()).ToString(), labelStyle);
//		    GUI.Label(new Rect(200, 200, 400, 50), "Camera offset: " + ((int)bearingOffset.eulerAngles.y).ToString(), labelStyle);
		    GUI.Label(new Rect(200, 200, 400, 50), "Indoor mode: " + indoor.ToString(), labelStyle);
		}

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

	// if the user does a 2-top to reset the gyros, we need to
	// reset the bearing as well (to straight ahead)
	void ResetBearing()
	{
		// check to see if indoor has been changed, e.g. at the beginning
		// of the game but after start() runs
		indoor = Platform.Instance.IsIndoor();

		if (Platform.Instance.Bearing() != -999.0f) {
			initialBearing = Quaternion.Euler (0.0f, Platform.Instance.Bearing(), 0.0f);
			bearingOffset = Quaternion.identity;
		} else {
			initialBearing = null;
			bearingOffset = Quaternion.identity;
		}
	}
	
	/// <summary>
	/// Update this instance. Updates the rotation
	/// </summary>
	void Update () {

#if !UNITY_EDITOR	
		// Set the offset if it hasn't been set already, doesn't work in Start() function
		if(!started)
		{

			Platform.Instance.GetPlayerOrientation().Reset();
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
		Quaternion headOffset = Platform.Instance.GetPlayerOrientation().AsQuaternion();
		
		// Check for rearview
		Quaternion rearviewOffset = Quaternion.Euler(0, (rearview ? 180 : 0), 0);
				
		if(!sensorRotationPaused)
		{
			// Rotate the camera
			if(!indoor) {
				transform.rotation = bearingOffset * rearviewOffset * headOffset;
			} else {
				transform.rotation = rearviewOffset * headOffset;
			}
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
	
	/// <summary>
	/// Pauses the sensor rotation.
	/// To be called when 
	/// </summary>
	static public void PauseSensorRotation()
	{
		sensorRotationPaused = true;
	}
	
	static public void ResumeSensorRotation()
	{
		sensorRotationPaused = false;
		//maybe reset gyros here on coming out of this?
	}
	
	void OnDestroy() 
	{
		GestureHelper.onTwoTap -= twoHandler;
		GestureHelper.onThreeTap -= threeHandler;
		//GestureHelper.onSwipeLeft -= leftHandler;
		//GestureHelper.onTap -= tapHandler;
	}
}