using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public class ButtonFunctionCollection  
{
   
    /// <summary>
    /// Every function in collection have to accept FlowButton variable and return boolean. 
    /// </summary>
    /// <param name="fb"> button providng event </param>
    /// <returns> Is button in state to continue? If False is returned button will not navigate forward on its own connection!</returns>
    static public bool MyFunction1(FlowButton fb, Panel panel)
    {
        Debug.Log("Testing linked function true");

        return true;
    }

    static public bool MyFunction2(FlowButton fb, Panel panel)
    {
        Debug.Log("Testing linked function false");

        return false;
    }

    static public bool isAuthent(FlowButton fb, Panel panel) 
	{
		// Connection followed asynchronously
		return false; 
	}

    static public bool StartGame(FlowButton fb, Panel panel)
	{
		AutoFade.LoadLevel(1, 1.0f, 1.0f, Color.black);
		return true;
	}

    static public bool Challenge(FlowButton button, Panel panel)
    {
		int friendId = (int)DataVault.Get("current_friend");
		if (friendId == 0) return false; // TODO: Challenge by third-party identity
		Platform.Instance.QueueAction(string.Format(@"{{
			'action' : 'challenge',
			'target' : {0},
			'taunt' : 'Your mother is a hamster!',
			'challenge' : {{
					'distance': 333,
					'duration': 42,
					'location': null,
					'public': false,
					'start_time': null,
					'stop_time': null,
					'type': 'duration'
			}}
		}}", friendId).Replace("'", "\""));		
		Debug.Log ("Challenge: " + friendId + " challenged");
		Platform.Instance.syncToServer();
		
		return true;
	}
}
