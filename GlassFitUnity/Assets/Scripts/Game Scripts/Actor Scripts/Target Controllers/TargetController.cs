using UnityEngine;
using System.Collections;

/// <summary>
/// Base class for the targets. Controls the movement and the target tracker
/// </summary>
public class TargetController : MonoBehaviour {
	protected double scaledDistance = 0.0f;
	public TargetTracker target { get; protected set; }
	
	protected float distanceOffset = 0.0f;
	protected float travelSpeed = 1.0f;
	protected float height = 0.0f;
	protected float xOffset = 0.0f;
	
	protected int lane = 1;
	
	// Use this for initialization
	protected void Start () {		
	}
	
	// TODO:
	// Set tracker on Instantiate
	// Set index/lane 
	// Only poll once
	
	protected void OnEnable() {
		UnityEngine.Debug.Log("Target: Just been enabled");
	}
	
	public void SetTracker(TargetTracker tracker) {
		target = tracker;
		UnityEngine.Debug.Log("Target: linked to tracker: " + target.ToString());
	}
	
	public void SetLane(int lane) {
		this.lane = lane;
	}
	
	public void IncreaseOffset() 
	{
		distanceOffset += 50f;
	}
	
	public void SetAttribs(float offset, float speed, float yDist, float xDist) {
		distanceOffset = offset;
		travelSpeed = speed;
		height = yDist;
		xOffset = xDist*(lane*3); // TODO: parent.gameObject.width?
	}
	
	// Update is called once per frame
	public virtual void Update () {
		
	
#if !UNITY_EDITOR
		if (object.ReferenceEquals(null, target)) return;
//		UnityEngine.Debug.Log("Target: Distance is " + target.GetTargetDistance().ToString());
		scaledDistance = (target.GetDistanceBehindTarget() - distanceOffset) * travelSpeed;
#else
		scaledDistance = (PlatformDummy.Instance.DistanceBehindTarget() - distanceOffset) * travelSpeed;
#endif
		Vector3 movement = new Vector3(xOffset, height, (float)scaledDistance);
		transform.position = movement;
	}
	
	public override string ToString() {
		return "TargetController";
	}
}
