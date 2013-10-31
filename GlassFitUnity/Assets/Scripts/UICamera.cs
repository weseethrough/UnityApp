using UnityEngine;
using System.Collections;

public class UICamera : MonoBehaviour {
	public Quaternion offsetFromStart;
	private bool started = false;
	private Vector3 scale;
	
	void Start() {
		scale.x = (float)Screen.width / 800.0f;
		scale.y = (float)Screen.height / 500.0f;
    	scale.z = 1;
	}
	
	void OnGUI()
	{
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
		GUI.skin.button.wordWrap = true;
		GUI.skin.button.fontSize = 15;
		GUI.skin.button.fontStyle = FontStyle.Bold;
		GUI.skin.button.alignment = TextAnchor.MiddleCenter;				
		
		// Reset the Gyro at the start
		if(!started)
		{
			Platform.Instance.resetGyro();
			offsetFromStart = Platform.Instance.getGlassfitQuaternion();
			offsetFromStart = Quaternion.Euler(0, offsetFromStart.eulerAngles.y, 0);
			started = true;
		}
		
		// Resets the gyro after a button press
		if(GUI.Button (new Rect(0, 450, 70, 50), "Set Gyro"))
		{ 
			Platform.Instance.resetGyro();
			offsetFromStart = Platform.Instance.getGlassfitQuaternion();
		}
		
		if(GUI.Button(new Rect(730, 450, 70, 50), "Reset Save")) {
			PlayerPrefs.DeleteAll();
		}
		GUI.matrix = Matrix4x4.identity;
	}
	
	void Update () {
		// Update the rotation and set it
		Quaternion newOffset = Quaternion.Inverse(offsetFromStart) * Platform.Instance.getGlassfitQuaternion();
		transform.rotation = newOffset;
	}
}