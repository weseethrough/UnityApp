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

	private Color defaultColor = Color.black;

    void Start()
    {
        label = GetComponent<UILabel>();
        label.enabled = false;
		defaultColor = label.color;
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

		if(challengeTime != null && !label.text.Contains("Expired") && !label.text.Contains("<db_")) {
			DateTime currentTime = System.DateTime.Now;
			if(challengeTime.Year < 2013) {
                currentLabelString = label.text;
				challengeTime = DateTime.ParseExact(label.text, "O", CultureInfo.InvariantCulture);
			}
			TimeSpan newDifference = challengeTime - currentTime;
			string previousText = label.text;
			string currentText;
			if(newDifference.Days > 0)
            	currentText = string.Format("{0:0}d {1:0}h", newDifference.Days, newDifference.Hours, newDifference.Minutes);
			else {
				currentText = string.Format("{0:0}h {1:0}m", newDifference.Hours, newDifference.Minutes);
			}

			if(!previousText.Equals(currentText))
			{ 
				if(newDifference.Days > 0)
				{
					label.color = defaultColor;
				}
				else
				{
					label.color = Color.red;
				}
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
