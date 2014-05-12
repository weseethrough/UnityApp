using System;
using UnityEngine;
using PositionTracker;

public class CrossPlatformSensorProvider : MonoBehaviour, ISensorProvider 
{
	private Vector3 _acceleration = Vector3.zero;

	public float[] LinearAcceleration { 
		get {
			float[] acc = new float[3];
			acc[0] = _acceleration.x;
			acc[1] = _acceleration.y;
			acc[2] = _acceleration.z;

			return acc;
		}
	}


	public float Yaw {
		get; set;
	}

	//Input.acceleration can't be called except from the main thread. So poll this regularly from main thread. PositionTracker reads when it wants from whatever thread.
	public void Update()
	{
		_acceleration = Input.acceleration;
		Yaw = Input.compass.trueHeading;
		//UnityEngine.Debug.Log("Polling accelerometers: " + _acceleration);
	}

}


