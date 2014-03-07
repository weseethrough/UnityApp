using System;
using UnityEngine;

public class PlayerOrientation
{
	private const float AUTO_RESET_THRESHOLD = 20.0f*Mathf.Deg2Rad; //  20 degrees+ from forward to the side will trigger auto-reset
	private const float AUTO_RESET_HUD_PITCH = 11.5f*Mathf.Deg2Rad; // degrees up to HUD from horizontal
	private const float AUTO_RESET_TIME_DELAY = 1.5f;  // seconds before auto-reset
	private const float AUTO_RESET_LERP_RATE = 1.0F/0.4f;  // 1/seconds animation duration
	private const float CORNERING_TOLERANCE = 20.0f;  // when outdoor, if GPS bearing and magnetic bearing are within this tolerance (degrees), the user must be facing forward

	private Boolean autoResetEnabled = true;

	private Quaternion northReference = new Quaternion(Mathf.Sqrt(0.5f), 0, 0, -Mathf.Sqrt(0.5f));  // rotation from real-world to facing north/horizontal
	private Quaternion forwardReference = new Quaternion(Mathf.Sqrt(0.5f), 0, 0, -Mathf.Sqrt(0.5f));  // rotation from real-world to facing forward (init north) /horizontal. Updated when player goes round a corner.
	private float pitchOffset = 0.0f; // degrees above/below pitch angle of glass unit to put game contents. e.g. -10 means everything will be rendered 10 degrees lower than normal.

	private Quaternion rotationFromDown = Quaternion.identity;  // rotation from real-world co-ordinate system (looking straight down, facing north) to player, as reported by sensors every frame
	private Quaternion rotationFromNorth = Quaternion.identity;  // rotation from northReference to player, calculated in this class every frame
	private Quaternion rotationFromForward = Quaternion.identity;  // rotation from forwardReference to player, calculated in this class every frame

	private float yawFromNorth = 0f;
	private float yawFromForward = 0f;
	private float cumulativeYaw = 0f; // from forward, doesn't flip at +/-180 degrees
	private float pitch = 0f;
	private float roll = 0f;

	// Accessor methods
	public Quaternion AsQuaternion() { return rotationFromForward; }

	public Quaternion AsQuaternionFromNorth() { return rotationFromNorth; }

	public Quaternion AsRealWorldQuaternion() { return rotationFromDown; }

	public float AsYaw() { return yawFromForward; }

	public float AsYawFromNorth() { return yawFromNorth; }

	public float AsCumulativeYaw() { return cumulativeYaw; }

	public float AsPitch() { return pitch; }

	public float AsRoll() { return roll; }

	// update the internal state
	// rotationFromDown is as reported by the sensors
	public void Update(Quaternion realWorldToPlayerRotation)
	{

		// update player orientation
		this.rotationFromDown = realWorldToPlayerRotation;
		updatePlayerOrientation();

		// reset the gyros if player has been looking to the side for a while
		if (autoResetEnabled) AutoReset();

	}
	
	public void SetAutoReset(Boolean res)
	{
		autoResetEnabled = res;
	}
	
	// reset the player orientation to current device orientation
	// future calls to the accessor methods will report offset from this position
	// currently locked to the horizontal plane
	public void Reset()
	{
		this.forwardReference = northReference*Quaternion.Euler (0, AsYawFromNorth()*Mathf.Rad2Deg, 0);

		updatePlayerOrientation();  // we've changed the initial offset, so need to update this too
		cumulativeYaw = 0f;

		UnityEngine.Debug.Log("PlayerOrientation reset");

	}

	// reset the player orientation to current device orientation
	// future calls to the accessor methods will report offset from this position
	// currently locked to the horizontal plane
	public void SetPitchOffset(float pitchDegrees)
	{
//		this.pitchOffset = pitchDegrees;
//
//		Vector3 currentYPR = OrientationUtils.QuaternionToYPR(forwardReference);  // returns yaw and pitch the wrong way round
//		Vector3 newYPR = new Vector3(currentYPR.z, (-90.0f-this.pitchOffset)*Mathf.Deg2Rad, 0.0f); // same yaw, new pitch, zero roll
//		this.forwardReference = OrientationUtils.YPRToQuaternion(newYPR);
//
//		updatePlayerOrientation();  // we've changed the initial offset, so need to update this too

		UnityEngine.Debug.Log("PlayerOrientation pitch offset updated");

	}


