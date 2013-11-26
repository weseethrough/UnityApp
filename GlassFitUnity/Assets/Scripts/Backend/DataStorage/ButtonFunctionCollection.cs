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
	
	static public bool SetType(FlowButton fb, Panel panel)
	{
		switch(fb.name) 
		{
		case "Run":
			DataVault.Set("type", "Runner");
			break;
			
		case "Bike":
			DataVault.Set("type", "Cyclist");
			break;
			
		case "Boulder Level 1":
			DataVault.Set("type", "Boulder");
			break;
			
		case "Eagle Level 1":
			DataVault.Set("type", "Eagle");
			break;
			
		case "Zombie Level 1":
			DataVault.Set("type", "Zombie");
			break;
			
		case "Train Level 1":
			DataVault.Set("type", "Train");
			break;
		}
		
		return true;
	}

    static public bool StartGame(FlowButton fb, Panel panel)
	{
		AutoFade.LoadLevel(1, 0.1f, 1.0f, Color.black);
		
		switch(fb.name) 
		{
		case "1km":
			DataVault.Set("finish", 1);
			break;
			
		case "2km":
			DataVault.Set("finish", 2);
			break;
			
		case "3km":
			DataVault.Set("finish", 3);
			break;
			
		case "4km":
			DataVault.Set("finish", 4);
			break;
			
		case "5km":
			DataVault.Set("finish", 5);
			break;
		}
		
		return true;
	}
	
	static public bool StartPursuitGame(FlowButton fb, Panel panel) 
	{
		AutoFade.LoadLevel(3, 0f, 1.0f, Color.black);
		
		switch(fb.name) {
		case "1km":
			DataVault.Set("finish", 1);
			break;
			
		case "2km":
			DataVault.Set("finish", 2);
			break;
			
		case "3km":
			DataVault.Set("finish", 3);
			break;
			
		case "4km":
			DataVault.Set("finish", 4);
			break;
			
		case "5km":
			DataVault.Set("finish", 5);
			break;
		}
		return true;
	}
	
	static public bool SetIndoor(FlowButton fb, Panel panel)
	{
		PursuitGame ps = (PursuitGame) GameObject.FindObjectOfType(typeof(PursuitGame));
		ps.SetIndoor();
		return false;
	}
	
	static public bool ChangeToEagle(FlowButton fb, Panel panel)
	{
		PursuitGame ps = (PursuitGame) GameObject.FindObjectOfType(typeof(PursuitGame));
		ps.SetTarget(PursuitGame.Targets.Eagle);
		return false;
	}
	
	static public bool ChangeToZombie(FlowButton fb, Panel panel)
	{
		PursuitGame ps = (PursuitGame) GameObject.FindObjectOfType(typeof(PursuitGame));
		ps.SetTarget(PursuitGame.Targets.Zombie);
		return false;
	}
	
	static public bool ChangeToTrain(FlowButton fb, Panel panel)
	{
		PursuitGame ps = (PursuitGame) GameObject.FindObjectOfType(typeof(PursuitGame));
		ps.SetTarget(PursuitGame.Targets.Train);
		return false;
	}
	
	static public bool ChangeToBoulder(FlowButton fb, Panel panel)
	{
		PursuitGame ps = (PursuitGame) GameObject.FindObjectOfType(typeof(PursuitGame));
		ps.SetTarget(PursuitGame.Targets.Boulder);
		return false;
	}
	
	static public bool BackPursuit(FlowButton fb, Panel panel)
	{
		PursuitGame ps = (PursuitGame) GameObject.FindObjectOfType(typeof(PursuitGame));
		ps.Back();
		GameObject h = GameObject.Find("blackPlane");
		h.renderer.enabled = false;
		return true;
	}
	
	static public bool EndGame(FlowButton fb, Panel panel)
	{
		Platform.Instance.reset();
		
		AutoFade.LoadLevel(2, 0f, 1.0f, Color.black);
		return true;
	}
	
	static public bool ClearHistory(FlowButton fb, Panel panel)
	{
		panel.parentMachine.ForbidBack();
		return true;
	}
	
	static public bool SetFriendType(FlowButton fb, Panel panel) 
	{
		DataVault.Set("friend_type", fb.name);
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
