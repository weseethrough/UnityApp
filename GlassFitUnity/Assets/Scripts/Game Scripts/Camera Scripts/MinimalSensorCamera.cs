using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Rotates the camera in-game and sets the grid
/// </summary>
public class MinimalSensorCamera : MonoBehaviour {

	public AnimationCurve cameraResponseCurve;		//mapping of head rotation to camera yaw
	public AnimationCurve constrainedWeightVsPace;	//how far constrained the camera is based on movement speed
	public float maxSpeedForCameraConstraint = 4f;

	public const Boolean DEBUG_ORIENTATION = false;
	const float pitchSensitivity = 2.3f;
	
	public Quaternion offsetFromStart;
	private Quaternion bearingOffset = Quaternion.identity; // rotation between initialBearing and player's current bearing.
	                                                        // applied to camera in each update() call.
	private Quaternion? initialBearing = null;  // first valid bearing we receive. Updated on 2-tap (along with gyros)
	                                            // null iff no valid bearing has been calculated yet

	protected float pitchOffset = 0.0f;

	private bool started;
	public GameObject grid;
	private bool gridOn = false;
	private float gridTimer = 0.0f;
	private bool timerActive = false;
	private float yRotate = 0f;
	private bool rearview = false;
	private bool noGrid = false;
	private Vector3 scale;
	
	private float startZoom = -1f;
	private float previousZoom = -1f;
	
	private float startPitch = -1f;
	private float previousPitch = -1f;
	private bool pitchActive = false;
	
	private bool indoor = false;
	
	private GestureHelper.TwoFingerTap twoHandler = null;
	
	private GestureHelper.ThreeFingerTap threeHandler = null;
	
	static bool sensorRotationPaused = false;
	
	private bool fovActive = false;
	
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

		fovActive = Convert.ToBoolean(DataVault.Get("activity_fov"));
		pitchActive = Convert.ToBoolean(DataVault.Get("activity_pitch"));
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
			// print bearing debug data
		    GUI.Label(new Rect(200, 050, 400, 50), "Bearing: " + ((int)Platform.Instance.LocalPlayerPosition.Bearing).ToString(), labelStyle);
			GUI.Label(new Rect(200, 100, 400, 50), "Yaw from north: "  + ((int)(Platform.Instance.GetPlayerOrientation().AsYawFromNorth()*Mathf.Rad2Deg)).ToString(), labelStyle);
			GUI.Label(new Rect(200, 150, 400, 50), "Yaw from forward: "  + ((int)(Platform.Instance.GetPlayerOrientation().AsYaw()*Mathf.Rad2Deg)).ToString(), labelStyle);
			GUI.Label(new Rect(200, 2000, 400, 50), "Cum-yaw from fwd: "  + ((int)(Platform.Instance.GetPlayerOrientation().AsCumulativeYaw()*Mathf.Rad2Deg)).ToString(), labelStyle);
//		    GUI.Label(new Rect(200, 100, 400, 50), "Camera offset: " + ((int)bearingOffset.eulerAngles.y).ToString(), labelStyle);
//		    GUI.Label(new Rect(200, 150, 400, 50), "Indoor mode: " + indoor.ToString(), labelStyle);

			// print orientation (yaw, pitch roll)
			Quaternion rff = Platform.Instance.GetPlayerOrientation().AsQuaternion();
			Quaternion rfn = Platform.Instance.GetPlayerOrientation().AsQuaternionFromNorth();
			Quaternion rfd = Platform.Instance.GetPlayerOrientation().AsRealWorldQuaternion();
//			Debug.Log("QXF (x,y,z,w): (" + rff.x + ", " + rff.y + ", " + rff.z + ", " +  rff.w + ")");
//			Debug.Log("QXD (x,y,z,w): (" + rfd.x + ", " + rfd.y + ", " + rfd.z + ", " +  rfd.w + ")");
//
			Vector3 YPR = OrientationUtils.QuaternionToYPR(rfn);
			GUI.Label(new Rect(200, 250, 400, 50), "Quat yaw: "   + ((int)(YPR.x*Mathf.Rad2Deg)).ToString(), labelStyle);
			GUI.Label(new Rect(200, 300, 400, 50), "Quat pitch: " + ((int)(YPR.y*Mathf.Rad2Deg)).ToString(), labelStyle);
			GUI.Label(new Rect(200, 350, 400, 50), "Quat roll: "  + ((int)(YPR.z*Mathf.Rad2Deg)).ToString(), labelStyle);



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
		indoor = Platform.Instance.LocalPlayerPosition.IsIndoor();

