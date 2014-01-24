using System;
using UnityEngine;

public static class OrientationUtils
{

	public static Vector3 QuaternionToYPR(Quaternion q)
	{
		// TODO: there is something wrong with yaw and roll, they correspond to rotation
		// around device axes not real-world axes. Needs fixing!
		float pitch = Mathf.Atan2(2*(q.w*q.x + q.y*q.z), 1-2*(q.x*q.x + q.y*q.y));
        float yaw = Mathf.Asin(2*(q.w*q.y - q.z*q.x));  //actually roll
        float roll = Mathf.Atan2(2*(q.w*q.z + q.x*q.y), 1-2*(q.y*q.y + q.z*q.z)); // actually yaw
        return new Vector3(yaw, pitch, roll);
	}

	public static Quaternion YPRToQuaternion(Vector3 YawPitchRoll)
	{
		float y2 = YawPitchRoll.x/2.0f;
        float p2 = YawPitchRoll.y/2.0f;
        float r2 = YawPitchRoll.z/2.0f;
        
        // code taken from wikipedia
        // http://en.wikipedia.org/wiki/Conversion_between_quaternions_and_Euler_angles#cite_note-nasa-rotation-1
        float w = (float)(Mathf.Cos(p2)*Mathf.Cos(r2)*Mathf.Cos(y2) + Mathf.Sin(p2)*Mathf.Sin(r2)*Mathf.Sin(y2));
        float x = (float)(Mathf.Sin(p2)*Mathf.Cos(r2)*Mathf.Cos(y2) - Mathf.Cos(p2)*Mathf.Sin(r2)*Mathf.Sin(y2));
        float y = (float)(Mathf.Cos(p2)*Mathf.Sin(r2)*Mathf.Cos(y2) + Mathf.Sin(p2)*Mathf.Cos(r2)*Mathf.Sin(y2));
        float z = (float)(Mathf.Cos(p2)*Mathf.Cos(r2)*Mathf.Sin(y2) - Mathf.Sin(p2)*Mathf.Sin(r2)*Mathf.Cos(y2));
        return new Quaternion(x,y,z,w);
	}

}


