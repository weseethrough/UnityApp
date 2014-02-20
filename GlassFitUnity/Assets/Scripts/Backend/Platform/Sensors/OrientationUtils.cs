using System;
using UnityEngine;

public static class OrientationUtils
{

	public static Vector3 QuaternionToYPRold(Quaternion q)
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

	public static Vector3 QuaternionToYPR(Quaternion q) {

		//Debug.Log("Start quaternion (x,y,z,w): (" + q.x + ", " + q.y + ", " + q.z + ", " +  q.w + ")");

		// extract yaw (around y axis) first
		Quaternion yaw = new Quaternion(0, Mathf.Sin(q.eulerAngles.y*Mathf.Deg2Rad*0.5f), 0, Mathf.Cos(q.eulerAngles.y*Mathf.Deg2Rad*0.5f));
		//Debug.Log("Yaw quaternion (x,y,z,w): (" + yaw.x + ", " + yaw.y + ", " + yaw.z + ", " +  yaw.w + ")");

		// extract pitch next. Create a rotation with just the pitch and roll components, then
		// rearrange so rotation around x comes out first
		Quaternion q3 = Quaternion.Inverse(yaw)*q; // apply reverse yaw in the world co-ordinate system, then the rotation
		//Debug.Log("Q3 quaternion (x,y,z,w): (" + q3.x + ", " + q3.y + ", " + q3.z + ", " +  q3.w + ")");
		Quaternion pitch = new Quaternion(Mathf.Sin(q3.eulerAngles.x*Mathf.Deg2Rad*0.5f), 0, 0, Mathf.Cos(q3.eulerAngles.x*Mathf.Deg2Rad*0.5f));
		//Debug.Log("Pitch quaternion (x,y,z,w): (" + pitch.x + ", " + pitch.y + ", " + pitch.z + ", " +  pitch.w + ")");

		// finally extract roll
		Quaternion q5 = Quaternion.Inverse(pitch)*q3; //should only have a z-component remaining, so doesn't matter what order we extract in
		Quaternion roll = new Quaternion(0, 0, Mathf.Sin(q5.eulerAngles.z*Mathf.Deg2Rad*0.5f), Mathf.Cos(q5.eulerAngles.z*Mathf.Deg2Rad*0.5f));
		//Debug.Log("Roll quaternion (x,y,z,w): (" + roll.x + ", " + roll.y + ", " + roll.z + ", " +  roll.w + ")");

		// return result between +-180 in radians
		return new Vector3(
			yaw.eulerAngles.y < 180 ? yaw.eulerAngles.y : yaw.eulerAngles.y - 360,
			pitch.eulerAngles.x < 180 ? pitch.eulerAngles.x : pitch.eulerAngles.x - 360,
			roll.eulerAngles.z < 180 ? roll.eulerAngles.z : roll.eulerAngles.z - 360
			)*Mathf.Deg2Rad;

	}

}


