using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public class FauxTargetTracker : TargetTracker {
	
	public FauxTargetTracker() {}
	
	public FauxTargetTracker(AndroidJavaObject target) {
		this.target = target;
		this.metadata = new Dictionary<string, object>();
	}
	
	public void SetTargetSpeed(float speed) {
		try {
			this.target.Call("setSpeed", speed);
		} catch {
			UnityEngine.Debug.Log("Faux Target Tracker: Speed has been set");
		}
	}
}
