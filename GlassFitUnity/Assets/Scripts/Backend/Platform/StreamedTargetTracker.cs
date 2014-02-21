using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public class StreamedTargetTracker : TargetTracker {
	private Stopwatch timer = new Stopwatch();
	private long timerOffset = 0;
	
	public StreamedTargetTracker() {}
	
	public StreamedTargetTracker(AndroidJavaObject target) : this(target, false) {
	}
	
	public StreamedTargetTracker(AndroidJavaObject target, bool realtime) {
		this.target = target;
		this.metadata = new Dictionary<string, object>();
		if (realtime) {
			SyncToRealtime();
		}
	}
	
	public override void PollTargetDistance() {
		long time = timer.ElapsedMilliseconds + timerOffset;
		if (!timer.IsRunning) time = Platform.Instance.Time();
		try {
			targetDistance = target.Call<double>("getCumulativeDistanceAtTime", time);
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("StreamedTargetTracker: getCumulativeDistanceAtTime() failed: " + e.Message);
			UnityEngine.Debug.LogException(e);
		}
	}

	public void SyncToRealtime() {
		try {
			timer.Reset();
			timer.Start();
			timerOffset = target.Call<long>("getRealtimeMillis");
		} catch (Exception e) {
			UnityEngine.Debug.LogWarning("StreamedTargetTracker: getRealtimeMillis() failed: " + e.Message);
			throw e;
		}
	}
	
}
