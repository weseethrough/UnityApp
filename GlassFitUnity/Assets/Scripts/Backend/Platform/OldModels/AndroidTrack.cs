using UnityEngine;
using System.Collections.Generic;

using RaceYourself.Models;

public class AndroidTrack : Track {
	
	public AndroidTrack() {}
	
	public AndroidTrack(string name, int device_id, int track_id, List<Position> positions, float dist, long time) 
	{
		trackName = name;
		trackId = track_id;
		deviceId = device_id;
		this.positions = positions;
		distance = dist;
		this.ts = Date.UnixTime.Milliseconds;
		this.time = time;
		UnityEngine.Debug.Log("Track: date is " + date);
	}

#if UNITY_ANDROID
	public AndroidTrack(AndroidJavaObject rawtrack)
	{
		UnityEngine.Debug.Log("Platform: converting track from java");

		this.trackName = rawtrack.Call<string>("getName");
		int[] ids = rawtrack.Call<int[]>("getIDs");
		this.trackId = ids[1];
		this.deviceId = ids[0];
		this.distance = rawtrack.Call<double>("getDistance");
		this.time = rawtrack.Call<long>("getTime");
		this.ts = rawtrack.Get<long>("ts");

		using(AndroidJavaObject poslist = rawtrack.Call<AndroidJavaObject>("getTrackPositions")) {
			int numPositions = poslist.Call<int>("size");
			positions = new List<Position>(numPositions);
			for(int j=0; j<numPositions; j++) {
				AndroidJavaObject position = poslist.Call<AndroidJavaObject>("get", j);
				Position current = new Position((float)position.Call<double>("getLatx"), (float)position.Call<double>("getLngx"));
				positions.Add(current);
			}
		}
	}
#endif
	
}
