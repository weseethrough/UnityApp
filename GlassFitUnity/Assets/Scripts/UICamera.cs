using UnityEngine;
using System.Collections;

public class UICamera : MonoBehaviour {
	public Quaternion offsetFromStart;
	private bool started = false;

	void OnGUI()
	{
		// Reset the Gyro at the start
		if(!started)
		{
			Platform.Instance.resetGyro();
			offsetFromStart = Platform.Instance.getGlassfitQuaternion();
			offsetFromStart = Quaternion.Euler(0, offsetFromStart.eulerAngles.y, 0);
			started = true;
		}
		
		// Resets the gyro after a button press
		if(GUI.Button (new Rect(0, Screen.height - 100, 100, 100), "setGyro"))
		{ 
			Platform.Instance.resetGyro();
			offsetFromStart = Platform.Instance.getGlassfitQuaternion();
		}
	}
	
	void Update () {
		// Update the rotation and set it
		Quaternion newOffset = Quaternion.Inverse(offsetFromStart) * Platform.Instance.getGlassfitQuaternion();
		transform.rotation = newOffset;
	}
}