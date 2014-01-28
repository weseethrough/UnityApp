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
	
	public virtual JSONObject AsJson {
		get {
			JSONObject json = new JSONObject();
			json.AddField("device_id", deviceId);
			json.AddField("track_id", trackId);
			return json;
		}
	}
}
