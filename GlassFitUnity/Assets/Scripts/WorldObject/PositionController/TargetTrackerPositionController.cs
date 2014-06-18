using UnityEngine;
using System;
using System.Collections;
using System;

/// <summary>
/// Target tracker position controller.
/// Component which updates the world position of an WorldObject, based on a TargetTracker (i.e. a previous track, or live remote opponent)
/// </summary>
[Obsolete("Used for Java-based TargetTrackers - now that these are in Unity, TrackPositionController should be used instead.")]
public class TargetTrackerPositionController : PositionController {

	public TargetTracker tracker = null;

	// Use this for initialization
	public override void Start () {
		base.Start();
	}

	// Update is called once per frame
	public override void Update () {
		if(worldObject == null)
		{ return; }
		if(!Platform.Instance.LocalPlayerPosition.IsTracking)
		{ return; }
		if(tracker == null)
		{ return; }

		//apply distance from tracker to world object
		double distance = tracker.GetTargetDistance();
		worldObject.setRealWorldDist((float)distance);

		//apply speed to world object
		worldObject.setRealWorldSpeed(tracker.PollCurrentSpeed());

		base.Update();
	}
}