	// update all relative values based on initial offset and rotationFromDown
	private void updatePlayerOrientation()
	{
		rotationFromForward = Quaternion.Inverse(forwardReference) * rotationFromDown;
		rotationFromNorth = Quaternion.Inverse(northReference) * rotationFromDown;

		// update yaw, pitch and roll from forward
		Vector3 YPR = OrientationUtils.QuaternionToYPR(rotationFromForward);
		if (Math.Abs(YPR[0]-yawFromForward) > 6) {
			// if we've gone past +/- pi radians (6 is just less than 2*pi radians)
			// need to add/subtract 2*pi
			this.cumulativeYaw += (YPR[0]-AsYaw()) + (float)(Math.Sign(AsYaw())*2*Math.PI);
		} else {
			this.cumulativeYaw += (YPR[0]-AsYaw());
		}
		this.yawFromForward = YPR[0];
		this.pitch = YPR[1];
		this.roll = YPR[2];

		// update yaw from north
		YPR = OrientationUtils.QuaternionToYPR(rotationFromNorth);
		this.yawFromNorth = YPR[0];
	}
	
	private float bearingDiffDegrees(float bearing1, float bearing2) {
    	float diff = bearing1 - bearing2;
    	diff  += (diff>180) ? -360 : (diff<-180) ? 360 : 0;
    	return Mathf.Abs(diff);
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
				autoResetYaw = autoResetYaw.HasValue ? 0.3f*autoResetYaw.Value + 0.7f*AsYawFromNorth() : AsYawFromNorth();
				autoResetTo = northReference*Quaternion.Euler(0, autoResetYaw.Value*Mathf.Rad2Deg, 0);
				UnityEngine.Debug.Log("AutoResetting from " + (int)(OrientationUtils.QuaternionToYPR(autoResetFrom)[0]*Mathf.Rad2Deg) + " to " + (int)(autoResetYaw.Value*Mathf.Rad2Deg));

				// lerp the world round to new bearing
				forwardReference = Quaternion.Lerp(autoResetFrom, autoResetTo, (1-Mathf.Cos (autoResetLerpTime*Mathf.PI))/2);
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
		else if (Mathf.Abs(AsYaw()) > AUTO_RESET_THRESHOLD  // facing more than AUTO_RESET_THRESHOLD away from current forward reference
			 && Mathf.Abs(AsPitch()) < AUTO_RESET_HUD_PITCH) // not looking too far up or down
		{
			// we've passed the auto-reset threshold:
			// increment timer and calculate average yaw since we passed the threshold
			autoResetTimer += Time.deltaTime;
			autoResetYaw = autoResetYaw.HasValue ? 0.7f*autoResetYaw.Value + 0.3f*AsYawFromNorth() : AsYawFromNorth();

			if (autoResetTimer > AUTO_RESET_TIME_DELAY
				&& (!Platform.Instance.LocalPlayerPosition.IsTracking // not tracking, i.e. paused or on a menu screen
			     || Platform.Instance.LocalPlayerPosition.Pace < 1.0f  // going slowly/stopped
			     || bearingDiffDegrees(Platform.Instance.LocalPlayerPosition.Bearing, Platform.Instance.Yaw()) < CORNERING_TOLERANCE))  // facing with 20 degrees of GPS-based forward movement, i.e. looking where they are going. Note GpsBearing is not valid (-999.0) if the user is stationary
			{
				// save current offset and start rotating
				autoResetFrom = forwardReference;
				resetting = true;

				// clean up
				autoResetTimer = 0;
			}

		}
		else
		{
			//UnityEngine.Debug.Log("AutoReset: Pitch: " + pitch + ", Roll: " + roll + ", Yaw: " + Platform.Instance.Yaw() + ", GPSbearing: " + Platform.Instance.LocalPlayerPosition.Bearing);
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

