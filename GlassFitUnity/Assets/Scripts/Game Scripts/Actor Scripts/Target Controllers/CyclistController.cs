using UnityEngine;
using System.Collections;

/// <summary>
/// Controls the cyclist
/// </summary>
public class CyclistController : FirstRaceOpponenet {

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
		base.SetAttribs(0, 1, transform.position.y, transform.position.x);
		bikeBell = GetComponent<AudioSource>();
	}
	
	/// <summary>
	/// Raises the enable event. Sets the attributes
	/// </summary>
	public override void OnEnable() {
		// Enable the base and set the attributes.
		base.OnEnable();
		Initialise();
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
	}

	protected override void SetAnimSpeed (float speed)
	{
		if(speed > 0.0f) {
			anim.speed = 1.0f;
		} else {
			anim.speed = 0.0f;
		}
	}
}
