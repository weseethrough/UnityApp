// PFC - prefrontal cortex
// Full Android Sensor Access for Unity3D
// Contact:
// 		contact.prefrontalcortex@gmail.com

using UnityEngine;
using System.Collections;

public class UICamera : MonoBehaviour {
	public Quaternion offsetFromStart;
	public Quaternion camFromStart;
	public Quaternion planarOffset;
	private Quaternion prevRot;
	private bool started = false;
	Platform inputData = null;
	
	//private bool firstRotate = true;

	
	void Start () {
		// you can use the API directly:
		//Sensor.Activate(Sensor.Type.RotationVector);
		inputData = new Platform();
		
		// or you can use the SensorHelper, which has built-in fallback to less accurate but more common sensors:
		SensorHelper.ActivateRotation();
		//offsetFromStart = inputData.getRotationVector();
		//float[] currentRot = inputData.getYPR();
		offsetFromStart = inputData.getGyroDroidQuaternion();
		camFromStart = transform.rotation;
		prevRot = transform.rotation;
		
		//SensorHelper.TryForceRotationFallback(RotationFallbackType.RotationQuaternion);
		useGUILayout = false;
	}
	
	void OnGUI()
	{
		if(!started)
		{
			//float[] currentRot = inputData.getYPR();
			//offsetFromStart = inputData.getRotationVector()* Quaternion.Euler(0, 0, 90);
			offsetFromStart = inputData.getGyroDroidQuaternion();
			offsetFromStart = Quaternion.Euler(0, offsetFromStart.eulerAngles.y, 0);
			started = true;
		}
		
		if(GUI.Button (new Rect(0, Screen.height - 100, 100, 100), "setGyro"))
		{ 
			//float[] currentRot = inputData.getYPR();
			//offsetFromStart = inputData.getRotationVector()* Quaternion.Euler(0, 0, 90);
			offsetFromStart = inputData.getGyroDroidQuaternion();
			//offsetFromStart = Quaternion.Euler(0, offsetFromStart.eulerAngles.y, 0);
		}
	}
	
	void Update () {
		
		//float[] currentRot = inputData.getYPR();
		//Quaternion newOffset = Quaternion.Inverse(offsetFromStart) * (inputData.getRotationVector() * Quaternion.Euler(0, 0, 90));
		Quaternion newOffset = Quaternion.Inverse(offsetFromStart) * inputData.getGyroDroidQuaternion();
		transform.rotation = newOffset;
		
		if(this.camera.fieldOfView == 10 || Input.GetKeyDown(KeyCode.Return) /** DEBUG: for editor */)
			AutoFade.LoadLevel(1, 1, 1, Color.black);
		
		if(Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
		
	}
}