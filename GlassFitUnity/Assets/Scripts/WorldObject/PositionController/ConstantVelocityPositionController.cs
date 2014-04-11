using UnityEngine;
using System.Collections;

public class ConstantVelocityPositionController : PositionController {

	public Vector3 velocity = Vector3.zero;

	public float getSpeed()
	{
		return velocity.magnitude;
	}

	public void setSpeed(float speed)
	{
		if(velocity.magnitude != 0)
		{
			float scale = speed / velocity.magnitude;
			velocity *= scale;
		}	
		else
		{
			velocity = new Vector3(0,0,speed);
		}
	}

	// Use this for initialization
	public override void Start () {
		velocity = Vector3.zero;
		base.Start();
	}
	
	// Update is called once per frame
	public override void Update () {
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