		if (Platform.Instance.LocalPlayerPosition.Bearing != -999.0f) {
			initialBearing = Quaternion.Euler (0.0f, Platform.Instance.LocalPlayerPosition.Bearing, 0.0f);
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

		if(!started)
		{

			Platform.Instance.GetPlayerOrientation().Reset();
			started = true;

		}
		
		// Check for changes in the player's bearing
		if (Platform.Instance.LocalPlayerPosition.Bearing != -999.0f) {
			Quaternion currentBearing = Quaternion.Euler (0.0f, Platform.Instance.LocalPlayerPosition.Bearing, 0.0f);
			if (initialBearing.HasValue == false) {
				// if this is the first valid bearing we've had, use it as the reference point and return identity
				initialBearing = currentBearing;
			}
			bearingOffset = Quaternion.Inverse (currentBearing) * initialBearing.Value;
		}
//		UnityEngine.Debug.Log("Bearing w-component: " + bearingOffset);
		
		// Check for changes in players head orientation
		PlayerOrientation ori = Platform.Instance.GetPlayerOrientation();
		Quaternion headOffset = Platform.Instance.GetPlayerOrientation().AsQuaternion();
		
		// Double the pitch
		Vector3 eulerAngles = headOffset.eulerAngles;
		eulerAngles.x *= 2.0f;

		//TEST - zero out the yaw
		//eulerAngles.y = 0f;

		//TEST 2 - non-linear yaw curve
		float yaw = eulerAngles.y;
		if(yaw > 180) { yaw -= 360; }
		float sign = yaw > 0 ? 1:-1;
		float parametricYaw = Mathf.Abs(yaw) / 180f;
		float adjustedYaw = sign * 180f * cameraResponseCurve.Evaluate(parametricYaw);

		//TEST 3 - apply varying weighting of real vs adjusted according to speed
		float adjustedWeight = constrainedWeightVsPace.Evaluate(Platform.Instance.LocalPlayerPosition.Pace / maxSpeedForCameraConstraint);

		//To see tests, uncomment this line.
		eulerAngles.y = Mathf.Lerp(yaw, adjustedYaw, adjustedWeight);


		//tilt down a little too
		//eulerAngles.x -= 15.0f;
		headOffset = Quaternion.Euler(eulerAngles);
		
		// Check for rearview
		Quaternion rearviewOffset = Quaternion.Euler(0, (rearview ? 180 : 0), 0);
			
		if(!sensorRotationPaused)
		{
			// Rotate the camera
			if(!indoor) {
				transform.rotation = /*bearingOffset */ rearviewOffset * headOffset;
			} else {
				transform.rotation = rearviewOffset * headOffset;
			}
		}

		///EDITOR-Specific keyboard controls
		if(Input.GetKeyDown(KeyCode.B)) {
			yRotate += 180f;
			if(yRotate >= 360f) {
				yRotate -= 360f;
			}
			transform.rotation = Quaternion.Euler(0, yRotate, 0);
		}
		
		if(Input.GetKeyDown(KeyCode.G))
		{
			FlowState.FollowFlowLinkNamed("straightPursuitExit");
		}
		
		
		///special camera controls
		if(fovActive) {
			if(Platform.Instance.GetTouchCount() == 1)
			{
				Vector2? xChange = Platform.Instance.GetTouchInput();
				if(xChange.HasValue)
				{
					if(startZoom != -1f) {
						float addedValue = xChange.Value.x - previousZoom;
						float newFOV = Camera.main.fieldOfView;
						newFOV += addedValue * 60f;
						//UnityEngine.Debug.Log("MinimalSensorCamera: current FOV is " + newFOV.ToString("f2"));
						Camera.main.fieldOfView = Mathf.Clamp(newFOV, 10f, 60f);
					} 
					else {
						startZoom = xChange.Value.x;
					}
					previousZoom = xChange.Value.x;
				}
			}
			else 
			{
				startZoom = -1f;
				previousZoom = -1f;
			}
		}
		
		if(pitchActive) {
			if(Platform.Instance.GetTouchCount() == 2)
			{
				Vector2? xChange = Platform.Instance.GetTouchInput();
				if(xChange.HasValue)
				{
					if(startPitch != -1f)
					{
						float addedValue = xChange.Value.x - previousPitch;
						float newPitch = Platform.Instance.GetPlayerOrientation().GetPitchOffset();
						newPitch += addedValue * 40f;
						pitchOffset = Mathf.Clamp(newPitch, -40f, 40f);
						UnityEngine.Debug.Log("MinimalSensorCamera: pitch is set to " + pitchOffset.ToString("f2"));
						Platform.Instance.GetPlayerOrientation().SetPitchOffset(pitchOffset);
					}
					else
					{
						startPitch = xChange.Value.x;
					}
					previousPitch = xChange.Value.x;
				}
			}
			else
			{
				startPitch = -1f;
				previousPitch = -1f;
			}
		} 
		
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
