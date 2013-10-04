using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System;

public class PlatformHelper : MonoBehaviour {
	
	private bool countdown = false;
	private bool started = false;
	private Stopwatch timer = new Stopwatch();
	private Platform platform;
	
	// Use this for initialization
	void Start () {
		platform = new Platform();
	}
	
	void OnGUI()
	{
		if(countdown)
		{
			GUI.skin.label.fontSize = 40;
			float currentTime = 3.0f - timer.Elapsed.Seconds;
			if(currentTime > 0.0f)
			{
				GUI.Label(new Rect(300, 200, 200, 200), currentTime.ToString("f0")); 
			}
			else if(currentTime > -1.0f && currentTime <= 0.0f)
			{
				GUI.Label(new Rect(300, 200, 200, 200), "GO!"); 
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		
		if(!countdown)
		{
			if(platform.hasLock())
			{
				countdown = true;
				timer.Start();
			}
		}
		else
		{
			if(!started && timer.Elapsed.Seconds > 2)
			{
				platform.StartTrack(false);
				started = true;
			}
		}
		
		if(Input.GetKey(KeyCode.Escape))
		{
			if(timer.Elapsed.Seconds > 3)
			{
				Application.LoadLevel(Application.loadedLevel);
			} else
			{
				Application.LoadLevel("Zoom Camera");
			}
		}
	}
}
