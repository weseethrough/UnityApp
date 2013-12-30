using UnityEngine;
using System.Collections;

/// <summary>
/// Base class for the targets. Controls the movement and the target tracker
/// </summary>
public class TargetController : MonoBehaviour {
	protected double scaledDistance = 0.0f;
	public TargetTracker target { get; protected set; }
	
	public bool shouldShowOverheadLabel = false;
	public float overheadLabelHeight = 300.0f;
	public string overheadLabelString = "TEST";
	public float overheadLabelScreenOffset = 0.0f;
	
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
	
	public void SetSpeed(float f) {
		if(target is FauxTargetTracker) {
			((FauxTargetTracker)target).SetTargetSpeed(f);
		}
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
	
		
	protected GUIStyle getLabelStylePace()
	{
		// set style for our labels
		GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
		labelStyle.alignment = TextAnchor.MiddleCenter;
		labelStyle.fontSize = 25;
		labelStyle.fontStyle = FontStyle.Normal;
		labelStyle.clipping = TextClipping.Overflow;

		return labelStyle;
	}
	
	public void OnGUI() {
		if(shouldShowOverheadLabel)
		{
			renderLabel();
		}
	}
	
	
	protected void renderLabel() {
		if(overheadLabelString == "")
		{
			return;
		}
		
		GUIStyle PaceLabelStyle = getLabelStylePace();
		
		Vector3 actorPos = transform.position;
		
		Vector3 headPos = actorPos + new Vector3(0, overheadLabelHeight, 0);
		//UnityEngine.Debug.Log("FirstRun: actor height: " + actorTop);
		//UnityEngine.Debug.Log("FirstRun: actor world pos y: " + actor.transform.position);
		
			Vector3 screenPos = Camera.main.WorldToScreenPoint(headPos);
			
			//only show actors in front of us.
			if(screenPos.z < 0) return;
			
			//create label
			GUIStyle paceStyle = getLabelStylePace();
			float paceHalfWidth = 200;
		
			//calculate yPos. Note, camera screen pos calculation comes out with y inverted.
			float yPos = Screen.height - screenPos.y - overheadLabelScreenOffset;
		
			Rect paceRect = new Rect(screenPos.x - paceHalfWidth, yPos, 2*paceHalfWidth, 1);
			
			GUI.Label(paceRect, overheadLabelString, paceStyle);
		
	}
	
	public override string ToString() {
		return "TargetController";
	}
}
