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
	public GameObject test;
	public GameObject checker;
	public bool zoomedIn = false;
	public bool stuck = false;
	private Quaternion prevRot;
	
	private float timer;
	private int touchCount=0;
	
	//private bool firstRotate = true;

	
	void Start () {
		// you can use the API directly:
		//Sensor.Activate(Sensor.Type.RotationVector);
		
		// or you can use the SensorHelper, which has built-in fallback to less accurate but more common sensors:
		SensorHelper.ActivateRotation();
		offsetFromStart = SensorHelper.rotation;
		camFromStart = transform.rotation;
		prevRot = transform.rotation;
		//SensorHelper.TryForceRotationFallback(RotationFallbackType.RotationQuaternion);
		useGUILayout = false;
	}
	
	void OnGUI()
	{
	//	if(GUI.Button (new Rect(0, Screen.height - 100, 100, 100), "setGyro"))
	//	{ 
	
		if(GUI.Button (new Rect(0, Screen.height - 100, 100, 100), "setGyro"))
		{ 
			offsetFromStart = SensorHelper.rotation;
			offsetFromStart = Quaternion.Euler(0, offsetFromStart.eulerAngles.y, 0);
		}
	}
	
	void Update () {
		
		Quaternion newOffset = Quaternion.Inverse(offsetFromStart) * SensorHelper.rotation;
		
		// direct Sensor usage:
		//transform.rotation = Sensor.rotationQuaternion; //--- is the same as Sensor.QuaternionFromRotationVector(Sensor.rotationVector);

		// Helper with fallback:
		//transform.rotation =  Quaternion.Slerp(prevRot, newOffset, Time.deltaTime*2);
		if(!stuck)
		{
			transform.rotation = newOffset;
		}
		//Quaternion i = prevRot.
		//prevRot = transform.rotation;
	}
}