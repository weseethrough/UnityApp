using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public class StreamedTargetTracker : TargetTracker {
	private Stopwatch timer = new Stopwatch();
	
	public StreamedTargetTracker() {}
	
	public StreamedTargetTracker(AndroidJavaObject target) : this(target, false) {
	}
	
	public StreamedTargetTracker(AndroidJavaObject target, bool realtime) {
		this.target = target;
		this.metadata = new Dictionary<string, object>();
		if (realtime) timer.Start();
	}
	
	public override void PollTargetDistance() {
		long time = timer.ElapsedMilliseconds;
		if (!timer.IsRunning) time = Platform.Instance.Time();
		try {
			targetDistance = target.Call<double>("getCumulativeDistanceAtTime", time);
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("TargetTracker: getCumulativeDistanceAtTime() failed: " + e.Message);
			UnityEngine.Debug.LogException(e);
		}
	}
	
}
