using UnityEngine;
using System.Collections.Generic;

public class Track {
	
	public virtual string trackName { get; set; }
	public int trackId { get; set; }
	public int deviceId { get; set; }
	public long time { get; set; }
	public double distance { get; set; }
	public string date { get; set; }
	public virtual List<Position> trackPositions { get; set; }
	
	public Track() {}
	
	public Track(string name, int device_id, int track_id, List<Position> positions, double dist, long time, string date) 
	{
		trackName = name;
		trackId = track_id;
		deviceId = device_id;
		trackPositions = positions;
		distance = dist;
		this.time = time;
		this.date = date;
		UnityEngine.Debug.Log("Track: date is " + date);
	}

#if UNITY_ANDROID
	public Track(AndroidJavaObject rawtrack)
	{
		UnityEngine.Debug.Log("Platform: converting track from java");

		this.trackName = rawtrack.Call<string>("getName");
		int[] ids = rawtrack.Call<int[]>("getIDs");
		this.trackId = ids[1];
		this.deviceId = ids[0];
		this.distance = rawtrack.Call<double>("getDistance");
		this.time = rawtrack.Call<long>("getTime");
		this.date = rawtrack.Call<string>("getDate");

		using(AndroidJavaObject poslist = rawtrack.Call<AndroidJavaObject>("getTrackPositions")) {
			int numPositions = poslist.Call<int>("size");
			trackPositions = new List<Position>(numPositions);
			for(int j=0; j<numPositions; j++) {
				AndroidJavaObject position = poslist.Call<AndroidJavaObject>("get", j);
				Position current = new Position((float)position.Call<double>("getLatx"), (float)position.Call<double>("getLngx"));
				trackPositions.Add(current);
			}
		}
	}
#endif
	
	public virtual JSONObject AsJson {
		get {
			JSONObject json = new JSONObject();
			json.AddField("device_id", deviceId);
			json.AddField("track_id", trackId);
			return json;
		}
	}
}
