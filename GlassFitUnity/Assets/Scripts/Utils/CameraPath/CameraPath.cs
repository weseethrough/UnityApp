using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraPath : MonoBehaviour {
	
	protected List<PathPoint> points = null;
	protected int lastPointIndex = 0;	//the index in the list of the last point we passed
	protected bool bRunning = false;
	protected float timer = 0;
	public GameObject splineHolder = null;
	
	protected bool bFinished = false;
	
	public bool doFlythrough = true;
	
	// Use this for initialization
	void Start () {
		//gather the path points from the scene
		if(splineHolder != null)
		{
			List<Component> pointsComponents = new List<Component>(splineHolder.GetComponentsInChildren(typeof(PathPoint)));
			points = pointsComponents.ConvertAll(c => (PathPoint)c);

			//sort chronologically
			points.Sort(delegate(PathPoint a, PathPoint b)
			{
				return a.time.CompareTo(b.time);
			});
			
#if UNITY_EDITOR
			StartFollowingPath();
#endif
		}
	}
	
	// Update is called once per frame
	void Update () {
		if(bRunning)
		{
			//update time
			timer += Time.deltaTime;
			if(doFlythrough) {
				ApplyTransFormForTime(timer);
			}
		}
		
	}
	
	public bool IsFinished()
	{
		return bFinished;
	}
	
	protected void ApplyTransFormForTime(float t)
	{
		PathPoint thisPoint = points[lastPointIndex];
		
		//Check if we've moved to the next node yet
		if(t > points[lastPointIndex+1].time)
		{
			//move onto the next one
			lastPointIndex++;
			thisPoint = points[lastPointIndex];
			
			//trigger the OnArrived event
			thisPoint.OnArrival();
			
			//finish if this is the last point
			if(lastPointIndex +1 >= points.Count)
			{
				StopFollowingPath();
				return;
			}
		}
			
		//get the next point on the path
		PathPoint nextPoint = points[lastPointIndex +1];
		
		//progress towards next point
		float p = (t-thisPoint.time)/(nextPoint.time - thisPoint.time);
		
		//lerp pos and ori
		Vector3 pos = Vector3.Lerp(thisPoint.transform.localPosition, nextPoint.transform.localPosition, p);
		transform.localPosition = pos;
		
		//lerp between which object we're looking at, or the static orientation of the nodes
		Quaternion rotThis, rotNext;
		if(thisPoint.lookAtTarget != null)
		{
			transform.LookAt(thisPoint.lookAtTarget.transform.localPosition);
			rotThis = transform.rotation;
		}
		else
		{
			rotThis = thisPoint.GetNodeOrientation();	
		}
		
		if(nextPoint.lookAtTarget != null)
		{
			transform.LookAt(nextPoint.lookAtTarget.transform.localPosition);
			rotNext = transform.rotation;
		}
		else
		{
			rotNext = nextPoint.GetNodeOrientation();	
		}
		
		Quaternion rot = Quaternion.Lerp(rotThis, rotNext, p);
		transform.rotation = rot;
		
		return;
	}
	
	public void StartFollowingPath()
	{
		//suppress minimal sensor cam
		//only if we're a camera?
		
		//cache time started.
		timer = 0;
		
		MinimalSensorCamera.PauseSensorRotation();
		
		bRunning = true;
		
		if(!doFlythrough)
		{
			StartCoroutine(CutCamera());
		}
		
		UnityEngine.Debug.LogError("CamPath: Started following path");
	}
	
	IEnumerator CutCamera()
	{
		Vector3 originalPosition = transform.position;
		
		transform.position = new Vector3(1.20425f, 0.6631389f, 350.6969f);
		
		GameObject damsel = GameObject.Find("Damsel_Tracks");
		
		Time.timeScale = 0.0f;
		
		if(damsel != null)
		{
			transform.LookAt(damsel.transform);
		}
		
		DataVault.Set("train_subtitle", "Help!");
		FollowFlowLinkNamed("Subtitle");
		
		//yield return new WaitForSeconds(2.0f);
		System.DateTime continueTime = System.DateTime.Now.AddSeconds(2.0f);
		while(System.DateTime.Now < continueTime)
		{
			yield return null;	
		}
		
		FollowFlowLinkNamed("ToBlank");
		
		Time.timeScale = 1.0f;
		
		transform.position = originalPosition;
		
		StopFollowingPath();
	}
	
	protected void FollowFlowLinkNamed(string linkName)
	{
		FlowState fs = FlowStateMachine.GetCurrentFlowState();
		if(fs != null) {
			GConnector gConnect = fs.Outputs.Find( r => r.Name == linkName );
			if(gConnect != null)
			{
				fs.parentMachine.FollowConnection(gConnect);
			}
			else
			{
				UnityEngine.Debug.LogWarning("Train, CameraFlythrough: Couldn't find flow exit named " + linkName);
			}
		}
		else
		{
			UnityEngine.Debug.Log("PathPoint: flowstate is null");
		}
	}
	
	public void StopFollowingPath()
	{
		bRunning = false;	
		MinimalSensorCamera.ResumeSensorRotation();
		bFinished = true;
	}
}
