using System;
using UnityEngine;
using PositionTracker;

public class CrossPlatformSensorProvider : ISensorProvider 
{
	private Vector3 _acc = Vector3.zero;

	public float[] LinearAcceleration { 
		get {
			float[] acc = new float[3];
			acc[0] = _acc.x;
			acc[1] = _acc.y;
			acc[2] = _acc.z;
			
			return acc;
		}
	}

	public void Start() {
		
	}

	public void Update() {
		_acc = Input.acceleration;
	}
}


