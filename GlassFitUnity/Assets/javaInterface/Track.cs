using UnityEngine;
using System.Collections.Generic;

public class Track {
	
	public string trackName { get; set; }
	public int trackID { get; set; }
	public int deviceID { get; set; }
	public List<Position> trackPositions { get; set; }
	
	public Track() {}
	
	public Track(string name, int device_id, int track_id, List<Position> positions) 
	{
		trackName = name;
		trackID = track_id;
		deviceID = device_id;
		trackPositions = positions;
	}
}
