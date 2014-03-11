using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class TargetTracker : System.Object {
	
	public string name { get; set; }
	public Dictionary<string, object> metadata { get; protected set; }
	protected AndroidJavaObject target;
	private double targetDistance = 0;
	
	public TargetTracker() {}
	
	private TargetTracker(AndroidJavaObject target) {
		this.target = target;
		this.metadata = new Dictionary<string, object>();
	}

	public static TargetTracker Build(AndroidJavaObject helper, int deviceId, int trackId) {
		try {
			AndroidJavaObject ajo = helper.Call<AndroidJavaObject>("getTrackTargetTracker", deviceId, trackId);
			if (ajo.GetRawObject().ToInt32() == 0) return null;
			UnityEngine.Debug.LogWarning("TargetTracker: track target tracker obtained");
			return new TargetTracker(ajo);
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("TargetTracker: Helper.getTrackTargetTracker() failed" + e.Message);
			UnityEngine.Debug.LogException(e);
			return null;
		}			
	}
	
	public double GetTargetDistance() {
		return targetDistance;
	}
	
	public void PollTargetDistance() {
		try {
			targetDistance = target.Call<double>("getCumulativeDistanceAtTime", Platform.Instance.LocalPlayerPosition.Time);
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("TargetTracker: getCumulativeDistanceAtTime() failed: " + e.Message);
			UnityEngine.Debug.LogException(e);
		}
	}
	
	public float GetDistanceBehindTarget() {
		return (float)(targetDistance - Platform.Instance.LocalPlayerPosition.Distance);
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
		return "TargetTracker " + name;
	}
}
