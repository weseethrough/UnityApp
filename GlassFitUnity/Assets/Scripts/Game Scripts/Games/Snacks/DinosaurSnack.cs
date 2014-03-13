using UnityEngine;
using System.Collections;

public class DinosaurSnack : SnackBase {
	
	// The dinosaur
	DinosaurController dinosaur;
	
	public override void Begin ()
	{
		base.Begin ();
		
		SetTrack(false);
		
		if(dinosaur != null)
		{
			dinosaur.enabled = true;
		}
		else
		{
			UnityEngine.Debug.Log("DinosaurSnack: Can't enable dinosaur - is null!");
		}
	}
	
	// Use this for initialization
	public override void Start () {
		
		base.Start();
		
		dinosaur = GetComponent<DinosaurController>();
		
	}
	
	// Update is called once per frame
	public override void Update () {
		base.Update();
		
		UpdateAhead(dinosaur.GetDistanceBehindTarget());
		
		if(dinosaur != null)
		{
			if(dinosaur.GetDistanceBehindTarget() > 0.0 && !finish)
			{				
				// Set the attributes for the banner
				DataVault.Set("death_colour", "EA0000FF");
				DataVault.Set("snack_result", "You survived for " + SiDistance(dinosaur.GetPlayerDistanceTravelled()));
				DataVault.Set("snack_result_desc", "returning to game...");
				// End the game
				finish = true;
				
				dinosaur.enabled = false;
				transform.position = new Vector3(0, 0, -1);
				
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
