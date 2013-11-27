using UnityEngine;
using System.Collections;
using System;

public class TargetTracker : System.Object {

	private AndroidJavaObject target;
	private double targetDistance = 0;
	
	public TargetTracker(AndroidJavaObject helper, float constantSpeed) {
		try {
			target = helper.Call<AndroidJavaObject>("getFauxTargetTracker", constantSpeed);
			UnityEngine.Debug.LogWarning("TargetTracker: faux target tracker obtained");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("TargetTracker: Helper.getFauxTargetTracker() failed" + e.Message);
			UnityEngine.Debug.LogException(e);
		}			
	}
	public TargetTracker(AndroidJavaObject helper, int deviceId, int trackId) {
		try {
			target = helper.Call<AndroidJavaObject>("getTrackTargetTracker", deviceId, trackId);
			UnityEngine.Debug.LogWarning("TargetTracker: track target tracker obtained");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("TargetTracker: Helper.getTrackTargetTracker() failed" + e.Message);
			UnityEngine.Debug.LogException(e);
		}			
	}
	
	public double GetTargetDistance() {
		return targetDistance;
	}
	
	public void PollTargetDistance() {
		try {
			targetDistance = target.Call<double>("getCumulativeDistanceAtTime", Platform.Instance.Time());
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("TargetTracker: getCumulativeDistanceAtTime() failed: " + e.Message);
			UnityEngine.Debug.LogException(e);
		}
	}
	
	public float GetDistanceBehindTarget() {
		return (float)(targetDistance - Platform.Instance.Distance());
	}
	
	public float PollCurrentSpeed() {
		try {
			float ret = target.Call<float>("getCurrentSpeed", 0L);
			return ret;
		} catch (Exception e) {
			UnityEngine.Debug.Log("Target Tracker: Error getting speed" + e.Message);
			return 0;
		}
	}
			
	public string ToString() {
		return "TargetTracker";
	}
}
