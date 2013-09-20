using UnityEngine;
using System.Collections;

public class SphereRotate : MonoBehaviour {
public Quaternion offsetFromStart;
	public Quaternion camFromStart;
	public Quaternion planarOffset;
	private bool firstRotate = true;
	float xRot = 0;
	
	void Start () {
		// you can use the API directly:
		
		// or you can use the SensorHelper, which has built-in fallback to less accurate but more common sensors:
		SensorHelper.ActivateRotation();
		offsetFromStart = SensorHelper.rotation;
		camFromStart = transform.rotation;
		//SensorHelper.TryForceRotationFallback(RotationFallbackType.RotationQuaternion);
	}
	
	void OnGUI()
	{
		
		//xRot = GUI.HorizontalSlider(new Rect(0, 0, 500, 100), xRot, -42, 42);
		
		if(GUI.Button (new Rect(0, Screen.height - 100, 100, 100), "setGyro"))
		{ 
			offsetFromStart = SensorHelper.rotation;
			//offsetFromStart = Quaternion.Euler(0, offsetFromStart.eulerAngles.y, 0);
		}
		//offsetFromStart = Quaternion.Euler(xRot, 0, 0);
	}
	
	void Update () {
		
		Quaternion newOffset = Quaternion.Inverse(offsetFromStart) * SensorHelper.rotation;
		
		// direct Sensor usage:
		//transform.rotation = Sensor.rotationQuaternion; //--- is the same as Sensor.QuaternionFromRotationVector(Sensor.rotationVector);

		// Helper with fallback:
		transform.rotation =  newOffset;		
	}
}
