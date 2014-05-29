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
			if(!string.IsNullOrEmpty(currentLabelString)) {
				challengeTime = DateTime.ParseExact(currentLabelString, "O", CultureInfo.InvariantCulture);
				DateTime currentTime = DateTime.Now;
				timeDifference = challengeTime - currentTime;
				string previousText = label.text;
				string currentText = string.Format("{0:00}:{1:00}:{2:00}", timeDifference.Days, timeDifference.Hours, timeDifference.Minutes);
				if(!previousText.Equals(currentText))
				{ 
//					timeDifference = newDifference;
					
					if(timeDifference.TotalMinutes > 0) {
						label.text = currentText;
					} else {
						label.text = "Expired";
					}
					
				} 
			} else {
				label.text = "Expired";
			}
		}
	}

//	void OnEnable() {
//		label = GetComponent<UILabel>();
//		if(label != null) {
//			currentLabelString = label.text;
//			if(!string.IsNullOrEmpty(currentLabelString) && !currentLabelString.Contains("Expired")) {
//
//				challengeTime = DateTime.ParseExact(currentLabelString, "O", CultureInfo.CurrentCulture);
//				DateTime currentTime = DateTime.Now;
//				timeDifference = challengeTime - currentTime;
//				string previousText = label.text;
//				string currentText = string.Format("{0:00}:{1:00}:{2:00}", timeDifference.Days, timeDifference.Hours, timeDifference.Minutes);
//				if(!previousText.Equals(currentText))
//				{ 
////					timeDifference = newDifference;
//					
//					if(timeDifference.TotalMinutes > 0) {
//						label.text = currentText;
//					} else {
//						label.text = "Expired";
//					}
//					
//				} 
//			}
//		}
//	}
	
	void Update() {
		if(challengeTime != null && !label.text.Contains("Expired")) {
			DateTime currentTime = System.DateTime.Now;
			if(challengeTime.Year < 2013) {
				challengeTime = DateTime.ParseExact(label.text, "O", CultureInfo.InvariantCulture);
			}
			TimeSpan newDifference = challengeTime - currentTime;
			string previousText = label.text;
			string currentText = string.Format("{0:00}:{1:00}:{2:00}",newDifference.Days, newDifference.Hours, newDifference.Minutes);
			if(!previousText.Equals(currentText))
			{ 
				timeDifference = newDifference;

				if(timeDifference.TotalMinutes > 0) {
					label.text = currentText;
				} else {
					label.text = "Expired";
				}

			} 
		}
	}

	void OnDestroy() {
		if(challengeTime != null && timeDifference.Minutes > 0) {
			label.text = challengeTime.ToString("O");
		}
	}
}
