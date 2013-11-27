using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System;

/// <summary>
/// Controls the position of the train
/// </summary>
public class TrainController : TargetController {
	
	/// <summary>
	/// Start this instance. Sets the attributes
	/// </summary>
	void Start () {
		// Set the base and initial attributes.
		base.Start();
		SetAttribs(50, 135, -300, 103.8f);
	}
	
	/// <summary>
	/// Raises the enable event. Sets the attributes
	/// </summary>
	void OnEnable() {
		// Enable the base and set the attributes.
		base.OnEnable();
		SetAttribs(50, 135, -300, 103.8f);
	}
	
	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update () {
		// Update the base
		base.Update();
<<<<<<< HEAD
		if (target == null) return;
		
		whistleTime += Time.deltaTime;
		
		if(whistleTime >= 10.0f)
		{
			//trainWhistle.Play();
			whistleTime -= 10.0f;
		}
		
		//Platform.Instance.Poll();
	
		//scaledDistance = (target.getTargetDistance() - 50) * 135;

		//Vector3 movement = new Vector3(103.8f,-300,(float)scaledDistance);
		//transform.position = movement;
=======
>>>>>>> master
	}
}
