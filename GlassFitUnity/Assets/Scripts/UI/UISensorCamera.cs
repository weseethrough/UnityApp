using UnityEngine;
using System.Collections;

/// <summary>
/// Used to control the rotation of the camera in the Hex menus
/// </summary>
public class UISensorCamera : MonoBehaviour {
	// Offset for the camera
	public Quaternion offsetFromStart;
	
	// Boolean to initialise the started variable
	private bool started = false;
	private Vector3 scale;
	
	private GestureHelper.OnSwipeLeft backHandler = null;
	
	private GestureHelper.TwoFingerTap twoTapHandler = null;
	
	/// <summary>
	/// Start this instance. Sets the scale for OnGUI
	/// </summary>
	void Start() {
		// Sets the scaling value for the OnGUI 
		scale.x = (float)Screen.width / 800.0f;
		scale.y = (float)Screen.height / 500.0f;
    	scale.z = 1;
		
		twoTapHandler = new GestureHelper.TwoFingerTap(() => {
			ResetGyroGlass();
		});
		GestureHelper.onTwoTap += twoTapHandler;
		
		backHandler = new GestureHelper.OnSwipeLeft(() => {
			GoBack();
		});
		
		DataVault.Set("rearview", false);
		
		GestureHelper.swipeLeft += backHandler;
	}
	
	void GoBack() 
	{
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		
		GConnector gConect = fs.Outputs.Find(r => r.Name == "MenuButton");
		if(gConect != null) {
			fs.parentMachine.FollowConnection(gConect);
		}
	}
	
	/// <summary>
	/// Raises the GU event. Displays buttons to reset gyros and save
	/// </summary>
	void OnGUI()
	{
		// Set the attributes for the OnGUI elements
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
		GUI.skin.button.wordWrap = true;
		GUI.skin.button.fontSize = 15;
		GUI.skin.button.fontStyle = FontStyle.Bold;
		GUI.skin.button.alignment = TextAnchor.MiddleCenter;				
		
		// Reset the Gyro at the start, didn't work in Start()
		if(!started)
		{
#if !UNITY_EDITOR
			Platform.Instance.ResetGyro();
			offsetFromStart = Platform.Instance.GetOrientation();
			offsetFromStart = Quaternion.Euler(0, offsetFromStart.eulerAngles.y, 0);
#endif
			// Reset the rotation in the hex list.
			DynamicHexList[] lists = GameObject.FindObjectsOfType(typeof(DynamicHexList)) as DynamicHexList[];
            foreach (DynamicHexList dhl in lists)
            {
                dhl.ResetGyro();
            }
			started = true;
		}
		
		// Resets the gyro after a button press
		if(GUI.Button (new Rect(0, 450, 70, 50), "Set Gyro"))
		{ 
#if !UNITY_EDITOR
			Platform.Instance.ResetGyro();
			offsetFromStart = Platform.Instance.GetOrientation();
#endif
			// Reset the gyros in the hex lists
            DynamicHexList[] lists = GameObject.FindObjectsOfType(typeof(DynamicHexList)) as DynamicHexList[];
            foreach (DynamicHexList dhl in lists)
            {
                dhl.ResetGyro();
            }

		}
		
		// Delete the save for the training mode
		if(GUI.Button(new Rect(730, 450, 70, 50), "Reset Save")) {
			PlayerPrefs.DeleteAll();
		}
		GUI.matrix = Matrix4x4.identity;
	}
	
	void ResetGyroGlass()
	{
		ResetGyro();
	}
	
	void ResetGyro()
	{
#if !UNITY_EDITOR
		Platform.Instance.ResetGyro();
		offsetFromStart = Platform.Instance.GetOrientation();
		DynamicHexList[] lists = GameObject.FindObjectsOfType(typeof(DynamicHexList)) as DynamicHexList[];
        foreach (DynamicHexList dhl in lists)
        {
            dhl.ResetGyro();
        }
#endif
	}
	
	/// <summary>
	/// Update this instance. Sets the rotation of the camera from Platform
	/// </summary>
	void Update () {
		// Update the rotation and set it
#if !UNITY_EDITOR
		Quaternion newOffset = Quaternion.Inverse(offsetFromStart) * Platform.Instance.GetOrientation();

		transform.rotation = newOffset;
		
#endif
    }
	
	void OnDestroy() {
		GestureHelper.onTwoTap -= twoTapHandler;
		GestureHelper.swipeLeft -= backHandler;
	}
}