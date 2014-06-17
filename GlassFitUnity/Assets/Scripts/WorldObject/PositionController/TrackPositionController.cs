using UnityEngine;
using System.Collections;
using RaceYourself.Models;
using RaceYourself;

public class TrackPositionController : PositionController {

	protected Track track = null;
	private int currentPosIndex = 0;
	private long trackStartTimestamp;
	private float elapsedtime;
	private double cumulativeDistance;
	private Log log = new Log("TrackPositionController");

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

		Position lastPos = track.positions[track.positions.Count-1];

		long duration = lastPos.device_ts - trackStartTimestamp;

		log.info("Starting Track " + track.trackId + ". Duration:" + duration + ", Distance:" + track.distance);
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
			int nextPosIndex = currentPosIndex+1;
			if(nextPosIndex >= track.positions.Count)
			{
				UnityEngine.Debug.LogError("TrackPositionController went too far into track");
				nextPosIndex = 0;
			}
			nextPos = track.positions[nextPosIndex];

			//elapsedtime = Platform.Instance.LocalPlayerPosition.Time;
			elapsedtime += Time.deltaTime;
			//until we're at the right index
			float nextPosTime = (float)(nextPos.device_ts - trackStartTimestamp);
			while(elapsedtime > nextPosTime && currentPosIndex < track.positions.Count)
			{
				//shuffle index
				currentPosIndex++;
				if(currentPosIndex >= track.positions.Count)
				{
					currentPosIndex--;
					log.error("TrackPositionController went too far into track");
					break;
				}

				long timeDelta = nextPos.device_ts - trackStartTimestamp;
				log.info("Moved to next pos in track: " + currentPosIndex + "/" + track.positions.Count + ". Time:" + timeDelta + "/" + elapsedtime);

				//increment distance
				double extraDistance = PositionUtils.distanceBetween(prevPos, nextPos);
				cumulativeDistance += extraDistance;

				//update positions in use
				prevPos = nextPos;
				nextPosIndex = currentPosIndex+1;
				if(nextPosIndex >= track.positions.Count)
				{
					UnityEngine.Debug.LogError("TrackPositionController went too far into track");
					nextPosIndex = currentPosIndex;
					break;
				}
				nextPos = track.positions[nextPosIndex];
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
