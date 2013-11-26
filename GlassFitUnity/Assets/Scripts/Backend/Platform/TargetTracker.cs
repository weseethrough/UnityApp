using UnityEngine;
using System.Collections;
using System;

public class TargetTracker : MonoBehaviour {

	private AndroidJavaObject target;
	private double targetDistance = 0;
	// Use this for initialization
	public TargetTracker(AndroidJavaObject helper) {
		try {
			target = helper.Call<AndroidJavaObject>("getTargetTracker");
			UnityEngine.Debug.LogWarning("TargetTracker: unique target tracker obtained");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("TargetTracker: Helper.getTargetTracker() failed" + e.Message);
			UnityEngine.Debug.LogException(e);
		}
			
	}
	
	public double GetTargetDistance() {
		return targetDistance;
	}
	
	public void SetTargetDistance() {
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
	
	public float GetCurrentSpeed() {
		try {
			float ret = target.Call<float>("getCurrentSpeed", 0L);
			UnityEngine.Debug.Log("Target Tracker: speed obtained, currently: " + ret.ToString());
			return ret;
		} catch (Exception e) {
			UnityEngine.Debug.Log("Target Tracker: Error getting speed" + e.Message);
			return 0;
		}
	}
	
	public void SetTargetSpeed(float s) {
		try {
			target.Call("setSpeed", s);
			Platform.Instance.SetBasePointsSpeed(s);
		} catch (Exception e) {
			UnityEngine.Debug.Log("Target Tracker: Error setting speed" + e.Message);
		}
	}
	
	public void SetTargetTrack(int trackID)
	{
		try {
			target.Call("setTrack", trackID);
			UnityEngine.Debug.LogWarning("TargetTracker: Track has been set to " + trackID.ToString ());
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("TargetTracker: SetTargetTrack() failed: " + e.Message);
			UnityEngine.Debug.LogException(e);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
