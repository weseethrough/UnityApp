﻿using UnityEngine;
using System.Collections;

public class BoulderSnack : SnackBase {
	
	// The boulder
	BoulderController boulder;
	
	public GameObject boulderCamera;
	
	/// <summary>
	/// Override base class. Will enable the boulder to start the game.
	/// </summary>
	public override void Begin ()
	{
		// Call the base function
		base.Begin ();
		
		SetMainCamera(false);
		
		SetThirdPerson(false);
		
		SetTrack(false);
		
		// Enable the boulder if it is there
		if(boulder != null)
		{
			boulder.enabled = true;
			UnityEngine.Debug.Log("BoulderSnack: Starting game");
		}
		else
		{
			UnityEngine.Debug.Log("BoulderSnack: can't enable boulder - is null!"); 
		}
	}
	
	// Use this for initialization
	void Start () {
		// Call the base function
		base.Start();
		// Find the boulder
		boulder = GetComponent<BoulderController>();
		UnityEngine.Debug.Log("BoulderSnack: Start");
	}
	
	// Update is called once per frame
	void Update () {
		// Call the base function
		base.Update();
		
		// Update the display
		UpdateAhead(boulder.GetDistanceBehindTarget());
		
		// Check if the boulder is null
		if(boulder != null)
		{
			// Check if the boulder reached the player
			if(boulder.GetDistanceBehindTarget() > 0.0 && !finish)
			{				
				// Set the attributes for the banner
				DataVault.Set("death_colour", "EA0000FF");
				DataVault.Set("snack_result", "You survived for " + SiDistance(boulder.GetPlayerDistanceTravelled()));
				DataVault.Set("snack_result_desc", "returning to game...");
				// End the game
				finish = true;
				
				// Show the banner
				StartCoroutine(ShowBanner(3.0f));
			}
		}
		else
		{
			UnityEngine.Debug.Log("BoulderSnack: can't check boulder distance - is null");
		}
	}
	
	/// <summary>
	/// Converts distance to a string.
	/// </summary>
	/// <returns>
	/// The distance with units attached.
	/// </returns>
	/// <param name='meters'>
	/// distance in meters.
	/// </param>
	protected string SiDistance(double meters) {
		string postfix = "m";
		string final;
		float value = (float)meters;
		if (value > 1000) {
			value = value/1000;
			postfix = "km";
			if(value >= 10) {
				final = value.ToString("f1");
			} else {
				final = value.ToString("f2");
			}
		}
		else
		{
			final = value.ToString("f0");
		}
		//set the units string for the HUD
		return final + postfix;
	}
}
