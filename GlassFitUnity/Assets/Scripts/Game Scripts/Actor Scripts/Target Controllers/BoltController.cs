using UnityEngine;
using System.Collections;

public class BoltController : ConstantVelocityPositionController {
	
	// Distance where Bolt and the player started running
	private float startDistance = 0.0f;
	
	// Level for the enemy
	private int currentLevel = 0;
	
	// Use this for initialization
	public override void Start () {
		base.Start();
		// Set the start distance
		startDistance = (float)Platform.Instance.LocalPlayerPosition.Distance;
		worldObject.setRealWorldDist(startDistance);
	}
	
	public void SetLevel(int l)
	{
		currentLevel = l;
		velocity.z = 3.0f + (currentLevel * 0.5f);
	}
	
	public void OnEnable()
	{
	}
	
	void OnDisable()
	{
		velocity = Vector3.zero;
		worldObject.setRealWorldSpeed(0);
	}
	
	// Update is called once per frame
	public override void Update () 
	{
		if(GetBoltDistanceTravelled() > 100)
		{
			//stop running
			velocity = Vector3.zero;
		}
		
		base.Update();
	}
		
	/// <summary>
	/// Gets the distance travelled for Bolt
	/// </summary>
	/// <returns>
	/// The distance in meters
	/// </returns>
	public float GetBoltDistanceTravelled()
	{
		return worldObject.getRealWorldPos().z - startDistance;
	}
	
	/// <summary>
	/// Gets the player's distance travelled.
	/// </summary>
	/// <returns>
	/// The distance travelled.
	/// </returns>
	public float GetPlayerDistanceTravelled()
	{
		return (float)Platform.Instance.LocalPlayerPosition.Distance - startDistance;
	}
}
