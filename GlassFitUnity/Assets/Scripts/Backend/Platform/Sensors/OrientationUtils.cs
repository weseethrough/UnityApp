using System;
using UnityEngine;

public static class OrientationUtils
{

	public static Vector3 QuaternionToYPR(Quaternion q)
	{
		// TODO: there is something wrong with yaw and roll, they correspond to rotation
		// around device axes not real-world axes. Needs fixing!
		float pitch = Mathf.Atan2(2*(q.w*q.x + q.y*q.z), 1-2*(q.x*q.x + q.y*q.y));
        float yaw = Mathf.Asin(2*(q.w*q.y - q.z*q.x));
        float roll = Mathf.Atan2(2*(q.w*q.z + q.x*q.y), 1-2*(q.y*q.y + q.z*q.z));
        return new Vector3(yaw, pitch, roll);
	}

}


