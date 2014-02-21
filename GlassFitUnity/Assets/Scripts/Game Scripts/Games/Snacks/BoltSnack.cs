using UnityEngine;
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
	
	// Use this for initialization
	void Start () {
		base.Start();
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
					DataVault.Set("snack_result_desc", "it was inevitable");
					StartCoroutine(ShowBanner());
				}
			}
			
			// If the player finishes the race
			if(bolt.GetPlayerDistanceTravelled() >= 100 && !playerFinished)
			{
				// If they finish before Bolt (unlikely)
				if(!boltFinished) {
					// Set the attributes and show the banner
					DataVault.Set("death_colour", "12D400FF");
					DataVault.Set("snack_result", "You won?!");
					DataVault.Set("snack_result_desc", "you must be cheating!");
					StartCoroutine(ShowBanner());
				}
				// Set the player finished to true
				playerFinished = true;
				// Calculate the final time
				finalTime = Platform.Instance.Time() - startTime;
			}
			
			// If both finished, end the game
			if(playerFinished && boltFinished)
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
			startTime = Platform.Instance.Time();
		}
		else
		{
			UnityEngine.Debug.Log("BoltSnack: can't find Bolt controller!");
		}
	}
}
