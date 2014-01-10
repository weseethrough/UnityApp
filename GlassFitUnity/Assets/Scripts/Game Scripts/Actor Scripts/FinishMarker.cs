using UnityEngine;
using System.Collections;

/// <summary>
/// Controls the finish markers
/// </summary>
public class FinishMarker : MonoBehaviour {
	
	// Target for the player to reach.
	private int target;
	
	// Current distance travelled.
	private double distance;
	
	// Bonus points for final sprint
	private float finalBonus = 1000;
	
	/// <summary>
	/// Sets the initial position and target
	/// </summary>
	void Start () {
		// Get the player's target.
#if UNITY_EDITOR
		target = 5000;
#else
		target = (int)DataVault.Get("finish");
#endif	
		// Set the initial position out of view.
		transform.position = new Vector3(0f, -595f, 500000f);
	}
	
	/// <summary>
	/// Updates the position
	/// </summary>
	void Update () {
		// Get the current distance travelled.
		distance = Platform.Instance.Distance();
		// If the finish line is in range, display it.
		if(distance > target - 100) 
		{
			DataVault.Set("ending_bonus", "Keep going for " + finalBonus.ToString("f0") + " bonus points!");
			if(finalBonus > 0) {
				finalBonus -= 50f * Time.deltaTime;
			} else {
				finalBonus = 0;
			}
			double deltDist = target - distance;
			//deltDist *= 135f;
			transform.position = new Vector3(0, -595f, (float)deltDist);
		} else 
		{
			DataVault.Set("ending_bonus", "");
		}
	}
}
