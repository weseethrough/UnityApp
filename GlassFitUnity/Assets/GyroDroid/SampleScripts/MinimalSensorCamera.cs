// PFC - prefrontal cortex
// Full Android Sensor Access for Unity3D
// Contact:
// 		contact.prefrontalcortex@gmail.com

using UnityEngine;
using System.Collections;

public class MinimalSensorCamera : MonoBehaviour {
	public Quaternion offsetFromStart;
	public Quaternion camFromStart;
	private bool firstRotate = true;
	// Use this for initialization
	void Start () {
		// you can use the API directly:
		//Sensor.Activate(Sensor.Type.RotationVector);
		
			
		// or you can use the SensorHelper, which has built-in fallback to less accurate but more common sensors:
		SensorHelper.ActivateRotation();
		offsetFromStart = SensorHelper.rotation;
		camFromStart = transform.rotation;
		//SensorHelper.TryForceRotationFallback(RotationFallbackType.RotationQuaternion);
		useGUILayout = false;
	}
	
	
	void OnGUI()
	{
			if(GUI.Button (new Rect(Screen.width/2-50,Screen.height/2 -50,100,100), "setGyro"))
		{ 
			
			
			offsetFromStart = SensorHelper.rotation;
	
			
		}
	}
	// Update is called once per frame
	void Update () {
		
	
		
		
		Quaternion newOffset = Quaternion.Inverse(offsetFromStart) * SensorHelper.rotation;
		
		
		// direct Sensor usage:
		//transform.rotation = Sensor.rotationQuaternion; //--- is the same as Sensor.QuaternionFromRotationVector(Sensor.rotationVector);
		
		// Helper with fallback:
		transform.rotation =  newOffset;
	}
}