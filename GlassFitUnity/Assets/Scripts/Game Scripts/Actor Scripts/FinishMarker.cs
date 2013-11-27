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
	
	/// <summary>
	/// Sets the initial position and target
	/// </summary>
	void Start () {
		// Get the player's target.
		target = (int)DataVault.Get("finish") * 1000;
		
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
			double deltDist = target - distance;
			deltDist *= 135f;
			transform.position = new Vector3(0, -595f, (float)deltDist);
		}
	}
}
