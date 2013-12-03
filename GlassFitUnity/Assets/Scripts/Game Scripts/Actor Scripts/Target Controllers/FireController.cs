using UnityEngine;
using System.Collections;


/// <summary>
/// Fire controller. Controls the movement of the fire.
/// </summary>
public class FireController : TargetController {
	
	/// <summary>
	/// Start this instance. Sets the initial attributes.
	/// </summary>
	void Start () {
	
		//set initial position
		base.Start();
		SetAttribs(100, 135, 0, 0);
	}
	
	/// <summary>
	/// Raises the enable event. Sets the initial attributes
	/// </summary>
	void OnEnable() {
		base.OnEnable();
		SetAttribs(100, 135, 0, 0);
	}
	/// <summary>
	/// Update this instance. Controls the movement of the fire.
	/// </summary>
	// Update is called once per frame
	void Update () {
		base.Update();
		//move to new position
	}
}
