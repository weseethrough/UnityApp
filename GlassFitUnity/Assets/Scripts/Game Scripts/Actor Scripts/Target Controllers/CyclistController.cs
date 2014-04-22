using UnityEngine;
using System.Collections;

/// <summary>
/// Controls the cyclist
/// </summary>
public class CyclistController : PBRunnerController {

	private AudioSource bikeBell;

	private bool hasOvertaken = false;

	/// <summary>
	/// Sets the attributes
	/// </summary>
	public override void Start () {
		// Start the base and set the attributes.
		base.Start();
		Initialise();
	}

	private void Initialise() {
		bikeBell = GetComponent<AudioSource>();
	}

	
	/// <summary>
	/// Raises the enable event. Sets the attributes
	/// </summary>
	public virtual void OnEnable() {
		// Enable the base and set the attributes.
		Initialise();
		FirstRaceOpponenet posController = gameObject.GetComponent<FirstRaceOpponenet>();
		posController.enabled = true;
	}
	
	/// <summary>
	/// Update this instance.
	/// </summary>
	public override void Update () {
		// Update the base.
		base.Update();
		if(!hasOvertaken && GetDistanceBehindTarget() > 0) {
			hasOvertaken = true;
			bikeBell.Play();
		}

		if(hasOvertaken && GetDistanceBehindTarget() < 0) {
			hasOvertaken = false;
		}

		if(realWorldMovementSpeed > 0)
		{
			anim.speed = 1.0f;
		}
		else
		{
			anim.speed = 0.0f;
		}
	}

//	protected virtual void SetAnimSpeed (float speed)
//	{
//		if(speed > 0.0f) {
//			anim.speed = 1.0f;
//		} else {
//			anim.speed = 0.0f;
//		}
//	}
}
