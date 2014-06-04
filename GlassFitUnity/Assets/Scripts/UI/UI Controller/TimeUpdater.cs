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

    void Start()
    {
        label = GetComponent<UILabel>();
        label.enabled = false;
    }

    void OnEnable()
    {
        label = GetComponent<UILabel>();
        label.enabled = false;
    }
	
	void Update() 
    {
        if (label.enabled == false)
        {
            label.enabled = true;
        }

		if(challengeTime != null && !label.text.Contains("Expired")) {
			DateTime currentTime = System.DateTime.Now;
			if(challengeTime.Year < 2013) {
                currentLabelString = label.text;
				challengeTime = DateTime.ParseExact(label.text, "O", CultureInfo.InvariantCulture);
			}
			TimeSpan newDifference = challengeTime - currentTime;
			string previousText = label.text;
            string currentText = string.Format("{0:00}d {1:00}h {2:00}m", newDifference.Days, newDifference.Hours, newDifference.Minutes);
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

    void OnDisable()
    {
        if (challengeTime != null && timeDifference.Minutes > 0)
        {
            label.text = "";
            challengeTime = DateTime.MinValue;
        }
    }

	void OnDestroy() {
		if(challengeTime != null && timeDifference.Minutes > 0) {
			label.text = challengeTime.ToString("O");
		}
	}
}
