using UnityEngine;
using System.Collections;

public class TargetController : MonoBehaviour {
	protected double scaledDistance = 0.0f;
	protected TargetTracker target = null;
	protected float distanceOffset = 0.0f;
	protected float travelSpeed = 1.0f;
	protected float height = 0.0f;
	protected float xOffset = 0.0f;
	
	// Use this for initialization
	protected void Start () {
		//target = Platform.Instance.getTargetTracker();
	}
	
	// TODO:
	// Set tracker on Instantiate
	// Set index/lane 
	// Only poll once
	
	protected void OnEnable() {
		UnityEngine.Debug.Log("Target: Just been enabled");
	}
	
	public void SetTracker(TargetTracker tracker) {
		this.target = tracker;
		UnityEngine.Debug.Log("Target: linked to tracker");
	}
	
	public void IncreaseOffset() 
	{
		distanceOffset += 50f;
	}
	
	public void SetAttribs(float offset, float speed, float yDist, float xDist) {
		distanceOffset = offset;
		travelSpeed = speed;
		height = yDist;
		xOffset = xDist;
	}
	
	// Update is called once per frame
	protected void Update () {
		if (target == null) return;
		Platform.Instance.Poll();
	
/*		UnityEngine.Debug.Log("Target: Distance is " + target.getDistanceBehindTarget().ToString());
		UnityEngine.Debug.Log("Target: Platform Distance is " + Platform.Instance.getHighestDistBehind());
		UnityEngine.Debug.Log("Target: Distance behind target is " + Platform.Instance.DistanceBehindTarget());
*/		scaledDistance = (target.GetDistanceBehindTarget() - distanceOffset) * travelSpeed;

		Vector3 movement = new Vector3(xOffset, height, (float)scaledDistance);
		transform.position = movement;
	}
	
	public string ToString() {
		return "TargetController";
	}
}
