using UnityEngine;
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
	public override void Start () {
		// Call the base function
		base.Start();
		// Find the boulder
		boulder = GetComponent<BoulderController>();
		UnityEngine.Debug.Log("BoulderSnack: Start");
	}
	
	// Update is called once per frame
	public override void Update () {
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
				DataVault.Set("snack_result", "You survived for " + UnitsHelper.SiDistance(boulder.GetPlayerDistanceTravelled()));
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
	

}
