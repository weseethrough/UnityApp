using System;
using UnityEngine;

public class PlayerOrientation
{
	private const Boolean AUTO_RESET_ENABLED = false;
	private const float AUTO_RESET_THRESHOLD = 0.270f; //  0.233 radians from straight ahead to the side to trigger auto-reset
	private const float AUTO_RESET_HUD_THRESHOLD = 10.0f; // radians to side when looking up at HUD height. Anything over Mathf.PI will disable reset.
	private const float AUTO_RESET_HUD_PITCH = 0.20f; // radians up to HUD from horizontal
	private const float AUTO_RESET_TIME_DELAY = 1.5f;  // seconds before auto-reset
	private const float AUTO_RESET_LERP_RATE = 1.0F/0.4f;  // 1/seconds animation duration

	private Quaternion realWorldToPlayerRotation = Quaternion.identity;  // player's rotation from real-world co-ordinate system (north, east, up)
	private Quaternion initialRotationOffset = Quaternion.identity;  // player's rotation when reset() was last called
	private float pitchOffset = 0.0f; // degrees above/below pitch angle of glass unit to put game contents. e.g. -10 means everything will be rendered 10 degrees lower than normal.
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
		updatePlayerOrientation();

		// reset the gyros if player has been looking to the side for a while
		if (AUTO_RESET_ENABLED) AutoReset();

	}

	// reset the player orientation to current device orientation
	// future calls to the accessor methods will report offset from this position
	// currently locked to the horizontal plane
	public void Reset()
	{
		Vector3 realWorldYPR = OrientationUtils.QuaternionToYPR(realWorldToPlayerRotation); // returns yaw and pitch the wrong way round
		Vector3 YPRoffset = new Vector3(realWorldYPR.z, (-90.0f-this.pitchOffset)*Mathf.Deg2Rad, 0.0f);
		this.initialRotationOffset = OrientationUtils.YPRToQuaternion(YPRoffset); // feed above in reverese order to compensate

		updatePlayerOrientation();  // we've changed the initial offset, so need to update this too
		cumulativeYaw = 0f;

		UnityEngine.Debug.Log("PlayerOrientation reset");

	}

	// reset the player orientation to current device orientation
	// future calls to the accessor methods will report offset from this position
	// currently locked to the horizontal plane
	public void SetPitchOffset(float pitchDegrees)
	{
		this.pitchOffset = pitchDegrees;

		Vector3 currentYPR = OrientationUtils.QuaternionToYPR(initialRotationOffset);  // returns yaw and pitch the wrong way round
		Vector3 newYPR = new Vector3(currentYPR.z, (-90.0f-this.pitchOffset)*Mathf.Deg2Rad, 0.0f); // same yaw, new pitch, zero roll
		this.initialRotationOffset = OrientationUtils.YPRToQuaternion(newYPR);

		updatePlayerOrientation();  // we've changed the initial offset, so need to update this too

		UnityEngine.Debug.Log("PlayerOrientation pitch offset updated");

	}


	// update all relative values based on initial offset and realWorldToPlayerRotation
	private void updatePlayerOrientation()
	{
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

	private float autoResetTimer = 0;
	private float autoResetLerpTime = 0;
	private float? autoResetYaw = null;
	private Boolean resetting = false;
	private Quaternion autoResetFrom = Quaternion.identity;
	private Quaternion autoResetTo = Quaternion.identity;

	// TODO: fix yaw/roll confusion. Think yaw when it says roll. Just a different starting point.
	private void AutoReset()
	{
		if (resetting)
		{
			// increment timer each loop
			autoResetLerpTime += Time.deltaTime * AUTO_RESET_LERP_RATE;

			if (autoResetLerpTime < 1.0f)
			{
				// keep updating new yaw
				float realWorldYaw = OrientationUtils.QuaternionToYPR(realWorldToPlayerRotation)[2];
				autoResetYaw = autoResetYaw.HasValue ? 0.3f*autoResetYaw.Value + 0.7f*realWorldYaw : realWorldYaw;
				Vector3 newOffset = new Vector3(autoResetYaw.Value, (-90.0f-this.pitchOffset)*Mathf.Deg2Rad, 0.0f);
				autoResetTo = OrientationUtils.YPRToQuaternion(newOffset);

				// lerp the world round to new bearing
				initialRotationOffset = Quaternion.Lerp(autoResetFrom, autoResetTo, (1-Mathf.Cos (autoResetLerpTime*Mathf.PI))/2);
				updatePlayerOrientation(); // we changed the initial rotation so must recalc player orientation values
			}
			else
			{
				// finished, clean up
				resetting = false;
				autoResetLerpTime = 0;
				autoResetYaw = null;
			}
		}
		else if (Mathf.Abs(pitch) < AUTO_RESET_HUD_PITCH && Mathf.Abs(cumulativeYaw) > AUTO_RESET_THRESHOLD
			|| Mathf.Abs(pitch) >= AUTO_RESET_HUD_PITCH && Mathf.Abs(cumulativeYaw) > AUTO_RESET_HUD_THRESHOLD)
		{
			// we've passed the auto-reset threshold:
			// increment timer and calculate average yaw since we passed the threshold
			autoResetTimer += Time.deltaTime;
			float realWorldYaw = OrientationUtils.QuaternionToYPR(realWorldToPlayerRotation)[2];
			autoResetYaw = autoResetYaw.HasValue ? 0.7f*autoResetYaw.Value + 0.3f*realWorldYaw : realWorldYaw;
			//UnityEngine.Debug.Log("AutoReset: Pitch: " + pitch + ", Roll: " + roll + ", Yaw: " + cumulativeYaw);
			//UnityEngine.Debug.Log("AutoReset: Smoothed Yaw: " + autoResetYaw.Value + "rad");

			if (autoResetTimer > AUTO_RESET_TIME_DELAY)
			{
				// save current offset and start rotating
				autoResetFrom = initialRotationOffset;
				resetting = true;

				// clean up
				autoResetTimer = 0;
			}

		}
		else
		{
			//UnityEngine.Debug.Log("AutoReset: Pitch: " + -pitch + ", Roll: " + roll + ", Yaw: " + cumulativeYaw);
			// back inside thresholds
			// reset timer and yaw
			autoResetTimer = 0;
			autoResetYaw = null;
			resetting = false;
		}
	}
	
	public float GetPitchOffset()
	{
		return pitchOffset;
	}


}

