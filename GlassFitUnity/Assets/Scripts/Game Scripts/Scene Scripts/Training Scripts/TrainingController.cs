using UnityEngine;
using System.Collections;

public class TrainingController : TargetController {

	// Avatar's animator.
	private Animator anim; 
	
	// Animator speed.
	private float speed;
<<<<<<< HEAD
=======
	
	// Boolean to check if started and has moved.
	private bool started = false;
>>>>>>> master
	private bool move = false;
	
	// Value for inital movement in the z-axis.
	private float zMove = 20.0f;
	
	
	void Start () {
		// Set the attributes and get the animator
		SetAttribs(20, 135, -254.6f, 100);
		anim = GetComponent<Animator>();
	}
	
	void OnEnable() {
		// Set the attributes
		SetAttribs(20, 135, -254.6f, 100);
		anim.speed = 0.5f;
		target = Platform.Instance.CreateTargetTracker(2.2f);
	}
	
	public void SetSpeed(float speed) {
		anim.speed = speed / 2.2f;
		target = Platform.Instance.CreateTargetTracker(speed);
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
	
	void Update () {
<<<<<<< HEAD
=======
				
		// First reset the targets and get a new target tracker.
		if(!started) {
			started = true;
			Platform.Instance.ResetTargets();
			target = Platform.Instance.GetTargetTracker();
			anim.speed = 0.5f;
			target.SetTargetSpeed(2.2f);
		} 
>>>>>>> master
		
		// If the avatar needs to move into position.
		if(move) {
			// Set the animation to "run".
			anim.SetBool("Looking", true);
			
			// Make the avatar run to the starting line.
			if(zMove > 0.0f) {
				zMove -= Time.deltaTime * 5.0f;			
				SetAttribs(zMove, 135, -254.6f, 100);
			}
			
<<<<<<< HEAD
			float newSpeed = target.PollCurrentSpeed();
=======
			// Get the current target speed.
			float newSpeed = target.GetCurrentSpeed();
			
			// If there is a new target speed.
>>>>>>> master
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
