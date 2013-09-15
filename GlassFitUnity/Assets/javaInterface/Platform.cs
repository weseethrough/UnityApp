using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;

public class Platform {
	private AndroidJavaClass jc;
	
	public void Start() {
		jc = new AndroidJavaClass("com.glassfitgames.glassfitplatform.gpstracker.HelperDummy"); 	
	}
	
	public int DistanceBehindTarget() {
		return jc.CallStatic<int>("DistanceBehindTarget");
	}
	
	public long Time() {
		return jc.CallStatic<long>("Time");
	}
	
	public int Distance() {
		return jc.CallStatic<int>("Distance");
	}
	
	public int Calories() {
		return jc.CallStatic<int>("Calories");
	}
	
	public int Pace() {
		return jc.CallStatic<int>("Pace");
	}
	
	public string DebugLog() {
		return jc.CallStatic<string>("DebugLog");
	}
}