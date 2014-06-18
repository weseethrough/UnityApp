using UnityEngine;
using System.Collections;
using RaceYourself;

public class PositionController : MonoBehaviour {

	protected WorldObject worldObject;

	// Use this for initialization
	public virtual void Start () {
		worldObject = (WorldObject)gameObject.GetComponent<WorldObject>();
		if(worldObject == null)
		{
			UnityEngine.Debug.LogError("PositionController: no WorldObject found for Position Controller. Object = " + gameObject);
		}
	}
	
	// Update is called once per frame
	public virtual void Update () {
	
	}

	public virtual WorldObject getWorldObject()
	{
		return worldObject;
	}
}
