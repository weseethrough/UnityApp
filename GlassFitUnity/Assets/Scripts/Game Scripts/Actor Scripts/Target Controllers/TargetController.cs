using UnityEngine;
using System.Collections;

/// <summary>
/// Base class for the targets. Controls the movement and the target tracker
/// </summary>
public class TargetController : RYWorldObject {

	//public TargetTracker target { get; protected set; }
	
	public bool shouldShowOverheadLabel = false;
	public float overheadLabelHeight = 300.0f;
	public string overheadLabelString = "TEST";
	public float overheadLabelScreenOffset = 20.0f;
	
	protected int lane = 1;
	protected float lanePitch = 3.0f;
	
	// Use this for initialization
	public virtual void Start () {
		base.Start();
	}
	
	// TODO:
	// Set tracker on Instantiate
	// Set index/lane 
	// Only poll once
	
	public virtual void OnEnable() {
		UnityEngine.Debug.Log("Target: Just been enabled");
	}
	
	public void SetTracker(TargetTracker tracker) {
//		target = tracker;
//		if(target != null) {
//			UnityEngine.Debug.Log("Target: linked to tracker: " + target.ToString());
//		}	
	}

	public virtual void SetHeadstart(float dist)
	{
		
	}

	public void SetLane(int lane) {
		this.lane = lane;
	}
	
	public void IncreaseOffset() 
	{
		UnityEngine.Debug.LogWarning("TargetController: call to deprecated method IncreaseOffset(). Use World Position instead");
//		distanceOffset += 50f;
	}
	
	public void SetAttribs(float offset, float speed, float yDist, float xDist) {
//		distanceOffset = offset;
//		travelSpeed = speed;
//		height = yDist;
//		xOffset = xDist*(lane*lanePitch); // TODO: parent.gameObject.width?
		UnityEngine.Debug.LogWarning("TargetController: call to deprecated method SetAttribs(). Use SetWorldPos/Vel instead");
	}
	
	public virtual void SetSpeed(float f) {
#if UNITY_ANDROID
		if(target is FauxTargetTracker) {
			UnityEngine.Debug.Log("TargetController: Setting Target speed");
			((FauxTargetTracker)target).SetTargetSpeed(f);
		}
#endif
	}
	
	// Update is called once per frame
	public virtual void Update () {
		base.Update();
	}
	
	/// <summary>
	/// Gets the distance behind target.
	/// Calls through to the Platform by default, but subclasses can implement their own way to report this
	/// </summary>
	/// <returns>
	/// The distance behind target.
	/// </returns>
	public virtual double GetDistanceBehindTarget()
	{
//		if(target != null)
//		{
//			return target.GetDistanceBehindTarget();
//		}
//		else
		{
			//probably means we're in the editor
			return Platform.Instance.DistanceBehindTarget();
		}
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
	
	public virtual void OnGUI() {
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
