using UnityEngine;
using System.Collections;

public class TrainingController : RYWorldObject {

	// Avatar's animator.
	private Animator anim; 
	
	// Animator speed.
	private float speed;

	// Boolean to check if has moved.
	private bool move = false;
	
	// Value for inital movement in the z-axis.
	private float zMove = 20.0f;
	
	
	public override void Start () {
		UnityEngine.Debug.LogError("Using deprecated class. Refactor required.");
	}

	/// <summary>
	/// Sets the move boolean so that the avatar gets in the correct position.
	/// </summary>
	/// <param name='b'>
	/// The new move boolean.
	/// </param>
	public void SetMove(bool b) {
		move = b;
	}
	
	public override void Update () {		
		// If the avatar needs to move into position.
		if(move) {
			// Set the animation to "run".
			anim.SetBool("Looking", true);
			
			// Make the avatar run to the starting line.
			if(zMove > 0.0f) {
				zMove -= Time.deltaTime * 5.0f;			
			}
			
			// Get the current target speed.
//			float newSpeed = target.PollCurrentSpeed();
			float newSpeed = 1;			
			// If there is a new target speed.
			if(speed != newSpeed)
			{
				// Set the speed and the animation variable.
				speed = newSpeed;
				anim.SetFloat("Speed", speed);
				if(speed >= 2.2f && speed < 4.0f) {
					anim.speed = newSpeed / 2.2f;
				} else if(speed > 4.0f) {
					anim.speed = Mathf.Clamp(newSpeed / 4.0f, 1, 2);
				} else {
					anim.speed = newSpeed / 1.25f;
				}
			}
		}
		
		base.Update();
	}
}
