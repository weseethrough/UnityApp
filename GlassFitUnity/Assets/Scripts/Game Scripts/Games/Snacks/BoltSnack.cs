﻿using UnityEngine;
using System.Collections;

public class BoltSnack : SnackBase {
	
	// Usain Bolt's controller
	private BoltController bolt;
	
	// Boolean to check if Bolt has finished the race
	private bool boltFinished = false;
	
	// Boolean to check if the player has finished the race
	private bool playerFinished = false;
	
	// Boolean to check if the banner is being displayed
	private bool displayingBanner = false;
	
	// The start time for the player
	private long startTime = 0;
	
	// The end time for the player
	private long finalTime = 0;
	
	StadiumController stadiumController;
	
	StadiumController finishController;
	
	private int boltLevel = 0;
	
	// Use this for initialization
	void Start () {
		base.Start();
		
		GameObject stadium = GameObject.Find("Stadium");
		if(stadium != null)
		{
			stadiumController = stadium.GetComponent<StadiumController>();
			if(stadiumController == null)
			{
				UnityEngine.Debug.Log("BoltSnack: couldn't find StadiumController");
			}
		}
		else
		{
			UnityEngine.Debug.Log("BoltSnack: couldn't find Stadium object");
		}
		
		stadium = GameObject.Find("Finish Line");
		if(stadium != null)
		{
			finishController = stadium.GetComponent<StadiumController>();
			if(finishController == null)
			{
				UnityEngine.Debug.Log("BoltSnack: couldn't find StadiumController");
			}
		}
		else
		{
			UnityEngine.Debug.Log("BoltSnack: couldn't find Stadium object");
		}
		
		boltLevel = (int)DataVault.Get("bolt_level");
	}
	
	// Update is called once per frame
	void Update () {
		base.Update();
		
		// Check if bolt is enabled
		if(bolt != null && bolt.enabled)
		{
			// Update the ahead/behind
			UpdateAhead(bolt.GetDistanceBehindTarget());
			
			// If bolt has finished the race
			if(bolt.GetBoltDistanceTravelled() >= 100 && !boltFinished)
			{
				// Set the boolean to true
				boltFinished = true;
				
				// Set the banner attributes and show it
				if(!playerFinished) {
					DataVault.Set("death_colour", "EA0000FF");
					DataVault.Set("snack_result", "You lost!");
					DataVault.Set("snack_result_desc", "Try again next time");
					StartCoroutine(ShowBanner(3.0f));
				}
			}
			
			// If the player finishes the race
			if(bolt.GetPlayerDistanceTravelled() >= 100 && !playerFinished)
			{
				// If they finish before Bolt (unlikely)
				if(!boltFinished) {
					// Set the attributes and show the banner
					DataVault.Set("death_colour", "12D400FF");
					DataVault.Set("snack_result", "You won!");
					DataVault.Set("snack_result_desc", "Your enemy has leveled up!");
					UnityEngine.Debug.Log("BoltSnack: Increasing level");
					boltLevel++;
					UnityEngine.Debug.Log("BoltSnack: Setting level in DataVault");
					DataVault.Set("bolt_level", boltLevel);
					UnityEngine.Debug.Log("BoltSnack: About to save to blob");
					DataVault.SaveToBlob();
					UnityEngine.Debug.Log("BoltSnack: Showing banner");
					StartCoroutine(ShowBanner(3.0f));
				}
				// Set the player finished to true
				playerFinished = true;
				// Calculate the final time
				finalTime = Platform.Instance.Time() - startTime;
			}
			
			// If both finished, end the game
			if(playerFinished)
			{
				StartCoroutine(EndGame());
			}
		}
	}
	
	/// <summary>
	/// Ends the game.
	/// </summary>
	/// <returns>
	/// N/A
	/// </returns>
	private IEnumerator EndGame()
	{
		// Show the player's time for 5 seconds
		DataVault.Set("countdown_subtitle", "100m time: " + finalTime / 1000 + " seconds");
		if(stadiumController != null)
		{
			stadiumController.ShouldMove(false);
		}
		
		if(finishController != null)
		{
			finishController.ShouldMove(false);
		}
		
		yield return new WaitForSeconds(5.0f);
		DataVault.Set("countdown_subtitle", "");
		// Finish the game
		Finish();
	}
	
	/// <summary>
	/// Begin this instance.
	/// </summary>
	public override void Begin ()
	{
		// Call the base function
		base.Begin ();
		// Set the default booleans
		boltFinished = false;
		playerFinished = false;
		SetTrack(false);
		
		// Start the countdown
		StartCoroutine(DoCountDown());
	}
	
	public override void Finish ()
	{
		//clear subtitle on HUD
		DataVault.Set("countdown_subtitle", "");
		
		base.Finish ();
	}
	
	/// <summary>
	/// Starts the countdown.
	/// </summary>
	/// <returns>
	/// N/A
	/// </returns>
	IEnumerator DoCountDown()
	{
		UnityEngine.Debug.Log("BoltSnack: Starting Countdown Coroutine");
		for(int i=3; i>=0; i--)
		{
			//set value for subtitle. 0 = GO
			string displayString = (i==0) ? "GO !" : i.ToString();
			DataVault.Set("countdown_subtitle", displayString);
			
			//wait half a second
			yield return new WaitForSeconds(1.0f);
		}
		//start the game
		DataVault.Set("countdown_subtitle", " ");
		
		if(stadiumController != null)
		{
			stadiumController.ShouldMove(true);
		}
		else
		{
			UnityEngine.Debug.Log("BoltSnack: couldn't find StadiumController on stadium");
		}
		
		if(finishController != null)
		{
			finishController.ShouldMove(true);
		}
		else
		{
			UnityEngine.Debug.Log("BoltSnack: couldn't find StadiumController on finish line");
		}
		
		StartRace();
	}
	
	/// <summary>
	/// Starts the race.
	/// </summary>
	void StartRace()
	{
		// Find bolt
		bolt = GetComponent<BoltController>();
		if(bolt)
		{
			// Enable him and save the start time
			bolt.enabled = true;
			bolt.SetLevel(boltLevel);
			startTime = Platform.Instance.Time();
		}
		else
		{
			UnityEngine.Debug.Log("BoltSnack: can't find Bolt controller!");
		}
	}
}
