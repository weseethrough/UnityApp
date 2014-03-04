using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// HUD controller.
/// Interface for showing/hiding HUD elements.
/// Responsible for 
/// </summary>
public class HUDController : MonoBehaviour {
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		//set distance etc on HUD
		DataVault.Set("calories", Platform.Instance.Calories().ToString()/* + "kcal"*/);
		float pace = UnitsHelper.SpeedToKmPace(Platform.Instance.Pace());
		string paceString = (pace > 20.0f || pace == 0.0f) ? "--:--" : UnitsHelper.TimestampMMSSnearestTenSecs(pace); // show dashes if slower than slow walk, otherwise round to nearest 10s
		DataVault.Set("pace", paceString/* + "min/km"*/);
		DataVault.Set("distance", UnitsHelper.SiDistanceUnitless(Platform.Instance.Distance(), "distance_units"));
		DataVault.Set("time", UnitsHelper.TimestampMMSSfromMillis(Platform.Instance.Time()));
		
		DataVault.Set("rawdistance", Platform.Instance.Distance());
		DataVault.Set("rawtime", Platform.Instance.Time());
        DataVault.Set("sweat_points", string.Format("{0:N0}", Platform.Instance.GetCurrentPoints()));

        TimeSpan span = TimeSpan.FromMilliseconds(Platform.Instance.Time());
						
        DataVault.Set("time_minutes_only", (int)(span.Minutes + span.Hours * 60));
        DataVault.Set("time_seconds_only", string.Format("{0:00}" ,span.Seconds));	
	}

		
}
