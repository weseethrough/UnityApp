using UnityEngine;
using System.Collections;

public class BoulderSnack : SnackBase {
	
	BoulderController boulder;
	
	public override void Begin ()
	{
		base.Begin ();
		
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
		base.Start();
		boulder = GetComponent<BoulderController>();
	}
	
	// Update is called once per frame
	void Update () {
		base.Update();
		UpdateAhead(boulder.GetDistanceBehindTarget());
		if(boulder != null)
		{
			if(boulder.GetDistanceBehindTarget() > 0.0)
			{				
				DataVault.Set("death_colour", "EA0000FF");
				DataVault.Set("snack_result", "You survived for " + SiDistance(boulder.GetPlayerDistanceTravelled()));
				DataVault.Set("snack_result_desc", "returning to game...");
				StartCoroutine(ShowBanner());
			}
		}
		else
		{
			UnityEngine.Debug.Log("BoulderSnack: can't check boulder distance - is null");
		}
	}
	
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
