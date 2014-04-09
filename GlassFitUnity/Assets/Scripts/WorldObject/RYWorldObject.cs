using UnityEngine;
using System.Collections;

public class RYWorldObject : MonoBehaviour {

	protected Vector3 realWorldPos = Vector3.zero;

	/// <summary>
	/// The movement speed for use in calculating any speed-dependent behaviour. Not used in updating the position.
	/// </summary>
	protected float realWorldMovementSpeed = 0.0f;

	// Use this for initialization
	protected virtual void Start () {
		//by default, set the real world pos as the scene pos
		realWorldPos = transform.position;
	}
	
	// Update is called once per frame
	protected virtual void Update () {
		//assume 1D for now, so just update distance
		float sceneZ = realWorldPos.z - (float)Platform.Instance.LocalPlayerPosition.Distance;
		transform.position = new Vector3(realWorldPos.x, realWorldPos.y, sceneZ);
	}

	/// <summary>
	/// Sets the real world position
	/// </summary>
	/// <param name="posW">real world Position</param>

	public void setRealWorldPos(Vector3 posW)
	{
		realWorldPos = posW;
	}

	public Vector3 getRealWorldPos()
	{
		return realWorldPos;
	}

	/// <summary>
	/// Sets the real world distance, keeping x and y elements of position
	/// </summary>
	/// <param name="dist">real world distance along track</param>
	public void setRealWorldDist(float dist)
	{
		realWorldPos.z = dist;
	}

	/// <summary>
	///  Set real world movement speed. Updates it in the object for use in speed-dependent evaluations. Does not update position.
	/// </summary>
	/// <param name="speed">worldSpeeed</param>
	public void setRealWorldSpeed(float speed)
	{
		realWorldMovementSpeed = speed;
	}
}
