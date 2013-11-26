using UnityEngine;
using System.Collections;

/// <summary>
/// Controls the animations for the zombie
/// </summary>
public class ZombieAnimation : MonoBehaviour {
	
	// Animation speed.
	private float speed;
	
	// Animator for the zombie.
	private Animator anim;
	
	/// <summary>
	/// Get components and set the speed
	/// </summary>
	void Start () {
		// Get the animator.
		anim = GetComponent<Animator>();
		
		// Get the speed of the target.
		speed = Platform.Instance.GetCurrentSpeed(0);
		
		// Set the animation speed based on the target speed and scale the value.
		anim.SetFloat("Speed", speed);
		if(speed > 2.2f) {
				anim.speed = Mathf.Clamp(speed / 4.0f, 1, 2.5f);
			} else {
				anim.speed = speed / 1.25f;
		}
	}
	
	/// <summary>
	/// Update this instance. Updates animation speed
	/// </summary>
	void Update () {
		// Get the speed of the target.
		float newSpeed = Platform.Instance.GetCurrentSpeed(0);
		
		// If the speed has changed.
		if(newSpeed != speed)
		{
			// Set the new speed and animation speed.
			speed = newSpeed;
			anim.SetFloat("Speed", speed);
			if(speed > 2.2f) {
				anim.speed = Mathf.Clamp(speed / 4.0f, 1, 2.0f);
			} else {
				anim.speed = speed / 1.25f;
			}
		}
		
	}
}
