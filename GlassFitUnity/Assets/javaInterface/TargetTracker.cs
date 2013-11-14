using UnityEngine;
using System.Collections;
using System;

public class TargetTracker : MonoBehaviour {

	private AndroidJavaObject target;
	private double targetDistance = 0;
	// Use this for initialization
	public TargetTracker(AndroidJavaObject helper) {
		try {
			//UnityEngine.Debug.Log("TargetTracker: about to get target tracker");
			target = helper.Call<AndroidJavaObject>("getTargetTracker");
			UnityEngine.Debug.LogWarning("TargetTracker: unique target tracker obtained");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("TargetTracker: Helper.getTargetTracker() failed" + e.Message);
			UnityEngine.Debug.LogException(e);
		}
			
	}
	
	public double getTargetDistance() {
		return targetDistance;
	}
	
	public void setTargetDistance() {
		try {
			targetDistance = target.Call<double>("getCumulativeDistanceAtTime", Platform.Instance.Time());
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("TargetTracker: getCumulativeDistanceAtTime() failed: " + e.Message);
			UnityEngine.Debug.LogException(e);
		}
	}
	
	public float getDistanceBehindTarget() {
		return (float)(targetDistance - Platform.Instance.Distance());
	}
	
	public float getCurrentSpeed() {
		try {
			float ret = target.Call<float>("getCurrentSpeed", 0L);
			UnityEngine.Debug.Log("Target Tracker: speed obtained, currently: " + ret.ToString());
			return ret;
		} catch (Exception e) {
			UnityEngine.Debug.Log("Target Tracker: Error getting speed" + e.Message);
			return 0;
		}
	}
	
	public void setTargetSpeed(float s) {
		try {
			target.Call("setSpeed", s);
			Platform.Instance.setBasePointsSpeed(s);
		} catch (Exception e) {
			UnityEngine.Debug.Log("Target Tracker: Error setting speed" + e.Message);
		}
	}
	
	public void setTargetTrack(int trackID)
	{
		try {
			target.Call("setTrack", trackID);
			UnityEngine.Debug.LogWarning("TargetTracker: Track has been set to " + trackID.ToString ());
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("TargetTracker: setTargetTrack() failed: " + e.Message);
			UnityEngine.Debug.LogException(e);
		}
//		targetTrackers[0].setTargetTrack(trackID);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
