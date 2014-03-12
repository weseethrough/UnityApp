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
	/// Raises the arrival event when the camera arrives at this node.
	/// </summary>
	public virtual void OnArrival() {
		UnityEngine.Debug.Log("CamPath: Arrived at point: " + gameObject.name);
		//to be implemented by subclasses which want to do something specific on arrival
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
			transform.LookAt(lookAtTarget.transform.localPosition);
		}
		return transform.rotation;
	}
}
