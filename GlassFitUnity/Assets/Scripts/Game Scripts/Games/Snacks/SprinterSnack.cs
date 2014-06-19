using UnityEngine;
using System.Collections;
using RaceYourself;

public class SprinterSnack : SnackBase {
	
	// Sprinter's controller
	private SprinterController sprinter;
	
	// Boolean to check if Sprinter has finished the race
	private bool sprinterFinished = false;
	
	// Boolean to check if the player has finished the race
	private bool playerFinished = false;

	// The start time for the player
	private long startTime = 0;
	
	// The end time for the player
	private long finalTime = 0;
	
	WorldObject stadiumObject;
	WorldObject finishLineObject;
	
	private int sprinterLevel = 0;
	
	// Use this for initialization
	public override void Start () {
		base.Start();
		
		stadiumObject = (WorldObject)GameObject.Find("Stadium").GetComponent<WorldObject>();
		finishLineObject = (WorldObject)GameObject.Find("Finish Line").GetComponent<WorldObject>();
		stadiumObject.setScenePositionFrozen(true);
		finishLineObject.setScenePositionFrozen(true);
		sprinterLevel = (int)DataVault.Get("sprinter_level");
	}
	
	// Update is called once per frame
	public override void Update () {
		base.Update();
		
		// Check if sprinter is enabled
		if(sprinter != null && sprinter.enabled)
		{
			// Update the ahead/behind
			UpdateAhead(sprinter.getWorldObject().GetDistanceBehindTarget());
			
			// If sprinter has finished the race
			if(sprinter.GetSprinterDistanceTravelled() >= 100 && !sprinterFinished)
			{
				// Set the boolean to true
				sprinterFinished = true;
				
				// Set the banner attributes and show it
				if(!playerFinished) {
					DataVault.Set("death_colour", "EA0000FF");
					DataVault.Set("snack_result", "You lost!");
					DataVault.Set("snack_result_desc", "Try again next time");
					StartCoroutine(ShowBanner(3.0f));
				}
			}
			
			// If the player finishes the race
			if(sprinter.GetPlayerDistanceTravelled() >= 100 && !playerFinished)
			{
				// If they finish before Sprinter (unlikely)
				if(!sprinterFinished) {
					// Set the attributes and show the banner
					DataVault.Set("death_colour", "12D400FF");
					DataVault.Set("snack_result", "You won!");
					DataVault.Set("snack_result_desc", "Your enemy has leveled up!");
					UnityEngine.Debug.Log("SprinterSnack: Increasing level");
					sprinterLevel++;
					UnityEngine.Debug.Log("SprinterSnack: Setting level in DataVault");
					DataVault.Set("sprinter_level", sprinterLevel);
					UnityEngine.Debug.Log("SprinterSnack: About to save to blob");
					DataVault.SaveToBlob();
					UnityEngine.Debug.Log("SprinterSnack: Showing banner");
					StartCoroutine(ShowBanner(3.0f));
				}
				// Set the player finished to true
				playerFinished = true;
				// Calculate the final time
				finalTime = Platform.Instance.LocalPlayerPosition.Time - startTime;
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
		if(stadiumObject != null)
		{
			stadiumObject.setScenePositionFrozen(true);
		}
		
		if(finish != null)
		{
			finishLineObject.setScenePositionFrozen(true);
		}

		sprinter.getWorldObject().setScenePositionFrozen(true);
		
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
		sprinterFinished = false;
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
		UnityEngine.Debug.Log("SprinterSnack: Starting Countdown Coroutine");
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

		float playerDist = (float)Platform.Instance.LocalPlayerPosition.Distance;
		stadiumObject.setScenePositionFrozen(false);
		finishLineObject.setScenePositionFrozen(false);

		StartRace();
	}
	
	/// <summary>
	/// Starts the race.
	/// </summary>
	void StartRace()
	{
		// Find sprinter
		sprinter = GetComponent<SprinterController>();
		if(sprinter)
		{
			// Enable him and save the start time
			sprinter.enabled = true;
			sprinter.SetLevel(sprinterLevel);
			startTime = Platform.Instance.LocalPlayerPosition.Time;
		}
		else
		{
			UnityEngine.Debug.Log("SprinterSnack: can't find Sprinter controller!");
		}
	}
}
