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
				DataVault.Set("snack_result", "Game Over");
				DataVault.Set("snack_result_desc", "returning to game...");
				StartCoroutine(ShowBanner());
			}
		}
		else
		{
			UnityEngine.Debug.Log("BoulderSnack: can't check boulder distance - is null");
		}
	}
}
