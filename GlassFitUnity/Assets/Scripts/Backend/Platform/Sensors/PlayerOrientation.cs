using System;
using UnityEngine;

public class PlayerOrientation
{
	private Quaternion realWorldToPlayerRotation = Quaternion.identity;  // player's rotation from real-world co-ordinate system (north, east, up)
	private Quaternion initialRotationOffset = Quaternion.identity;  // player's rotation when reset() was last called
	private Quaternion playerOrientation = Quaternion.identity;  // player's rotation from when reset() was last called

	private float yaw = 0f;
	private float cumulativeYaw = 0f; // doesn't flip at +/-180 degrees
	private float pitch = 0f;
	private float roll = 0f;

	// Accessor methods
	public Quaternion AsQuaternion() { return playerOrientation; }

	public Quaternion AsRealWorldQuaternion() { return realWorldToPlayerRotation; }

	public float AsYaw() { return yaw; }

	public float AsCumulativeYaw() { return cumulativeYaw; }

	public float AsPitch() { return pitch; }

	public float AsRoll() { return roll; }

	// update the internal state
	// realWorldToPlayerRotation is as reported by the sensors
	public void Update(Quaternion realWorldToPlayerRotation)
	{

		
		// update player orientation
		this.realWorldToPlayerRotation = realWorldToPlayerRotation;
		playerOrientation = Quaternion.Inverse(initialRotationOffset) * realWorldToPlayerRotation;

		// update yaw, pitch and roll
		Vector3 YPR = OrientationUtils.QuaternionToYPR(playerOrientation);
		if (Math.Abs(YPR[0]-yaw) > 6) {
			// if we've gone past +/- pi radians (6 is just less than 2*pi radians)
			// need to add/subtract 2*pi
			this.cumulativeYaw += (YPR[0]-yaw) + (float)(Math.Sign(yaw)*2*Math.PI);
		} else {
			this.cumulativeYaw += (YPR[0]-yaw);
		}
		this.yaw = YPR[0];
		this.pitch = YPR[1];
		this.roll = YPR[2];
	}

	// reset the player orientation to current deveice orientation
	// future calls to the accessor methods will report offset from this position
	// currently locked to the horizontal plane
	public void Reset()
	{
		Vector3 realWorldYPR = OrientationUtils.QuaternionToYPR(realWorldToPlayerRotation); // returns yaw and pitch the wrong way round
		Vector3 YPRoffset = new Vector3(realWorldYPR.z, -90.0f*Mathf.Deg2Rad, 0.0f);
		this.initialRotationOffset = OrientationUtils.YPRToQuaternion(YPRoffset); // feed above in reverese order to compensate

		Update (this.initialRotationOffset);
		cumulativeYaw = 0f;

		UnityEngine.Debug.Log("PlayerOrientation reset");

	}



}

