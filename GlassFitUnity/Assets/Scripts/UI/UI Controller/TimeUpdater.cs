using UnityEngine;
using System.Collections;
using System;
using System.Globalization;

public class TimeUpdater : MonoBehaviour {

	UILabel label;
	bool initialValueSet = false;
	string currentLabelString = string.Empty;
	DateTime challengeTime;
	TimeSpan timeDifference;

	// Use this for initialization
	void Start () {
		label = GetComponent<UILabel>();
		if(label != null) {
			currentLabelString = label.text;
			if(!string.IsNullOrEmpty(currentLabelString) && !currentLabelString.Contains("Expired")) {
				challengeTime = DateTime.ParseExact(currentLabelString, "O", CultureInfo.InvariantCulture);
				DateTime currentTime = DateTime.Now;
				timeDifference = challengeTime - currentTime;
				if(timeDifference != null && timeDifference.Minutes > 0) {
					label.text = string.Format("{0:00}:{1:00}:{2:00}", timeDifference.Days, timeDifference.Hours, timeDifference.Minutes);
				} else 
				{
					label.text = "Expired";
				}
			}
		}
	}
	
	void Update() {
		if(challengeTime != null && timeDifference.Minutes > 0) {
			DateTime currentTime = System.DateTime.Now;
			TimeSpan newDifference = challengeTime - currentTime;
			if(newDifference.Minutes != timeDifference.Minutes )
			{ 
				timeDifference = newDifference;
				if(timeDifference.Minutes > 0) {
					label.text = string.Format("{0:00}:{1:00}:{2:00}", timeDifference.Days, timeDifference.Hours, timeDifference.Minutes);
				} else {
					label.text = "Expired";
				}

			} 
		}
	}
}
