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
			ApplyTransFormForTime(timer);
		}
		
	}
	
	public bool IsFinished()
	{
		return bFinished;
	}
	
	protected void ApplyTransFormForTime(float t)
	{
		//Check if we've moved to the next node yet
		if(t > points[lastPointIndex+1].time)
		{
			//move onto the next one
			lastPointIndex++;
			//finish if this is the last point
			if(lastPointIndex +1 >= points.Count)
			{
				StopFollowingPath();
				return;
			}
		}
			
		//get the two points we're between
		PathPoint thisPoint = points[lastPointIndex];
		PathPoint nextPoint = points[lastPointIndex +1];
		
		//progress towards next point
		float p = (t-thisPoint.time)/(nextPoint.time - thisPoint.time);
		
		//lerp pos and ori
		Vector3 pos = Vector3.Lerp(thisPoint.transform.position, nextPoint.transform.position, p);
		transform.localPosition = pos;
		
		//lerp between which object we're looking at, or the static orientation of the nodes
		Quaternion rotThis, rotNext;
		if(thisPoint.lookAtTarget != null)
		{
			transform.LookAt(thisPoint.lookAtTarget.transform.position);
			rotThis = transform.rotation;
		}
		else
		{
			rotThis = thisPoint.GetNodeOrientation();	
		}
		
		if(nextPoint.lookAtTarget != null)
		{
			transform.LookAt(nextPoint.lookAtTarget.transform.position);
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
		bRunning = true;
		MinimalSensorCamera.PauseSensorRotation();
		
		UnityEngine.Debug.LogError("CamPath: Started following path");
	}
	
	public void StopFollowingPath()
	{
		bRunning = false;	
		MinimalSensorCamera.ResumeSensorRotation();
		bFinished = true;
	}
}
