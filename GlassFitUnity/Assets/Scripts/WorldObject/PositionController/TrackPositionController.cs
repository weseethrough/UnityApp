using UnityEngine;
using System.Collections;
using RaceYourself.Models;

public class TrackPositionController : PositionController {

	protected Track track = null;
	private int currentPosIndex = 0;
	private long trackStartTimestamp;
	private float elapsedtime;
	private double cumulativeDistance;

//	private bool started = false;

	// Use this for initialization
	public override void Start () {
		base.Start();
	}

	public void setTrack(Track t) {
		//TODO : load relevant track from the DataBase, and store reference to track
		track = t;
		//check start time of track
		trackStartTimestamp = track.positions[0].device_ts;
	}

//	public void Begin() {
//		if(track == null)
//		{
//			UnityEngine.Debug.Log("TrackPositionController: can't begin with no track!");
//		}
//		else
//		{
//			started = true;
//		}
//	}

	// Update is called once per frame
	public override void Update () {
		double totalDistance = 0;
		float speed = 0;

		if(Platform.Instance.LocalPlayerPosition.IsTracking)
		{
			//find the position entry before and after current time.
			Position prevPos;
			Position nextPos;

			prevPos = track.positions[currentPosIndex];
			nextPos = track.positions[currentPosIndex+1];

			//elapsedtime = Platform.Instance.LocalPlayerPosition.Time;
			elapsedtime += Time.deltaTime;
			//until we're at the right index
			float nextPosTime = (float)(nextPos.device_ts - trackStartTimestamp);
			while(elapsedtime > nextPosTime)
			{
				//shuffle index
				currentPosIndex++;

				//increment distance
				double extraDistance = PositionUtils.distanceBetween(prevPos, nextPos);
				cumulativeDistance += extraDistance;

				//update positions in use
				prevPos = nextPos;
				nextPos = track.positions[currentPosIndex+1];
				nextPosTime = (float)(nextPos.device_ts - trackStartTimestamp);
			}

			//calculate precise distance by lerping between entries
			float timeProportion = (elapsedtime - (prevPos.device_ts - trackStartTimestamp))/(nextPos.device_ts - prevPos.device_ts);
			double distToNext = PositionUtils.distanceBetween(prevPos, nextPos);
			double partialDistance = timeProportion * distToNext;
			totalDistance = cumulativeDistance + partialDistance;
			speed = Mathf.Lerp(prevPos.speed, nextPos.speed, timeProportion);
		}

		//update world object's distance and speed
		if(worldObject == null)
		{ return; }
		if(!Platform.Instance.LocalPlayerPosition.IsTracking)
		{ return; }
		if(track == null)
		{ return; }

		//apply distance from tracker to world object
		worldObject.setRealWorldDist((float)totalDistance);
		
		//apply speed to world object
		worldObject.setRealWorldSpeed(speed);
		
		base.Update();

	}

}
