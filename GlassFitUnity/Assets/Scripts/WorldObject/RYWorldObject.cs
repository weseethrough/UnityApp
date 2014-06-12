using UnityEngine;
using System.Collections;

public class RYWorldObject : MonoBehaviour {

	public Vector3 realWorldPos = Vector3.zero;

	/// <summary>
	/// The movement speed for use in calculating any speed-dependent behaviour. Not used in updating the position.
	/// </summary>
	protected float realWorldMovementSpeed = 0.0f;
	protected bool scenePosIsFrozen = false;

	// Use this for initialization
	public virtual void Start () {
		//by default, set the real world pos as the scene pos
		realWorldPos = new Vector3( 0,0,(float)Platform.Instance.LocalPlayerPosition.Distance) + transform.position;
	}
	
	// Update is called once per frame
	public virtual void Update () {
		if(!scenePosIsFrozen)
		{
			updateScenePos();
		}
		else
		{
			//update the real world pos
		}
	}

	private void updateScenePos()
	{
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
		updateScenePos();
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
		updateScenePos();
	}

    /// <summary>
    /// Gets the real world dist.
    /// </summary>
    /// <returns>The real world dist.</returns>
    public float getRealWorldDist()
    {
        return realWorldPos.z;
    }

	/// <summary>
	///  Set real world movement speed. Updates it in the object for use in speed-dependent evaluations. Does not update position.
	/// </summary>
	/// <param name="speed">worldSpeeed</param>
	public void setRealWorldSpeed(float speed)
	{
		realWorldMovementSpeed = speed;
	}

    /// <summary>
    /// Gets the real world speed.
    /// </summary>
    /// <returns>The real world speed.</returns>
    public float getRealWorldSpeed()
    {
        return realWorldMovementSpeed;
    }

	public virtual double GetDistanceBehindTarget()
	{
		return realWorldPos.z - Platform.Instance.LocalPlayerPosition.Distance;
	}

	/// <summary>
	/// Sets whether the object's position should be updated.
	/// </summary>
	/// <param name="frozen">If set to <c>true</c> the object will no longer update its scene position based on changes to its own world position, or the player's world position.</param>
	public void setScenePositionFrozen(bool frozen)
	{
		//if we're unfreezing, then jump the world position to maintain current scene position
		if(scenePosIsFrozen && !frozen)
		{
			realWorldPos.z = (float)Platform.Instance.LocalPlayerPosition.Distance + transform.position.z;
		}

		//set the flag
		scenePosIsFrozen = frozen;
	}
}
