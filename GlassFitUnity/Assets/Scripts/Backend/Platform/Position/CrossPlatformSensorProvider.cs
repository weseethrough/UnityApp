using System;
using UnityEngine;
using PositionTracker;

public class CrossPlatformSensorProvider : ISensorProvider 
{
	public float[] LinearAcceleration { 
		get {
			float[] acc = new float[3];
			acc[0] = Input.acceleration.x;
			acc[1] = Input.acceleration.y;
			acc[2] = Input.acceleration.z;
			
			return acc;
		}
	}

}


