using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;
using System.Diagnostics;

public class PlatformDummy {
	
	private Stopwatch timer;
	private System.Random random = new System.Random();
	private long update = 0;
	private int distance = 0;
	private int target = 1;
	
	public void Start() {
		timer = new Stopwatch();
		timer.Start();
	}
	
	public void Simulate() {
		if (Time() - update > 1000) { 
			distance += 2;			
			target += 2;
			if (random.Next() % 5 == 0) target += 1;
			if (random.Next() % 5 == 5) target -= 1;
			update = Time();
		}
	}
	
	public long DistanceBehindTarget() {
		return target;
	}
	
	public long Time() {
		return timer.ElapsedMilliseconds;
	}
	
	public long Distance() {
		return distance;
	}
	
	public int Calories() {
		return 36;
	}
	
	public float Pace() {
		return 3f*60+24;
	}
	
	public string DebugLog() {
		return "On editor, really long string to test word wrap. Bla bla blaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa bla.";
	}	
}
