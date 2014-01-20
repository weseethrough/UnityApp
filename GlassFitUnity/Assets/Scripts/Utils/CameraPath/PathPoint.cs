using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathPoint : MonoBehaviour {
	
	public GameObject lookAtTarget = null;
	public float time;
	
	// Use this for initialization
	void Start () {
		//hide subobjects
		List<Component> renderableObjects = new List<Component>(GetComponentsInChildren(typeof(Renderer)));
		foreach(Renderer renderer in renderableObjects)
		{
			renderer.enabled = false;
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	/// <summary>
	/// Gets the node orientation.
	/// Returns either the fixed orientation of this node, or the required orientation to look at the look-at-target
	/// </summary>
	/// <returns>
	/// The node orientation.
	/// </returns>
	public Quaternion GetNodeOrientation() {
		if(lookAtTarget != null)
		{
			transform.LookAt(lookAtTarget.transform.position);
		}
		return transform.rotation;
	}
}
