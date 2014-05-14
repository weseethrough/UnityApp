using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// HUD controller.
/// Interface for showing/hiding HUD elements.
/// Responsible for 
/// </summary>
public class HUDController : MonoBehaviour {

	int goal_dist = 0;
	// Use this for initialization
	void Start () {
		goal_dist = GameBase.getTargetDistance();
		string distKm = string.Format("{0:0}", goal_dist/1000);
		DataVault.Set("goal_dist", distKm);
	}
	
	// Update is called once per frame
	void Update () {
		//set distance etc on HUD
		DataVault.Set("calories", Platform.Instance.LocalPlayerPosition.Calories.ToString()/* + "kcal"*/);
		float pace = UnitsHelper.SpeedToKmPace(Platform.Instance.LocalPlayerPosition.Pace);
		string paceString = (pace > 20.0f || pace == 0.0f) ? "--:--" : UnitsHelper.TimestampMMSSnearestTenSecs(pace); // show dashes if slower than slow walk, otherwise round to nearest 10s
		DataVault.Set("pace", paceString/* + "min/km"*/);
		DataVault.Set("distance", UnitsHelper.SiDistanceUnitless(Platform.Instance.LocalPlayerPosition.Distance, "distance_units"));
		DataVault.Set("time", UnitsHelper.TimestampMMSSfromMillis(Platform.Instance.LocalPlayerPosition.Time));
		
		DataVault.Set("rawdistance", Platform.Instance.LocalPlayerPosition.Distance);
		DataVault.Set("rawtime", Platform.Instance.LocalPlayerPosition.Time);
        DataVault.Set("sweat_points", string.Format("{0:N0}", Platform.Instance.PlayerPoints.CurrentActivityPoints));

        TimeSpan span = TimeSpan.FromMilliseconds(Platform.Instance.LocalPlayerPosition.Time);
						
        DataVault.Set("time_minutes_only", (int)(span.Minutes + span.Hours * 60));
        DataVault.Set("time_seconds_only", string.Format("{0:00}" ,span.Seconds));

		string distKm = string.Format("{0:0}", goal_dist/1000);
		DataVault.Set("goal_dist", distKm);
	}

		
}
