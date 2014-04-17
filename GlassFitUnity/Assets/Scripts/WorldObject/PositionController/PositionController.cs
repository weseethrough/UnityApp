using UnityEngine;
using System.Collections;

public class PositionController : MonoBehaviour {

	protected RYWorldObject worldObject;

	// Use this for initialization
	public virtual void Start () {
		worldObject = (RYWorldObject)gameObject.GetComponent<RYWorldObject>();
		if(worldObject == null)
		{
			UnityEngine.Debug.LogError("PositionController: no RYWorldObject found for Position Controller. Object = " + gameObject);
		}
	}
	
	// Update is called once per frame
	public virtual void Update () {
	
	}

	public RYWorldObject getWorldObject()
	{
		return worldObject;
	}
}
