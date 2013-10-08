// PFC - prefrontal cortex
// Full Android Sensor Access for Unity3D
// Contact:
// 		contact.prefrontalcortex@gmail.com

using UnityEngine;
using System.Collections;

public class MinimalSensorCamera : MonoBehaviour {
	public Quaternion offsetFromStart;
	public Quaternion camFromStart;
	public Quaternion planarOffset;
	private float timer;
	private bool started = false;
	private int touchCount=0;
	private float scaleX;
	private float scaleY;
	public GameObject grid;
	private bool gridOn = false;
	//private bool firstRotate = true;

	
	void Start () {
		// you can use the API directly:
		//Sensor.Activate(Sensor.Type.RotationVector);
		grid.SetActive(false);
		scaleX = (float)Screen.width / 800.0f;
		scaleY = (float)Screen.height / 500.0f;
		// or you can use the SensorHelper, which has built-in fallback to less accurate but more common sensors:
		SensorHelper.ActivateRotation();
		offsetFromStart = SensorHelper.rotation;
		camFromStart = transform.rotation;
		//SensorHelper.TryForceRotationFallback(RotationFallbackType.RotationQuaternion);
		useGUILayout = false;
	}
	
	void OnGUI()
	{
		if(!started)
		{
			offsetFromStart = SensorHelper.rotation;
			//offsetFromStart = Quaternion.Euler(0, offsetFromStart.eulerAngles.y, 0);
			started = true;
		}
		
		GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scaleX,scaleY, 1));		
		GUI.depth = 7;
		if(GUI.RepeatButton(new Rect(200, 0, 400, 250), "", GUIStyle.none))
		{ 
			offsetFromStart = SensorHelper.rotation;
			gridOn = true;
			//offsetFromStart = Quaternion.Euler(0, offsetFromStart.eulerAngles.y, 0);
		}
		else if(Event.current.type == EventType.Repaint)
		{
			gridOn = false;
		}
	}
	
	void Update () {
		
		Quaternion newOffset = Quaternion.Inverse(offsetFromStart) * SensorHelper.rotation;
		
		// direct Sensor usage:
		//transform.rotation = Sensor.rotationQuaternion; //--- is the same as Sensor.QuaternionFromRotationVector(Sensor.rotationVector);

		// Helper with fallback:
		if(gridOn)
		{
			grid.SetActive(true);
		}
		else
		{
			grid.SetActive(false);
		}
		transform.rotation =  newOffset;
		//
		
	}
}