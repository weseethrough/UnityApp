using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;
using System.Diagnostics;

public class PlatformDummy {
	
	private Stopwatch timer = new Stopwatch();
	private System.Random random = new System.Random();
	private long update = 0;
	private int distance = 0;
	private int target = 1;
	private Position position = null;
	private float bearing = 0.3f;
	
	public void Start(Boolean indoor) {
		timer.Start();
	}
	
	public Boolean hasLock() {
		return true;
	}
	
	public void Poll() {
		if (!timer.IsRunning) return;
		if (Time() - update > 1000) { 
			distance += 4;			
			target += 4;
			if (random.Next() % 5 == 0) target += 1;
			if (random.Next() % 5 == 5) target -= 1;
			update = Time();
		}
		if (Time () > 1000) {
			position = new Position((float)(51.400+Math.Cos(bearing)*distance/111229d), (float)(-0.15+Math.Cos(bearing)*distance/111229d));
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
		return 1.0f;
	}
	
	public Position Position() {
		return position;
	}	
	
	public float Bearing() {
		return bearing;
	}
	
	public string DebugLog() {
		return "On editor, really long string to test word wrap. Bla bla blaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa bla.";
	}	
}
