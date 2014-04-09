using UnityEngine;
using System.Collections;

public class ConstantVelocityPositionController : PositionController {

	public Vector3 velocity;

	// Use this for initialization
	void Start () {
		base.Start();
	}
	
	// Update is called once per frame
	void Update () {
		//only update positions if we're currently tracking
		if(!Platform.Instance.LocalPlayerPosition.IsTracking)
		{ return; }
		if(worldObject == null)
		{ return; }

		//update position based on velocity
		worldObject.setRealWorldSpeed(velocity.magnitude);
		worldObject.setRealWorldPos( worldObject.getRealWorldPos() + velocity*Time.deltaTime );

		base.Update();
	}
}
