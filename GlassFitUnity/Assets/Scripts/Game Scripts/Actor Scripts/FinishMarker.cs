using UnityEngine;
using System.Collections;

/// <summary>
/// Controls the finish markers
/// </summary>
public class FinishMarker : RYWorldObject {
	
	// Target for the player to reach.
	private int target;
	
	// Current distance travelled.
	private double distance;
	
	// Bonus points for final sprint
	private float finalBonus = 1000;
	
	/// <summary>
	/// Sets the initial position and target
	/// </summary>
	protected override void Start () {
		// Get the player's target.
		if(Application.isEditor)
		{
			target = 5000;
		}
		else
		{
			target = (int)DataVault.Get("finish");
		}
		// Set the initial position out of view.

		base.Start();

		setRealWorldDist(target);

	}
	
	/// <summary>
	/// Updates the position
	/// </summary>
	void Update () {
		// Get the current distance travelled.
		distance = Platform.Instance.LocalPlayerPosition.Distance;

		//display ending bonus messages
		if(distance > target - 100) 
		{
			DataVault.Set("ending_bonus", "Keep going for " + finalBonus.ToString("f0") + " bonus points!");
			if(finalBonus > 0) {
				finalBonus -= 50f * Time.deltaTime;
			} else {
				finalBonus = 0;
			}
		} else 
		{
			DataVault.Set("ending_bonus", "");
		}

		base.Update();
	}
}
