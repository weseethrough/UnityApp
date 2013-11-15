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
    static public bool MyFunction1(FlowButton fb)
    {
        Debug.Log("Testing linked function");

        return true;
    }

    static public bool MyFunction2(FlowButton fb)
    {
        Debug.Log("Testing linked function number2");

        return false;
    }
	
	static public bool isAuthent(FlowButton fb) 
	{
		return Platform.Instance.authorize("any", "login");
	}

    public void Pure()
    {
        Debug.Log("Testing linked function number2");

        return;
    }
	
	static public bool Challenge(FlowButton button) {
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
