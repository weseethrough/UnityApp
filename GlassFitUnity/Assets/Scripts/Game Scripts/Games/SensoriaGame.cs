using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class SensoriaGame : MonoBehaviour {
	
	public GameObject runnerHolder;
	
	private float targSpeed = 2.2f;
	
	// Use this for initialization
	void Start () {
		Platform.Instance.SetIndoor(true);
		Platform.Instance.Reset();
		
		Platform.Instance.ResetTargets();
		
		TargetTracker tracker = Platform.Instance.CreateTargetTracker(targSpeed);
		
		TargetController controller = runnerHolder.GetComponent<TargetController>();
		controller.SetTracker(tracker);
		controller.SetLane(1);
		
		Platform.Instance.StartTrack();
	}
	
	// Update is called once per frame
	void Update () {
		//Update variables for GUI	
		Platform.Instance.Poll();
		
		DataVault.Set("calories", Platform.Instance.Calories().ToString() + "kcal");
		DataVault.Set("pace", SpeedToKmPace(Platform.Instance.Pace()).ToString("f1") + "min/km");
		DataVault.Set("distance", SiDistance(Platform.Instance.Distance()));
		DataVault.Set("time", TimestampMMSSdd( Platform.Instance.Time()));
		//DataVault.Set("indoor_text", indoorText);
		
		DataVault.Set("rawdistance", Platform.Instance.Distance());
		DataVault.Set("rawtime", Platform.Instance.Time());
		
		double targetDistance = Platform.Instance.GetHighestDistBehind();

		if (targetDistance > 0) {
			DataVault.Set("ahead_header", "Behind!");
			DataVault.Set("ahead_col_box", "D20000EE");
		} else {
			DataVault.Set("ahead_header", "Ahead!"); 
			DataVault.Set("ahead_col_box", "19D200EE");
		}
		DataVault.Set("ahead_box", SiDistance(Math.Abs(targetDistance)));
	}
	
	protected string SiDistance(double meters) {
		string postfix = "m";
		string final;
		float value = (float)meters;
		if (value > 1000) {
			value = value/1000;
			postfix = "km";
			final = value.ToString("f3");
		}
		else
		{
			final = value.ToString("f0");
		}
		return final+postfix;
	}
	
	protected long SpeedToKmPace(float speed) {
		if (speed <= 0) {
			return 0;
		}
		// m/s -> mins/Km
		return Convert.ToInt64( ((1/speed)/60) * 1000);
	}
	
	protected string TimestampMMSSdd(long milliseconds) {
		TimeSpan span = TimeSpan.FromMilliseconds(milliseconds);
		//if we're into hours, show them
		if(span.Hours > 0)
		{
			return string.Format("{0:0}:{1:00}:{2:00}:{3:00}", span.Hours, span.Minutes, span.Seconds, span.Milliseconds/10);
		}
		else
		{				
			return string.Format("{0:0}:{1:00}:{2:00}",span.Hours*60 + span.Minutes, span.Seconds, span.Milliseconds/10);
		}
			
	}
	
}
